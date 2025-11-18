namespace Restaurant.MVC.Middleware
{
	public class SecurityHeadersMiddleware
	{
		private readonly RequestDelegate _next;

		public SecurityHeadersMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			// X-Content-Type-Options: zapobiega MIME type sniffing
			context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

			// X-Frame-Options: zapobiega clickjacking
			context.Response.Headers.Add("X-Frame-Options", "DENY");

			// X-XSS-Protection: włącza ochronę XSS w przeglądarce
			context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

			// Referrer-Policy: kontroluje ile informacji jest wysyłanych w nagłówku Referer
			context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

			// Content-Security-Policy: zapobiega XSS i innym atakom injection
			context.Response.Headers.Add("Content-Security-Policy",
				"default-src 'self'; " +
				"script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; " +
				"style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
				"img-src 'self' data: https:; " +
				"font-src 'self' https://cdn.jsdelivr.net; " +
				"connect-src 'self'; " +
				"frame-ancestors 'none';");

			// Permissions-Policy: kontroluje które API przeglądarki są dostępne
			context.Response.Headers.Add("Permissions-Policy",
				"accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");

			// Strict-Transport-Security: wymusza HTTPS
			if (context.Request.IsHttps)
			{
				context.Response.Headers.Add("Strict-Transport-Security",
					"max-age=31536000; includeSubDomains; preload");
			}

			// Usuń nagłówki które ujawniają informacje o serwerze
			context.Response.Headers.Remove("Server");
			context.Response.Headers.Remove("X-Powered-By");
			context.Response.Headers.Remove("X-AspNet-Version");
			context.Response.Headers.Remove("X-AspNetMvc-Version");

			await _next(context);
		}
	}
}
