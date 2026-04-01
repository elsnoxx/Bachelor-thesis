namespace server.Models.DTO
{
    public class BioFeedbackMessage
    {
        public string email { get; set; }
        public Guid roomId { get; set; }
        public float value { get; set; }
        public BioFeedbackMessage(string email, Guid roomId, float value)
        {
            this.email = email;
            this.roomId = roomId;
            this.value = value;
        }
    }
}
