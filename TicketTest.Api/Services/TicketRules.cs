namespace TicketTest.Api.Services;

public static class TicketRules
{
    public static readonly IReadOnlySet<string> AllowedStatuses =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Open",
            "InProgress",
            "Resolved",
            "Closed"
        };

    public static readonly IReadOnlySet<string> AllowedPriorities =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Low",
            "Medium",
            "High",
            "Critical"
        };

    public static bool IsValidStatus(string status) =>
        AllowedStatuses.Contains(status);

    public static bool IsValidPriority(string priority) =>
        AllowedPriorities.Contains(priority);

    public static bool IsValidCreateStatus(string status) =>
        status.Equals("Open", StringComparison.OrdinalIgnoreCase) ||
        status.Equals("InProgress", StringComparison.OrdinalIgnoreCase);

    public static bool RequiresAssignee(string priority) =>
        priority.Equals("Critical", StringComparison.OrdinalIgnoreCase);

    public static bool CanTransition(string currentStatus, string newStatus)
    {
        if (currentStatus.Equals(newStatus, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return currentStatus.ToLowerInvariant() switch
        {
            "open" =>
                newStatus.Equals("InProgress", StringComparison.OrdinalIgnoreCase) ||
                newStatus.Equals("Resolved", StringComparison.OrdinalIgnoreCase),

            "inprogress" =>
                newStatus.Equals("Open", StringComparison.OrdinalIgnoreCase) ||
                newStatus.Equals("Resolved", StringComparison.OrdinalIgnoreCase),

            "resolved" =>
                newStatus.Equals("InProgress", StringComparison.OrdinalIgnoreCase) ||
                newStatus.Equals("Closed", StringComparison.OrdinalIgnoreCase),

            "closed" => false,

            _ => false
        };
    }
}