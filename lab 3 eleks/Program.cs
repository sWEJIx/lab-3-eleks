using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Підключення БД та офіційного Microsoft Identity API
builder.Services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("AppDb"));
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<AppDbContext>();

// 2. Реєстрація шару Бізнес-логіки (BLL)
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddControllers();

// 3. Підключення Swagger для візуального тестування
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. Активація Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

// 5. Маршрути Identity (/register, /login) та контролерів
app.MapIdentityApi<IdentityUser>();
app.MapControllers();

app.MapGet("/", () => "Сервер працює! Допиши /swagger в адресному рядку браузера, щоб відкрити панель тестування.");

app.Run();

// =====================================================================
// 1. ШАР ДАНИХ (DAL - Моделі та Контекст БД)
// =====================================================================
public class Order
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string UserId { get; set; } = string.Empty;
}

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Order> Orders => Set<Order>();
}

// =====================================================================
// 2. ШАР БІЗНЕС-ЛОГІКИ (BLL - DTO, Інтерфейс та Сервіс)
// =====================================================================
public record CreateOrderDto(string ProductName, int Quantity, decimal UnitPrice);
public record OrderDto(int Id, string ProductName, int Quantity, decimal TotalPrice);

public interface IOrderService
{
    Task<List<OrderDto>> GetUserOrdersAsync(string userId);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, string userId);
}

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    public OrderService(AppDbContext db) => _db = db;

    public async Task<List<OrderDto>> GetUserOrdersAsync(string userId)
    {
        return await _db.Orders
            .Where(o => o.UserId == userId)
            .Select(o => new OrderDto(o.Id, o.ProductName, o.Quantity, o.TotalPrice))
            .ToListAsync();
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, string userId)
    {
        // БІЗНЕС-ЛОГІКА: валідація та автоматична знижка 10%, якщо замовлено від 5 шт.
        if (dto.Quantity <= 0 || dto.UnitPrice <= 0)
            throw new ArgumentException("Кількість та ціна мають бути більшими за 0.");

        decimal total = dto.Quantity * dto.UnitPrice;
        if (dto.Quantity >= 5) total *= 0.9m;

        var order = new Order
        {
            ProductName = dto.ProductName,
            Quantity = dto.Quantity,
            TotalPrice = total,
            UserId = userId
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return new OrderDto(order.Id, order.ProductName, order.Quantity, order.TotalPrice);
    }
}

// =====================================================================
// 3. КОНТРОЛЕР (Очищений від бізнес-логіки, із підключеним [Authorize])
// =====================================================================
[ApiController]
[Route("api/[controller]")]
[Authorize] // Захист ендпоінтів через Microsoft Identity
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    public OrdersController(IOrderService orderService) => _orderService = orderService;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _orderService.GetUserOrdersAsync(UserId));

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderDto dto)
    {
        try
        {
            return Ok(await _orderService.CreateOrderAsync(dto, UserId));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}