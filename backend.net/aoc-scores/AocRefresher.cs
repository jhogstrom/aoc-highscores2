using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;

namespace RegenAoc
{
    public class AocRefresher
    {
        private readonly ILambdaLogger _logger;
        private string S3BucketName;
        private TimeSpan _throttlingTime = TimeSpan.FromSeconds(20);

        public AocRefresher(ILambdaLogger logger, string bucketName)
        {
            _logger = logger;
            S3BucketName = bucketName;
        }

        public async Task<bool> EnsureFresh(BoardConfig boardConfig, int year)
        {
            using (var client = new AmazonS3Client(AwsHelpers.S3Region))
            {
                var key = AwsHelpers.InternalBucketKey(year, boardConfig.AocId);
                var l = await client.ListObjectsAsync(S3BucketName, key);
                if (l.S3Objects.Any())
                {
                    var metadata = await client.GetObjectMetadataAsync(S3BucketName, key);

                    var age = DateTime.UtcNow - metadata.LastModified;
                    if (age < _throttlingTime)
                    {
                        _logger.LogLine($"S3 object is only {age.Minutes} min, {age.Seconds} s old, skipped download from AoC");
                        return false;
                    }
                }
                await DownloadLatestAocData(boardConfig, year, client, key);
                return true;
            }
        }

        private async Task<bool> DownloadLatestAocData(BoardConfig boardConfig, int year, AmazonS3Client client, string key)
        {
            var aocData = DownloadAocData(boardConfig, year);
            // compare aocData with stored data in S3, abort if identical?
            if (string.IsNullOrEmpty(aocData))
                return false;
            var putObjectRequest = new PutObjectRequest
            {
                BucketName = S3BucketName,
                Key = key,
                ContentBody = aocData
            };
            await client.PutObjectAsync(putObjectRequest, CancellationToken.None);
            return true;
        }

        private string DownloadAocData(BoardConfig boardConfig, int year)
        {
            _logger.LogLine($"Downloading new data for {boardConfig.Name}/{year} ({boardConfig.AocId})");

            var url = $"https://adventofcode.com/{year}/leaderboard/private/view/{boardConfig.AocId}.json";
            try
            {
                var s = DownloadFromURL(url, boardConfig.SessionCookie);
                if (string.IsNullOrEmpty(s))
                    _logger.LogLine("No data from AOC, Session cookie expired???");
                return s;
            }
            catch (Exception e)
            {
                _logger.LogLine("Failed to download:" + e.Message);
                return "";
            }
        }

        private string DownloadFromURL(string url, string cookie)
        {
            var cookies = new CookieContainer();

            // Create a WebRequest object and assign it a cookie container and make them think your Mozilla ;)
            cookies.Add(new Cookie("session", cookie, "/", ".adventofcode.com"));
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "GET";
            webRequest.Accept = "*/*";
            webRequest.AllowAutoRedirect = false;
            webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.0; .NET CLR 1.0.3705)";
            webRequest.CookieContainer = cookies;
            webRequest.Credentials = null;

            // Grab the response from the server for the current WebRequest
            using (var webResponse = webRequest.GetResponse())
            using (var stream = webResponse.GetResponseStream())
            using (var tr = new StreamReader(stream))
            {
                return tr.ReadToEnd();
            }
        }
    }
}