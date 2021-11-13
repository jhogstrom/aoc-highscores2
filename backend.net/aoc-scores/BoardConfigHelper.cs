using System;
using System.IO;
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
                Years = new[] { 2019, 2020, 2021 },
                Guid = "7d3e8718-f15c-41ed-a561-fbba4f3fa37c",
                ExcludeDays = new []{6},
                SessionCookieExpiration = DateTime.Now.AddDays(20),
            };
        }

        public static void SaveFile(BoardConfig config)
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);

            File.WriteAllText("boardconfig-roundtrip.json", json);
        }
    }
}