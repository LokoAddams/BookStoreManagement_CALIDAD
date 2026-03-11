using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Text.Json.Serialization;

namespace MicroServiceWeb.External.Http;

public interface IProductsApiClient
{
    // Productos
    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken ct);
    Task<PagedResult<ProductDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct); // nuevo m�todo paginado
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<ProductApiResult> CreateAsync(ProductCreateDto dto, CancellationToken ct);
    Task<ProductApiResult> UpdateAsync(Guid id, ProductUpdateDto dto, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    // categorías (del mismo microservicio)
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct);
}
public interface ISalesApiClient
{
    Task<SaleApiResult> CreateAsync(SaleCreateDto dto, CancellationToken ct);
    Task<SaleDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<SaleStatusResult> GetStatusAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<SaleDto>> GetAllAsync(CancellationToken ct);
    Task<PagedResult<SaleDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct);
}
public interface IUsersApiClient
{
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<UserFullDto>> GetAllRawAsync(CancellationToken ct);
    Task<PagedResult<UserFullDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct);
    Task<IReadOnlyList<string>> GetRolesAsync(Guid id, CancellationToken ct);
    Task<AuthLoginResult> LoginAsync(AuthLoginRequest request, CancellationToken ct);
    Task<UserFullDto?> SearchAsync(string userOrEmail, CancellationToken ct);
    Task<UserApiResult> CreateAsync(UserCreateRequest dto, CancellationToken ct); // mantiene /api/User
    Task<UserApiResult> RegisterAsync(UserCreateRequest dto, CancellationToken ct); // nuevo /api/Auth/register
    Task<UserApiResult> UpdateAsync(Guid id, UserUpdateRequest dto, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    Task<ApiSimpleResult> ChangePasswordAsync(ChangePasswordRequest dto, CancellationToken ct);
    Task<ApiSimpleResult> ChangePasswordAsync(ChangePasswordRequest dto, string? bearerToken, CancellationToken ct);
}
public interface IClientsApiClient
{
    Task<ClientDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<ClientDto?> GetByCiAsync(string ci, CancellationToken ct);
    Task<IReadOnlyList<ClientDto>> GetAllAsync(CancellationToken ct);
    Task<PagedResult<ClientDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct);
    Task<ClientApiResult> CreateAsync(ClientCreateDto dto, CancellationToken ct);
    Task<ClientApiResult> UpdateAsync(Guid id, ClientUpdateDto dto, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
public interface IDistributorsApiClient
{
    Task<DistributorDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<DistributorDto>> GetAllAsync(CancellationToken ct);
    Task<PagedResult<DistributorDto>> GetPagedAsync(int? page_parameter, int? pageSize_parameter, CancellationToken ct);
    Task<DistributorApiResult> CreateAsync(DistributorCreateDto dto, CancellationToken ct);
    Task<DistributorApiResult> UpdateAsync(Guid id, DistributorUpdateDto dto, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}

// Resultado paginado gen�rico
public record PagedResult<T>(List<T> Items, int Page, int PageSize, int TotalItems, int TotalPages);

// ===== DTOs =====
public record UserDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("email")] string? Email);

public record UserFullDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("firstName")] string? FirstName,
    [property: JsonPropertyName("middleName")] string? MiddleName,
    [property: JsonPropertyName("lastName")] string? LastName,
    [property: JsonPropertyName("mustChangePassword")] bool MustChangePassword,
    [property: JsonPropertyName("roles")] List<string> Roles,
    [property: JsonPropertyName("passwordHash")] string PasswordHash);

public record ClientDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("firstName")] string FirstName,
    [property: JsonPropertyName("lastName")] string LastName,
    [property: JsonPropertyName("ci")] string Ci,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("phone")] string? Phone,
    [property: JsonPropertyName("address")] string? Address);

