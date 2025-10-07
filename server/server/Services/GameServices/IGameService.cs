namespace server.Services.GameServices
{
    public interface IGameService
    {
        void ProcessInput(string playerId, double value);
        Dictionary<string, double> GetScores();
    }
}
