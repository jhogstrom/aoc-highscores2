using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public async Task Generate(BoardConfig boardConfig)
        {
            var sw = Stopwatch.StartNew();
            LeaderBoard leaderBoard = null;
            int highestDay = -1;
            if (boardConfig.AocId != null)
            {
                _logger.LogLine($"{sw.ElapsedMilliseconds}: Loading AoC data from S3");
                var aocData = await GetAocDataFromS3(boardConfig);

                var aocList = DeserializeAocJson(aocData.Item1);
                _logger.LogLine($"{sw.ElapsedMilliseconds}: Computing AoC Stats");
                leaderBoard = ConvertList(aocList, boardConfig, aocData.LastModified);

                _logger.LogLine($"{sw.ElapsedMilliseconds}: Aoc Json converted");
                highestDay = leaderBoard.HighestDay;

                var privateLeaderboardParser = new PrivateLeaderboardParser(_logger);
                await privateLeaderboardParser.UpdatePlayersFromPrivateLeaderboard(boardConfig, leaderBoard.Players);
                _logger.LogLine($"{sw.ElapsedMilliseconds}: PrivateLeaderboard metadata Done...");

                HandlePlayerRenames(leaderBoard, boardConfig);
                _logger.LogLine($"{sw.ElapsedMilliseconds}: Player Renames Done...");
            }
            else
            {
                if (boardConfig.Year < DateTime.Now.Year)
                    highestDay = 25;
                else if (DateTime.Now.Month < 12)
                    highestDay = 0;
                else
                    highestDay = DateTime.Now.Day;
            }

            var globalMgr = new GlobalManager(_logger);
            var globalScore = await globalMgr.GetGlobalScore(boardConfig, highestDay);
            _logger.LogLine($"{sw.ElapsedMilliseconds}: Get GlobalScore Done...");

            if (boardConfig.AocId == null)
            {
                leaderBoard = CreateLeaderboardFromGlobalScore(globalScore, boardConfig, highestDay);
            }

            DeriveMoreStats(leaderBoard, boardConfig);
            _logger.LogLine($"{sw.ElapsedMilliseconds}: DeriveMoreStats Done...");

            ComputeGlobalScores(leaderBoard, globalScore);
            _logger.LogLine($"{sw.ElapsedMilliseconds}: ComputeGlobalScores Done...");
            leaderBoard.ProcessTime = DateTime.UtcNow - leaderBoard.Generated;
            _logger.LogLine($"{sw.ElapsedMilliseconds}: Uploading results to public S3 bucket");
            await SaveToS3(leaderBoard, boardConfig);
            _logger.LogLine($"{sw.ElapsedMilliseconds}: Upload complete");
        }

        private LeaderBoard CreateLeaderboardFromGlobalScore(GlobalScore globalScore, BoardConfig boardConfig, int highestDay)
        {
            var players = new Dictionary<string, Player>();
            foreach (var d in globalScore.Days.Keys)
            {
                var day = globalScore.Days[d];
                var unixDateTimeOfPublish = new DateTime(boardConfig.Year, 12, d + 1, 5, 0, 0, DateTimeKind.Utc);
                var unixTimeOfPublish = ((DateTimeOffset)unixDateTimeOfPublish).ToUnixTimeSeconds();

                foreach (var star in day.Stars.Keys)
                {
                    var starinfo = day.Stars[star];
                    foreach (var p in starinfo.Players)
                    {
                        if (!players.TryGetValue(p.Name, out var player))
                        {
                            player = new Player()
                            {
                                AoCName = p.Name,
                                Name = p.Name,
                                Id = -players.Count,
                                Supporter = p.Supporter,
                                Avatar = p.Avatar,
                                PublicProfile = p.PublicProfile
                            };
                            players[p.Name] = player;
                        }

                        player.UnixCompletionTime[d - 1][star] = (int)p.SolveTime.TotalSeconds + unixTimeOfPublish;
                        player.Stars++;
                    }
                }
            }

            return new LeaderBoard(players.Values.ToList(), highestDay, boardConfig.ExcludeDays, new List<string>(), DateTime.Now, boardConfig.Year, boardConfig.Name);
        }

        private void ComputeGlobalScores(LeaderBoard leaderBoard, GlobalScore globalScore)
        {
            for (var day = 0; day < leaderBoard.HighestDay; day++)
            {
                for (var star = 0; star < 2; star++)
                {
                    var globalStar = globalScore.Days[day + 1].Stars[star];
                    var scores = new Dictionary<string, List<GlobalPlayer>>();
                    foreach (var g in globalStar.Players)
                    {
                        var key = $"{g.Name}|{g.PublicProfile}";
                        if (!scores.TryGetValue(key, out var list))
                        {
                            list = new List<GlobalPlayer>();
                            scores[key] = list;
                        }
                        list.Add(g);
                    }

                    foreach (var p in leaderBoard.Players)
                    {
                        if (p.GlobalScore > 0 && scores.TryGetValue($"{p.AoCName}|{p.PublicProfile}", out var globalPlayerList))
                        {
                            GlobalPlayer globalPlayer = null;
                            foreach (var g in globalPlayerList)
                            {
                                // make sure to find the person with the correct solve time
                                if ((long)g.SolveTime.TotalSeconds == p.TimeToComplete[day][star])
                                {
                                    globalPlayer = g;
                                }
                            }

                            if (globalPlayer != null)
                            {
                                p.GlobalScoreForDay[day][star] = globalPlayer.Points;
                                p.Avatar = globalPlayer.Avatar;
                                p.Supporter = globalPlayer.Supporter;
                                p.PublicProfile = globalPlayer.PublicProfile;
                            }
                        }
                    }
                }
            }
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

        private async Task SaveToS3(LeaderBoard leaderBoard, BoardConfig boardConfig)
        {
            using (var client = new AmazonS3Client())
            {
                var json = JsonConvert.SerializeObject(leaderBoard);

                using (var stream = new MemoryStream(StreamHelper.Zip(json)))
                {
                    var req = new PutObjectRequest()
                    {
                        BucketName = AwsHelpers.PublicBucket,
                        Key = AwsHelpers.PublicBucketKey(boardConfig.Year, boardConfig.Guid),
                        InputStream = stream,
                        ContentType = "text/json"
                    };
                    req.Headers.ContentEncoding = "gzip";
                    await client.PutObjectAsync(req);
                }
            }
        }

        private LeaderBoard ConvertList(AocList aocList, BoardConfig boardConfig, DateTime aocLastModified)
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

            return new LeaderBoard(players, highestDay, boardConfig.ExcludeDays, excludedPlayers, aocLastModified, boardConfig.Year, boardConfig.Name);
        }

        private void DeriveMoreStats(LeaderBoard leaderBoard, BoardConfig boardConfig)
        {
            var bestTime = new int[leaderBoard.HighestDay][];
            var runningLastStar = new Dictionary<Player, long>();
            var playerCount = leaderBoard.Players.Count;
            var activePlayerCount = leaderBoard.Players.Count(p => p.Stars > 0);

            foreach (var player in leaderBoard.Players)
            {
                runningLastStar[player] = -1;
            }

            var localScoreComparer = new Player.LocalScoreComparer(p => p.LocalScoreAll);
            var activeLocalScoreComparer = new Player.LocalScoreComparer(p => p.LocalScoreActive);
            var tobiiScoreComparer = new Player.TobiiScoreComparer();

            for (int day = 0; day < leaderBoard.HighestDay; day++)
            {
                var publishTimeUTC = new DateTime(boardConfig.Year, 12, day + 1, 5, 0, 0);
                bestTime[day] = new int[2];

                // calculate the running starCount
                foreach (var player in leaderBoard.Players)
                {
                    for (int star = 0; star < 2; star++)
                    {
                        var prevStars = 0;
                        if (day > 0)
                        {
                            if (star == 0)
                                prevStars = player.StarCount[day - 1][1];
                            else
                            {
                                prevStars = player.StarCount[day][0];
                            }
                        }

                        if (player.UnixCompletionTime[day][star] != -1)
                            player.StarCount[day][star] = prevStars + 1;
                        else
                        {
                            player.StarCount[day][star] = prevStars;
                        }
                    }
                }


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
                            if (starTime.Year == boardConfig.Year)
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
                    }
                }

                for (int star = 0; star < 2; star++)
                {
                    CalculateAccumulatedPosition(leaderBoard, p => p.LocalScoreAll, day, star,
                        list => list
                            .OrderByDescending(p => p.LocalScoreAll.AccumulatedScore[day][star])
                            .ThenBy(p => p.LastStar),
                        localScoreComparer
                        );
                    CalculateAccumulatedPosition(leaderBoard, p => p.LocalScoreActive, day, star,
                        list => list
                            .OrderByDescending(p => p.LocalScoreActive.AccumulatedScore[day][star])
                            .ThenBy(p => p.LastStar),
                        activeLocalScoreComparer
                        );
                    CalculateAccumulatedPosition(leaderBoard, p => p.TobiiScore, day, star,
                        list => list
                            .OrderByDescending(p => p.Stars)
                            .ThenBy(p => p.TobiiScore.AccumulatedScore[day][star])
                            .ThenBy(p => p.LastStar),
                        tobiiScoreComparer
                    );
                }
            }

            CalculatePosition(leaderBoard, p => p.LocalScoreAll, localScoreComparer);
            CalculatePosition(leaderBoard, p => p.LocalScoreActive, activeLocalScoreComparer);
            CalculatePosition(leaderBoard, p => p.TobiiScore, tobiiScoreComparer);

        }

        private void ComputeAccumulatedScore(Player player, Player.ScoreRec scoreRec, int day, int star, Func<int, int> posToScore, BoardConfig boardConfig)
        {
            var completionTime = player.UnixCompletionTime[day][star];
            if (completionTime != -1)
            {
                if (!boardConfig.ExcludeDays.Contains(day))
                {
                    if (scoreRec.Score == -1)
                        scoreRec.Score = 0;
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

        private static void CalculateAccumulatedPosition(LeaderBoard leaderBoard,
            Func<Player, Player.ScoreRec> f,
            int day, int star,
            Func<IEnumerable<Player>, IEnumerable<Player>> orderFunc,
            Player.PlayerComparer comparer)
        {
            var players = leaderBoard.Players
                .Where(p => f(p).AccumulatedScore[day][star] != -1);
            var orderedPlayers = orderFunc(players).ToList();
            foreach (var player in leaderBoard.Players)
            {
                if (f(player).AccumulatedScore[day][star] != -1)
                {
                    var index = orderedPlayers.IndexOf(player);
                    var pos = index + 1;

                    // handle ties
                    if (index > 0 && comparer.ComparePosition(player, orderedPlayers[index - 1], day, star) == 0)
                        pos = f(orderedPlayers[index - 1]).AccumulatedPosition[day][star];

                    f(player).AccumulatedPosition[day][star] = pos;

                    f(player).ScoreDiff[day][star] =
                        f(orderedPlayers.First()).AccumulatedScore[day][star] -
                        f(player).AccumulatedScore[day][star];
                }
            }
        }

        public AocList DeserializeAocJson(string list)
        {
            return JsonConvert.DeserializeObject<AocList>(list);
        }

        private async Task<(string, DateTime LastModified)> GetAocDataFromS3(BoardConfig boardConfig)
        {
            using var client = new AmazonS3Client();

            var key = AwsHelpers.InternalBucketKey(boardConfig.Year, boardConfig.AocId);

            var metadataRequest = new GetObjectMetadataRequest();
            metadataRequest.BucketName = AwsHelpers.InternalBucket;
            metadataRequest.Key = key;
            var metadata = await client.GetObjectMetadataAsync(metadataRequest);

            var obj = await client.GetObjectAsync(AwsHelpers.InternalBucket, key);

            await using var s = obj.ResponseStream;
            var reader = new StreamReader(s);
            return (await reader.ReadToEndAsync(), metadata.LastModified);
        }
    }
}
