using Microsoft.AspNetCore.Http;

namespace DateAccess.HelperMethods
{
    public static class HttpContextHelper
    {
        public static string GetTokenFromAuthorizationHeader(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var authorizationHeader = (string)request.Headers["Authorization"];
            var userToken = authorizationHeader.Replace("Bearer ", "");

            return userToken;
        }
    }
}