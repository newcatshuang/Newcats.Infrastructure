namespace Newcats.DenpendencyInjection
{
    /// <summary>
    /// HttpContext
    /// </summary>
    public static class HttpContext
    {
        /// <summary>
        /// IHttpContextAccessor
        /// </summary>
        private static Microsoft.AspNetCore.Http.IHttpContextAccessor _accessor;

        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="httpContextAccessor">IHttpContextAccessor</param>
        public static void Configure(Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
        {
            _accessor = httpContextAccessor;
        }

        /// <summary>
        /// Current
        /// </summary>
        public static Microsoft.AspNetCore.Http.HttpContext Current => _accessor.HttpContext;
    }
}