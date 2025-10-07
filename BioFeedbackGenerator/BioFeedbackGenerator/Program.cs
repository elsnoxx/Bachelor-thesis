using Microsoft.AspNetCore.SignalR.Client;

namespace BioFeedbackGenerator
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string hubUrl = "https://localhost:7202/gamehub";

            var manager = new ClientManager(hubUrl);

            // přidat simulované hráče
            manager.AddPlayer("player1", "Alice", "RelaxRace");
            manager.AddPlayer("player2", "Bob", "RelaxRace");
            manager.AddPlayer("player3", "Charlie", "RelaxRace");

            // spustit všechny
            await manager.StartAllAsync();

            // zabránit ukončení aplikace
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
