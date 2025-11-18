using Domain.Entities;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Infrastructure.Services
{
	public interface IAuditLogService
	{
		Task LogAsync(string action, string entityType, string? entityId, object? oldValues, object? newValues, bool isSuccessful = true, string? errorMessage = null);
		Task<IEnumerable<AuditLog>> GetUserActivityAsync(string userId, int count = 50);
		Task<IEnumerable<AuditLog>> GetEntityHistoryAsync(string entityType, string entityId);
		Task<IEnumerable<AuditLog>> GetFailedAttemptsAsync(string userId, DateTime since);
	}

	public class AuditLogService : IAuditLogService
	{
		private readonly RestaurantDbContext _context;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ILogger<AuditLogService> _logger;

		public AuditLogService(
			RestaurantDbContext context,
			IHttpContextAccessor httpContextAccessor,
			ILogger<AuditLogService> logger)
		{
			_context = context;
			_httpContextAccessor = httpContextAccessor;
			_logger = logger;
		}

		public async Task LogAsync(
			string action,
			string entityType,
			string? entityId,
			object? oldValues,
			object? newValues,
			bool isSuccessful = true,
			string? errorMessage = null)
		{
			try
			{
				var httpContext = _httpContextAccessor.HttpContext;
				var userId = httpContext?.User?.Identity?.Name ?? "System";
				var ipAddress = GetIpAddress(httpContext);
				var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

				var auditLog = new AuditLog
				{
					UserId = userId,
					UserEmail = httpContext?.User?.Identity?.Name,
					Action = action,
					EntityType = entityType,
					EntityId = entityId,
					OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
					NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
					IpAddress = ipAddress,
					UserAgent = userAgent,
					Timestamp = DateTime.UtcNow,
					IsSuccessful = isSuccessful,
					ErrorMessage = errorMessage
				};

				_context.AuditLogs.Add(auditLog);
				await _context.SaveChangesAsync();

				_logger.LogInformation(
					"Audit Log: {Action} on {EntityType}({EntityId}) by {UserId} from {IpAddress} - {Status}",
					action, entityType, entityId, userId, ipAddress, isSuccessful ? "Success" : "Failed");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to write audit log for action {Action} on {EntityType}", action, entityType);
			}
		}

		public async Task<IEnumerable<AuditLog>> GetUserActivityAsync(string userId, int count = 50)
		{
			return await Task.Run(() =>
				_context.AuditLogs
					.Where(a => a.UserId == userId)
					.OrderByDescending(a => a.Timestamp)
					.Take(count)
					.ToList());
		}

		public async Task<IEnumerable<AuditLog>> GetEntityHistoryAsync(string entityType, string entityId)
		{
			return await Task.Run(() =>
				_context.AuditLogs
					.Where(a => a.EntityType == entityType && a.EntityId == entityId)
					.OrderByDescending(a => a.Timestamp)
					.ToList());
		}

		public async Task<IEnumerable<AuditLog>> GetFailedAttemptsAsync(string userId, DateTime since)
		{
			return await Task.Run(() =>
				_context.AuditLogs
					.Where(a => a.UserId == userId && !a.IsSuccessful && a.Timestamp >= since)
					.OrderByDescending(a => a.Timestamp)
					.ToList());
		}

		private string GetIpAddress(HttpContext? context)
		{
			if (context == null) return "Unknown";

			var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
			if (!string.IsNullOrEmpty(forwardedFor))
			{
				return forwardedFor.Split(',')[0].Trim();
			}

			return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
		}
	}
}
