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
    public class HighScoreAPI
    {
        private readonly ILogger _logger;
        private readonly TableClient _tableClient;

        public HighScoreAPI(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HttpTrigger>();
            _tableClient = new TableClient("DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;", "HighScores");
            _tableClient.CreateIfNotExists();
        }

        [Function("GetHighScores")]
        public HttpResponseData GetHighScores([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetHighScores/{name}")] HttpRequestData req, string name)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            var result = _tableClient.Query<HighScore>().Where(h => h.PartitionKey == name).OrderBy(h => -h.Score).Take(10).ToList();
            // var result = _tableClient.Query<HighScore>().ToList();

            response.WriteAsJsonAsync(result);
            return response;
        }

        [Function("AddHighScore")]
        public HttpResponseData AddHighScore([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req, [FromBody] HighScore highScore)
        {
            _tableClient.AddEntity(highScore);
            return req.CreateResponse(HttpStatusCode.OK);
        }

    }
}