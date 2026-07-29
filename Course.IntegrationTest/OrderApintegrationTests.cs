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

        [Fact]
        public async Task PostOrders_WhenCustomerDoesNotExist_ShouldReturnNotFound()
        {
            //Arrange
            var request = new CreateOrderRequest(Guid.NewGuid(), new List<CreateOrderItemRequest>
            {
                new CreateOrderItemRequest(DemoData.MouseId, 2)
            });

            //Act
            var response = await _client.PostAsJsonAsync("/api/orders", request);

            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            var problemDetails = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
            problemDetails.Should().NotBeNull();
            problemDetails!.Title.Should().Be("Recurso no encontrado");
        }

        [Fact]
        public async Task PostOrders_WhenProductDoesNotExist_ShouldReturnNotFound()
        {
            //Arrange
            var request = new CreateOrderRequest(DemoData.CustomerId, new List<CreateOrderItemRequest>
            {
                new CreateOrderItemRequest(Guid.NewGuid(), 2)
            });

            //Act
            var response = await _client.PostAsJsonAsync("/api/orders", request);

            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            var problemDetails = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
            problemDetails.Should().NotBeNull();
            problemDetails!.Title.Should().Be("Recurso no encontrado");
        }

        [Fact]
        public async Task PostOrders_WhenNoItems_ShouldReturnBadRequest()
        {
            //Arrange
            var request = new CreateOrderRequest(DemoData.CustomerId, new List<CreateOrderItemRequest>());

            //Act
            var response = await _client.PostAsJsonAsync("/api/orders", request);

            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

            var problemDetails = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
            problemDetails.Should().NotBeNull();
            problemDetails!.Title.Should().Be("Regla de negocio invalida");
        }

        [Fact]
        public async Task GetOrderById_WhenOrderExists_ShouldReturnOk()
        {
            //Arrange - Create an order first
            var createRequest = new CreateOrderRequest(DemoData.CustomerId, new List<CreateOrderItemRequest>
            {
                new CreateOrderItemRequest(DemoData.KeyboardId, 1)
            });
            var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
            var createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderResponse>();

            //Act
            var response = await _client.GetAsync($"/api/orders/{createdOrder!.Id}");

            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
            order.Should().NotBeNull();
            order!.Id.Should().Be(createdOrder.Id);
            order.Total.Should().Be(createdOrder.Total);
            order.Status.Should().Be(OrderStatus.Paid);
        }

        [Fact]
        public async Task GetOrderById_WhenOrderDoesNotExist_ShouldReturnNotFound()
        {
            //Arrange
            var nonExistentId = Guid.NewGuid();

            //Act
            var response = await _client.GetAsync($"/api/orders/{nonExistentId}");

            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CancelOrder_WhenOrderExists_ShouldReturnOkAndCancelledStatus()
        {
            //Arrange - Create an order first
            var createRequest = new CreateOrderRequest(DemoData.CustomerId, new List<CreateOrderItemRequest>
            {
                new CreateOrderItemRequest(DemoData.LaptopId, 1)
            });
            var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
            var createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderResponse>();

            //Act
            var response = await _client.PostAsync($"/api/orders/{createdOrder!.Id}/cancel", null);

            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var cancelledOrder = await response.Content.ReadFromJsonAsync<OrderResponse>();
            cancelledOrder.Should().NotBeNull();
            cancelledOrder!.Id.Should().Be(createdOrder.Id);
            cancelledOrder.Status.Should().Be(OrderStatus.Cancelled);
        }

        [Fact]
        public async Task CancelOrder_WhenOrderDoesNotExist_ShouldReturnNotFound()
        {
            //Arrange
            var nonExistentId = Guid.NewGuid();

            //Act
            var response = await _client.PostAsync($"/api/orders/{nonExistentId}/cancel", null);

            //Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            var problemDetails = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
            problemDetails.Should().NotBeNull();
            problemDetails!.Title.Should().Be("Recurso no encontrado");
        }
    }
}
