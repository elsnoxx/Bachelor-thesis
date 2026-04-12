namespace server.Models.DTO
{
    /// <summary>
    /// Represents a real-time data packet sent via SignalR from the biofeedback sensor.
    /// Used for low-latency synchronization of physiological data during gameplay.
    /// </summary>
    public class BioFeedbackMessage
    {
        public string Email { get; set; }
        public Guid RoomId { get; set; }

        /// <summary>
        /// Current GSR/EDA value measured by the hardware.
        /// </summary>
        public float Value { get; set; }

        public BioFeedbackMessage(string email, Guid roomId, float value)
        {
            Email = email;
            RoomId = roomId;
            Value = value;
        }
    }
}