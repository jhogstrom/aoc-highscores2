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
            var aocData = await GetAocDataFromS3(boardConfig, year);
            var aocList = DeserializeAocJson(aocData.Item1);
            _logger.LogLine("Computing AoC Stats");
            var leaderBoard = ConvertList(aocList, boardConfig, aocData.LastModified, year);
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
                    p.Name = string.IsNullOrEmpty(p.AoCName) ? $"Anon ({p.Id})" : p.AoCName;
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

        private LeaderBoard ConvertList(AocList aocList, BoardConfig boardConfig, DateTime aocLastModified, int year)
        {
            var highestDay = 0;
            var players = new List<Player>();
            var excludedPlayers = new List<string>();
            foreach (var member in aocList.Members.Values)
            {
                if (boardConfig.ExcludePlayers.ContainsKey(member.id))
                {
                    _logger.LogLine($"Excluded player {member.id}, {member.name} ({boardConfig.ExcludePlayers[member.id]})");
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

            return new LeaderBoard(players, highestDay, boardConfig.ExcludeDays, excludedPlayers, aocLastModified, year);
        }

        private void DeriveMoreStats(LeaderBoard leaderBoard, int year, BoardConfig boardConfig)
        {
            var bestTime = new int[leaderBoard.HighestDay][];
            var runningLastStar = new Dictionary<Player, long>();
            var playerCount = leaderBoard.Players.Count;
            var activePlayerCount = leaderBoard.Players.Count(p => p.Stars > 0);

            foreach (var player in leaderBoard.Players)
            {
                runningLastStar[player] = -1;
            }

            for (int day = 0; day < leaderBoard.HighestDay; day++)
            {
                var publishTimeUTC = new DateTime(year, 12, day + 1, 5, 0, 0);
                bestTime[day] = new int[2];
                
                foreach (var player in leaderBoard.Players)
                {
                    for (int star = 0; star < 2; star++)
                    {
                        var unixStarTime = player.UnixCompletionTime[day][star];
                        if (unixStarTime != -1)
                        {
                            var starTime = DateTimeOffset.FromUnixTimeSeconds(unixStarTime).DateTime;
                            var timeSpan = starTime - publishTimeUTC;
                            var timeToSolve = (int)timeSpan.TotalSeconds;
                            player.TimeToComplete[day][star] = timeToSolve;

                            // get the accumulated time to solve previous days
                            var accumulatedTime = day == 0 ? 0 : player.AccumulatedTimeToComplete[day - 1][1];
                            if (accumulatedTime != -1)
                                player.AccumulatedTimeToComplete[day][star] = accumulatedTime + timeToSolve;
                            if (bestTime[day][star] == 0 || timeToSolve < bestTime[day][star])
                                bestTime[day][star] = timeToSolve;

                            // raffle tickets awarded for stars before new years
                            if (starTime.Year == year)
                                player.RaffleTickets++;
                            // raffle tickets awarded for completing both stars in 24h 
                            if (star == 1 && timeSpan.TotalDays < 1)
                                player.RaffleTickets++;
                        }
                        else
                        {
                            player.LocalScoreAll.PendingPoints += playerCount - leaderBoard.StarsAwarded[day][star];
                            player.LocalScoreActive.PendingPoints +=
                                activePlayerCount - leaderBoard.StarsAwarded[day][star];
                        }
                    }

                    if (player.UnixCompletionTime[day][1] != -1)
                    {
                        player.TimeToCompleteStar2[day] = player.TimeToComplete[day][1] - player.TimeToComplete[day][0];
                        // punish anyone who solves star 2 in < 10 seconds (except for xmas day where it is actually possible
                        if (player.TimeToCompleteStar2[day] < 10 && day != 24)
                        {
                            player.TimeToCompleteStar2[day] = 60 * 60 * 24;
                            player.Fraud.Add(day);
                        }
                    }
                }

                {
                    var orderedPlayers = leaderBoard.Players
                        .Where(p => p.TimeToCompleteStar2[day] != -1)
                        .OrderBy(p => p.TimeToCompleteStar2[day])
                        .ToList();
                    foreach (var player in orderedPlayers)
                    {
                        var index = orderedPlayers.IndexOf(player);
                        var pos = index;
                        if (index > 0 && player.TimeToCompleteStar2[day] ==
                            orderedPlayers[index - 1].TimeToCompleteStar2[day])
                            pos = orderedPlayers[index - 1].PositionStar2[day];

                        player.PositionStar2[day] = pos;
                    }
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
                            var pos = index;
                            // handle ties
                            if (index > 0 && completionTime == orderedPlayers[index - 1].UnixCompletionTime[day][star])
                                pos = orderedPlayers[index - 1].PositionForStar[day][star];

                            player.PositionForStar[day][star] = pos;
                            if (pos < 3)
                                player.Medals[pos]++;

                            player.OffsetFromWinner[day][star] = player.TimeToComplete[day][star] - bestTime[day][star];
                            runningLastStar[player] = completionTime;

                            // raffle tickets awarded for medals
                            if (pos < 3)
                                player.RaffleTickets += 3 - pos;
                        }

                        ComputeAccumulatedScore(player, player.LocalScoreAll, day, star, pos => playerCount - pos, boardConfig);
                        ComputeAccumulatedScore(player, player.LocalScoreActive, day, star, pos => activePlayerCount - pos, boardConfig);
                        ComputeAccumulatedScore(player, player.TobiiScore, day, star, pos => pos, boardConfig);

                        // if (player.LocalScore > leaderBoard.TopLocalScore[day][star])
                        //     leaderBoard.TopLocalScore[day][star] = player.LocalScore;
                        // if (player.ActiveLocalScore > leaderBoard.TopActiveLocalScore[day][star])
                        //     leaderBoard.TopActiveLocalScore[day][star] = player.ActiveLocalScore;
                    }
                }

                for (int star = 0; star < 2; star++)
                {
                    CalculateAccumulatedPosition(leaderBoard, p => p.LocalScoreAll, day, star, -1);
                    CalculateAccumulatedPosition(leaderBoard, p => p.LocalScoreActive, day, star, -1);
                    CalculateAccumulatedPosition(leaderBoard, p => p.TobiiScore, day, star, +1);
                }
            }
            CalculatePosition(leaderBoard, p => p.LocalScoreAll, new Player.LocalScoreComparer(p => p.LocalScoreAll));
            CalculatePosition(leaderBoard, p => p.LocalScoreActive, new Player.LocalScoreComparer(p => p.LocalScoreActive));
            CalculatePosition(leaderBoard, p => p.TobiiScore, new Player.TobiiScoreComparer());

        }

        private void ComputeAccumulatedScore(Player player, Player.ScoreRec scoreRec, int day, int star, Func<int, int> posToScore, BoardConfig boardConfig)
        {
            var completionTime = player.UnixCompletionTime[day][star];
            if (completionTime != -1)
            {
                if (!boardConfig.ExcludeDays.Contains(day))
                {
                    scoreRec.Score += posToScore(player.PositionForStar[day][star]);
                }
            }

            scoreRec.AccumulatedScore[day][star] = scoreRec.Score;
        }

        private static void CalculatePosition(LeaderBoard leaderBoard, Func<Player, Player.ScoreRec> func, IComparer<Player> localScoreComparer)
        {
            leaderBoard.Players.Sort(localScoreComparer);

            for (int i = 0; i < leaderBoard.Players.Count; i++)
                func(leaderBoard.Players[i]).Position = i + 1;
        }

        private static void CalculateAccumulatedPosition(LeaderBoard leaderBoard, Func<Player, Player.ScoreRec> f,
            int day, int star, int sortOrder)
        {
            var orderedPlayers = leaderBoard.Players
                .Where(p => p.PositionForStar[day][star] != -1)
                .OrderBy(p => sortOrder * f(p).AccumulatedScore[day][star])
                .ToList();
            foreach (var player in leaderBoard.Players)
            {
                var index = orderedPlayers.IndexOf(player);
                // handle ties
                if (index > 0 && f(player).AccumulatedScore[day][star] ==
                    f(orderedPlayers[index - 1]).AccumulatedScore[day][star])
                    f(player).AccumulatedPosition[day][star] = f(orderedPlayers[index - 1]).AccumulatedPosition[day][star];
                else
                    f(player).AccumulatedPosition[day][star] = index;

                f(player).ScoreDiff[day][star] = 
                    f(orderedPlayers.First()).AccumulatedScore[day][star]-
                    f(player).AccumulatedScore[day][star];
            }
        }

        public AocList DeserializeAocJson(string list)
        {
            return JsonConvert.DeserializeObject<AocList>(list);
        }

        private async Task<(string, DateTime LastModified)> GetAocDataFromS3(BoardConfig boardConfig, int year)
        {
            using var client = new AmazonS3Client();
            var obj = await client.GetObjectAsync(AwsHelpers.InternalBucket, AwsHelpers.InternalBucketKey(year, boardConfig.AocId));
            var metadataRequest = new GetObjectMetadataRequest();
            metadataRequest.BucketName = AwsHelpers.InternalBucket;
            metadataRequest.Key = AwsHelpers.InternalBucketKey(year, boardConfig.AocId);
            var obj2 = await client.GetObjectMetadataAsync(metadataRequest);


            await using var s = obj.ResponseStream;
            var reader = new StreamReader(s);
            return (await reader.ReadToEndAsync(), obj2.LastModified);
        }
    }
}
