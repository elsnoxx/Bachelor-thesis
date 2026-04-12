namespace server.Helpers
{
    /// <summary>
    /// Utility class providing helper methods for extracting information from HTTP requests.
    /// </summary>
    public static class HttpHelper
    {
        /// <summary>
        /// Retrieves the remote IP address of the client from the current HTTP context.
        /// </summary>
        /// <param name="httpContext">The current <see cref="HttpContext"/> of the request.</param>
        /// <returns>
        /// The string representation of the client's IP address, 
        /// or "unknown" if the address cannot be determined.
        /// </returns>
        public static string GetClientIp(HttpContext httpContext)
        {
            if (httpContext == null) return "unknown";

            // Note: If the application is running behind a proxy (like Nginx or Docker),
            // you might need to check X-Forwarded-For headers.
            return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
