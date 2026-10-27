namespace StoreWebApi.BLL.DTOs;

public record OrderDto(int Id, int ProductId, string ProductName, int Quantity, decimal TotalPrice, DateTime CreatedAt);
public record CreateOrderDto(int ProductId, int Quantity);
