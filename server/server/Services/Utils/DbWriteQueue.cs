using server.Models.DTO;
using System.Threading.Channels;

namespace server.Services.Utils
{
    /// <summary>
    /// A thread-safe, asynchronous messaging queue based on System.Threading.Channels.
    /// It decouples real-time game logic from database persistence by buffering 
    /// telemetry data, game results, and room status updates.
    /// </summary>
    public class DbWriteQueue
    {
        // Buffered channel for high-frequency biofeedback (telemetry) data
        private readonly Channel<BioFeedbackMessage> _bioQueue;

        // Unbounded channel for final game outcomes and statistics
        private readonly Channel<GameResultContext> _resultQueue;

        // Unbounded channel for orchestrating game room life cycle (Start/Finish)
        private readonly Channel<RoomStatusMessage> _roomStatusQueue = Channel.CreateUnbounded<RoomStatusMessage>();

        public DbWriteQueue()
        {
            // Bounded to 10k items to prevent memory exhaustion during extreme load
            _bioQueue = Channel.CreateBounded<BioFeedbackMessage>(10000);

            // Unbounded since these events are infrequent compared to telemetry
            _resultQueue = Channel.CreateUnbounded<GameResultContext>();
        }

        /// <summary> Queues biofeedback data points for background persistence. </summary>
        public ValueTask QueueBioFeedbackAsync(BioFeedbackMessage item) => _bioQueue.Writer.WriteAsync(item);

        /// <summary> Queues final game results and session summaries. </summary>
        public ValueTask QueueGameResultAsync(GameResultContext item) => _resultQueue.Writer.WriteAsync(item);

        /// <summary> Queues a room status update (e.g., setting a room to InProgress or Finished). </summary>
        public void QueueRoomStatus(RoomStatusMessage item) => _roomStatusQueue.Writer.TryWrite(item);

        // Methods used by the Background Worker (DbWriterWorker) to consume data

        /// <summary> Provides an asynchronous stream of all queued biofeedback messages. </summary>
        public IAsyncEnumerable<BioFeedbackMessage> ReadBioAllAsync(CancellationToken ct) => _bioQueue.Reader.ReadAllAsync(ct);

        /// <summary> Provides an asynchronous stream of all queued game results. </summary>
        public IAsyncEnumerable<GameResultContext> ReadResultsAllAsync(CancellationToken ct) => _resultQueue.Reader.ReadAllAsync(ct);

        /// <summary> Provides an asynchronous stream of room status updates. </summary>
        public IAsyncEnumerable<RoomStatusMessage> ReadRoomStatusAllAsync(CancellationToken ct) => _roomStatusQueue.Reader.ReadAllAsync(ct);
    }
}