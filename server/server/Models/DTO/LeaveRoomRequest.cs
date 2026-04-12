using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request object used when a player intentionally leaves a room.
/// </summary>
public class LeaveRoomRequest
{
    [Required]
    [EmailAddress]
    public string UserEmail { get; set; }
}