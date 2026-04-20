using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using System.Text.RegularExpressions;

namespace server.Hubs
{
    /// <summary>
    /// Jednoduchý Echo Hub pro testování přímé komunikace mezi klientem a serverem.
    /// Ignoruje logiku místností a pouze vrací přijatá data odesílateli.
    /// </summary>
    public class TestHub : Hub
    {
        public async Task PingSensorData(double value)
        {
            // Logujeme přijetí dat pro kontrolu v konzoli serveru
            Log.Information("TestHub: Přijata data od {Id}: {Val}", Context.ConnectionId, value);

            // Odešleme data okamžitě zpět pouze volajícímu klientovi (Caller)
            await Clients.Caller.SendAsync("PongSensorData", new
            {
                ReceivedValue = value,
                ServerTimestamp = DateTime.UtcNow
            });
        }
    }
}
