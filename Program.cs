using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StoreWebApi.BLL.Interfaces;
using StoreWebApi.BLL.Services;
using StoreWebApi.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Підключення EF Core InMemory Database (згідно з документацією Microsoft Identity API)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("AppDb"));

// 2. Підключення Авторизації та Identity API Endpoints
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 3. Реєстрація сервісів шару Бізнес-логіки (BLL)
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Мапінг стандартних ендпоінтів Microsoft Identity (/register, /login, /refresh тощо)
app.MapIdentityApi<IdentityUser>();

app.MapControllers();

app.Run();
