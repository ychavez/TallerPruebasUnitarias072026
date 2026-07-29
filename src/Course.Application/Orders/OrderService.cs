using Course.Application.Abstractions;
using Course.Domain.Entities;
using Course.Domain.Exceptions;

namespace Course.Application.Orders;

public sealed class OrderService : IOrderService
{
    private readonly ICustomerRepository _customers;
    private readonly IOrderRepository _orders;
    private readonly IPaymentGateway _payments;
    private readonly IProductRepository _products;

    public OrderService(
        ICustomerRepository customers,
        IProductRepository products,
        IOrderRepository orders,
        IPaymentGateway payments)
    {
        _customers = customers;
        _products = products;
        _orders = orders;
        _payments = payments;
    }

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
        {
            throw new DomainException("El pedido requiere al menos un item.");
        }

        var customer = await _customers.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null)
        {
            throw new KeyNotFoundException("Cliente no encontrado.");
        }
                                     ///xx1
        var order = new Order(Guid.NewGuid(), customer.Id, DateTimeOffset.UtcNow);

        foreach (var item in request.Items)
        {
            var product = await _products.GetByIdAsync(item.ProductId, cancellationToken);
            if (product is null)
            {
                throw new KeyNotFoundException($"Producto no encontrado: {item.ProductId}.");
            }

          order.AddItem(product, item.Quantity);
            await _products.UpdateAsync(product, cancellationToken);
        }

        var payment = await _payments.PayAsync(order.Id, order.CalculateTotal(), cancellationToken);
        order.RegisterPayment(payment);


      //  await _orders.AddAsync(order, cancellationToken);

     

        return OrderResponse.FromOrder(order);
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orders.GetByIdAsync(id, cancellationToken);
        return order is null ? null : OrderResponse.FromOrder(order);
    }

    public async Task<OrderResponse> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orders.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            throw new KeyNotFoundException("Pedido no encontrado.");
        }

        order.Cancel();

        foreach (var item in order.Items)
        {
            var product = await _products.GetByIdAsync(item.ProductId, cancellationToken);
            product?.IncreaseStock(item.Quantity);
        }

        await _orders.UpdateAsync(order, cancellationToken);

        return OrderResponse.FromOrder(order);
    }
}
