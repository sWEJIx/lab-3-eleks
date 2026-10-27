namespace StoreWebApi.BLL.DTOs;

public record ProductDto(int Id, string Name, decimal Price, int StockQuantity);
public record CreateProductDto(string Name, decimal Price, int StockQuantity);
