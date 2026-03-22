using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static MicroServiceWeb.External.Http.ProductsApiClient;

namespace MicroServiceWeb.External.Http
{



    public static class JsonHelper
    {
        public static string GetString(JsonElement el, params string[] names)
        {
            foreach (var name in names)
            {
                if (el.TryGetProperty(name, out var prop))
                    return prop.GetString() ?? string.Empty;
            }
            return string.Empty;
        }

        public static int GetInt(JsonElement el, params string[] names)
        {
            foreach (var name in names)
            {
                if (el.TryGetProperty(name, out var prop) && prop.TryGetInt32(out var val))
                    return val;
            }
            return 0;
        }

        public static decimal GetDecimal(JsonElement el, params string[] names)
        {
            foreach (var name in names)
            {
                if (el.TryGetProperty(name, out var prop) && prop.TryGetDecimal(out var val))
                    return val;
            }
            return 0m;
        }

        public static Guid GetGuid(JsonElement el, params string[] names)
        {
            foreach (var name in names)
            {
                if (el.TryGetProperty(name, out var prop) && Guid.TryParse(prop.GetString(), out var res))
                    return res;
            }
            return Guid.Empty;
        }

        public static string? GetCategoryName(JsonElement el)
        {
            if (el.TryGetProperty("categoryName", out var cnP)) return cnP.GetString();
            if (el.TryGetProperty("category_name", out var cnSnake)) return cnSnake.GetString();
            return null;
        }
    }
    public class ProductsApiClient : IProductsApiClient
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions CamelCaseOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        public ProductsApiClient(IHttpClientFactory f)=>_http=f.CreateClient("ProductsService");

        private static string? GetCategoryName(JsonElement el)
        {
            if (el.TryGetProperty("categoryName", out var cnP))
            {
                return cnP.GetString();
            }

            if (el.TryGetProperty("category_name", out var cnSnake))
            {
                return cnSnake.GetString();
            }

            return null;
        }
        

