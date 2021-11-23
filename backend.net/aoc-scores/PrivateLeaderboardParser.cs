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

            MatchData(players, dynamoPlayers, aocPlayers, config);
            await SaveToDynamo(config, dynamoPlayers.Values);
        }

        public void MatchData(List<Player> players, Dictionary<int, LeaderboardPlayer> dynamoPlayers,
            Dictionary<string, LeaderboardPlayer> aocPlayers, BoardConfig config)
        {

            foreach (var p in dynamoPlayers.Values)
                p.Deleted = true;

//            var updateDynamo = false;
            foreach (var p in players)
            {
                LeaderboardPlayer aoc = null;
                if (p.AoCName != null)
                    aocPlayers.TryGetValue(p.AoCName, out aoc);
                if (aoc == null)
                    aocPlayers.TryGetValue($"(anonymous user #{p.Id})", out aoc);

                dynamoPlayers.TryGetValue(p.Id, out var dbPlayer);

                if (aoc != null)
                {
                    // fresh data available
                    p.Supporter = aoc.Supporter;
                    p.PublicProfile = aoc.PublicProfile;

                    if (dbPlayer == null)
                    {
                        aoc.FirstSeen = DateTime.Now;
                        aoc.AocId = p.Id;
                        aoc.Updated = true;
                        dynamoPlayers[p.Id] = aoc;
                    }
                    else
                    {
                        if (dbPlayer.Supporter != aoc.Supporter)
                        {
                            dbPlayer.Supporter = aoc.Supporter;
                            dbPlayer.Updated = true;
                        }
                        if (dbPlayer.PublicProfile != aoc.PublicProfile)
                        {
                            dbPlayer.PublicProfile = aoc.PublicProfile;
                            dbPlayer.Updated = true;
                        }
                    }
                }
                else if (dbPlayer != null)
                {
                    dbPlayer.Deleted = false;
                    p.Supporter = dbPlayer.Supporter;
                    p.PublicProfile = dbPlayer.PublicProfile;
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
                // _logger.LogLine($"Private Leaderboard: Refreshing from s3 ({key})");
                // // load the data from S3 and parse
                // var contents = await client.GetContentsFromS3(key, AwsHelpers.InternalBucket);
                // var doc = new HtmlDocument();
                // doc.LoadHtml(contents);
                // return ParseHtml(doc);
                return new Dictionary<string, LeaderboardPlayer>();
            }

            var url = $"https://adventofcode.com/{config.Year}/leaderboard/private/view/{config.AocId}";
            _logger.LogLine($"Private Leaderboard, downloding html from aoc ({url})");
            var privateLeaderBoard = AocRefresher.DownloadFromURL(url, config.SessionCookie);
            var doc = new HtmlDocument();
            doc.LoadHtml(privateLeaderBoard);

            var players = ParseHtml(doc);

            // save it to S3 for future use
            await client.WriteContentsToS3(AwsHelpers.InternalBucket, key, doc.Text);

            return players;
        }


        public Dictionary<string, LeaderboardPlayer> ParseHtml(HtmlDocument html)
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
                        p.PublicProfile = profileLink.Attributes["href"].Value??"";
                        p.Name = profileLink.InnerText;
                    }
                    else
                        p.Name = nameNode.FirstChild.InnerText.Trim();

                    if (supporterNode != null)
                    {
                        p.Supporter = true;
                    }
                }

                if (p.Name.StartsWith("(anonymous user #"))
                    p.AocId = int.Parse(p.Name.Split(new[] { '#', ')' })[1]);
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