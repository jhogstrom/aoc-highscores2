using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Newtonsoft.Json;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]


/*
 
{
  "Records": [
    {
      "messageId": "19dd0b57-b21e-4ac1-bd88-01bbb068cb78",
      "receiptHandle": "MessageReceiptHandle",
      "body": "{\"BoardGuid\": \"abc\", \"Year\": 2021}",
      "attributes": {
        "ApproximateReceiveCount": "1",
        "SentTimestamp": "1523232000000",
        "SenderId": "123456789012",
        "ApproximateFirstReceiveTimestamp": "1523232000001"
      },
      "messageAttributes": {},
      "md5OfBody": "7b270e59b47ff90a553787216d55d91d",
      "eventSource": "aws:sqs",
      "eventSourceARN": "arn:{partition}:sqs:{region}:123456789012:MyQueue",
      "awsRegion": "{region}"
    }
  ]
}

 */
namespace RegenAoc
{
    public class RegenAocLambda
    {
        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public RegenAocLambda()
        {

        }


        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task ReceiveEvent(SQSEvent evnt, ILambdaContext context)
        {
            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context);
            }
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            context.Logger.LogLine($"Processing message {message.Body}");
            var msg = JsonConvert.DeserializeObject<RegenQueueBody>(message.Body);
            context.Logger.LogLine($"List ID: {msg.BoardGuid} - year {msg.Year}");
            var refresher = new AocRefresher(context.Logger, AwsHelpers.InternalBucket);
            var listConfig = await GetBoardConfig(msg.BoardGuid, msg.Year);
            await refresher.EnsureFresh(listConfig, msg.Year);
            var gen = new AocGenerator();
            await gen.Generate(listConfig, msg.Year);
        }

        private async Task<BoardConfig> GetBoardConfig(string guid, int year)
        {
            return await BoardConfigHelper.LoadFromDynamo(guid, year);
            //            return BoardConfigHelper.LoadFromFile();
        }


    }

    public class BoardConfig
    {
        public BoardConfig()
        {
            ExcludeDays = new List<int>();
            Years = new List<int>();
            NameMap = new Dictionary<int, string>();
        }

        public string Guid { get; set; }
        public string AocId { get; set; }
        public List<int> Years { get; set; }
        public string SessionCookie { get; set; }
        public DateTime SessionCookieExpiration { get; set; }
        public string Name { get; set; }
        public List<int> ExcludeDays { get; set; }
        public Dictionary<int, string> NameMap { get; set; }
    }

    internal class RegenQueueBody
    {
        [JsonProperty("boardguid")]
        public string BoardGuid { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }
    }
}
