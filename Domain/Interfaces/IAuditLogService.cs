using Domain.Entities;

namespace Domain.Interfaces
{
	public interface IAuditLogService
	{
		Task LogAsync(string action, string entityType, string? entityId, object? oldValues, object? newValues, bool isSuccessful = true, string? errorMessage = null);
		Task<IEnumerable<AuditLog>> GetUserActivityAsync(string userId, int count = 50);
		Task<IEnumerable<AuditLog>> GetEntityHistoryAsync(string entityType, string entityId);
		Task<IEnumerable<AuditLog>> GetFailedAttemptsAsync(string userId, DateTime since);
	}
}
