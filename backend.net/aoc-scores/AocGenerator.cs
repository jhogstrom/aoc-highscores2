using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;

namespace RegenAoc
{
    public class AocGenerator
    {
        public async Task Generate(BoardConfig boardConfig, int year)
        {
            var list = await GetAocDataFromS3(boardConfig, year);
            var aocList = DeserializeAocJson(list);
            var leaderBoard = ConvertList(aocList);
            DeriveMoreStats(leaderBoard, year, boardConfig, true);
            await SaveToS3(leaderBoard, boardConfig, year);
        }

        private async Task SaveToS3(LeaderBoard leaderboard, BoardConfig boardConfig, int year)
        {
            using (var client = new AmazonS3Client())
            {
                var json = JsonConvert.SerializeObject(leaderboard);
                File.WriteAllText("leaderboard.json", json);


                PutObjectRequest req = new PutObjectRequest()
                {
                    BucketName = AwsHelpers.PublicBucket,
                    Key = AwsHelpers.PublicBucketKey(year, boardConfig.Guid),
                    ContentBody = json
                };
                var obj = await client.PutObjectAsync(req);
            }
        }

        private LeaderBoard ConvertList(AocList aocList)
        {
            var highestDay = 0;
            var players = new List<Player>();
            foreach (var member in aocList.Members.Values)
            {
                var player = new Player()
                {
                    Id = member.id,
                    GlobalScore = member.global_score,
                    LocalScore = member.local_score,
                    Stars = member.stars,
                    LastStar = member.last_star_ts,
                    Name = member.name
                };

                foreach (var dayNumber in member.completion_day_level.Keys)
                {
                    highestDay = Math.Max(highestDay, dayNumber);
                    var dayInfo = member.completion_day_level[dayNumber];
                    foreach (var star in dayInfo.Keys)
                    {
                        player.unixCompletionTime[dayNumber - 1][star - 1] = dayInfo[star].get_star_ts;
                    }
                }

                players.Add(player);
            }

            var leaderboard = new LeaderBoard(players, highestDay);
            return leaderboard;
        }

