using Microsoft.EntityFrameworkCore;
using MicroServiceReports.Infraestructure.Persistence;
using MicroServiceReports.Infraestructure.Rabbit;
using MicroServiceReports.Domain.Ports;
using MicroServiceReports.Application.UseCases;
using MicroServiceReports.Application.Services;
using MicroServiceReports.Application.Builders;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;

// Add services to the container.
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

// Register RabbitMQ settings
builder.Services.Configure<RabbitSettings>(configuration.GetSection("RabbitMq"));

// Register DbContext - PostgreSQL
var connectionString = configuration.GetConnectionString("MicroServiceReports");
builder.Services.AddDbContext<MicroServiceReportsDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register repositories
builder.Services.AddScoped<ISaleEventRepository, SaleEventRepositoryEf>();
builder.Services.AddScoped<ISaleDetailRepository, SaleDetailRepositoryEf>();

// Register Builder Pattern components for PDF generation
builder.Services.AddScoped<ISalePdfBuilder, QuestPdfSaleBuilder>();
builder.Services.AddScoped<SalePdfDirector>();
builder.Services.AddScoped<SalePdfGenerator>();

// Register application handlers
builder.Services.AddScoped<GetSaleBySaleIdHandler>();
builder.Services.AddScoped<GenerateSalePdfHandler>();

// Register background service to consume RabbitMQ
builder.Services.AddHostedService<SalesConfirmedBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Usar CORS
app.UseCors("AllowAll");

app.UseHttpsRedirection();

// Endpoint para obtener todos los reportes
app.MapGet("/api/reports", async (ISaleEventRepository repo) =>
{
    var records = await repo.GetAllAsync();
    return Results.Ok(records.Select(r => new
    {
        id = r.Id,
        saleId = r.SaleId,
        payload = System.Text.Json.JsonDocument.Parse(r.Payload),
        exchange = r.Exchange,
        routingKey = r.RoutingKey,
        receivedAt = r.ReceivedAt
    }));
})
.WithName("GetAllReports")
.WithOpenApi();

// Endpoint para obtener reporte por SaleId
app.MapGet("/api/reports/sale/{saleId}", async (string saleId, GetSaleBySaleIdHandler handler) =>
{
    var record = await handler.HandleAsync(saleId);
    if (record == null)
    {
        return Results.NotFound(new { message = $"No se encontró reporte para la venta {saleId}" });
    }

    return Results.Ok(new
    {
        id = record.Id,
        saleId = record.SaleId,
        payload = System.Text.Json.JsonDocument.Parse(record.Payload),
        exchange = record.Exchange,
        routingKey = record.RoutingKey,
        receivedAt = record.ReceivedAt
    });
})
.WithName("GetSaleBySaleId")
.WithOpenApi();

// Endpoint para generar y visualizar PDF del comprobante de venta
app.MapGet("/api/reports/sale/{saleId}/pdf", async (string saleId, GenerateSalePdfHandler handler, HttpContext context) =>
{
    var pdfBytes = await handler.HandleAsync(saleId);
    
    if (pdfBytes == null)
    {
        return Results.NotFound(new { message = $"No se encontró reporte para la venta {saleId}" });
    }

    // inline = abre en el navegador, attachment = descarga
    context.Response.Headers["Content-Disposition"] = $"inline; filename=comprobante_venta_{saleId}.pdf";
    return Results.File(pdfBytes, "application/pdf");
})
.WithName("GetSalePdf")
.WithOpenApi()
.Produces<byte[]>(StatusCodes.Status200OK, "application/pdf")
.Produces(StatusCodes.Status404NotFound);

// Endpoint de salud
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "MicroServiceReports" }))
.WithName("HealthCheck")
.WithOpenApi();

app.Run();
