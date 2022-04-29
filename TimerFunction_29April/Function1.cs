using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using CloudStorageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;

namespace TimerFunction_29April
{
    public class LogEntity : TableEntity
    {
        public string Number { get; set; }
    }
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            CloudStorageAccount cls = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("ConnectionString", EnvironmentVariableTarget.Process));
            CloudQueueClient queueClient = cls.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference(Environment.GetEnvironmentVariable("QueueName",EnvironmentVariableTarget.Process));
            CloudQueueMessage queueMessage =await queue.GetMessageAsync();

            if (queueMessage != null)
            {
                CloudTableClient tableClient = cls.CreateCloudTableClient();
                CloudTable table = tableClient.GetTableReference(Environment.GetEnvironmentVariable("TableName",EnvironmentVariableTarget.Process));
                TableQuery<LogEntity> tableQuery = new TableQuery<LogEntity>();
                foreach(LogEntity entity in await table.ExecuteQuerySegmentedAsync(tableQuery, null))
                {
                    Console.WriteLine("Roll No : "+entity.PartitionKey);
                    Console.WriteLine("Name : "+entity.RowKey);
                    Console.WriteLine("Mobile Number : "+entity.Number);
                    Console.WriteLine();
                }
                Console.WriteLine("Queue Message : "+queueMessage.AsString);
                //await cloudQueue.DeleteMessageAsync(message.Id, message.PopReceipt);
            }
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}