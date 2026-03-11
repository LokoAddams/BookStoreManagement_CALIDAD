var builder = WebApplication.CreateBuilder(args);

// Conexi�n (lee de appsettings.json -> ConnectionStrings:Default)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection no configurada.");

// Registro de dependencias
builder.Services.AddSingleton<MicroServiceSales.Domain.Interfaces.IDataBase>(sp => MicroServiceSales.Infrastructure.DataBase.DataBaseConnection.GetInstance(connectionString));
builder.Services.AddScoped<MicroServiceSales.Domain.Interfaces.ISalesRepository, MicroServiceSales.Infrastructure.Repositories.SalesRepository>();
builder.Services.AddScoped<MicroServiceSales.Domain.Interfaces.ISalesService, MicroServiceSales.Application.Services.SalesService>();
// Mensajería RabbitMQ: publisher y consumer
builder.Services.AddSingleton<MicroServiceSales.Domain.Interfaces.IEventPublisher>(sp => new MicroServiceSales.Infrastructure.Messaging.RabbitPublisher(builder.Configuration));
builder.Services.AddHostedService<MicroServiceSales.Infrastructure.Messaging.RabbitConsumerForSales>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar CORS para permitir peticiones desde el frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Usar CORS
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