        private void DeriveMoreStats(LeaderBoard leaderboard, int year, BoardConfig boardConfig, bool excludeZero)
        {
            var bestTime = new TimeSpan[leaderboard.HighestDay][];
            var lastStar = new Dictionary<Player, long>();
            var playerCount = leaderboard.Players.Count;
            if (excludeZero)
                playerCount = leaderboard.Players.Count(p => p.Stars > 0);

            foreach (var player in leaderboard.Players)
            {
                lastStar[player] = -1;
            }

            for (int day = 0; day < leaderboard.HighestDay; day++)
            {
                var publishTime = new DateTime(year, 12, day + 1, 5, 0, 0);
                bestTime[day] = new TimeSpan[2];
                foreach (var player in leaderboard.Players)
                {
                    for (int star = 0; star < 2; star++)
                    {
                        var unixStarTime = player.unixCompletionTime[day][star];
                        if (unixStarTime != -1)
                        {
                            var starTime = DateTimeOffset.FromUnixTimeSeconds(unixStarTime).DateTime;
                            var timeSpan = starTime - publishTime;
                            player.TimeToComplete[day][star] = timeSpan;

                            var lastTime = day == 0 ? TimeSpan.Zero : player.AccumulatedTimeToComplete[day - 1][1];
                            if (lastTime.HasValue)
                                player.AccumulatedTimeToComplete[day][star] = lastTime + timeSpan;
                            if (bestTime[day][star] == TimeSpan.Zero || timeSpan < bestTime[day][star])
                                bestTime[day][star] = timeSpan;
                        }
                        else
                            player.PendingPoints += playerCount - leaderboard.StarsAwarded[day][star];
                    }

                    player.TimeToCompleteStar2[day] = player.TimeToComplete[day][1] - player.TimeToComplete[day][0];
                    if (player.TimeToCompleteStar2[day] < TimeSpan.FromSeconds(10) && day != 24)
                        player.TimeToCompleteStar2[day] = TimeSpan.MaxValue;
                }

                for (int star = 0; star < 2; star++)
                {
                    var orderedPlayers = leaderboard.Players.Where(p => p.unixCompletionTime[day][star] != -1)
                        .OrderBy(p => p.unixCompletionTime[day][star]).ThenBy(p => lastStar[p]).ToList();
                    foreach (var player in leaderboard.Players.OrderBy(p => p.unixCompletionTime[day][star]).ThenBy(p => lastStar[p]))
                    {
                        if (player.unixCompletionTime[day][star] != -1)
                        {
                            var index = orderedPlayers.IndexOf(player);
                            // handle ties
                            if (index > 0 && player.unixCompletionTime[day][star] == orderedPlayers[index - 1].unixCompletionTime[day][star])
                                player.PositionForStar[day][star] = orderedPlayers[index - 1].PositionForStar[day][star];
                            else
                                player.PositionForStar[day][star] = index;

                            if (!boardConfig.ExcludeDays.Contains(day))
                            {
                                player.TotalScore += playerCount - player.PositionForStar[day][star];
                                player.AccumulatedTobiiScoreTotal += player.PositionForStar[day][star];
                            }
                            player.OffsetFromWinner[day][star] = player.TimeToComplete[day][star] - bestTime[day][star];
                            lastStar[player] = player.unixCompletionTime[day][star];
                        }

                        player.AccumulatedScore[day][star] = player.TotalScore;
                        if (player.TotalScore > leaderboard.TopScorePerDay[day][star])
                            leaderboard.TopScorePerDay[day][star] = player.TotalScore;

                        player.AccumulatedTobiiScore[day][star] = player.AccumulatedTobiiScoreTotal;

                        player.LocalScore = player.TotalScore;

                    }
                }

                for (int star = 0; star < 2; star++)
                {
                    var orderedPlayers = leaderboard.Players.Where(p => p.AccumulatedScore[day][star] != 0)
                        .OrderByDescending(p => p.AccumulatedScore[day][star]).ToList();
                    foreach (var player in leaderboard.Players)
                    {
                        var index = orderedPlayers.IndexOf(player);
                        // handle ties
                        if (index > 0 && player.AccumulatedScore[day][star] == orderedPlayers[index - 1].AccumulatedScore[day][star])
                            player.AccumulatedPosition[day][star] = orderedPlayers[index - 1].AccumulatedPosition[day][star];
                        else
                            player.AccumulatedPosition[day][star] = index;
                    }
                }

                leaderboard.Players.Sort(new PlayerComparer());

                for (int i = 0; i < leaderboard.Players.Count; i++)
                    leaderboard.Players[i].CurrentPosition = i + 1;
            }
        }

        public AocList DeserializeAocJson(string list)
        {
            var x = JsonConvert.DeserializeObject(list);
            return JsonConvert.DeserializeObject<AocList>(list);
        }

        private async Task<string> GetAocDataFromS3(BoardConfig boardConfig, int year)
        {
            using (var client = new AmazonS3Client())
            {
                var obj = await client.GetObjectAsync(AwsHelpers.InternalBucket, AwsHelpers.InternalBucketKey(year, boardConfig.AocId));
                using (var s = obj.ResponseStream)
                {
                    var reader = new StreamReader(s);
                    return await reader.ReadToEndAsync();
                }
            }
        }

        public class AocList
        {
            [JsonProperty("members")]
            public Dictionary<int, AocMember> Members { get; set; }

            [JsonProperty("event")]
            public string Event;

            [JsonProperty("owner_id")]
            public string OwnerId;
        }

        public class AocMember
        {
            public int stars;
            public long last_star_ts;
            public int global_score;
            public string name;
            public int local_score;
            public int id;
            public Dictionary<int, Dictionary<int, AocStarInfo>> completion_day_level { get; set; }
        }

        public class AocStarInfo
        {
            public int get_star_ts;
        }
    }

    internal class PlayerComparer : IComparer<Player>
    {
        public int Compare(Player x, Player y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;

            var comp = x.LocalScore.CompareTo(y.LocalScore);
            if (comp != 0) return comp;

            comp = x.LastStar.CompareTo(y.LastStar);
            if (comp != 0) return comp;    
            
            return x.Id.CompareTo(y.Id);
        }
    }
}
