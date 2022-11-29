using HwRemind.Endpoints.Authentication.Services;
using HwRemind.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HwRemind.Api.Endpoints.Authentication.Filters
{
    public class ExpiredTokenWithLoginFilter : IAuthorizationFilter
    {
        
        private IJWTService _jwtService;
        private string[] types = { "id" };

        public ExpiredTokenWithLoginFilter(IJWTService jwtService)
        {
            _jwtService = jwtService;
        }

        public async void OnAuthorization(AuthorizationFilterContext context)
        {
            var jwt = context.HttpContext.GetJWT();

            if (string.IsNullOrEmpty(jwt))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var isValid = await _jwtService.ValidateToken(jwt, claimValueTypes: types);
            var isExpired = await _jwtService.IsTokenExpired(jwt);

            if (!isValid || !isExpired) { context.Result = new UnauthorizedResult(); }

        }
    }

    internal class ExpiredTokenWithLoginAtrribute : ServiceFilterAttribute
    {
        public ExpiredTokenWithLoginAtrribute() : base(typeof(ExpiredTokenWithLoginFilter))
        { }
    }
}
