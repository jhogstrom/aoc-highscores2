using Amazon;

namespace RegenAoc
{
    public class AwsHelpers
    {
        public const string InternalBucket = "aochsstack-cache";
        public const string PublicBucket = "aochsstack-website";
        public static string ConfigTableName = "AoCHSStack-boardsconfig";
        public static RegionEndpoint DynamoRegion = RegionEndpoint.USEast2;
        public static RegionEndpoint S3Region = RegionEndpoint.USEast2;

        public static string InternalBucketKey(int year, string aocId)
        {
            return $"{year}/{aocId}.json";
        }

        public static string PublicBucketKey(int year, string boardGuid)
        {
            return $"{year}/{boardGuid}.json";
        }
    }
}
