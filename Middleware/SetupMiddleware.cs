namespace ClinicManagementSystem.Middleware
{
    public class SetupMiddleware
    {
        private readonly RequestDelegate _next;

        public SetupMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ISettingsService settingsService)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            if (path.StartsWith("/setup")||
                    path.StartsWith("/css")||
                    path.StartsWith("/js") ||
                    path.StartsWith("/images") ||
                    path.StartsWith("/lib") ||
                    path.StartsWith("/uploads") ||
                    path.StartsWith("/favicon") ||
                    path.StartsWith("/favicon.ico") ||
                    path.Contains("."))
            {
                await _next(context);
                return;
            }

            var setupCompleted =
                await settingsService.GetBool(SettingKeys.SetupCompleted);

            if (!setupCompleted)
            {
                context.Response.Redirect("/Setup");
                return;
            }

            await _next(context);
        }
    }
}
