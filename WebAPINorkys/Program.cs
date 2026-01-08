using Microsoft.EntityFrameworkCore;
using NorkysAPI.Data;
using NorkysAPI.Interfaces;
using NorkysAPI.Operaciones;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// DB
//builder.Services.AddDbContext<MyDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<MyDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        
);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DAO services
builder.Services.AddScoped<IItemDAO, ItemDAO>();
builder.Services.AddScoped<ICarritoDAO, CarritoDAO>();
builder.Services.AddScoped<IUsuarioDAO, UsuarioDAO>();
builder.Services.AddScoped<ICategoriaDAO, CategoriaDAO>();
builder.Services.AddScoped<IWishListItemDAO, WishListItemDAO>();
builder.Services.AddScoped<IDireccionesDAO, DireccionesDAO>();
builder.Services.AddScoped<IDashboardDAO, DashboardDAO>();

//Agregar Cors
builder.Services.AddCors(policyBuilder => policyBuilder.AddDefaultPolicy
(policy => policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//use Cors
app.UseCors();
app.UseStaticFiles();
app.UseAuthorization();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);


app.MapControllers();

app.Run();
