using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Nest;
using Newtonsoft.Json;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace WebAdvert.SearchWorker
{
    public class Function
    {
        public Function() : this(ElasticSearchHelper.GetInstance(ConfigurationHelper.Instance))
        {

        }

        private readonly IElasticClient _client;
        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function(IElasticClient client)
        {
            _client = client;
        }


        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SNS event object and can be used 
        /// to respond to SNS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SNSEvent evnt, ILambdaContext context)
        {           
            foreach(var record in evnt.Records)
            {
                await ProcessRecordAsync(record, context);
            }
        }

        private async Task ProcessRecordAsync(SNSEvent.SNSRecord record, ILambdaContext context)
        {
            context.Logger.LogLine($"Processed record {record.Sns.Message}");

            var message = JsonConvert.DeserializeObject<AdvertConfirmedMessage>(record.Sns.Message);
            //Write the message into Elastic search
            var response= await _client.IndexDocumentAsync(message);
            context.Logger.LogLine($"Elastic search response {response.Result.ToString()}");

            await Task.CompletedTask;
        }
    }
}
