using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AuthApp
{
    public class AuthTimeMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthTimeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext.User != null && httpContext.User.Identity.IsAuthenticated)
            {
                (httpContext.User.Identity as ClaimsIdentity).AddClaim(new Claim("LoginTime", DateTime.Now.ToString()));
            }

            await _next(httpContext);
        }
    }
}
