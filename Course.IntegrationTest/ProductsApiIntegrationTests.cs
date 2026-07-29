using Course.Application.Orders;
using Course.Infrastructure.Repositories;
using FluentAssertions;
using System.Net.Http.Json;

namespace Course.IntegrationTest
{
    public class ProductsApiIntegrationTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ProductsApiIntegrationTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetProducts_ShouldReturnOkWithSeededProducts()
        {
            //Act
            var response = await _client.GetAsync("/api/products");

            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var products = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<ProductResponse>>();

            products.Should().NotBeNull();
            products.Should().HaveCount(3);

            products.Should().Contain(p => p.Id == DemoData.KeyboardId && p.Name == "Mechanical Keyboard" && p.Price == 85m);
            products.Should().Contain(p => p.Id == DemoData.MouseId && p.Name == "Ergonomic Mouse" && p.Price == 45m);
            products.Should().Contain(p => p.Id == DemoData.LaptopId && p.Name == "Developer Laptop" && p.Price == 1200m);
        }
    }
}