public record DistributorDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("contactEmail")] string? ContactEmail,
    [property: JsonPropertyName("phone")] string? Phone,
    [property: JsonPropertyName("address")] string? Address);

public record CategoryDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name);

public record ProductDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("categoryId")] Guid CategoryId,
    [property: JsonPropertyName("categoryName")] string? CategoryName,
    [property: JsonPropertyName("price")] decimal Price,
    [property: JsonPropertyName("stock")] int Stock);

public class ProductCreateDto
{
    [Required(ErrorMessage = "El nombre es obligatorio."), MinLength(3, ErrorMessage = "Mínimo 3 caracteres."), MaxLength(100, ErrorMessage = "Máximo 100 caracteres."), Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria."), MaxLength(500, ErrorMessage = "Máximo 500 caracteres."), Display(Name = "Descripción")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria."), Display(Name = "Categoría")]
    public Guid? CategoryId { get; set; }

    // Campo opcional para ayudar a backends que mantienen columna denormalizada category_name
    public string? CategoryName { get; set; }

    [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor a 0."), Display(Name = "Precio")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo."), Display(Name = "Stock")]
    public int Stock { get; set; }
}
public class ProductUpdateDto : ProductCreateDto { }
public class ProductApiResult { public bool Success { get; set; } public ProductDto? Product { get; set; } public Dictionary<string, List<string>> Errors { get; set; } = new(); }

