using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using System.Linq;
using ServiceUsers.Domain.Interfaces;
using ServiceUsers.Application.Facade;
using ServiceProducts.Domain.Interfaces;
using ServiceProducts.Domain.Interfaces.Reports;
using ServiceClients.Domain.Interfaces;
using ServiceDistributors.Domain.Interfaces;
using ServiceSales.Domain.Interfaces;
using MicroServiceWeb.External.Http;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthHeaderHandler>();

// Cultura es-BO para moneda Bs.
var cultureInfo = new CultureInfo("es-BO");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
builder.Services.Configure<RequestLocalizationOptions>(options => { options.DefaultRequestCulture = new RequestCulture(cultureInfo); });

// === Registro de HttpClients para microservicios ===
builder.Services.AddHttpClient("ProductsService", c => c.BaseAddress = new Uri(builder.Configuration["Services:Products"] ?? "https://localhost:57307/"))
                .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient("SalesService", c => c.BaseAddress = new Uri(builder.Configuration["Services:Sales"] ?? "https://placeholder-sales"))
                .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient("UsersService", c => c.BaseAddress = new Uri(builder.Configuration["Services:Users"] ?? "https://placeholder-users"))
                .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient("ClientsService", c => c.BaseAddress = new Uri(builder.Configuration["Services:Clients"] ?? "https://placeholder-clients"))
                .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient("DistributorsService", c => c.BaseAddress = new Uri(builder.Configuration["Services:Distributors"] ?? "https://localhost:62293/"))
                .AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddTransient<IProductsApiClient, ProductsApiClient>();
builder.Services.AddTransient<ISalesApiClient, SalesApiClient>();
builder.Services.AddTransient<IUsersApiClient, UsersApiClient>();
builder.Services.AddTransient<IClientsApiClient, ClientsApiClient>();
builder.Services.AddTransient<IDistributorsApiClient, DistributorsApiClient>();

// Autenticaci�n Cookie y pol�ticas de autorizaci�n
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("EmployeeOnly", p => p.RequireRole("Employee"));
    options.AddPolicy("AdminOrEmployee", p => p.RequireRole("Admin", "Employee"));
    options.AddPolicy("RequireEmployeeOrAdmin", p => p.RequireRole("Admin", "Employee"));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToPage("/Auth/Login");
    options.Conventions.AllowAnonymousToPage("/Auth/Logout");
    options.Conventions.AllowAnonymousToPage("/Users/ChangePassword");
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AuthorizePage("/Products/Index", "AdminOrEmployee");
    options.Conventions.AuthorizePage("/Products/Create", "AdminOnly");
    options.Conventions.AuthorizePage("/Products/Edit", "AdminOnly");
    options.Conventions.AuthorizePage("/Products/Delete", "AdminOnly");
    options.Conventions.AuthorizePage("/Distributors/Index", "AdminOrEmployee");
    options.Conventions.AuthorizePage("/Distributors/Create", "AdminOnly");
    options.Conventions.AuthorizePage("/Distributors/Edit", "AdminOnly");
    options.Conventions.AuthorizePage("/Distributors/Delete", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Clients", "AdminOrEmployee");
    options.Conventions.AuthorizeFolder("/Users", "AdminOnly");
    options.Conventions.AuthorizePage("/Users/ChangePassword", "AdminOrEmployee");
    options.Conventions.AuthorizePage("/Index", "AdminOrEmployee");
    options.Conventions.AuthorizePage("/Error", "AdminOrEmployee");
    options.Conventions.AuthorizeFolder("/Sales", "AdminOrEmployee");
    options.Conventions.AuthorizePage("/Sales/Create", "AdminOrEmployee");
})
.AddMvcOptions(options =>
{
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "Debe seleccionar una categoría.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((x, y) => "El valor ingresado no es válido.");
    options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(x => $"Falta el valor para {x}.");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseRequestLocalization();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.Use(async (context, next) =>
{
    // Forzar cambio de contrase�a: si hay flag en TempData se maneja en p�gina; si ya autenticado con claim MustChange (no usamos claim), bloqueamos rutas protegidas
    if (context.User?.Identity?.IsAuthenticated == true)
    {
        // Nada adicional, el flujo de primera vez se maneja antes de crear cookie.
    }
    await next();
});
app.UseAuthorization();

// === Endpoint para obtener PDF de comprobante de venta ===
app.MapGet("/api/reports/sale/{saleId}/pdf", async (Guid saleId, IHttpClientFactory httpClientFactory) =>
{
    try
    {
        using var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(builder.Configuration["Services:Reports"] ?? "http://localhost:7200/");
        
        var response = await client.GetAsync($"api/reports/sale/{saleId}/pdf");
        
        if (!response.IsSuccessStatusCode)
            return Results.NotFound($"No se encontró el comprobante para la venta {saleId}");
        
        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        return Results.File(pdfBytes, "application/pdf", $"Comprobante-{saleId}.pdf");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error al obtener el PDF: {ex.Message}");
    }
}).RequireAuthorization();

app.MapRazorPages();
app.Run();