        public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken ct)
        {
            var resp = await _http.GetAsync("api/products", ct);
            if (!resp.IsSuccessStatusCode) return Array.Empty<ProductDto>();

            var json = await resp.Content.ReadAsStringAsync(ct);

            try
            {
                // 1. Intento de deserialización automática
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var list = JsonSerializer.Deserialize<List<ProductDto>>(json, opts);
                if (list != null) return list;

                // 2. Fallback manual usando el Helper
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Array) return Array.Empty<ProductDto>();

                var items = new List<ProductDto>();
                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    var product = MapJsonToProductDto(el);
                    if (product.Id != Guid.Empty) items.Add(product);
                }
                return items;
            }
            catch
            {
                return Array.Empty<ProductDto>();
            }
        }

        // Función privada para separar la lógica de mapeo
        private ProductDto MapJsonToProductDto(JsonElement el)
        {
            return new ProductDto(
                Id: JsonHelper.GetGuid(el, "id", "Id"),
                Name: JsonHelper.GetString(el, "name", "Name"),
                Description: JsonHelper.GetString(el, "description", "Description"),
                CategoryId: JsonHelper.GetGuid(el, "categoryId", "category_id"),
                CategoryName: JsonHelper.GetCategoryName(el) ?? string.Empty,
                Price: JsonHelper.GetDecimal(el, "price", "Price"),
                Stock: JsonHelper.GetInt(el, "stock", "Stock")
            );
        }

        public async Task<PagedResult<ProductDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct)
        {
            var url = $"api/products/paged?page={page}&pageSize={pageSize}";
            var resp = await _http.GetAsync(url, ct);

            if (!resp.IsSuccessStatusCode)
                return new PagedResult<ProductDto>(new List<ProductDto>(), page, pageSize, 0, 0);

            try
            {
                var json = await resp.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // 1. Extraer los items usando LINQ y el método de mapeo que ya tenemos
                var items = new List<ProductDto>();
                if (root.TryGetProperty("items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in itemsProp.EnumerateArray())
                    {
                        // Reutilizamos el método MapJsonToProductDto para matar la duplicidad
                        var dto = MapJsonToProductDto(el);
                        if (dto.Id != Guid.Empty) items.Add(dto);
                    }
                }

                // 2. Extraer metadatos de paginación usando el Helper
                int totalItems = JsonHelper.GetInt(root, "totalItems") > 0 ? JsonHelper.GetInt(root, "totalItems") : items.Count;
                int totalPages = JsonHelper.GetInt(root, "totalPages") > 0 ? JsonHelper.GetInt(root, "totalPages") : (int)Math.Ceiling((double)totalItems / pageSize);
                int currentPage = JsonHelper.GetInt(root, "page", "currentPage");
                int currentSize = JsonHelper.GetInt(root, "pageSize");

                return new PagedResult<ProductDto>(items, currentPage, currentSize, totalItems, totalPages);
            }
            catch
            {
                return new PagedResult<ProductDto>(new List<ProductDto>(), page, pageSize, 0, 0);
            }
        }

        public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var resp = await _http.GetAsync($"api/products/{id}", ct);
            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync(ct);

            try
            {
                // 1. Intento automático
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var dto = JsonSerializer.Deserialize<ProductDto>(json, opts);
                if (dto != null) return dto;

                // 2. Fallback usando nuestro método de mapeo centralizado
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object) return null;

                var mappedDto = MapJsonToProductDto(doc.RootElement);

                return mappedDto.Id == Guid.Empty ? null : mappedDto;
            }
            catch
            {
                return null;
            }
        }

        public async Task<ProductApiResult> CreateAsync(ProductCreateDto dto, CancellationToken ct)
        { var resp = await _http.PostAsJsonAsync("api/products", dto, CamelCaseOptions, ct); return await ParseProductResult(resp, ct); }
        public async Task<ProductApiResult> UpdateAsync(Guid id, ProductUpdateDto dto, CancellationToken ct)
        { var resp = await _http.PutAsJsonAsync($"api/products/{id}", dto, CamelCaseOptions, ct); return await ParseProductResult(resp, ct); }
        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        { var resp = await _http.DeleteAsync($"api/products/{id}", ct); return resp.IsSuccessStatusCode; }
        public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct)
        {
            var resp = await _http.GetAsync("api/categories", ct);
            if (!resp.IsSuccessStatusCode) return Array.Empty<CategoryDto>();
            try { return await resp.Content.ReadFromJsonAsync<IReadOnlyList<CategoryDto>>(cancellationToken: ct) ?? Array.Empty<CategoryDto>(); }
            catch { return Array.Empty<CategoryDto>(); }
        }

        private static async Task<ProductApiResult> ParseProductResult(HttpResponseMessage resp, CancellationToken ct)
        {
            var result = new ProductApiResult { Success = resp.IsSuccessStatusCode };

            // 1. Verificación de tipo de contenido (Fail Fast)
            var mediaType = resp.Content.Headers.ContentType?.MediaType;
            if (mediaType != "application/json")
            {
                return result;
            }

            var json = await resp.Content.ReadAsStringAsync(ct);

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // 2. Si la respuesta es exitosa (200 OK), deserializamos el objeto principal
                if (resp.IsSuccessStatusCode)
                {
                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        result.Product = JsonSerializer.Deserialize<ProductDto>(root.GetRawText());
                    }
                    return result;
                }

                // 3. Si la respuesta es de error (400, 500, etc.), procesamos el diccionario de errores
                if (root.ValueKind == JsonValueKind.Object)
                {
                    ExtractErrorsFromElement(root, result.Errors);
                }
            }
            catch (JsonException)
            {
                // Se mantiene el manejo silencioso para evitar excepciones en cascada por JSON mal formado
            }

            return result;
        }

        private static void ExtractErrorsFromElement(JsonElement root, Dictionary<string, List<string>> errorDict)
        {
            foreach (var prop in root.EnumerateObject())
            {
                var errorList = new List<string>();

                // Caso A: El error viene como una lista de strings
                if (prop.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in prop.Value.EnumerateArray())
                    {
                        errorList.Add(item.GetString() ?? "Error desconocido");
                    }
                }
                // Caso B: El error viene como un string simple
                else if (prop.Value.ValueKind == JsonValueKind.String)
                {
                    errorList.Add(prop.Value.GetString() ?? "Error desconocido");
                }

                if (errorList.Count > 0)
                {
                    errorDict[prop.Name] = errorList;
                }
            }
        }
    }

    public class SalesApiClient : ISalesApiClient
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions CamelCaseOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public SalesApiClient(IHttpClientFactory f) => _http = f.CreateClient("SalesService");

        public async Task<IReadOnlyList<SaleDto>> GetAllAsync(CancellationToken ct)
        {
            try
            {
                var resp = await _http.GetAsync("api/sales", ct);
                if (!resp.IsSuccessStatusCode) return Array.Empty<SaleDto>();

                var json = await resp.Content.ReadAsStringAsync(ct);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var list = JsonSerializer.Deserialize<List<SaleDto>>(json, options);
                return list as IReadOnlyList<SaleDto> ?? Array.Empty<SaleDto>();
            }
            catch
            {
                return Array.Empty<SaleDto>();
            }
        }

        public async Task<PagedResult<SaleDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct)
        {
            try
            {
                var url = $"api/sales/paged?page={page}&pageSize={pageSize}";
                var resp = await _http.GetAsync(url, ct);

                // 1. Lógica de Fallback (Si el endpoint no existe o falla)
                if (!resp.IsSuccessStatusCode)
                {
                    return await GetManualPagedResult(page, pageSize, ct);
                }

                // 2. Procesar respuesta paginada exitosa
                var json = await resp.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var items = ParseSaleDtoList(root);

                // 3. Extraer metadatos usando el Helper
                int totalItems = JsonHelper.GetInt(root, "totalItems") > 0 ? JsonHelper.GetInt(root, "totalItems") : items.Count;
                int totalPages = JsonHelper.GetInt(root, "totalPages") > 0 ? JsonHelper.GetInt(root, "totalPages") : (int)Math.Ceiling((double)totalItems / pageSize);
                int currentPage = JsonHelper.GetInt(root, "page") > 0 ? JsonHelper.GetInt(root, "page") : page;

                return new PagedResult<SaleDto>(items, currentPage, pageSize, totalItems, totalPages);
            }
            catch
            {
                return new PagedResult<SaleDto>(new List<SaleDto>(), page, pageSize, 0, 0);
            }
        }


        private async Task<PagedResult<SaleDto>> GetManualPagedResult(int page, int pageSize, CancellationToken ct)
        {
            var all = await GetAllAsync(ct);
            var ordered = all.OrderByDescending(s => s.SaleDate).ToList();
            var totalItems = ordered.Count;
            var totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize) : 0;
            var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResult<SaleDto>(items, page, pageSize, totalItems, totalPages);
        }

        private List<SaleDto> ParseSaleDtoList(JsonElement root)
        {
            var items = new List<SaleDto>();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (root.TryGetProperty("items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in itemsProp.EnumerateArray())
                {
                    try
                    {
                        var dto = JsonSerializer.Deserialize<SaleDto>(el.GetRawText(), options);
                        if (dto != null) items.Add(dto);
                    }
                    catch (JsonException) { /* Ignorar corruptos */ }
                }
            }
            return items;
        }

        public async Task<SaleApiResult> CreateAsync(SaleCreateDto dto, CancellationToken ct)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/sales", dto, CamelCaseOptions, ct);
                var result = new SaleApiResult { Success = resp.IsSuccessStatusCode };

                // 1. Verificación rápida de contenido (Fail Fast)
                if (resp.Content.Headers.ContentType?.MediaType != "application/json")
                    return result;

                var json = await resp.Content.ReadAsStringAsync(ct);

                try
                {
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    // 2. Procesar éxito o error de forma separada
                    if (resp.IsSuccessStatusCode)
                    {
                        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        result.Sale = JsonSerializer.Deserialize<SaleDto>(root.GetRawText(), opts);
                    }
                    else
                    {
                        ParseSaleErrors(root, result);
                    }
                }
                catch (JsonException)
                {
                    // Mantenemos el silencio administrativo para evitar crasheos
                }

                return result;
            }
            catch (Exception ex)
            {
                return CreateErrorResult(ex.Message);
            }
        }


        private void ParseSaleErrors(JsonElement root, SaleApiResult result)
        {
            // Extraer mensaje general
            if (root.TryGetProperty("message", out var msgProp))
                result.Message = msgProp.GetString();

            // Extraer lista de errores detallados
            if (root.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var error in errorsProp.EnumerateArray())
                {
                    var field = JsonHelper.GetString(error, "field") ?? "general";
                    var message = JsonHelper.GetString(error, "message") ?? "Error";

                    if (!result.Errors.ContainsKey(field))
                        result.Errors[field] = new List<string>();

                    result.Errors[field].Add(message);
                }
            }
        }

        private SaleApiResult CreateErrorResult(string message)
        {
            return new SaleApiResult
            {
                Success = false,
                Message = $"Error al comunicarse con el servicio: {message}",
                Errors = new Dictionary<string, List<string>> { { "general", new List<string> { message } } }
            };
        }

        public async Task<SaleDto?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            try
            {
                var resp = await _http.GetAsync($"api/sales/{id}", ct);
                if (!resp.IsSuccessStatusCode) return null;

                var json = await resp.Content.ReadAsStringAsync(ct);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<SaleDto>(json, options);
            }
            catch
            {
                return null;
            }
        }

        public async Task<SaleStatusResult> GetStatusAsync(Guid id, CancellationToken ct)
        {
            try
            {
                var resp = await _http.GetAsync($"api/sales/{id}/status", ct);
                if (!resp.IsSuccessStatusCode)
                {
                    return new SaleStatusResult { Status = "PENDING", Message = "Verificando estado de la venta..." };
                }

                var json = await resp.Content.ReadAsStringAsync(ct);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<SaleStatusResult>(json, options) 
                    ?? new SaleStatusResult { Status = "PENDING", Message = "Verificando estado de la venta..." };
            }
            catch
            {
                return new SaleStatusResult { Status = "PENDING", Message = "Verificando estado de la venta..." };
            }
        }
    }

    public class UsersApiClient : IUsersApiClient
    {
        private readonly HttpClient _http;
        public UsersApiClient(IHttpClientFactory factory) => _http = factory.CreateClient("UsersService");
        public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var resp = await _http.GetAsync($"api/User/{id}", ct);
            if (!resp.IsSuccessStatusCode) return null;
            try { return await resp.Content.ReadFromJsonAsync<UserDto>(cancellationToken: ct); } catch { return null; }
        }
        public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct)
        {
            var resp = await _http.GetAsync("api/User", ct);
            if (!resp.IsSuccessStatusCode) return Array.Empty<UserDto>();
            try { return await resp.Content.ReadFromJsonAsync<IReadOnlyList<UserDto>>(cancellationToken: ct) ?? Array.Empty<UserDto>(); } catch { return Array.Empty<UserDto>(); }
        }
        public async Task<IReadOnlyList<UserFullDto>> GetAllRawAsync(CancellationToken ct)
        {
            var resp = await _http.GetAsync("api/User", ct);
            if (!resp.IsSuccessStatusCode) return Array.Empty<UserFullDto>();
            try { return await resp.Content.ReadFromJsonAsync<IReadOnlyList<UserFullDto>>(cancellationToken: ct) ?? Array.Empty<UserFullDto>(); } catch { return Array.Empty<UserFullDto>(); }
        }
        public async Task<PagedResult<UserFullDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct)
        {
            var url = $"api/User/paged?page={page}&pageSize={pageSize}";
            var resp = await _http.GetAsync(url, ct);

            if (!resp.IsSuccessStatusCode)
                return new PagedResult<UserFullDto>(new List<UserFullDto>(), page, pageSize, 0, 0);

            try
            {
                var json = await resp.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // 1. Extraer los items usando el nuevo método de mapeo
                var items = new List<UserFullDto>();
                if (root.TryGetProperty("items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in itemsProp.EnumerateArray())
                    {
                        var dto = MapJsonToUserFullDto(el);
                        if (dto != null) items.Add(dto);
                    }
                }

                // 2. Extraer metadatos de paginación usando el Helper
                int totalItems = JsonHelper.GetInt(root, "totalItems") > 0 ? JsonHelper.GetInt(root, "totalItems") : items.Count;
                int totalPages = JsonHelper.GetInt(root, "totalPages") > 0 ? JsonHelper.GetInt(root, "totalPages") : (int)Math.Ceiling((double)totalItems / pageSize);
                int currentPage = JsonHelper.GetInt(root, "page") > 0 ? JsonHelper.GetInt(root, "page") : page;

                return new PagedResult<UserFullDto>(items, currentPage, pageSize, totalItems, totalPages);
            }
            catch
            {
                return new PagedResult<UserFullDto>(new List<UserFullDto>(), page, pageSize, 0, 0);
            }
        }


        private UserFullDto MapJsonToUserFullDto(JsonElement el)
        {
            try
            {
                return new UserFullDto(
                    Id: JsonHelper.GetGuid(el, "id", "Id"),
                    Username: JsonHelper.GetString(el, "username", "Username"),
                    Email: JsonHelper.GetString(el, "email", "Email"),
                    FirstName: JsonHelper.GetString(el, "firstName", "FirstName"),
                    MiddleName: JsonHelper.GetString(el, "middleName", "MiddleName"),
                    LastName: JsonHelper.GetString(el, "lastName", "LastName"),
                    MustChangePassword: el.TryGetProperty("mustChangePassword", out var mc) && mc.GetBoolean(),
                    Roles: new List<string>(), // Se asume lista vacía por defecto
                    PasswordHash: JsonHelper.GetString(el, "passwordHash", "PasswordHash")
                );
            }
            catch (JsonException)
            {
                return null; // El bucle lo ignorará
            }
        }
        public async Task<IReadOnlyList<string>> GetRolesAsync(Guid id, CancellationToken ct)
        {
            var resp = await _http.GetAsync($"api/User/{id}/roles", ct);
            if (!resp.IsSuccessStatusCode) return Array.Empty<string>();
            try { return await resp.Content.ReadFromJsonAsync<IReadOnlyList<string>>(cancellationToken: ct) ?? Array.Empty<string>(); } catch { return Array.Empty<string>(); }
        }
        public async Task<UserFullDto?> SearchAsync(string userOrEmail, CancellationToken ct)
        {
            var resp = await _http.GetAsync($"api/User/search/{Uri.EscapeDataString(userOrEmail)}", ct);
            if (!resp.IsSuccessStatusCode) return null;
            try { return await resp.Content.ReadFromJsonAsync<UserFullDto>(cancellationToken: ct); } catch { return null; }
        }
        public async Task<AuthLoginResult> LoginAsync(AuthLoginRequest request, CancellationToken ct) { var resp = await _http.PostAsJsonAsync("api/Auth/login", request, ct); return await ParseAuthResponse(resp, ct); }
        public async Task<UserApiResult> CreateAsync(UserCreateRequest dto, CancellationToken ct) { var resp = await _http.PostAsJsonAsync("api/User", dto, ct); return await ParseUserResult(resp, ct); }
        public async Task<UserApiResult> RegisterAsync(UserCreateRequest dto, CancellationToken ct) { var resp = await _http.PostAsJsonAsync("api/Auth/register", dto, ct); return await ParseUserResult(resp, ct); }
        public async Task<UserApiResult> UpdateAsync(Guid id, UserUpdateRequest dto, CancellationToken ct) { var resp = await _http.PutAsJsonAsync($"api/User/{id}", dto, ct); return await ParseUserResult(resp, ct); }
        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct) { var resp = await _http.DeleteAsync($"api/User/{id}", ct); return resp.IsSuccessStatusCode; }
        public async Task<ApiSimpleResult> ChangePasswordAsync(ChangePasswordRequest dto, CancellationToken ct)
        { return await ChangePasswordAsync(dto, null, ct); }
        public async Task<ApiSimpleResult> ChangePasswordAsync(ChangePasswordRequest dto, string? bearerToken, CancellationToken ct)
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "api/Auth/change-password") { Content = JsonContent.Create(dto) };
            if (!string.IsNullOrWhiteSpace(bearerToken)) req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            var resp = await _http.SendAsync(req, ct);
            if (resp.IsSuccessStatusCode) return new ApiSimpleResult { Success = true };
            var text = await resp.Content.ReadAsStringAsync(ct);
            try
            {
                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;
                var msg = root.TryGetProperty("message", out var m) ? m.GetString() : "Error al cambiar contraseña.";
                return new ApiSimpleResult { Success = false, Error = msg };
            }
            catch { return new ApiSimpleResult { Success = false, Error = "Error al cambiar contraseña." }; }
        }

        private static async Task<AuthLoginResult> ParseAuthResponse(HttpResponseMessage resp, CancellationToken ct)
        {
            var result = new AuthLoginResult();

            // 1. Si no es JSON, manejamos el error básico por Status Code y salimos
            if (resp.Content.Headers.ContentType?.MediaType != "application/json")
            {
                result.Success = resp.IsSuccessStatusCode;
                if (!result.Success) result.Error = GetDefaultErrorMessage(resp.StatusCode);
                return result;
            }

            try
            {
                var json = await resp.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // 2. Si es ÉXITO (200 OK)
                if (resp.IsSuccessStatusCode)
                {
                    MapSuccessfulAuth(root, result);
                }
                // 3. Si es ERROR (401, 403, etc.)
                else
                {
                    result.Success = false;
                    result.Error = ExtractAuthError(root, resp.StatusCode);
                }
            }
            catch
            {
                result.Success = false;
                result.Error = "Error al procesar la respuesta del servidor.";
            }

            return result;
        }

        // --- MÉTODOS DE APOYO PARA MATAR LA COMPLEJIDAD ---

        private static void MapSuccessfulAuth(JsonElement root, AuthLoginResult result)
        {
            result.Success = true;
            result.Token = JsonHelper.GetString(root, "accessToken", "token");
            result.UserName = JsonHelper.GetString(root, "userName", "user");
            result.Email = JsonHelper.GetString(root, "email");
            result.FirstName = JsonHelper.GetString(root, "firstName");
            result.MiddleName = JsonHelper.GetString(root, "middleName");
            result.LastName = JsonHelper.GetString(root, "lastName");
            result.MustChangePassword = root.TryGetProperty("mustChangePassword", out var mcp) && mcp.GetBoolean();

            if (root.TryGetProperty("expiresAt", out var exp) && DateTimeOffset.TryParse(exp.GetString(), out var dtoExp))
                result.ExpiresAt = dtoExp;

            if (root.TryGetProperty("roles", out var roles) && roles.ValueKind == JsonValueKind.Array)
            {
                foreach (var r in roles.EnumerateArray())
                    if (r.ValueKind == JsonValueKind.String) result.Roles.Add(r.GetString()!);
            }
        }

        private static string ExtractAuthError(JsonElement root, System.Net.HttpStatusCode status)
        {
            // Buscar mensaje en el JSON
            var msg = JsonHelper.GetString(root, "message", "Message", "error", "title");

            // Si no hay mensaje en JSON, usar el default por Status Code
            return !string.IsNullOrWhiteSpace(msg) ? msg : GetDefaultErrorMessage(status);
        }

        private static string GetDefaultErrorMessage(System.Net.HttpStatusCode status)
        {
            return status switch
            {
                System.Net.HttpStatusCode.Unauthorized => "Credenciales inválidas o usuario inactivo.",
                System.Net.HttpStatusCode.Forbidden => "Acceso denegado.",
                _ => "Error en la autenticación."
            };
        }
        private static async Task<UserApiResult> ParseUserResult(HttpResponseMessage resp, CancellationToken ct)
        {
            var result = new UserApiResult { Success = resp.IsSuccessStatusCode };

            // 1. Verificación de tipo de contenido (Fail Fast)
            if (resp.Content.Headers.ContentType?.MediaType != "application/json")
            {
                return result;
            }

            var json = await resp.Content.ReadAsStringAsync(ct);

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // 2. Si la respuesta es exitosa, mapeamos el usuario
                if (resp.IsSuccessStatusCode && root.ValueKind == JsonValueKind.Object)
                {
                    result.User = MapToUserDto(root);
                    return result;
                }

                // 3. Si hubo error, extraemos el diccionario de errores
                if (root.ValueKind == JsonValueKind.Object)
                {
                    ExtractApiErrors(root, result.Errors);
                }
            }
            catch (JsonException)
            {
                // Se mantiene el manejo silencioso para evitar excepciones en cascada
            }

            return result;
        }


        private static UserFullDto MapToUserDto(JsonElement root)
        {
            var id = JsonHelper.GetGuid(root, "id", "Id");
            var username = JsonHelper.GetString(root, "username", "Username");
            var email = JsonHelper.GetString(root, "email", "Email");

            var roles = new List<string>();
            if (root.TryGetProperty("roles", out var rlProp) && rlProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var r in rlProp.EnumerateArray())
                {
                    if (r.ValueKind == JsonValueKind.String) roles.Add(r.GetString()!);
                }
            }

            return new UserFullDto(id, username, email, null, null, null, false, roles, string.Empty);
        }

        private static void ExtractApiErrors(JsonElement root, Dictionary<string, List<string>> errorDict)
        {
            foreach (var prop in root.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.Array)
                {
                    var list = new List<string>();
                    foreach (var item in prop.Value.EnumerateArray())
                    {
                        list.Add(item.GetString() ?? "Error");
                    }
                    errorDict[prop.Name] = list;
                }
            }
        }
    }

    public class ClientsApiClient : IClientsApiClient
    {
        private readonly HttpClient _http; public ClientsApiClient(IHttpClientFactory f)=>_http=f.CreateClient("ClientsService");

        public async Task<ClientDto?> GetByCiAsync(string ci, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(ci)) return null;
            var url = $"api/Client/by-ci/{Uri.EscapeDataString(ci)}";
            var resp = await _http.GetAsync(url, ct);
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            if (!resp.IsSuccessStatusCode) return null;
            try { return await resp.Content.ReadFromJsonAsync<ClientDto>(cancellationToken: ct); } catch { return null; }
        }

        public async Task<ClientDto?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var resp = await _http.GetAsync($"api/Client/{id}", ct);
            if (!resp.IsSuccessStatusCode) return null;
            try { return await resp.Content.ReadFromJsonAsync<ClientDto>(cancellationToken: ct); } catch { return null; }
        }
        public async Task<IReadOnlyList<ClientDto>> GetAllAsync(CancellationToken ct)
        {
            var resp = await _http.GetAsync("api/Client", ct);
            if (!resp.IsSuccessStatusCode) return Array.Empty<ClientDto>();
            try { return (await resp.Content.ReadFromJsonAsync<List<ClientDto>>(cancellationToken: ct)) ?? new List<ClientDto>(); }
            catch { return Array.Empty<ClientDto>(); }
        }
        public async Task<PagedResult<ClientDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct)
        {
            var url = $"api/Client/paged?page={page}&pageSize={pageSize}";
            var resp = await _http.GetAsync(url, ct);

            if (!resp.IsSuccessStatusCode)
                return new PagedResult<ClientDto>(new List<ClientDto>(), page, pageSize, 0, 0);

            try
            {
                var json = await resp.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // 1. Mapeo de items usando la función privada para limpiar el bucle
                var items = new List<ClientDto>();
                if (root.TryGetProperty("items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in itemsProp.EnumerateArray())
                    {
                        var dto = MapJsonToClientDto(el);
                        if (dto != null) items.Add(dto);
                    }
                }

                // 2. Extraer metadatos usando el JsonHelper que ya tienes en el archivo
                int totalItems = JsonHelper.GetInt(root, "totalItems") > 0 ? JsonHelper.GetInt(root, "totalItems") : items.Count;
                int totalPages = JsonHelper.GetInt(root, "totalPages") > 0 ? JsonHelper.GetInt(root, "totalPages") : (int)Math.Ceiling((double)totalItems / pageSize);
                int currentPage = JsonHelper.GetInt(root, "page") > 0 ? JsonHelper.GetInt(root, "page") : page;

                return new PagedResult<ClientDto>(items, currentPage, pageSize, totalItems, totalPages);
            }
            catch
            {
                return new PagedResult<ClientDto>(new List<ClientDto>(), page, pageSize, 0, 0);
            }
        }

        private static ClientDto? MapJsonToClientDto(JsonElement el)
        {
            try
            {
                return new ClientDto(
                    Id: JsonHelper.GetGuid(el, "id", "Id"),
                    FirstName: JsonHelper.GetString(el, "firstName", "FirstName"),
                    LastName: JsonHelper.GetString(el, "lastName", "LastName"),
                    Ci: JsonHelper.GetString(el, "ci", "Ci"),
                    Email: JsonHelper.GetString(el, "email", "Email"),
                    Phone: JsonHelper.GetString(el, "phone", "Phone"),
                    Address: JsonHelper.GetString(el, "address", "Address")
                );
            }
            catch
            {
                return null; // El bucle lo ignorará si el elemento está corrupto
            }
        }
        public async Task<ClientApiResult> CreateAsync(ClientCreateDto dto, CancellationToken ct) { var resp = await _http.PostAsJsonAsync("api/Client", dto, ct); return await BuildResult(resp, ct); }
        public async Task<ClientApiResult> UpdateAsync(Guid id, ClientUpdateDto dto, CancellationToken ct) { var resp = await _http.PutAsJsonAsync($"api/Client/{id}", dto, ct); return await BuildResult(resp, ct); }
        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct) { var resp = await _http.DeleteAsync($"api/Client/{id}", ct); return resp.IsSuccessStatusCode; }
        private static async Task<ClientApiResult> BuildResult(HttpResponseMessage resp, CancellationToken ct)
        {
            var result = new ClientApiResult { Success = resp.IsSuccessStatusCode };

            // 1. Verificación rápida (Early Return)
            if (resp.Content.Headers.ContentType?.MediaType != "application/json")
            {
                return result;
            }

            try
            {
                var json = await resp.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // 2. Si es ÉXITO
                if (resp.IsSuccessStatusCode)
                {
                    if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("id", out _))
                    {
                        result.Client = JsonSerializer.Deserialize<ClientDto>(root.GetRawText());
                    }
                    return result;
                }

                // 3. Si es ERROR (Procesamos el diccionario de errores)
                if (root.ValueKind == JsonValueKind.Object)
                {
                    ParseClientApiErrors(root, result.Errors);
                }
            }
            catch (JsonException) { /* Silencio intencional para evitar colapso */ }
            catch (Exception) { /* Silencio de seguridad de red */ }

            return result;
        }

        private static void ParseClientApiErrors(JsonElement root, Dictionary<string, List<string>> errorDict)
        {
            foreach (var prop in root.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.Array)
                {
                    var list = new List<string>();
                    foreach (var item in prop.Value.EnumerateArray())
                    {
                        list.Add(item.GetString() ?? "Error");
                    }
                    errorDict[prop.Name] = list;
                }
            }
        }
    }
    public class DistributorsApiClient : IDistributorsApiClient
    {
        private readonly HttpClient _http;
        public DistributorsApiClient(IHttpClientFactory factory) => _http = factory.CreateClient("DistributorsService");
        public async Task<DistributorDto?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var resp = await _http.GetAsync($"api/distributors/{id}", ct);
            if (!resp.IsSuccessStatusCode) return null;
            try { return await resp.Content.ReadFromJsonAsync<DistributorDto>(cancellationToken: ct); } catch { return null; }
        }
        public async Task<IReadOnlyList<DistributorDto>> GetAllAsync(CancellationToken ct)
        {
            var resp = await _http.GetAsync("api/distributors", ct);
            if (!resp.IsSuccessStatusCode) return Array.Empty<DistributorDto>();
            try { return await resp.Content.ReadFromJsonAsync<IReadOnlyList<DistributorDto>>(cancellationToken: ct) ?? Array.Empty<DistributorDto>(); } catch { return Array.Empty<DistributorDto>(); }
        }
        public async Task<PagedResult<DistributorDto>> GetPagedAsync(int? page_parameter, int? pageSize_parameter, CancellationToken ct)
        {
            var page = page_parameter ?? 1;
            var pageSize = pageSize_parameter ?? 10;
            var url = $"api/distributors/paged?page={page}&pageSize={pageSize}";

            var resp = await _http.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode)
            {
                return new PagedResult<DistributorDto>(new List<DistributorDto>(), page, pageSize, 0, 0);
            }

            try
            {
                var json = await resp.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // 1. Extraer items usando un método privado (baja la complejidad cognitiva)
                var items = ParseDistributorList(root);

                // 2. Extraer metadatos usando el JsonHelper centralizado
                int totalItems = JsonHelper.GetInt(root, "totalItems") > 0 ? JsonHelper.GetInt(root, "totalItems") : items.Count;
                int totalPages = JsonHelper.GetInt(root, "totalPages") > 0 ? JsonHelper.GetInt(root, "totalPages") : (int)Math.Ceiling((double)totalItems / pageSize);
                int currentPage = JsonHelper.GetInt(root, "page") > 0 ? JsonHelper.GetInt(root, "page") : page;
                int currentSize = JsonHelper.GetInt(root, "pageSize") > 0 ? JsonHelper.GetInt(root, "pageSize") : pageSize;

                return new PagedResult<DistributorDto>(items, currentPage, currentSize, totalItems, totalPages);
            }
            catch
            {
                return new PagedResult<DistributorDto>(new List<DistributorDto>(), page, pageSize, 0, 0);
            }
        }

        private static List<DistributorDto> ParseDistributorList(JsonElement root)
        {
            var items = new List<DistributorDto>();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (root.TryGetProperty("items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in itemsProp.EnumerateArray())
                {
                    try
                    {
                        var dto = JsonSerializer.Deserialize<DistributorDto>(el.GetRawText(), options);
                        if (dto != null) items.Add(dto);
                    }
                    catch (JsonException)
                    {
                        // Tolerancia a fallos para elementos corruptos
                    }
                }
            }
            return items;
        }
        public async Task<DistributorApiResult> CreateAsync(DistributorCreateDto dto, CancellationToken ct)
        { var resp = await _http.PostAsJsonAsync("api/distributors", dto, ct); return await ParseResult(resp, ct); }
        public async Task<DistributorApiResult> UpdateAsync(Guid id, DistributorUpdateDto dto, CancellationToken ct)
        { var resp = await _http.PutAsJsonAsync($"api/distributors/{id}", dto, ct); return await ParseResult(resp, ct); }
        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        { var resp = await _http.DeleteAsync($"api/distributors/{id}", ct); return resp.IsSuccessStatusCode; }
        private static async Task<DistributorApiResult> ParseResult(HttpResponseMessage resp, CancellationToken ct)
        {
            var result = new DistributorApiResult { Success = resp.IsSuccessStatusCode };

            // 1. Verificación de tipo (Early Return)
            if (resp.Content.Headers.ContentType?.MediaType != "application/json")
            {
                return result;
            }

            var json = await resp.Content.ReadAsStringAsync(ct);

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // 2. Si es ÉXITO
                if (resp.IsSuccessStatusCode)
                {
                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        result.Distributor = JsonSerializer.Deserialize<DistributorDto>(root.GetRawText());
                    }
                    return result;
                }

                // 3. Si es ERROR (Procesamos el diccionario de errores)
                if (root.ValueKind == JsonValueKind.Object)
                {
                    ExtractDistributorErrors(root, result.Errors);
                }
            }
            catch (JsonException)
            {
                // Se ignora el error de parseo para mantener la estabilidad
            }

            return result;
        }

        private static void ExtractDistributorErrors(JsonElement root, Dictionary<string, List<string>> errorDict)
        {
            foreach (var prop in root.EnumerateObject())
            {
                var list = new List<string>();

                if (prop.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in prop.Value.EnumerateArray())
                        list.Add(item.GetString() ?? "Error");
                }
                else if (prop.Value.ValueKind == JsonValueKind.String)
                {
                    list.Add(prop.Value.GetString() ?? "Error");
                }

                if (list.Count > 0) errorDict[prop.Name] = list;
            }
        }
    }
}
