using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;

namespace RegenAoc
{
    public class AocGenerator
    {
        private readonly ILambdaLogger _logger;

        public AocGenerator(ILambdaLogger logger)
        {
            _logger = logger;
        }

        public async Task Generate(BoardConfig boardConfig, int year)
        {
            _logger.LogLine("Loading AoC data from S3");
            var list = await GetAocDataFromS3(boardConfig, year);
            var aocList = DeserializeAocJson(list);
            _logger.LogLine("Computing AoC Stats");
            var leaderBoard = ConvertList(aocList, boardConfig);
            HandlePlayerRenames(leaderBoard, boardConfig);
            DeriveMoreStats(leaderBoard, year, boardConfig);
            _logger.LogLine("Uploading results to public S3 bucket");
            await SaveToS3(leaderBoard, boardConfig, year);
        }

        private void HandlePlayerRenames(LeaderBoard leaderBoard, BoardConfig boardConfig)
        {
            foreach (var p in leaderBoard.Players)
            {
                if (boardConfig.NameMap.TryGetValue(p.Id, out var name))
                    p.Name = name;
                else
                {
                    p.Name = p.AoCName;
                }
            }
        }

        private async Task SaveToS3(LeaderBoard leaderBoard, BoardConfig boardConfig, int year)
        {
            using (var client = new AmazonS3Client())
            {
                var json = JsonConvert.SerializeObject(leaderBoard);

                using (var stream = new MemoryStream(StreamHelper.Zip(json)))
                {
                    var req = new PutObjectRequest()
                    {
                        BucketName = AwsHelpers.PublicBucket,
                        Key = AwsHelpers.PublicBucketKey(year, boardConfig.Guid),
                        InputStream = stream,
                        ContentType = "text/json"
                    };
                    req.Headers.ContentEncoding = "gzip";
                    await client.PutObjectAsync(req);
                }
            }
        }

        private LeaderBoard ConvertList(AocList aocList, BoardConfig boardConfig)
        {
            var highestDay = 0;
            var players = new List<Player>();
            var excludedPlayers = new List<string>();
            foreach (var member in aocList.Members.Values)
            {
                if (boardConfig.ExcludePlayers.Contains(member.id))
                {
                    _logger.LogLine($"Excluded player {member.id}, {member.name}");
                    excludedPlayers.Add($"{member.id}:{member.name}");
                    continue;
                }

                var player = new Player
                {
                    Id = member.id,
                    GlobalScore = member.global_score,
                    AoCLocalScore = member.local_score,
                    Stars = member.stars,
                    LastStar = member.last_star_ts,
                    AoCName = member.name
                };

                foreach (var dayNumber in member.completion_day_level.Keys)
                {
                    highestDay = Math.Max(highestDay, dayNumber);
                    var dayInfo = member.completion_day_level[dayNumber];
                    foreach (var star in dayInfo.Keys)
                    {
                        player.UnixCompletionTime[dayNumber - 1][star - 1] = dayInfo[star].get_star_ts;
                    }
                }

                players.Add(player);
            }

            return new LeaderBoard(players, highestDay, boardConfig.ExcludeDays, excludedPlayers);
        }

