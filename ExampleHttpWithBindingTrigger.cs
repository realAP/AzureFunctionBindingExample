using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AzureFunctionBindingExample
{

    public class ExampleData
    {
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string partitionKey { get; set; } = "1";
        public DateTime timestamp { get; set; } = DateTime.UtcNow;
        public string message { get; set; }
    }

    public class MultiOutput
    {
        [CosmosDBOutput(
            databaseName: "ToDoList",
            containerName: "Items",
            Connection = "CosmosDbConnection",
            PartitionKey = "/partitionKey")]
        public ExampleData CosmosData { get; set; }
        
        [BlobOutput("dotnet/{rand-guid}.json", 
            Connection = "StorageConnection")]
        public string BlobData { get; set; }
        
        public HttpResponseData HttpResponse { get; set; }
    }


    public class ExampleHttpWithBindingTrigger
    {
        private readonly ILogger<ExampleHttpWithBindingTrigger> _logger;

        public ExampleHttpWithBindingTrigger(ILogger<ExampleHttpWithBindingTrigger> logger)
        {
            _logger = logger;
        }

        [Function("ExampleHttpWithBindingTrigger")]
        public async Task<MultiOutput> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            try
            {
                _logger.LogInformation("Processing request at {time}", DateTime.UtcNow);

                var data = new ExampleData
                {
                    message = $"Function executed at {DateTime.UtcNow}"
                };

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new
                {
                    message = "Welcome to Azure Functions!",
                    id = data.id,
                    timestamp = data.timestamp,
                    functionMessage = data.message
                });

                _logger.LogInformation("Data will be written to Cosmos DB with ID: {id}", data.id);

                return new MultiOutput
                {
                    CosmosData = data,
                    BlobData = JsonSerializer.Serialize(data, new JsonSerializerOptions 
                    { 
                        WriteIndented = true 
                    }),
                    HttpResponse = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { error = "Internal server error occurred" });
                
                return new MultiOutput
                {
                    HttpResponse = errorResponse
                };
            }
        }
    }
}
