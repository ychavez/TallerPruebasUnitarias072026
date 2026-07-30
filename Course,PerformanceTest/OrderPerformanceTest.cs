using Course.Application.Orders;
using Course.Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using System.Net.Http.Json;

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

        [Fact]
        public void GetProducts_PerformanceTest()
        {
            using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(x =>
                {
                    x.UseEnvironment("Testing");
                    x.ConfigureLogging(l => l.ClearProviders());
                });

            using var client = factory.CreateClient();

            var scenario = Scenario.Create("get_products", async context =>
            {
                var response = await client.GetAsync("api/products");

                return response.IsSuccessStatusCode
                    ? Response.Ok()
                    : Response.Fail(statusCode: response.StatusCode.ToString());
            }).WithLoadSimulations(
              Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10))
            );

            var result = NBomberRunner
                .RegisterScenarios(scenario)
                .WithTestSuite("Products API")
                .WithTestName("Get products performance")
                .WithReportFolder("performance-reports")
                .WithReportFileName("get-products-report")
                .WithReportFormats(
                    ReportFormat.Html,
                    ReportFormat.Csv,
                    ReportFormat.Md,
                    ReportFormat.Txt)
                .Run();

            var htmlReport = result.ReportFiles.FirstOrDefault(x => x.ReportFormat == ReportFormat.Html);

            Console.WriteLine($"HTML report generated at: {htmlReport?.FilePath}");
        }

        [Fact]
        public void GetOrderById_PerformanceTest()
        {
            using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(x =>
                {
                    x.UseEnvironment("Testing");
                    x.ConfigureLogging(l => l.ClearProviders());
                });

            using var client = factory.CreateClient();

            var scenario = Scenario.Create("get_order_by_id", async context =>
            {
                // Primero crear una orden
                var createRequest = new CreateOrderRequest(
                    DemoData.CustomerId,
                    [new CreateOrderItemRequest(DemoData.MouseId, 1)]
                );

                var createResponse = await client.PostAsJsonAsync("api/orders", createRequest);

                if (!createResponse.IsSuccessStatusCode)
                {
                    return Response.Fail(statusCode: createResponse.StatusCode.ToString());
                }

                // Deserializar para obtener el ID
                var orderResponse = await createResponse.Content.ReadFromJsonAsync<OrderResponse>();

                if (orderResponse == null)
                {
                    return Response.Fail(statusCode: "Failed to deserialize order response");
                }

                // Ahora consultar la orden por ID
                var getResponse = await client.GetAsync($"api/orders/{orderResponse.Id}");

                return getResponse.IsSuccessStatusCode
                    ? Response.Ok()
                    : Response.Fail(statusCode: getResponse.StatusCode.ToString());
            }).WithLoadSimulations(
              Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10))
            );

            var result = NBomberRunner
                .RegisterScenarios(scenario)
                .WithTestSuite("Orders API")
                .WithTestName("Get order by ID performance")
                .WithReportFolder("performance-reports")
                .WithReportFileName("get-order-byid-report")
                .WithReportFormats(
                    ReportFormat.Html,
                    ReportFormat.Csv,
                    ReportFormat.Md,
                    ReportFormat.Txt)
                .Run();

            var htmlReport = result.ReportFiles.FirstOrDefault(x => x.ReportFormat == ReportFormat.Html);

            Console.WriteLine($"HTML report generated at: {htmlReport?.FilePath}");
        }

        [Fact]
        public void CancelOrder_PerformanceTest()
        {
            using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(x =>
                {
                    x.UseEnvironment("Testing");
                    x.ConfigureLogging(l => l.ClearProviders());
                });

            using var client = factory.CreateClient();

            var scenario = Scenario.Create("cancel_order", async context =>
            {
                // Primero crear una orden
                var createRequest = new CreateOrderRequest(
                    DemoData.CustomerId,
                    [new CreateOrderItemRequest(DemoData.MouseId, 1)]
                );

                var createResponse = await client.PostAsJsonAsync("api/orders", createRequest);

                if (!createResponse.IsSuccessStatusCode)
                {
                    return Response.Fail(statusCode: createResponse.StatusCode.ToString());
                }

                // Deserializar para obtener el ID
                var orderResponse = await createResponse.Content.ReadFromJsonAsync<OrderResponse>();

                if (orderResponse == null)
                {
                    return Response.Fail(statusCode: "Failed to deserialize order response");
                }

                // Ahora cancelar la orden
                var cancelResponse = await client.PostAsync($"api/orders/{orderResponse.Id}/cancel", null);

                return cancelResponse.IsSuccessStatusCode
                    ? Response.Ok()
                    : Response.Fail(statusCode: cancelResponse.StatusCode.ToString());
            }).WithLoadSimulations(
              Simulation.Inject(rate: 3, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10))
            );

            var result = NBomberRunner
                .RegisterScenarios(scenario)
                .WithTestSuite("Orders API")
                .WithTestName("Cancel order performance")
                .WithReportFolder("performance-reports")
                .WithReportFileName("cancel-order-report")
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
