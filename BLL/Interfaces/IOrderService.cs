using StoreWebApi.BLL.DTOs;

namespace StoreWebApi.BLL.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, string userId);
}
