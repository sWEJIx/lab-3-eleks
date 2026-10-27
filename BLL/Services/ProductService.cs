using Microsoft.EntityFrameworkCore;
using StoreWebApi.BLL.DTOs;
using StoreWebApi.BLL.Interfaces;
using StoreWebApi.Data;
using StoreWebApi.Data.Entities;

namespace StoreWebApi.BLL.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.StockQuantity))
            .ToListAsync();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var p = await _context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return p is null ? null : new ProductDto(p.Id, p.Name, p.Price, p.StockQuantity);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Назва товару не може бути порожньою.");

        if (dto.Price <= 0)
            throw new ArgumentException("Ціна товару має бути більшою за 0.");

        if (dto.StockQuantity < 0)
            throw new ArgumentException("Кількість на складі не може бути від'ємною.");

        var entity = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity
        };

        _context.Products.Add(entity);
        await _context.SaveChangesAsync();

        return new ProductDto(entity.Id, entity.Name, entity.Price, entity.StockQuantity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Products.FindAsync(id);
        if (entity is null) return false;

        _context.Products.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
