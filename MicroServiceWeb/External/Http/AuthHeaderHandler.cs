using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace MicroServiceWeb.External.Http;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _accessor;
    
    public AuthHeaderHandler(IHttpContextAccessor accessor) 
    { 
        _accessor = accessor; 
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var ctx = _accessor.HttpContext;
        var token = ctx?.User?.FindFirst("access_token")?.Value;
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
        return await base.SendAsync(request, cancellationToken);
    }
}
