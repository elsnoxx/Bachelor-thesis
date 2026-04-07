namespace server.Services.GameServices
{
    public interface IGameService
    {
        object ProcessInput(string roomId, string playerId, double value);
    }
}
