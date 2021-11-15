using System.Collections.Generic;
using Newtonsoft.Json;

namespace RegenAoc
{
    public class AocList
    {
        [JsonProperty("members")]
        public Dictionary<int, AocMember> Members { get; set; }

        [JsonProperty("event")]
        public string Event;

        [JsonProperty("owner_id")]
        public string OwnerId;

        public class AocMember
        {
            public int stars;
            public long last_star_ts;
            public int global_score;
            public string name;
            public int local_score;
            public int id;
            public Dictionary<int, Dictionary<int, AocStarInfo>> completion_day_level { get; set; }
        }

        public class AocStarInfo
        {
            public int get_star_ts;
        }
    }
}