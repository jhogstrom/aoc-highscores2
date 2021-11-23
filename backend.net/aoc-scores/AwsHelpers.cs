using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;
using HtmlAgilityPack;

namespace RegenAoc
{
    public static class AwsHelpers
    {
        public const string InternalBucket = "aochsstack-cache";
        public const string PublicBucket = "aochsstack-website";

        public static string ConfigTableName = "AoCHSStack-boardsconfig";
        public static string GlobalScoreTableName = "AoCHSStack-globalscores";
        public static string PlayerInfoTableName = "AoCHSStack-playerdata";

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

        public static QueryRequest CreateBoardConfigPKQueryRequest(string key)
        {
            return CreatePKQueryRequest(a => a.S = key, ConfigTableName, "id");
        }
        public static QueryRequest CreateGlobalScorePKQueryRequest(int year)
        {
            return CreatePKQueryRequest(a => a.N = year.ToString(), GlobalScoreTableName, "year");
        }
        public static QueryRequest CreatePlayerInfoPKQueryRequest(string boardGuid)
        {
            return CreatePKQueryRequest(a => a.S = boardGuid, PlayerInfoTableName, "id");
        }

        public static QueryRequest CreatePKQueryRequest(Action<AttributeValue> setValue, string tableName, string pkName)
        {
            var attributeValues = new List<AttributeValue> { new AttributeValue() };
            setValue(attributeValues.First());
            var condition = new Condition() { AttributeValueList = attributeValues, ComparisonOperator = ComparisonOperator.EQ };
            var request = new QueryRequest
            {
                TableName = tableName,
                KeyConditions = new Dictionary<string, Condition>() { [$"{pkName}"] = condition },
            };
            return request;
        }

        public static async Task<string> GetContentsFromS3(this AmazonS3Client client, string key, string bucket)
        {
            var req = new GetObjectRequest()
            {
                BucketName = bucket,
                Key = key
            };
            using var resp = await client.GetObjectAsync(req);
            await using var s = resp.ResponseStream;
            using var reader = new StreamReader(s);
            return await reader.ReadToEndAsync();
        }

        public static async Task WriteContentsToS3(this AmazonS3Client client, string bucket, string key,
            string content)
        {
            var putObjectRequest = new PutObjectRequest
            {
                BucketName = bucket,
                Key = key,
                ContentBody = content
            };
            await client.PutObjectAsync(putObjectRequest, CancellationToken.None);
        }
    }
}
