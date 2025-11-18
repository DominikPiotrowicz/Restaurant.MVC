namespace Domain.Entities
{
	public class AuditLog
	{
		public int Id { get; set; }
		public string UserId { get; set; } = default!;
		public string? UserEmail { get; set; }
		public string Action { get; set; } = default!;
		public string EntityType { get; set; } = default!;
		public string? EntityId { get; set; }
		public string? OldValues { get; set; }
		public string? NewValues { get; set; }
		public string IpAddress { get; set; } = default!;
		public string? UserAgent { get; set; }
		public DateTime Timestamp { get; set; }
		public bool IsSuccessful { get; set; }
		public string? ErrorMessage { get; set; }
	}
}
