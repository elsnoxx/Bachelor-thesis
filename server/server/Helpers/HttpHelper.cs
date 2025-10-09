namespace server.Helpers
{
    public class HttpHelper
    {
        public static string GetClientIp(HttpContext httpContext)
        {
            return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
