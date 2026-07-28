using Course.Application.Abstractions;
using Course.Application.Orders;
using Course.Domain.Entities;
using Course.Domain.Enums;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Course.UnitTest.ProductTest
{
    public class OrderServiceTest
    {

        [Fact]
        public async Task CustomerRepositoryMock_WhenCustomerExists_ShouldConfiguredCuster()
        {
            //Arrange
            var customerId = Guid.NewGuid();

            var customers = new Mock<ICustomerRepository>();

            customers.Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Customer(customerId, "Juan Backend", "Juan@Backend.Com"));

            //Act
            var customer = await customers.Object.GetByIdAsync(customerId);

            //Assert
            customer.Should().NotBeNull();
            customer.Id.Should().Be(customerId);
            customer.Email.Should().Be("Juan@Backend.Com");

        }

        [Fact]
        public async Task CreateAsync_WhenCustomerExists_ShouldCreateOrder()
        {
            //Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            var customerRepositoryMock = new Mock<ICustomerRepository>();
            var productRepositoryMock = new Mock<IProductRepository>();
            var orderRepositoryMock = new Mock<IOrderRepository>();
            var paymentGatewayMock = new Mock<IPaymentGateway>();

            customerRepositoryMock.Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Customer(customerId, "Juan Backend", "Juan@Backend.Com"));

            productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Product(productId, "Producto 1", 100m, 10));

            productRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            paymentGatewayMock.Setup(x => x.PayAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid orderId, decimal amount, CancellationToken _) =>
                    new Payment(Guid.NewGuid(), orderId, amount, PaymentStatus.Approved, Guid.NewGuid().ToString()));

            orderRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var orderService = new OrderService(customerRepositoryMock.Object, productRepositoryMock.Object, orderRepositoryMock.Object, paymentGatewayMock.Object);

            var request = new CreateOrderRequest(customerId, new[] { new CreateOrderItemRequest(productId, 1) });

            //Act
            var result = await orderService.CreateAsync(request, CancellationToken.None);

            //Assert
            result.Should().NotBeNull();
            result.CustomerId.Should().Be(customerId);
            orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
            customerRepositoryMock.Verify(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
            productRepositoryMock.Verify(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
            productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
            paymentGatewayMock.Verify(x => x.PayAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenCustomerDoesNotExist_ShouldThrowException()
        {
            //Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            var customerRepositoryMock = new Mock<ICustomerRepository>();
            var productRepositoryMock = new Mock<IProductRepository>();
            var orderRepositoryMock = new Mock<IOrderRepository>();
            var paymentGatewayMock = new Mock<IPaymentGateway>();

            customerRepositoryMock.Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Customer)null);

            var orderService = new OrderService(customerRepositoryMock.Object, productRepositoryMock.Object, orderRepositoryMock.Object, paymentGatewayMock.Object);

            var request = new CreateOrderRequest(customerId, new[] { new CreateOrderItemRequest(productId, 1) });

            //Act
            Func<Task> act = async () => await orderService.CreateAsync(request, CancellationToken.None);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
            orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