        private void DeriveMoreStats(LeaderBoard leaderBoard, int year, BoardConfig boardConfig)
        {
            var bestTime = new TimeSpan[leaderBoard.HighestDay][];
            var runningLastStar = new Dictionary<Player, long>();
            var playerCount = leaderBoard.Players.Count;
            var activePlayerCount = leaderBoard.Players.Count(p => p.Stars > 0);

            foreach (var player in leaderBoard.Players)
            {
                runningLastStar[player] = -1;
            }

            for (int day = 0; day < leaderBoard.HighestDay; day++)
            {
                var publishTime = new DateTime(year, 12, day + 1, 5, 0, 0);
                bestTime[day] = new TimeSpan[2];
                foreach (var player in leaderBoard.Players)
                {
                    for (int star = 0; star < 2; star++)
                    {
                        var unixStarTime = player.UnixCompletionTime[day][star];
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
                        {
                            player.PendingLocalPoints += playerCount - leaderBoard.StarsAwarded[day][star];
                            player.PendingActiveLocalPoints += activePlayerCount - leaderBoard.StarsAwarded[day][star];
                        }
                    }

                    player.TimeToCompleteStar2[day] = player.TimeToComplete[day][1] - player.TimeToComplete[day][0];
                    if (player.TimeToCompleteStar2[day] < TimeSpan.FromSeconds(10) && day != 24)
                        player.TimeToCompleteStar2[day] = TimeSpan.MaxValue;
                }

                for (int star = 0; star < 2; star++)
                {
                    var orderedPlayers = leaderBoard.Players
                        .Where(p => p.UnixCompletionTime[day][star] != -1)
                        .OrderBy(p => p.UnixCompletionTime[day][star])
                        .ThenBy(p => runningLastStar[p]).ToList();

                    foreach (var player in leaderBoard.Players.OrderBy(p => p.UnixCompletionTime[day][star]).ThenBy(p => runningLastStar[p]))
                    {
                        var completionTime = player.UnixCompletionTime[day][star];
                        if (completionTime != -1)
                        {
                            var index = orderedPlayers.IndexOf(player);
                            // handle ties
                            if (index > 0 && completionTime == orderedPlayers[index - 1].UnixCompletionTime[day][star])
                                player.PositionForStar[day][star] = orderedPlayers[index - 1].PositionForStar[day][star];
                            else
                                player.PositionForStar[day][star] = index;

                            if (!boardConfig.ExcludeDays.Contains(day))
                            {
                                var playerPosition = player.PositionForStar[day][star];
                                player.LocalScore += playerCount - playerPosition;
                                player.ActiveLocalScore += activePlayerCount - playerPosition;
                                player.TobiiScore += playerPosition;
                            }
                            player.OffsetFromWinner[day][star] = player.TimeToComplete[day][star] - bestTime[day][star];
                            runningLastStar[player] = completionTime;
                        }

                        player.AccumulatedLocalScore[day][star] = player.LocalScore;
                        player.AccumulatedActiveLocalScore[day][star] = player.ActiveLocalScore;
                        player.AccumulatedTobiiScore[day][star] = player.TobiiScore;

                        // if (player.LocalScore > leaderBoard.TopLocalScore[day][star])
                        //     leaderBoard.TopLocalScore[day][star] = player.LocalScore;
                        // if (player.ActiveLocalScore > leaderBoard.TopActiveLocalScore[day][star])
                        //     leaderBoard.TopActiveLocalScore[day][star] = player.ActiveLocalScore;
                    }
                }

                for (int star = 0; star < 2; star++)
                {
                    var orderedPlayers = leaderBoard.Players.Where(p => p.AccumulatedLocalScore[day][star] != 0)
                        .OrderByDescending(p => p.AccumulatedLocalScore[day][star]).ToList();
                    foreach (var player in leaderBoard.Players)
                    {
                        var index = orderedPlayers.IndexOf(player);
                        // handle ties
                        if (index > 0 && player.AccumulatedLocalScore[day][star] == orderedPlayers[index - 1].AccumulatedLocalScore[day][star])
                            player.AccumulatedPosition[day][star] = orderedPlayers[index - 1].AccumulatedPosition[day][star];
                        else
                            player.AccumulatedPosition[day][star] = index;
                    }
                }

                leaderBoard.Players.Sort(new Player.PlayerComparer(false));

                for (int i = 0; i < leaderBoard.Players.Count; i++)
                    leaderBoard.Players[i].Position = i + 1;

                leaderBoard.Players.Sort(new Player.PlayerComparer(true));

                for (int i = 0; i < leaderBoard.Players.Count; i++)
                    leaderBoard.Players[i].ActivePosition = i + 1;
            }
        }

        public AocList DeserializeAocJson(string list)
        {
            return JsonConvert.DeserializeObject<AocList>(list);
        }

        private async Task<string> GetAocDataFromS3(BoardConfig boardConfig, int year)
        {
            using var client = new AmazonS3Client();
            var obj = await client.GetObjectAsync(AwsHelpers.InternalBucket, AwsHelpers.InternalBucketKey(year, boardConfig.AocId));
            await using var s = obj.ResponseStream;
            var reader = new StreamReader(s);
            return await reader.ReadToEndAsync();
        }
    }
}
