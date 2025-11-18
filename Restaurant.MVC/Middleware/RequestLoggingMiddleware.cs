using System.Diagnostics;
using System.Text;

namespace Restaurant.MVC.Middleware
{
	public class RequestLoggingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<RequestLoggingMiddleware> _logger;

		public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			var startTime = Stopwatch.GetTimestamp();
			var requestId = Guid.NewGuid().ToString();

			// Logowanie szczegółów żądania
			await LogRequest(context, requestId);

			// Przechwycenie oryginalnego body stream
			var originalBodyStream = context.Response.Body;

			try
			{
				using var responseBody = new MemoryStream();
				context.Response.Body = responseBody;

				// Wywołanie następnego middleware
				await _next(context);

				// Obliczenie czasu wykonania
				var elapsedTime = GetElapsedMilliseconds(startTime, Stopwatch.GetTimestamp());

				// Logowanie odpowiedzi
				await LogResponse(context, requestId, elapsedTime);

				// Kopiowanie zawartości do oryginalnego stream
				await responseBody.CopyToAsync(originalBodyStream);
			}
			finally
			{
				context.Response.Body = originalBodyStream;
			}
		}

		private async Task LogRequest(HttpContext context, string requestId)
		{
			var request = context.Request;

			var logData = new
			{
				RequestId = requestId,
				Timestamp = DateTime.UtcNow,
				Method = request.Method,
				Path = request.Path,
				QueryString = request.QueryString.ToString(),
				IpAddress = GetClientIpAddress(context),
				UserAgent = request.Headers["User-Agent"].ToString(),
				UserId = context.User?.Identity?.Name ?? "Anonymous",
				IsAuthenticated = context.User?.Identity?.IsAuthenticated ?? false
			};

			_logger.LogInformation(
				"Incoming Request: {Method} {Path}{QueryString} from {IpAddress} | RequestId: {RequestId} | User: {UserId} | Authenticated: {IsAuthenticated}",
				logData.Method, logData.Path, logData.QueryString, logData.IpAddress,
				logData.RequestId, logData.UserId, logData.IsAuthenticated);

			// Logowanie body dla POST/PUT (z wyłączeniem haseł)
			if (request.Method == "POST" || request.Method == "PUT")
			{
				if (request.ContentLength > 0 && request.ContentLength < 10000)
				{
					request.EnableBuffering();
					var body = await new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true).ReadToEndAsync();
					request.Body.Position = 0;

					// Nie loguj haseł
					if (!request.Path.ToString().Contains("login", StringComparison.OrdinalIgnoreCase) &&
						!request.Path.ToString().Contains("register", StringComparison.OrdinalIgnoreCase))
					{
						_logger.LogDebug("Request Body: {Body}", body);
					}
				}
			}
		}

		private async Task LogResponse(HttpContext context, string requestId, double elapsedMs)
		{
			var response = context.Response;

			var logData = new
			{
				RequestId = requestId,
				StatusCode = response.StatusCode,
				ElapsedMs = elapsedMs,
				ContentType = response.ContentType
			};

			var logLevel = response.StatusCode >= 500 ? LogLevel.Error :
						   response.StatusCode >= 400 ? LogLevel.Warning :
						   LogLevel.Information;

			_logger.Log(logLevel,
				"Response: {StatusCode} | {ElapsedMs}ms | RequestId: {RequestId}",
				logData.StatusCode, logData.ElapsedMs, logData.RequestId);

			// Loguj powolne żądania (> 5 sekund)
			if (elapsedMs > 5000)
			{
				_logger.LogWarning(
					"Slow Request Detected: {Method} {Path} took {ElapsedMs}ms | RequestId: {RequestId}",
					context.Request.Method, context.Request.Path, elapsedMs, requestId);
			}
		}

		private string GetClientIpAddress(HttpContext context)
		{
			// Sprawdź X-Forwarded-For (dla proxy/load balancer)
			var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
			if (!string.IsNullOrEmpty(forwardedFor))
			{
				return forwardedFor.Split(',')[0].Trim();
			}

			// Sprawdź X-Real-IP
			var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
			if (!string.IsNullOrEmpty(realIp))
			{
				return realIp;
			}

			// Domyślnie użyj RemoteIpAddress
			return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
		}

		private static double GetElapsedMilliseconds(long start, long stop)
		{
			return (stop - start) * 1000.0 / Stopwatch.Frequency;
		}
	}
}
