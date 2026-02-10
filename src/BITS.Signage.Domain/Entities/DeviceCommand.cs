namespace BITS.Signage.Domain.Entities;

/// <summary>
/// Represents a command sent to a device for remote operation.
/// Examples: reboot, clear cache, fetch manifest, screenshot, update app, etc.
/// Tracks the status and result of the command.
/// </summary>
public class DeviceCommand
{
    public string CommandId { get; set; } = null!;
    public string DeviceId { get; set; } = null!;
    public string Type { get; set; } = null!; // REBOOT, CLEAR_CACHE, FETCH_MANIFEST, SCREENSHOT, UPDATE_APP, etc
    public string? ParametersJson { get; set; } // Command-specific parameters (JSON)
    public string Status { get; set; } = "PENDING"; // PENDING, QUEUED, ACKNOWLEDGED, IN_PROGRESS, COMPLETED, FAILED, TIMEOUT
    public string? IssuedByUserId { get; set; } // Who issued the command (nullable for system commands)
    public DateTime IssuedAtUtc { get; set; }
    public DateTime? AcknowledgedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime? TimeoutAtUtc { get; set; }
    public string? ResultJson { get; set; } // Command result (JSON)
    public string? ErrorMessage { get; set; } // Error details if failed
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    // Navigation properties
    public Device Device { get; set; } = null!;
    public User? IssuedByUser { get; set; }
}
