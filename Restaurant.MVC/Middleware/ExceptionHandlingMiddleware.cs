using System.Net;
using System.Text.Json;

namespace Restaurant.MVC.Middleware
{
	public class ExceptionHandlingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ExceptionHandlingMiddleware> _logger;
		private readonly IWebHostEnvironment _environment;

		public ExceptionHandlingMiddleware(
			RequestDelegate next,
			ILogger<ExceptionHandlingMiddleware> logger,
			IWebHostEnvironment environment)
		{
			_next = next;
			_logger = logger;
			_environment = environment;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (UnauthorizedAccessException ex)
			{
				await HandleExceptionAsync(context, ex, HttpStatusCode.Forbidden);
			}
			catch (InvalidOperationException ex)
			{
				await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest);
			}
			catch (KeyNotFoundException ex)
			{
				await HandleExceptionAsync(context, ex, HttpStatusCode.NotFound);
			}
			catch (Exception ex)
			{
				await HandleExceptionAsync(context, ex, HttpStatusCode.InternalServerError);
			}
		}

		private async Task HandleExceptionAsync(HttpContext context, Exception exception, HttpStatusCode statusCode)
		{
			var requestId = context.TraceIdentifier;
			var userId = context.User?.Identity?.Name ?? "Anonymous";
			var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
			var path = context.Request.Path;
			var method = context.Request.Method;

			// Logowanie szczegółów błędu
			_logger.LogError(exception,
				"Unhandled Exception | RequestId: {RequestId} | User: {UserId} | IP: {IpAddress} | {Method} {Path} | StatusCode: {StatusCode}",
				requestId, userId, ipAddress, method, path, (int)statusCode);

			// Tworzenie odpowiedzi
			context.Response.ContentType = "application/json";
			context.Response.StatusCode = (int)statusCode;

			var errorResponse = new ErrorResponse
			{
				RequestId = requestId,
				StatusCode = (int)statusCode,
				Message = GetUserFriendlyMessage(statusCode, exception),
				Timestamp = DateTime.UtcNow
			};

			// W środowisku deweloperskim dodaj szczegóły
			if (_environment.IsDevelopment())
			{
				errorResponse.DeveloperMessage = exception.Message;
				errorResponse.StackTrace = exception.StackTrace;
			}

			var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				WriteIndented = true
			});

			await context.Response.WriteAsync(json);
		}

		private string GetUserFriendlyMessage(HttpStatusCode statusCode, Exception exception)
		{
			return statusCode switch
			{
				HttpStatusCode.Forbidden => "Nie masz uprawnień do wykonania tej operacji.",
				HttpStatusCode.NotFound => "Nie znaleziono żądanego zasobu.",
				HttpStatusCode.BadRequest => exception.Message,
				HttpStatusCode.InternalServerError => "Wystąpił nieoczekiwany błąd. Spróbuj ponownie później.",
				_ => "Wystąpił błąd podczas przetwarzania żądania."
			};
		}

		private class ErrorResponse
		{
			public string RequestId { get; set; } = default!;
			public int StatusCode { get; set; }
			public string Message { get; set; } = default!;
			public DateTime Timestamp { get; set; }
			public string? DeveloperMessage { get; set; }
			public string? StackTrace { get; set; }
		}
	}
}
