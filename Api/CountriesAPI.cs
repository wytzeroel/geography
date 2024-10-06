using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;

using System.Net;
using BlazorApp.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Net;
using System.Web.Mvc;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using HttpMultipartParser;
using System.Reflection;
using System.Drawing;

namespace Api
{
    public class HttpTrigger
    {
        private readonly ILogger _logger;
        private readonly TableClient _tableClient;
        private readonly BlobServiceClient _blobServiceClient;

        public HttpTrigger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HttpTrigger>();
            _tableClient = new TableClient("DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;", "Countries");
            _tableClient.CreateIfNotExists();
            _blobServiceClient = new BlobServiceClient("DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;");
        }

        [Function("GetCountries")]
        public HttpResponseData GetCountries([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            var result = _tableClient.Query<Country>().ToList();
            response.WriteAsJsonAsync(result);

            return response;
        }

        [Function("AddCountry")]
        public HttpResponseData AddCountry([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req, [FromBody] Country country)
        {
            _logger.LogInformation(country.Name);
            country.RowKey = country.Name;
            country.PartitionKey = "Country";
            _tableClient.AddEntity(country);
            return req.CreateResponse(HttpStatusCode.OK);
        }

        [Function("DeleteCountry")]
        public HttpResponseData DeleteCountry([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "DeleteCountry/{name}")] HttpRequestData req, string name)
        {
            _tableClient.DeleteEntity("Country", name);
            return req.CreateResponse(HttpStatusCode.OK);
        }

        [Function("UploadFlag")]
        public HttpResponseData UploadFlag([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            try
            {
                // Read form data
                var formData = MultipartFormDataParser.Parse(req.Body);

                // Extract the uploaded file
                var file = formData.Files[0];

                if (file == null || file.Data.Length == 0)
                {
                    return CreateErrorResponse(req, HttpStatusCode.BadRequest, "No file uploaded.");
                }

                // Extract form fields
                var name = formData.GetParameterValue("name");

                _logger.LogInformation($"Received file: {file.FileName}, Name: {name}");

                // Upload the file to Azure Blob Storage
                var blobContainerClient = _blobServiceClient.GetBlobContainerClient("flags");

                // Ensure the container exists
                blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                // Get a reference to the blob
                var blobClient = blobContainerClient.GetBlobClient(name);

                // Upload file
                blobClient.UploadAsync(file.Data, new BlobHttpHeaders { ContentType = file.ContentType });

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.WriteStringAsync("File uploaded to Blob Storage successfully.");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred: {ex.Message}");
                return CreateErrorResponse(req, HttpStatusCode.InternalServerError, $"Error: {ex.Message}");
            }
        }


        // Helper to create error response
        private HttpResponseData CreateErrorResponse(HttpRequestData req, HttpStatusCode statusCode, string message)
        {
            var response = req.CreateResponse(statusCode);
            response.WriteString(message);
            return response;
        }

    }
}