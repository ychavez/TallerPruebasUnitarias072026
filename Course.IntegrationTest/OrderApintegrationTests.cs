using Course.Application.Orders;
using Course.Domain.Enums;
using Course.Infrastructure.Repositories;
using FluentAssertions;
using System.Net.Http.Json;

namespace Course.IntegrationTest
{
    public class OrderApintegrationTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public OrderApintegrationTests(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }



        [Fact]
        public async Task PostOrders_WhenRequestIsValid_ShouldReturnCreated()
        {
            //Aarrange
            var request = new CreateOrderRequest(DemoData.CustomerId, new List<CreateOrderItemRequest>
            {
                new CreateOrderItemRequest(DemoData.MouseId, 2)
               
            });

            //Act
            var response = await _client.PostAsJsonAsync("/api/orders", request);


            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

            var order = await response.Content.ReadFromJsonAsync<OrderResponse>();

            order.Should().NotBeNull();
            order.Status.Should().Be(OrderStatus.Paid);
            order.Total.Should().Be(90m);
            order.Items.Should().ContainSingle();

        }
    }
}
