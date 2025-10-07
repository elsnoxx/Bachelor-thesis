using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BioFeedbackGenerator
{
    public class PlayerSimulator
    {
        private readonly Player _player;
        private readonly HubConnection _connection;
        private readonly Random _random = new Random();

        public PlayerSimulator(Player player, string hubUrl)
        {
            _player = player;
            _connection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .Build();

            _connection.On<string, double>("ReceiveBioData", (playerId, value) =>
            {
                Console.WriteLine($"{_player.Name} received: {playerId} -> {value}");
            });
        }

        public async Task StartAsync()
        {
            await _connection.StartAsync();
            Console.WriteLine($"{_player.Name} connected");

            await _connection.InvokeAsync("JoinGame", _player.Id, _player.GameType);

            // spustit generování dat
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        double value = 50 + _random.NextDouble() * 50;
                        _player.CurrentValue = value;
                        await _connection.InvokeAsync("SendBioData", _player.Id, _player.GameType, value);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending data: {ex.Message}");
                    }
                    await Task.Delay(1000);
                }
            });
        }
    }
}
