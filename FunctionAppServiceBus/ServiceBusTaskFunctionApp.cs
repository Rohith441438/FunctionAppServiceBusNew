using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionAppServiceBusTask
{
    public class ServiceBusTaskFunctionApp
    {
        private readonly ILogger<ServiceBusTaskFunctionApp> _logger;

        public ServiceBusTaskFunctionApp(ILogger<ServiceBusTaskFunctionApp> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ServiceBusTaskFunctionApp))]
        public async Task Run(
            [ServiceBusTrigger("orderqueue", Connection = "Endpoint=sb://servicebusdemorohith.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ea1IWccjcSUzRqXpzAs4a5p2syCL8wYaU+ASbAqPFa4=")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            string Connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            //string containerName = Environment.GetEnvironmentVariable("Container-name");
            _logger.LogInformation("Order was recieved");
            try
            {
                BlobContainerClient containerClient = GetContainerClient(Connection);

                // string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                _logger.LogInformation("Directory Creation started");
                string localPath = "data";
                //Directory.CreateDirectory(localPath);
                _logger.LogInformation("Directory Created");
                string fileName = "OrderModelPayload" + Guid.NewGuid().ToString() + ".json";
                string localFilePath = Path.Combine(localPath, fileName);
                _logger.LogInformation($"file name {fileName}, and localfilePath {localFilePath}");
                //await File.WriteAllTextAsync(localFilePath, message.ToString());
                _logger.LogInformation("Writing to path is done");

                BlobClient blobClient = containerClient.GetBlobClient(fileName);

                await blobClient.UploadAsync(message.Body.ToStream(), true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            //return new OkObjectResult("Order Data uploaded to Blob Successfully");
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }

        private static BlobContainerClient GetContainerClient(string Connection)
        {
            var blobServiceClient = new BlobServiceClient(Connection);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("order-item-reserver");
            containerClient.CreateIfNotExistsAsync().Wait();

            containerClient.SetAccessPolicy(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
            return containerClient;
        }
    }
}
