using Microsoft.EntityFrameworkCore;
using StoreWebApi.BLL.DTOs;
using StoreWebApi.BLL.Interfaces;
using StoreWebApi.Data;
using StoreWebApi.Data.Entities;

namespace StoreWebApi.BLL.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .Select(o => new OrderDto(o.Id, o.ProductId, o.ProductName, o.Quantity, o.TotalPrice, o.CreatedAt))
            .ToListAsync();
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, string userId)
    {
        if (dto.Quantity <= 0)
            throw new ArgumentException("Кількість замовлення має бути більшою за нуль.");

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == dto.ProductId)
            ?? throw new KeyNotFoundException("Товар із вказаним ID не знайдено.");

        if (product.StockQuantity < dto.Quantity)
            throw new InvalidOperationException($"Недостатньо товару на складі. Доступно: {product.StockQuantity}");

        // Бізнес-правило: списання залишку зі складу
        product.StockQuantity -= dto.Quantity;

        // Бізнес-правило: розрахунок вартості + знижка 10% при замовленні від 5 одиниць
        decimal totalPrice = product.Price * dto.Quantity;
        if (dto.Quantity >= 5)
        {
            totalPrice *= 0.9m;
        }

        var order = new Order
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Quantity = dto.Quantity,
            TotalPrice = totalPrice,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return new OrderDto(order.Id, order.ProductId, order.ProductName, order.Quantity, order.TotalPrice, order.CreatedAt);
    }
}
