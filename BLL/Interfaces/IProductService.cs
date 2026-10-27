using StoreWebApi.BLL.DTOs;

namespace StoreWebApi.BLL.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<bool> DeleteAsync(int id);
}
