using Domain.Entities;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
	public interface ISecurityMonitoringService
	{
		Task LogLoginAttemptAsync(string email, bool isSuccessful, string? errorMessage = null);
		Task<bool> IsAccountLockedAsync(string email);
		Task<bool> IsSuspiciousActivityAsync(string userId);
		Task<SecurityThreatLevel> AssessThreatLevelAsync(string ipAddress);
	}

	public enum SecurityThreatLevel
	{
		None,
		Low,
		Medium,
		High,
		Critical
	}

	public class SecurityMonitoringService : ISecurityMonitoringService
	{
		private readonly RestaurantDbContext _context;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ILogger<SecurityMonitoringService> _logger;
		private readonly IAuditLogService _auditLogService;

		// Konfiguracja progów
		private const int MaxFailedLoginAttempts = 5;
		private const int FailedLoginWindowMinutes = 15;
		private const int SuspiciousActivityThreshold = 10;
		private const int RapidRequestsThreshold = 50;

		public SecurityMonitoringService(
			RestaurantDbContext context,
			IHttpContextAccessor httpContextAccessor,
			ILogger<SecurityMonitoringService> logger,
			IAuditLogService auditLogService)
		{
			_context = context;
			_httpContextAccessor = httpContextAccessor;
			_logger = logger;
			_auditLogService = auditLogService;
		}

		public async Task LogLoginAttemptAsync(string email, bool isSuccessful, string? errorMessage = null)
		{
			var ipAddress = GetIpAddress();

			await _auditLogService.LogAsync(
				action: isSuccessful ? "Login.Success" : "Login.Failed",
				entityType: "Authentication",
				entityId: email,
				oldValues: null,
				newValues: new { Email = email, IpAddress = ipAddress },
				isSuccessful: isSuccessful,
				errorMessage: errorMessage);

			if (!isSuccessful)
			{
				var failedAttempts = await GetRecentFailedLoginAttemptsAsync(email);

				if (failedAttempts >= MaxFailedLoginAttempts)
				{
					_logger.LogWarning(
						"SECURITY ALERT: Account lockout - {Email} has {Count} failed login attempts from {IpAddress}",
						email, failedAttempts, ipAddress);

					// Opcjonalnie: wysłanie emaila, powiadomienia do admina
				}
			}
		}

		public async Task<bool> IsAccountLockedAsync(string email)
		{
			var failedAttempts = await GetRecentFailedLoginAttemptsAsync(email);
			return failedAttempts >= MaxFailedLoginAttempts;
		}

		public async Task<bool> IsSuspiciousActivityAsync(string userId)
		{
			var since = DateTime.UtcNow.AddHours(-1);

			// Sprawdź liczbę akcji w ostatniej godzinie
			var activityCount = await _context.AuditLogs
				.Where(a => a.UserId == userId && a.Timestamp >= since)
				.CountAsync();

			// Sprawdź nieudane operacje
			var failedAttempts = await _context.AuditLogs
				.Where(a => a.UserId == userId && !a.IsSuccessful && a.Timestamp >= since)
				.CountAsync();

			// Podejrzane jeśli > 10 nieudanych operacji lub > 50 operacji w ciągu godziny
			var isSuspicious = failedAttempts > SuspiciousActivityThreshold ||
							   activityCount > RapidRequestsThreshold;

			if (isSuspicious)
			{
				_logger.LogWarning(
					"SECURITY ALERT: Suspicious activity detected for user {UserId} - {ActivityCount} actions, {FailedCount} failures in last hour",
					userId, activityCount, failedAttempts);
			}

			return isSuspicious;
		}

		public async Task<SecurityThreatLevel> AssessThreatLevelAsync(string ipAddress)
		{
			var since = DateTime.UtcNow.AddMinutes(-30);

			// Zlicz aktywność z tego IP
			var ipActivity = await _context.AuditLogs
				.Where(a => a.IpAddress == ipAddress && a.Timestamp >= since)
				.GroupBy(a => a.UserId)
				.Select(g => new
				{
					UserId = g.Key,
					TotalActions = g.Count(),
					FailedActions = g.Count(a => !a.IsSuccessful)
				})
				.ToListAsync();

			var totalActions = ipActivity.Sum(a => a.TotalActions);
			var totalFailures = ipActivity.Sum(a => a.FailedActions);
			var uniqueUsers = ipActivity.Count;

			// Ocena poziomu zagrożenia
			SecurityThreatLevel threatLevel;

			if (totalFailures > 20 || totalActions > 200)
			{
				threatLevel = SecurityThreatLevel.Critical;
			}
			else if (totalFailures > 10 || totalActions > 100 || uniqueUsers > 10)
			{
				threatLevel = SecurityThreatLevel.High;
			}
			else if (totalFailures > 5 || totalActions > 50)
			{
				threatLevel = SecurityThreatLevel.Medium;
			}
			else if (totalFailures > 2 || totalActions > 20)
			{
				threatLevel = SecurityThreatLevel.Low;
			}
			else
			{
				threatLevel = SecurityThreatLevel.None;
			}

			if (threatLevel >= SecurityThreatLevel.High)
			{
				_logger.LogWarning(
					"SECURITY ALERT: {ThreatLevel} threat level from IP {IpAddress} - {TotalActions} actions, {TotalFailures} failures, {UniqueUsers} users",
					threatLevel, ipAddress, totalActions, totalFailures, uniqueUsers);
			}

			return threatLevel;
		}

		private async Task<int> GetRecentFailedLoginAttemptsAsync(string email)
		{
			var since = DateTime.UtcNow.AddMinutes(-FailedLoginWindowMinutes);

			return await _context.AuditLogs
				.Where(a =>
					a.EntityId == email &&
					a.Action == "Login.Failed" &&
					a.Timestamp >= since)
				.CountAsync();
		}

		private string GetIpAddress()
		{
			var context = _httpContextAccessor.HttpContext;
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
