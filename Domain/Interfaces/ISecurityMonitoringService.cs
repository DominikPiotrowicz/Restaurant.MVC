namespace Domain.Interfaces
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
}
