namespace server.Models.DTO
{
    public record RoomStatusMessage
    {
        public Guid RoomId { get; init; }
        public RoomStatus Status { get; init; }
        public RoomStatusMessage(Guid roomId, RoomStatus status)
        {
            RoomId = roomId;
            Status = status;
        }
    }
}
