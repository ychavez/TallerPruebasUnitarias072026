using Course.Application.Orders;
using Course.Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace Course.PerformanceTest
{
    public class OrderPerformanceTest
    {

        [Fact]
        public void CreateOrder_PerformanceTest()
        {
            using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(x =>
                {

                    x.UseEnvironment("Testing");
                    x.ConfigureLogging(l => l.ClearProviders());
                });

            using var client = factory.CreateClient();

            var scenario = Scenario.Create("create_order", async context =>
            {

                var request = new CreateOrderRequest(
                    DemoData.CustomerId,
                    [new CreateOrderItemRequest(DemoData.MouseId, 1)]
                    );

                var response = await client.PostAsJsonAsync("api/orders", request);

                return response.IsSuccessStatusCode
                    ? Response.Ok()
                    : Response.Fail(statusCode: response.StatusCode.ToString());
            }).WithLoadSimulations(
              Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10))
            );

          var result =  NBomberRunner
                .RegisterScenarios(scenario)
                .WithTestSuite("Orders API")
    .WithTestName("Create order performance")
    .WithReportFolder("performance-reports")
    .WithReportFileName("create-order-report")
    .WithReportFormats(
        ReportFormat.Html,
        ReportFormat.Csv,
        ReportFormat.Md,
        ReportFormat.Txt)
                .Run();

            var htmlReport = result.ReportFiles.FirstOrDefault(x => x.ReportFormat == ReportFormat.Html);

            Console.WriteLine($"HTML report generated at: {htmlReport?.FilePath}");

        }
    }
}