public class AuthLoginRequest { [Required] public string UserOrEmail { get; set; } = string.Empty; [Required] public string Password { get; set; } = string.Empty; }
public class AuthLoginResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? Token { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public bool MustChangePassword { get; set; }
    public List<string> Roles { get; set; } = new();
}
public class ChangePasswordRequest
{
    [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
    public string CurrentPassword { get; set; } = string.Empty;
    [Required(ErrorMessage = "La nueva contraseña es obligatoria."), MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
    [Required, Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
public class ApiSimpleResult { public bool Success { get; set; } public string? Error { get; set; } }
public class UserCreateRequest { [Required(ErrorMessage="El correo es obligatorio."), EmailAddress(ErrorMessage="Correo inválido.")] public string Email { get; set; } = string.Empty; [Required(ErrorMessage="El rol es obligatorio.")] public string Role { get; set; } = string.Empty; }
public class UserUpdateRequest { [Required, EmailAddress] public string Email { get; set; } = string.Empty; public List<string> Roles { get; set; } = new(); }
public class UserApiResult { public bool Success { get; set; } public UserFullDto? User { get; set; } public Dictionary<string, List<string>> Errors { get; set; } = new(); }

public class ClientCreateDto
{
    [Required(ErrorMessage = "El nombre es obligatorio."), MinLength(3, ErrorMessage = "Mínimo 3 caracteres."), MaxLength(100, ErrorMessage = "Máximo 100 caracteres."), RegularExpression(@"^[A-Za-z��������������' -]+$", ErrorMessage = "Solo letras y espacios."), Display(Name = "Nombre")] public string FirstName { get; set; } = string.Empty;
    [Required(ErrorMessage = "El apellido es obligatorio."), MinLength(3, ErrorMessage = "Mínimo 3 caracteres."), MaxLength(100, ErrorMessage = "Máximo 100 caracteres."), RegularExpression(@"^[A-Za-z��������������' -]+$", ErrorMessage = "Solo letras y espacios."), Display(Name = "Apellido")] public string LastName { get; set; } = string.Empty;
    [Required(ErrorMessage = "El CI es obligatorio."), StringLength(20), RegularExpression(@"^\d{5,10}(-(LP|CB|SC|CH|OR|PT|TJ|BN|PD))?$", ErrorMessage = "CI inválido. Ej: 1234567-CB"), Display(Name = "CI")] public string Ci { get; set; } = string.Empty;
    [Required(ErrorMessage = "El correo electr�nico es obligatorio."), EmailAddress(ErrorMessage = "Correo inválido."), MaxLength(150, ErrorMessage = "Máximo 150 caracteres."), Display(Name = "Correo electr�nico")] public string? Email { get; set; }
    [Required(ErrorMessage = "El Telófono es obligatorio."), RegularExpression(@"^\d{8}$", ErrorMessage = "Debe tener 8 d�gitos."), Display(Name = "Telófono")] public string? Phone { get; set; }
    [Required(ErrorMessage = "La Dirección es obligatoria."), MinLength(5, ErrorMessage = "Mínimo 5 caracteres."), MaxLength(255, ErrorMessage = "Máximo 255 caracteres."), Display(Name = "Dirección")] public string? Address { get; set; }
}
public class ClientUpdateDto : ClientCreateDto { }
public class ClientApiResult { public bool Success { get; set; } public ClientDto? Client { get; set; } public Dictionary<string, List<string>> Errors { get; set; } = new(); }

public class DistributorCreateDto
{
    [Required(ErrorMessage = "El nombre es obligatorio."), MinLength(3, ErrorMessage = "Mínimo 3 caracteres."), MaxLength(120, ErrorMessage = "Máximo 120 caracteres."), RegularExpression(@"^[A-Za-z��������������' -]+$", ErrorMessage = "Solo letras y espacios."), Display(Name = "Nombre")] public string Name { get; set; } = string.Empty;
    [Required(ErrorMessage = "El correo de contacto es obligatorio."), EmailAddress(ErrorMessage = "Correo inválido."), Display(Name = "Correo de contacto")] public string? ContactEmail { get; set; }
    [Required(ErrorMessage = "El Telófono es obligatorio."), RegularExpression(@"^\d{8}$", ErrorMessage = "Debe tener 8 d�gitos."), Display(Name = "Telófono")] public string? Phone { get; set; }
    [Required(ErrorMessage = "La Dirección es obligatoria."), MinLength(5, ErrorMessage = "Mínimo 5 caracteres."), MaxLength(255, ErrorMessage = "Máximo 255 caracteres."), Display(Name = "Dirección")] public string? Address { get; set; }
}
public class DistributorUpdateDto : DistributorCreateDto { }
public class DistributorApiResult { public bool Success { get; set; } public DistributorDto? Distributor { get; set; } public Dictionary<string, List<string>> Errors { get; set; } = new(); }

// ===== Sales DTOs =====
public record SaleDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("clientId")] Guid ClientId,
    [property: JsonPropertyName("clientName")] string? ClientName,
    [property: JsonPropertyName("clientCi")] string? ClientCi,
    [property: JsonPropertyName("userId")] Guid UserId,
    [property: JsonPropertyName("userName")] string? UserName,
    [property: JsonPropertyName("saleDate")] DateTimeOffset SaleDate,
    [property: JsonPropertyName("subtotal")] decimal Subtotal,
    [property: JsonPropertyName("total")] decimal Total,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("details")] List<SaleDetailDto>? Details);

public record SaleDetailDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("saleId")] Guid SaleId,
    [property: JsonPropertyName("productId")] Guid ProductId,
    [property: JsonPropertyName("productName")] string? ProductName,
    [property: JsonPropertyName("quantity")] int Quantity,
    [property: JsonPropertyName("unitPrice")] decimal UnitPrice,
    [property: JsonPropertyName("subtotal")] decimal Subtotal);

public class SaleCreateDto
{
    public Guid ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ClientCi { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public List<SaleDetailCreateDto> Details { get; set; } = new();
}

public class SaleDetailCreateDto
{
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class SaleApiResult
{
    public bool Success { get; set; }
    public SaleDto? Sale { get; set; }
    public Dictionary<string, List<string>> Errors { get; set; } = new();
    public string? Message { get; set; }
}

// Resultado del estado de la venta
public class SaleStatusResult
{
    public string Status { get; set; } = "PENDING";
    public string? Message { get; set; }
}
