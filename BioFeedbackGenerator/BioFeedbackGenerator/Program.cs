using Microsoft.AspNetCore.SignalR.Client;

namespace BioFeedbackGenerator
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string port = "";
            Console.WriteLine("Enter the port number for the SignalR hub (default is 5000): ");
            port = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(port))
            {
                port = "5000";
            }
            string hubUrl = $"http://localhost:{port}/gamehub";
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
