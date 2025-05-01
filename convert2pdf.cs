
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using HttpMultipartParser;

namespace risys.legal365
{
    public class convert2pdf
    {
        private readonly ILogger _logger;
        private readonly FileConverterService _converter;

        public convert2pdf(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<convert2pdf>();
            _converter = new FileConverterService();
        }

        [Function("ConvertToPdf")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "convert")] HttpRequestData req)
        {
            var parser = await MultipartFormDataParser.ParseAsync(req.Body);
            var file = parser.Files.FirstOrDefault();

            if (file == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("No file uploaded.");
                return badResponse;
            }

            var ext = Path.GetExtension(file.FileName).ToLower();
            using var outputPdf = await _converter.ConvertToPdfAsync(file.Data, ext);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/pdf");
            response.Headers.Add("Content-Disposition", "attachment; filename=converted.pdf");
            await response.Body.WriteAsync(outputPdf.ToArray());

            return response;
        }
    }
}
