using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TGS.Apis.BackendOffice.Workers
{
    public interface IStationLetterTaskQueue
    {
        ValueTask QueueStationLetterAsync(
            StationLetterQueueModel model);

        ValueTask<StationLetterQueueModel> DequeueAsync(
            CancellationToken cancellationToken);
    }

    public class StationLetterTaskQueue : IStationLetterTaskQueue
    {
        private readonly Channel<StationLetterQueueModel> _queue;

        public StationLetterTaskQueue(int capacity)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel
                .CreateBounded<StationLetterQueueModel>(options);
        }

        public async ValueTask QueueStationLetterAsync(
            StationLetterQueueModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            await _queue.Writer.WriteAsync(model);
        }

        public async ValueTask<StationLetterQueueModel>
            DequeueAsync(
                CancellationToken cancellationToken)
        {
            var workItem = await _queue.Reader.ReadAsync(cancellationToken);

            return workItem;
        }
    }
}