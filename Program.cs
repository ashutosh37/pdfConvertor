using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using System.Text; // Add this using statement

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Register the CodePagesEncodingProvider to support additional encodings like 1252
// This is often needed for libraries handling older file formats (e.g., .msg)
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
