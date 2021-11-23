using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

namespace RegenAoc
{
    public class BoardConfigHelper
    {
        public static BoardConfig LoadFromFile()
        {
            var fileName = "boardconfig.json";
            if (File.Exists(fileName))
            {
                var readAllText = File.ReadAllText(fileName);
                return JsonConvert.DeserializeObject<BoardConfig>(readAllText);
            }

            return new BoardConfig()
            {
                AocId = "123456",
                Year = 2020,
                Name = "Test-list",
                SessionCookie = "dummycookie",
                Years = new List<int> { 2019, 2020, 2021 },
                Guid = "7d3e8718-f15c-41ed-a561-fbba4f3fa37c",
                ExcludeDays = new List<int> { 6 },
                SessionCookieExpiration = DateTime.Now.AddDays(20),
            };
        }

        public static async Task<BoardConfig> LoadFromDynamo(string guid, int year, ILambdaLogger logger)
        {
            var conf = new BoardConfig
            {
                Guid = guid,
                Year = year
            };

            using (var client = new AmazonDynamoDBClient(AwsHelpers.DynamoRegion))
            {
                // sk keys: 
                // BOARDINFO|<aoc-boardid> owner, name
                // NAMEMAP|<userid> to_name
                // YEAR|<year>
                // SESSION|<sessioncookie>

                // var scanResponse = await client.ScanAsync(AwsHelpers.ConfigTableName, new List<string>());
                //
                // ProcessConf(conf, scanResponse.Items.Where(i=>i["id"].S == guid));

                var request = AwsHelpers.CreateBoardConfigPKQueryRequest(guid);

                var resp = await client.QueryAsync(request);

                ProcessConf(conf, resp.Items, year, logger);

                request = AwsHelpers.CreateBoardConfigPKQueryRequest(year.ToString());

                resp = await client.QueryAsync(request);
                foreach (var r in resp.Items)
                {
                    var parts = r["sk"].S.Split('|');
                    switch (parts[0])
                    {
                        case "EXCLUDEDAY":
                            if (int.TryParse(parts[1], out var day))
                                conf.ExcludeDays.Add(day);
                            break;
                    }
                }
            }
            return conf;
        }

        private static void ProcessConf(BoardConfig conf, IEnumerable<Dictionary<string, AttributeValue>> items, int year, ILambdaLogger logger)
        {
            foreach (var r in items)
            {
                var sk = r["sk"].S;
                var parts = sk.Split('|');
                switch (parts[0])
                {
                    case "BOARDINFO":
                        conf.AocId = parts[1];
                        if (r.ContainsKey("name"))
                            conf.Name = r["name"].S;
                        break;
                    case "SESSION":
                        conf.SessionCookie = parts[1];
                        break;
                    case "YEAR":
                        if (int.TryParse(parts[1], out var y))
                            conf.Years.Add(y);
                        break;
                    case "NAMEMAP":
                        if (int.TryParse(parts[1], out var userid))
                            conf.NameMap[userid] = r["to_name"].S;
                        break;
                    case "EXCLUDEPLAYER":
                        if (parts.Length == 3 &&
                            int.TryParse(parts[1], out var excludeYear) &&
                            year >= excludeYear &&
                            int.TryParse(parts[2], out var playerId))
                            conf.ExcludePlayers[playerId] = excludeYear;
                        break;
                    default:
                        logger.LogLine("Unknown config directive: "+sk);
                        break;
                        
                }
            }
        }

  

        public static void SaveFile(BoardConfig config)
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);

            File.WriteAllText("boardconfig-roundtrip.json", json);
        }
    }
}