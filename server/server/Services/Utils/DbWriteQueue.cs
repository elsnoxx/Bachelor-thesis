using server.Models.DTO;
using System.Threading.Channels;

namespace server.Services.Utils
{
    public class DbWriteQueue
    {
        private readonly Channel<BioFeedbackMessage> _bioQueue;
        private readonly Channel<GameResultContext> _resultQueue;

        public DbWriteQueue()
        {
            _bioQueue = Channel.CreateBounded<BioFeedbackMessage>(10000);
            // Menší kapacita, výsledky nechodí tak často
            _resultQueue = Channel.CreateUnbounded<GameResultContext>();
        }

        public ValueTask QueueBioFeedbackAsync(BioFeedbackMessage item) => _bioQueue.Writer.WriteAsync(item);
        public ValueTask QueueGameResultAsync(GameResultContext item) => _resultQueue.Writer.WriteAsync(item);

        public IAsyncEnumerable<BioFeedbackMessage> ReadBioAllAsync(CancellationToken ct) => _bioQueue.Reader.ReadAllAsync(ct);
        public IAsyncEnumerable<GameResultContext> ReadResultsAllAsync(CancellationToken ct) => _resultQueue.Reader.ReadAllAsync(ct);
    }
}
