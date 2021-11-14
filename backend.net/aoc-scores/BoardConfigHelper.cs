using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
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
                Name = "Test-list",
                SessionCookie = "dummycookie",
                Years = new List<int> { 2019, 2020, 2021 },
                Guid = "7d3e8718-f15c-41ed-a561-fbba4f3fa37c",
                ExcludeDays = new List<int> { 6 },
                SessionCookieExpiration = DateTime.Now.AddDays(20),
            };
        }

        public static async Task<BoardConfig> LoadFromDynamo(string guid, int year)
        {
            var conf = new BoardConfig();
            conf.Guid = guid;

            using (var client = new AmazonDynamoDBClient())
            {
                // sk keys: 
                // BOARDINFO|<aoc-boardid> owner, name
                // NAMEMAP|<userid> to_name
                // YEAR|<year>
                // SESSION|<sessioncookie>

                var scanResponse = await client.ScanAsync(AwsHelpers.ConfigTableName, new List<string>());

                ProcessConf(conf, scanResponse.Items);

                var request = CreatePKQueryRequest(guid);

                var resp = await client.QueryAsync(request);

                ProcessConf(conf, resp.Items);

                request = CreatePKQueryRequest(year.ToString());

                resp = await client.QueryAsync(request);
                foreach (var r in resp.Items)
                {
                    if (int.TryParse(r["sk"].S, out var day))
                        conf.ExcludeDays.Add(day);
                }
            }
            return conf;
        }

        private static void ProcessConf(BoardConfig conf, List<Dictionary<string, AttributeValue>> items)
        {
            foreach (var r in items)
            {
                if (r["id"].S != conf.Guid)
                    continue;

                var sk = r["sk"].S;
                var parts = sk.Split('|');
                switch (parts[0])
                {
                    case "BOARDINFO":
                        conf.AocId = parts[1];
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
                }
            }
        }

        private static QueryRequest CreatePKQueryRequest(string key)
        {
            var request = new QueryRequest
            {
                TableName = AwsHelpers.ConfigTableName,
                KeyConditionExpression = "id = :pk",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":pk", new AttributeValue { S = key } },
                }
            };
            return request;
        }

        public static void SaveFile(BoardConfig config)
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);

            File.WriteAllText("boardconfig-roundtrip.json", json);
        }
    }
}