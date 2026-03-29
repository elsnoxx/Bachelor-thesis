using server.Models.DB;
using System.Text.Json.Serialization;

public class BioFeedback
{
    public int Id { get; set; }

    // Cizí klíče (vždy velká písmena pro konzistenci)
    public Guid GameRoomId { get; set; }
    public Guid UserId { get; set; }

    // Navigační vlastnosti (přejmenujte na velká písmena)
    [JsonIgnore]
    public User User { get; set; } = null!;

    [JsonIgnore]
    public GameRoom GameRoom { get; set; } = null!;

    public float GsrValue { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}