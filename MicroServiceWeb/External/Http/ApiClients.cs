using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MicroServiceWeb.External.Http
{
    public class ProductsApiClient : IProductsApiClient
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions CamelCaseOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        public ProductsApiClient(IHttpClientFactory f)=>_http=f.CreateClient("ProductsService");
        public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken ct)
        {
            var resp = await _http.GetAsync("api/products", ct);
            if (!resp.IsSuccessStatusCode) return Array.Empty<ProductDto>();
            var json = await resp.Content.ReadAsStringAsync(ct);
            try
            {
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var list = JsonSerializer.Deserialize<List<ProductDto>>(json, opts);
                if (list != null) return list;
                // Fallback manual parsing
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var items = new List<ProductDto>();
                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in root.EnumerateArray())
                    {
                        var id = el.TryGetProperty("id", out var idP) && Guid.TryParse(idP.GetString(), out var gid) ? gid : Guid.Empty;
                        var name = el.TryGetProperty("name", out var nP) ? nP.GetString() ?? string.Empty : string.Empty;
                        var desc = el.TryGetProperty("description", out var dP) ? dP.GetString() : null;
                        Guid catId = Guid.Empty;
                        if (el.TryGetProperty("categoryId", out var cidP)) Guid.TryParse(cidP.GetString(), out catId);
                        else if (el.TryGetProperty("category_id", out var cidSnake) && cidSnake.ValueKind == JsonValueKind.String) Guid.TryParse(cidSnake.GetString(), out catId);
                        var catName = el.TryGetProperty("categoryName", out var cnP) ? cnP.GetString() : (el.TryGetProperty("category_name", out var cnSnake) ? cnSnake.GetString() : null);
                        var price = el.TryGetProperty("price", out var prP) && prP.TryGetDecimal(out var prVal) ? prVal : 0m;
                        var stock = el.TryGetProperty("stock", out var stP) && stP.TryGetInt32(out var stVal) ? stVal : 0;
                        if (id != Guid.Empty)
                            items.Add(new ProductDto(id, name, desc, catId, catName, price, stock));
                    }
                }
                return items;
            }
            catch { return Array.Empty<ProductDto>(); }
        }
        public async Task<PagedResult<ProductDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct)
        {
            // Llama al endpoint paginado: api/products/paged?page={page}&pageSize={pageSize}
            var url = $"api/products/paged?page={page}&pageSize={pageSize}";
            var resp = await _http.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode)
            {
                return new PagedResult<ProductDto>(new List<ProductDto>(), page, pageSize, 0, 0);
            }
            try
            {
                var json = await resp.Content.ReadAsStringAsync(ct);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                // Se espera estructura: { items: [...], page: n, pageSize: n, totalItems: n, totalPages: n }
                var items = new List<ProductDto>();
                if (root.TryGetProperty("items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in itemsProp.EnumerateArray())
                    {
                        try 
                        { 
                            // Deserializaci�n tolerante con snake_case fallback
                            var dto = System.Text.Json.JsonSerializer.Deserialize<ProductDto>(el.GetRawText(), options);
                            if (dto == null)
                            {
                                var id = el.TryGetProperty("id", out var idP) && Guid.TryParse(idP.GetString(), out var gid) ? gid : Guid.Empty;
                                var name = el.TryGetProperty("name", out var nP) ? nP.GetString() ?? string.Empty : string.Empty;
                                var desc = el.TryGetProperty("description", out var dP) ? dP.GetString() : null;
                                Guid catId = Guid.Empty;
                                if (el.TryGetProperty("categoryId", out var cidP)) Guid.TryParse(cidP.GetString(), out catId);
                                else if (el.TryGetProperty("category_id", out var cidSnake) && cidSnake.ValueKind == JsonValueKind.String) Guid.TryParse(cidSnake.GetString(), out catId);
                                var catName = el.TryGetProperty("categoryName", out var cnP) ? cnP.GetString() : (el.TryGetProperty("category_name", out var cnSnake) ? cnSnake.GetString() : null);
                                var price = el.TryGetProperty("price", out var prP) && prP.TryGetDecimal(out var prVal) ? prVal : 0m;
                                var stock = el.TryGetProperty("stock", out var stP) && stP.TryGetInt32(out var stVal) ? stVal : 0;
                                dto = new ProductDto(id, name, desc, catId, catName, price, stock);
                            }
                            items.Add(dto);
                        } 
                        catch { }
                    }
                }
                int totalItems = root.TryGetProperty("totalItems", out var ti) && ti.TryGetInt32(out var tiVal) ? tiVal : items.Count;
                int totalPages = root.TryGetProperty("totalPages", out var tp) && tp.TryGetInt32(out var tpVal) ? tpVal : (int)Math.Ceiling((double)totalItems / pageSize);
                int currentPage = root.TryGetProperty("page", out var pg) && pg.TryGetInt32(out var pgVal) ? pgVal : page;
                int currentPageSize = root.TryGetProperty("pageSize", out var ps) && ps.TryGetInt32(out var psVal) ? psVal : pageSize;
                return new PagedResult<ProductDto>(items, currentPage, currentPageSize, totalItems, totalPages);
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
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var dto = JsonSerializer.Deserialize<ProductDto>(json, opts);
                if (dto != null) return dto;
                using var doc = JsonDocument.Parse(json);
                var el = doc.RootElement;
                if (el.ValueKind != JsonValueKind.Object) return null;
                var pid = el.TryGetProperty("id", out var idP) && Guid.TryParse(idP.GetString(), out var gid) ? gid : Guid.Empty;
                var name = el.TryGetProperty("name", out var nP) ? nP.GetString() ?? string.Empty : string.Empty;
                var desc = el.TryGetProperty("description", out var dP) ? dP.GetString() : null;
                Guid catId = Guid.Empty;
                if (el.TryGetProperty("categoryId", out var cidP)) Guid.TryParse(cidP.GetString(), out catId);
                else if (el.TryGetProperty("category_id", out var cidSnake) && cidSnake.ValueKind == JsonValueKind.String) Guid.TryParse(cidSnake.GetString(), out catId);
                var catName = el.TryGetProperty("categoryName", out var cnP) ? cnP.GetString() : (el.TryGetProperty("category_name", out var cnSnake) ? cnSnake.GetString() : null);
                var price = el.TryGetProperty("price", out var prP) && prP.TryGetDecimal(out var prVal) ? prVal : 0m;
                var stock = el.TryGetProperty("stock", out var stP) && stP.TryGetInt32(out var stVal) ? stVal : 0;
                return pid == Guid.Empty ? null : new ProductDto(pid, name, desc, catId, catName, price, stock);
            }
            catch { return null; }
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
            if (resp.Content.Headers.ContentType?.MediaType == "application/json")
            {
                var json = await resp.Content.ReadAsStringAsync(ct);
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (resp.IsSuccessStatusCode && root.ValueKind == JsonValueKind.Object)
                    { result.Product = System.Text.Json.JsonSerializer.Deserialize<ProductDto>(root.GetRawText()); }
                    else if (root.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in root.EnumerateObject())
                        {
                            if (prop.Value.ValueKind == JsonValueKind.Array)
                            { var list = new List<string>(); foreach (var item in prop.Value.EnumerateArray()) list.Add(item.GetString() ?? "Error"); result.Errors[prop.Name] = list; }
                            else if (prop.Value.ValueKind == JsonValueKind.String)
                            { result.Errors[prop.Name] = new List<string> { prop.Value.GetString() ?? "Error" }; }
                        }
                    }
                }
                catch { }
            }
            return result;
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
                // Primero intentar con endpoint paginado
                var url = $"api/sales/paged?page={page}&pageSize={pageSize}";
                var resp = await _http.GetAsync(url, ct);
                
                if (!resp.IsSuccessStatusCode)
                {
                    // Si no existe endpoint paginado, usar GetAll y paginar manualmente
                    var all = await GetAllAsync(ct);
                    var ordered = all.OrderByDescending(s => s.SaleDate).ToList();
                    var totalItems = ordered.Count;
                    var totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize) : 0;
                    var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                    return new PagedResult<SaleDto>(items, page, pageSize, totalItems, totalPages);
                }

                var json = await resp.Content.ReadAsStringAsync(ct);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var pagedItems = new List<SaleDto>();
                if (root.TryGetProperty("items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in itemsProp.EnumerateArray())
                    {
                        try
                        {
                            var dto = JsonSerializer.Deserialize<SaleDto>(el.GetRawText(), options);
                            if (dto != null) pagedItems.Add(dto);
                        }
                        catch { }
                    }
                }

                int totalItemsPaged = root.TryGetProperty("totalItems", out var ti) && ti.TryGetInt32(out var tiVal) ? tiVal : pagedItems.Count;
                int totalPagesPaged = root.TryGetProperty("totalPages", out var tp) && tp.TryGetInt32(out var tpVal) ? tpVal : (int)Math.Ceiling((double)totalItemsPaged / pageSize);
                int currentPage = root.TryGetProperty("page", out var pg) && pg.TryGetInt32(out var pgVal) ? pgVal : page;
                int currentPageSize = root.TryGetProperty("pageSize", out var ps) && ps.TryGetInt32(out var psVal) ? psVal : pageSize;

                return new PagedResult<SaleDto>(pagedItems, currentPage, currentPageSize, totalItemsPaged, totalPagesPaged);
            }
            catch
            {
                return new PagedResult<SaleDto>(new List<SaleDto>(), page, pageSize, 0, 0);
            }
        }

        public async Task<SaleApiResult> CreateAsync(SaleCreateDto dto, CancellationToken ct)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/sales", dto, CamelCaseOptions, ct);
                var result = new SaleApiResult { Success = resp.IsSuccessStatusCode };

                if (resp.Content.Headers.ContentType?.MediaType == "application/json")
                {
                    var json = await resp.Content.ReadAsStringAsync(ct);
                    try
                    {
                        using var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;

                        if (resp.IsSuccessStatusCode)
                        {
                            // Parsear la respuesta exitosa
                            result.Sale = JsonSerializer.Deserialize<SaleDto>(root.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        }
                        else
                        {
                            // Parsear errores de validación
                            if (root.TryGetProperty("message", out var msgProp))
                                result.Message = msgProp.GetString();

                            if (root.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var error in errorsProp.EnumerateArray())
                                {
                                    var field = error.TryGetProperty("field", out var f) ? f.GetString() ?? "general" : "general";
                                    var message = error.TryGetProperty("message", out var m) ? m.GetString() ?? "Error" : "Error";
                                    
                                    if (!result.Errors.ContainsKey(field))
                                        result.Errors[field] = new List<string>();
                                    result.Errors[field].Add(message);
                                }
                            }
                        }
                    }
                    catch { }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new SaleApiResult
                {
                    Success = false,
                    Message = $"Error al comunicarse con el servicio: {ex.Message}",
                    Errors = new Dictionary<string, List<string>> { { "general", new List<string> { ex.Message } } }
                };
            }
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
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var items = new List<UserFullDto>();
                if (root.TryGetProperty("items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in itemsProp.EnumerateArray())
                    {
                        try
                        {
                            var id = el.TryGetProperty("id", out var idP) && Guid.TryParse(idP.GetString(), out var gid) ? gid : Guid.Empty;
                            var username = el.TryGetProperty("username", out var unP) ? unP.GetString() ?? "" : "";
                            var email = el.TryGetProperty("email", out var emP) ? emP.GetString() : null;
                            var firstName = el.TryGetProperty("firstName", out var fnP) ? fnP.GetString() : null;
                            var middleName = el.TryGetProperty("middleName", out var mnP) ? mnP.GetString() : null;
                            var lastName = el.TryGetProperty("lastName", out var lnP) ? lnP.GetString() : null;
                            var mustChange = el.TryGetProperty("mustChangePassword", out var mcP) && mcP.GetBoolean();
                            var pwdHash = el.TryGetProperty("passwordHash", out var phP) ? phP.GetString() ?? "" : "";
                            items.Add(new UserFullDto(id, username, email, firstName, middleName, lastName, mustChange, new List<string>(), pwdHash));
                        }
                        catch { }
                    }
                }
                int totalItems = root.TryGetProperty("totalItems", out var ti) && ti.TryGetInt32(out var tiVal) ? tiVal : items.Count;
                int totalPages = root.TryGetProperty("totalPages", out var tp) && tp.TryGetInt32(out var tpVal) ? tpVal : (int)Math.Ceiling((double)totalItems / pageSize);
                int currentPage = root.TryGetProperty("page", out var pg) && pg.TryGetInt32(out var pgVal) ? pgVal : page;
                int currentPageSize = root.TryGetProperty("pageSize", out var ps) && ps.TryGetInt32(out var psVal) ? psVal : pageSize;
                return new PagedResult<UserFullDto>(items, currentPage, currentPageSize, totalItems, totalPages);
            }
            catch
            {
                return new PagedResult<UserFullDto>(new List<UserFullDto>(), page, pageSize, 0, 0);
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
            var status = resp.StatusCode;
            if (resp.Content.Headers.ContentType?.MediaType == "application/json")
            {
                var json = await resp.Content.ReadAsStringAsync(ct);
                try
                {
                    using var doc = JsonDocument.Parse(json); var root = doc.RootElement;
                    if (resp.IsSuccessStatusCode && root.ValueKind == JsonValueKind.Object)
                    {
                        result.Success = true;
                        // Soportar 'accessToken' (backend actual) y 'token' (fallback)
                        if (root.TryGetProperty("accessToken", out var accessToken)) result.Token = accessToken.GetString();
                        else if (root.TryGetProperty("token", out var token)) result.Token = token.GetString();
                        if (root.TryGetProperty("expiresAt", out var exp) && exp.ValueKind == JsonValueKind.String && DateTimeOffset.TryParse(exp.GetString(), out var dtoExp)) result.ExpiresAt = dtoExp;
                        if (root.TryGetProperty("userName", out var uname)) result.UserName = uname.GetString() ?? string.Empty;
                        else if (root.TryGetProperty("user", out var u) && u.ValueKind == JsonValueKind.String) result.UserName = u.GetString() ?? string.Empty;
                        if (root.TryGetProperty("email", out var em)) result.Email = em.GetString();
                        if (root.TryGetProperty("firstName", out var fn) && fn.ValueKind == JsonValueKind.String) result.FirstName = fn.GetString();
                        if (root.TryGetProperty("middleName", out var mn) && mn.ValueKind == JsonValueKind.String) result.MiddleName = mn.GetString();
                        if (root.TryGetProperty("lastName", out var ln) && ln.ValueKind == JsonValueKind.String) result.LastName = ln.GetString();
                        if (root.TryGetProperty("roles", out var roles) && roles.ValueKind == JsonValueKind.Array)
                            foreach (var r in roles.EnumerateArray()) if (r.ValueKind == JsonValueKind.String) result.Roles.Add(r.GetString()!);
                        if (root.TryGetProperty("mustChangePassword", out var mcp)) result.MustChangePassword = mcp.GetBoolean();
                    }
                    else
                    {
                        string? msg = null;
                        foreach (var name in new[] { "message", "Message", "error", "title" })
                        {
                            if (root.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String)
                            { msg = prop.GetString(); break; }
                        }
                        if (string.IsNullOrWhiteSpace(msg))
                        {
                            msg = status switch
                            {
                                System.Net.HttpStatusCode.Unauthorized => "Credenciales inválidas o usuario inactivo.",
                                System.Net.HttpStatusCode.Forbidden => "Acceso denegado.",
                                _ => "Error al iniciar sesión."
                            };
                        }
                        result.Error = msg;
                        result.Success = false;
                    }
                }
                catch
                {
                    result.Success = false; result.Error = "Error al procesar la respuesta del servidor.";
                }
            }
            else
            {
                result.Success = resp.IsSuccessStatusCode;
                if (!result.Success)
                {
                    result.Error = status switch
                    {
                        System.Net.HttpStatusCode.Unauthorized => "Credenciales inválidas o usuario inactivo.",
                        System.Net.HttpStatusCode.Forbidden => "Acceso denegado.",
                        _ => "Error en la autenticación."
                    };
                }
            }
            Console.WriteLine(result);
            return result;
        }
        private static async Task<UserApiResult> ParseUserResult(HttpResponseMessage resp, CancellationToken ct)
        {
            var result = new UserApiResult { Success = resp.IsSuccessStatusCode };
            if (resp.Content.Headers.ContentType?.MediaType == "application/json")
            {
                var json = await resp.Content.ReadAsStringAsync(ct);
                try
                {
                    using var doc = JsonDocument.Parse(json); var root = doc.RootElement;
                    if (resp.IsSuccessStatusCode && root.ValueKind == JsonValueKind.Object)
                    {
                        var id = root.TryGetProperty("id", out var idProp) && Guid.TryParse(idProp.GetString(), out var gid) ? gid : Guid.Empty;
                        var username = root.TryGetProperty("username", out var unProp) ? unProp.GetString() : null;
                        var email = root.TryGetProperty("email", out var emProp) ? emProp.GetString() : null;
                        var roles = new List<string>();
                        if (root.TryGetProperty("roles", out var rlProp) && rlProp.ValueKind == JsonValueKind.Array)
                            foreach (var r in rlProp.EnumerateArray()) if (r.ValueKind == JsonValueKind.String) roles.Add(r.GetString()!);
                        result.User = new UserFullDto(id, username ?? string.Empty, email, null, null, null, false, roles, string.Empty);
                    }
                    else if (root.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in root.EnumerateObject())
                        {
                            if (prop.Value.ValueKind == JsonValueKind.Array)
                            {
                                var list = new List<string>(); foreach (var item in prop.Value.EnumerateArray()) list.Add(item.GetString() ?? "Error");
                                result.Errors[prop.Name] = list;
                            }
                        }
                    }
                }
                catch { }
            }
            return result;
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
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var items = new List<ClientDto>();
                if (root.TryGetProperty("items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in itemsProp.EnumerateArray())
                    {
                        try
                        {
                            var dto = System.Text.Json.JsonSerializer.Deserialize<ClientDto>(el.GetRawText(), options);
                            if (dto == null)
                            {
                                var id = el.TryGetProperty("id", out var idP) && Guid.TryParse(idP.GetString(), out var gid) ? gid : Guid.Empty;
                                var firstName = el.TryGetProperty("firstName", out var fnP) ? fnP.GetString() ?? string.Empty : string.Empty;
                                var lastName = el.TryGetProperty("lastName", out var lnP) ? lnP.GetString() ?? string.Empty : string.Empty;
                                var ci = el.TryGetProperty("ci", out var ciP) ? (ciP.GetString() ?? string.Empty) : string.Empty;
                                var email = el.TryGetProperty("email", out var emP) ? emP.GetString() : null;
                                var phone = el.TryGetProperty("phone", out var phP) ? phP.GetString() : null;
                                var address = el.TryGetProperty("address", out var adP) ? adP.GetString() : null;
                                dto = new ClientDto(id, firstName, lastName, ci, email, phone, address);
                            }
                            if (dto != null) items.Add(dto);
                        }
                        catch { }
                    }
                }
                int totalItems = root.TryGetProperty("totalItems", out var ti) && ti.TryGetInt32(out var tiVal) ? tiVal : items.Count;
                int totalPages = root.TryGetProperty("totalPages", out var tp) && tp.TryGetInt32(out var tpVal) ? tpVal : (int)Math.Ceiling((double)totalItems / pageSize);
                int currentPage = root.TryGetProperty("page", out var pg) && pg.TryGetInt32(out var pgVal) ? pgVal : page;
                int currentPageSize = root.TryGetProperty("pageSize", out var ps) && ps.TryGetInt32(out var psVal) ? psVal : pageSize;
                return new PagedResult<ClientDto>(items, currentPage, currentPageSize, totalItems, totalPages);
            }
            catch
            {
                return new PagedResult<ClientDto>(new List<ClientDto>(), page, pageSize, 0, 0);
            }
        }
        public async Task<ClientApiResult> CreateAsync(ClientCreateDto dto, CancellationToken ct) { var resp = await _http.PostAsJsonAsync("api/Client", dto, ct); return await BuildResult(resp, ct); }
        public async Task<ClientApiResult> UpdateAsync(Guid id, ClientUpdateDto dto, CancellationToken ct) { var resp = await _http.PutAsJsonAsync($"api/Client/{id}", dto, ct); return await BuildResult(resp, ct); }
        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct) { var resp = await _http.DeleteAsync($"api/Client/{id}", ct); return resp.IsSuccessStatusCode; }
        private static async Task<ClientApiResult> BuildResult(HttpResponseMessage resp, CancellationToken ct)
        {
            var result = new ClientApiResult { Success = resp.IsSuccessStatusCode };
            if (resp.Content.Headers.ContentType?.MediaType == "application/json")
            {
                try
                {
                    using var stream = await resp.Content.ReadAsStreamAsync(ct);
                    using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
                    if (resp.IsSuccessStatusCode)
                    {
                        if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("id", out _))
                            result.Client = System.Text.Json.JsonSerializer.Deserialize<ClientDto>(doc.RootElement.GetRawText());
                    }
                    else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in doc.RootElement.EnumerateObject())
                        {
                            if (prop.Value.ValueKind == JsonValueKind.Array)
                            {
                                var list = new List<string>(); foreach (var item in prop.Value.EnumerateArray()) list.Add(item.GetString() ?? "Error");
                                result.Errors[prop.Name] = list;
                            }
                        }
                    }
                }
                catch { }
            }
            return result;
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
            // Llama al endpoint paginado: api/distributors/paged?page={page}&pageSize={pageSize}
            var url = $"api/distributors/paged?page={page}&pageSize={pageSize}";
            var resp = await _http.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode)
            {
                return new PagedResult<DistributorDto>(new List<DistributorDto>(), page, pageSize, 0, 0);
            }
            try
            {
                var json = await resp.Content.ReadAsStringAsync(ct);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                // Se espera estructura: { items: [...], page: n, pageSize: n, totalItems: n, totalPages: n }
                var items = new List<DistributorDto>();
                if (root.TryGetProperty("items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in itemsProp.EnumerateArray())
                    {
                        try
                        {
                            var dto = System.Text.Json.JsonSerializer.Deserialize<DistributorDto>(el.GetRawText(), options);
                            if (dto != null) items.Add(dto);
                        }
                        catch { }
                    }
                }
                int totalItems = root.TryGetProperty("totalItems", out var ti) && ti.TryGetInt32(out var tiVal) ? tiVal : items.Count;
                int totalPages = root.TryGetProperty("totalPages", out var tp) && tp.TryGetInt32(out var tpVal) ? tpVal : (int)Math.Ceiling((double)totalItems / pageSize);
                int currentPage = root.TryGetProperty("page", out var pg) && pg.TryGetInt32(out var pgVal) ? pgVal : page;
                int currentPageSize = root.TryGetProperty("pageSize", out var ps) && ps.TryGetInt32(out var psVal) ? psVal : pageSize;
                return new PagedResult<DistributorDto>(items, currentPage, currentPageSize, totalItems, totalPages);
            }
            catch
            {
                return new PagedResult<DistributorDto>(new List<DistributorDto>(), page, pageSize, 0, 0);
            }
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
            if (resp.Content.Headers.ContentType?.MediaType == "application/json")
            {
                var json = await resp.Content.ReadAsStringAsync(ct);
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (resp.IsSuccessStatusCode && root.ValueKind == JsonValueKind.Object)
                    { result.Distributor = System.Text.Json.JsonSerializer.Deserialize<DistributorDto>(root.GetRawText()); }
                    else if (root.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in root.EnumerateObject())
                        {
                            if (prop.Value.ValueKind == JsonValueKind.Array)
                            { var list = new List<string>(); foreach (var item in prop.Value.EnumerateArray()) list.Add(item.GetString() ?? "Error"); result.Errors[prop.Name] = list; }
                            else if (prop.Value.ValueKind == JsonValueKind.String)
                            { result.Errors[prop.Name] = new List<string> { prop.Value.GetString() ?? "Error" }; }
                        }
                    }
                }
                catch { }
            }
            return result;
        }
    }
}
