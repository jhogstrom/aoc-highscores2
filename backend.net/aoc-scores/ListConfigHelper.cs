using System;
using System.IO;
using Newtonsoft.Json;

namespace RegenAoc
{
    public class ListConfigHelper
    {
        public static ListConfig LoadFromFile()
        {
            var fileName = "listconfig.json";
            if (File.Exists(fileName))
            {
                var readAllText = File.ReadAllText(fileName);
                return JsonConvert.DeserializeObject<ListConfig>(readAllText);
            }

            return new ListConfig()
            {
                AocId = "123456",
                ListName = "Test-list",
                SessionCookie = "dummycookie",
                Years = new []{2019, 2020, 2021},
                Guid = "7d3e8718-f15c-41ed-a561-fbba4f3fa37c",
                SessionCookieExpiration = DateTime.Now.AddDays(20),
                


            };
        }
    }
}