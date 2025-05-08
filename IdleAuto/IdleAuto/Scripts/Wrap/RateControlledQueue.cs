using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public class RateControlledQueue<T>
{
    private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
    private readonly SemaphoreSlim _semaphore;
    private readonly int _itemsPerInterval;
    private readonly TimeSpan _interval;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private bool _isProcessing;

    /// <summary>
    /// 创建一个频率可控队列
    /// </summary>
    /// <param name="itemsPerInterval">每个时间间隔内处理的最大项目数</param>
    /// <param name="interval">时间间隔</param>
    public RateControlledQueue(int itemsPerInterval, TimeSpan interval)
    {
        if (itemsPerInterval <= 0)
            throw new ArgumentOutOfRangeException(nameof(itemsPerInterval), "必须大于0");

        _itemsPerInterval = itemsPerInterval;
        _interval = interval;
        _semaphore = new SemaphoreSlim(itemsPerInterval, itemsPerInterval);
    }

    /// <summary>
    /// 将项目加入队列
    /// </summary>
    public void Enqueue(T item)
    {
        _queue.Enqueue(item);
        if (!_isProcessing)
        {
            StartProcessing();
        }
    }

    /// <summary>
    /// 开始处理队列中的项目
    /// </summary>
    private void StartProcessing()
    {
        _isProcessing = true;
        Task.Run(async () =>
        {
            while (!_cts.IsCancellationRequested && !_queue.IsEmpty)
            {
                await _semaphore.WaitAsync(_cts.Token);

                if (_queue.TryDequeue(out var item))
                {
                    // 触发处理事件
                    ItemProcessed?.Invoke(this, item);
                }

                // 在间隔时间后释放信号量
                _ = Task.Delay(_interval, _cts.Token)
                    .ContinueWith(_ => _semaphore.Release(), _cts.Token);
            }
            _isProcessing = false;
        }, _cts.Token);
    }

    /// <summary>
    /// 停止处理队列
    /// </summary>
    public void Stop()
    {
        _cts.Cancel();
    }

    /// <summary>
    /// 当项目被处理时触发的事件
    /// </summary>
    public event EventHandler<T> ItemProcessed;

    /// <summary>
    /// 队列中剩余的项目数
    /// </summary>
    public int Count => _queue.Count;
}
