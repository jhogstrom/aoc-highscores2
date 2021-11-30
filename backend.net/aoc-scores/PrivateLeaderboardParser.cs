using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace RegenAoc
{
    public class PrivateLeaderboardParser
    {
        private readonly ILambdaLogger _logger;

        public PrivateLeaderboardParser(ILambdaLogger logger)
        {
            _logger = logger;
        }

        public async Task UpdatePlayersFromPrivateLeaderboard(BoardConfig config, List<Player> players)
        {
            var dynamoPlayers = await LoadFromDynamo(config);

            var aocPlayers = await RefreshLeaderboardData(config, players.Count != dynamoPlayers.Count);

            MatchData(players, dynamoPlayers, aocPlayers);
            await SaveToDynamo(config, dynamoPlayers.Values);
        }

        public void MatchData(List<Player> players, Dictionary<int, LeaderboardPlayer> dynamoPlayers,
            Dictionary<string, LeaderboardPlayer> aocPlayers)
        {
            foreach (var p in dynamoPlayers.Values)
                p.Deleted = true;

            foreach (var p in players)
            {
                LeaderboardPlayer aoc = null;
                if (p.AoCName != null)
                    aocPlayers.TryGetValue(p.AoCName, out aoc);
                if (aoc == null)
                    aocPlayers.TryGetValue($"(anonymous user #{p.Id})", out aoc);

                dynamoPlayers.TryGetValue(p.Id, out var dynamoPlayer);

                if (aoc != null)
                {
                    if (dynamoPlayer == null)
                    {
                        // new player from the private leaderboard, add to dynamo-list
                        dynamoPlayer = aoc;
                        dynamoPlayer.FirstSeen = DateTime.UtcNow;
                        dynamoPlayer.AocId = p.Id;
                        dynamoPlayer.Updated = true;
                        dynamoPlayers[p.Id] = aoc;
                    }
                    else
                    {
                        // player exists in dynamo, update fields
                        if (dynamoPlayer.Supporter != aoc.Supporter)
                        {
                            dynamoPlayer.Supporter = aoc.Supporter;
                            dynamoPlayer.Updated = true;
                        }
                        if (dynamoPlayer.PublicProfile != aoc.PublicProfile)
                        {
                            dynamoPlayer.PublicProfile = aoc.PublicProfile;
                            dynamoPlayer.Updated = true;
                        }
                    }
                }

                if (dynamoPlayer != null)
                {
                    // copy data to real player object
                    dynamoPlayer.Deleted = false;
                    p.Supporter = dynamoPlayer.Supporter;
                    p.PublicProfile = dynamoPlayer.PublicProfile;
                }
            }
        }

        public async Task<Dictionary<string, LeaderboardPlayer>> RefreshLeaderboardData(BoardConfig config, bool forceReload)
        {
            using var client = new AmazonS3Client(AwsHelpers.S3Region);

            var key = $"private/{config.Year}/{config.AocId}.html";

            var l = await client.ListObjectsAsync(AwsHelpers.InternalBucket, key);
            if (l.S3Objects.Any() && DateTime.Now < l.S3Objects.First().LastModified.AddDays(1) && !forceReload)
            {
                _logger.LogLine($"Private Leaderboard: Refreshing from s3 ({key})");
                // load the data from S3 and parse
                var contents = await client.GetContentsFromS3(key, AwsHelpers.InternalBucket);
                var doc = new HtmlDocument();
                doc.LoadHtml(contents);
                return ParseHtml(doc, config.Year);
            }

            var url = $"https://adventofcode.com/{config.Year}/leaderboard/private/view/{config.AocId}";
            _logger.LogLine($"Private Leaderboard, downloading html from aoc ({url})");
            var privateLeaderBoard = AocRefresher.DownloadFromURL(url, config.SessionCookie);
            var doc2 = new HtmlDocument();
            doc2.LoadHtml(privateLeaderBoard);

            var players = ParseHtml(doc2, config.Year);

            // save it to S3 for future use
            await client.WriteContentsToS3(AwsHelpers.InternalBucket, key, doc2.Text);

            return players;
        }


        public Dictionary<string, LeaderboardPlayer> ParseHtml(HtmlDocument html, int year)
        {
            var res = new Dictionary<string, LeaderboardPlayer>();
            var entries = html.DocumentNode.SelectNodes("//div[@class='privboard-row']");
            if (entries == null)
                return res;
            foreach (var entry in entries)
            {
                var p = new LeaderboardPlayer();

                if (entry.SelectNodes("span[@class='privboard-position']") == null)
                    continue;

                var nameNode = entry.SelectSingleNode("span[@class='privboard-name']");
                if (nameNode != null)
                {
                    var supporterNode = nameNode.SelectSingleNode("a[@class='supporter-badge']");
                    var profileLink = nameNode.SelectSingleNode("a[@target='_blank']");

                    if (profileLink != null)
                    {
                        p.PublicProfile = profileLink.Attributes["href"].Value;
                        p.Name = profileLink.InnerText;
                    }
                    else
                        p.Name = nameNode.FirstChild.InnerText.Trim();

                    if (supporterNode != null)
                    {
                        p.Supporter = true;
                    }
                }

                if (p.PublicProfile == null)
                    p.PublicProfile = "";
                if (p.Name.StartsWith("(anonymous user #"))
                    p.AocId = int.Parse(p.Name.Split(new[] { '#', ')' })[1]);
                p.Year = year;
                res[p.Name] = p;
            }
            return res;
        }


        private async Task<Dictionary<int, LeaderboardPlayer>> LoadFromDynamo(BoardConfig config)
        {
            var res = new Dictionary<int, LeaderboardPlayer>();
            using (var client = new AmazonDynamoDBClient(AwsHelpers.DynamoRegion))
            {
                var request = AwsHelpers.CreatePlayerInfoPKQueryRequest(config.Guid);

                var resp = await client.QueryAsync(request);

                foreach (var x in resp.Items)
                {
                    var playerName = x["name"].S;
                    var firstSeen = DateTime.Parse(x["firstDate"].S);
                    var supporter = x["supporter"].BOOL;
                    var profile = x["profile"].S;
                    var sk = x["sk"].S.Split('|');
                    var year = int.Parse(sk[0]);
                    var aocId = int.Parse(sk[1]);

                    if (year != config.Year)
                        continue; // should actually ad dthis to the sortkey in the query
                    var p = new LeaderboardPlayer()
                    {
                        Name = playerName,
                        PublicProfile = profile,
                        Supporter = supporter,
                        AocId = aocId,
                        Year = year,
                        FirstSeen = firstSeen
                    };
                    res[p.AocId.Value] = p;
                }
            }
            _logger.LogLine($"Private Leaderboard: Loaded {res.Count} players from dynamo");
            return res;
        }

        private async Task SaveToDynamo(BoardConfig config, IEnumerable<LeaderboardPlayer> players)
        {
            using (var client = new AmazonDynamoDBClient(AwsHelpers.DynamoRegion))
            {
                foreach (var p in players)
                {
                    if (!p.Updated)
                        continue;
                    var attributeValues = new Dictionary<string, AttributeValue>
                    {
                        ["id"] = new AttributeValue(config.Guid),
                        ["sk"] = new AttributeValue($"{p.Year}|{p.AocId}")
                    };

                    var deleteResp = await client.DeleteItemAsync(new DeleteItemRequest(AwsHelpers.PlayerInfoTableName, attributeValues));

                    if (p.Deleted)
                    {
                        _logger.LogLine($"Deleted player {p.Name} data from dynamo: ({deleteResp.HttpStatusCode})");
                    }
                    else
                    {
                        var putItemRequest = new PutItemRequest(AwsHelpers.PlayerInfoTableName, attributeValues)
                        {
                            Item =
                            {
                                ["name"] = new AttributeValue(p.Name),
                                ["firstDate"] = new AttributeValue(p.FirstSeen.ToString("O")),
                                ["supporter"] = new AttributeValue { BOOL = p.Supporter },
                                ["profile"] = new AttributeValue(p.PublicProfile ?? ""),
                            }
                        };
                        var resp = await client.PutItemAsync(putItemRequest);
                        _logger.LogLine($"Saved player {p.Name} data to dynamo: ({resp.HttpStatusCode})");
                    }
                }
            }
        }
    }

    public class LeaderboardPlayer
    {
        public string Name { get; set; }
        public bool Supporter { get; set; }
        public string PublicProfile { get; set; }
        public DateTime FirstSeen { get; set; }
        public int Year { get; set; }
        public int? AocId { get; set; }
        public bool Deleted { get; set; }
        public bool Updated { get; set; }
    }
}