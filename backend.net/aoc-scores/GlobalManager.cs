using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    public class GlobalManager
    {
        private readonly ILambdaLogger _logger;

        public GlobalManager(ILambdaLogger logger)
        {
            _logger = logger;
        }

        public async Task<GlobalScore> GetGlobalScore(BoardConfig config, int highestDay)
        {
            var g = await LoadFromDynamo(config);
            for (int d = 1; d <= highestDay; d++)
            {
                var newDay = !g.Days.ContainsKey(d);
                if (newDay)
                {
                    var dayData = await RefreshGlobalData(config.Year, d);
                    g.Days[d] = dayData;
                    if (dayData.Stars[1].Players.Count == 100)
                    {
                        _logger.LogLine($"Saving day {d} to dynamo (*1:{dayData.Stars[0].Players.Count}, *2:{dayData.Stars[1].Players.Count})");
                        await SaveToDynamo(config, d, dayData, newDay);
                    }
                }
            }
            return g;
        }

        public async Task<GlobalDay> RefreshGlobalData(int year, int day)
        {
            using var client = new AmazonS3Client(AwsHelpers.S3Region);

            var key = $"global/{year}/{day}.html";

            var l = await client.ListObjectsAsync(AwsHelpers.InternalBucket, key);
            if (l.S3Objects.Any())
            {
                _logger.LogLine($"GlobalScore: Refreshing day {day} from s3 ({key})");
                // load the data from S3 and parse
                var req = new GetObjectRequest()
                {
                    BucketName = AwsHelpers.InternalBucket,
                    Key = key
                };
                var resp = await client.GetObjectAsync(req);
                await using var s = resp.ResponseStream;
                var reader = new StreamReader(s);
                var doc = new HtmlDocument();
                doc.LoadHtml(await reader.ReadToEndAsync());
                return ParseHtml(doc, day);
            }
            else
            {
                var url = $"https://adventofcode.com/{year}/leaderboard/day/{day}";
                _logger.LogLine($"GlobalScore: Refreshing day {day} from aoc ({url})");
                var web = new HtmlWeb();
                var doc = web.Load(url);

                var globalDay = ParseHtml(doc, day);

                var complete = globalDay.Stars[1].Players.Count == 100;
                if (complete)
                {
                    // save it to S3 for future use
                    var putObjectRequest = new PutObjectRequest
                    {
                        BucketName = AwsHelpers.InternalBucket,
                        Key = key,
                        ContentBody = doc.Text
                    };
                    await client.PutObjectAsync(putObjectRequest, CancellationToken.None);
                }
                else
                {
                    _logger.LogLine($"GlobalScore: data for day {day} not complete, skipping cache on S3");
                }

                return globalDay;
            }
        }

        public GlobalDay ParseHtml(HtmlDocument html, int day)
        {
            var res = new GlobalDay();
            var entries = html.DocumentNode.SelectNodes("//div[@class='leaderboard-entry']");
            var lastPosition = 0;
            var star = 1;
            res.Stars[0] = new GlobalStar();
            res.Stars[1] = new GlobalStar();
            foreach (var entry in entries)
            {
                var p = new GlobalPlayer();

                var posNode = entry.SelectSingleNode("span[@class='leaderboard-position']");
                p.Position = int.Parse(posNode.InnerText.TrimEnd(')'));
                p.Points = 100 - (p.Position-1);
                if (p.Position < lastPosition)
                {
                    star = 0;
                }

                lastPosition = p.Position;

                var time = entry.SelectSingleNode("span[@class='leaderboard-time']").InnerText;
                var days = int.Parse(time.Split(' ')[1]) - day;
                var solveTime = TimeSpan.Parse(time.Split(' ').Last()).Add(TimeSpan.FromDays(days));
                p.SolveTime = solveTime;

                var nameNode = entry.SelectSingleNode("a[span]"); // users with links
                if (nameNode == null)
                    nameNode = entry.SelectSingleNode("span[@class='leaderboard-anon']");
                if (nameNode == null)
                    nameNode = entry.ChildNodes.LastOrDefault(n => n.Name == "#text");
                var links = entry.SelectNodes("a");
                if (links != null)
                    foreach (var l in links)
                    {
                        if (l.InnerText == "(AoC++)")
                            p.Supporter = true;
                        else
                            p.PublicProfile = l.Attributes["href"].Value;

                    }

                p.Avatar = entry.SelectSingleNode("//span[@class='leaderboard-userphoto']/img[@src]")?.Attributes["src"].Value;
                p.Name = nameNode.InnerText.Trim();
                res.Stars[star].Players.Add(p);
            }
            return res;
        }


        private async Task<GlobalScore> LoadFromDynamo(BoardConfig config)
        {
            _logger.LogLine($"GlobalScore: Load from Dynamo");
            var res = new GlobalScore();
            using (var client = new AmazonDynamoDBClient(AwsHelpers.DynamoRegion))
            {
                var request = AwsHelpers.CreateGlobalScorePKQueryRequest(config.Year);

                var resp = await client.QueryAsync(request);

                foreach (var x in resp.Items)
                {
                    var day = int.Parse(x["day"].N);
                    var json = x["data"].S;
                    var dayData = JsonConvert.DeserializeObject<GlobalDay>(json);
                    res.Days[day] = dayData;
                    _logger.LogLine($"GlobalScore: Loaded day {day} from dynamo");
                }
            }
            _logger.LogLine($"GlobalScore: Loaded {res.Days.Count} days from dynamo");
            return res;
        }

        private async Task SaveToDynamo(BoardConfig config, int day, GlobalDay dayData, bool newDay)
        {
            using (var client = new AmazonDynamoDBClient(AwsHelpers.DynamoRegion))
            {
                var attributeValues = new Dictionary<string, AttributeValue>
                {
                    ["year"] = new AttributeValue { N = config.Year.ToString() },
                    ["day"] = new AttributeValue { N = day.ToString() }
                };

                if (!newDay)
                {
                    await client.DeleteItemAsync(new DeleteItemRequest(AwsHelpers.GlobalScoreTableName, attributeValues));
                }

                var putItemRequest = new PutItemRequest(AwsHelpers.GlobalScoreTableName, attributeValues)
                {
                    Item =
                    {
                        ["data"] = new AttributeValue(JsonConvert.SerializeObject(dayData))
                    }
                };

                var resp = await client.PutItemAsync(putItemRequest);
                _logger.LogLine($"Saved day {day} data to dynamo: ({resp.HttpStatusCode})");

            }
        }

    }

    public class GlobalScore
    {
        public GlobalScore()
        {
            Days = new Dictionary<int, GlobalDay>();
        }

        public Dictionary<int, GlobalDay> Days { get; }
        //        public List<GlobalPlayer> Players { get; }
    }

    public class GlobalDay
    {
        public GlobalDay()
        {
            Stars = new Dictionary<int, GlobalStar>();
        }

        public Dictionary<int, GlobalStar> Stars { get; }
    }

    public class GlobalStar
    {
        public GlobalStar()
        {
            Players = new List<GlobalPlayer>();
        }

        public List<GlobalPlayer> Players { get; }
    }

    [DebuggerDisplay("{Name}; {Position}; {SolveTime}")]
    public class GlobalPlayer
    {
        public string Name { get; set; }
        public int Position { get; set; }
        public bool Supporter { get; set; }
        public string PublicProfile { get; set; }
        public string Avatar { get; set; }
        public TimeSpan SolveTime { get; set; }
        public int Points { get; set; }
    }
}