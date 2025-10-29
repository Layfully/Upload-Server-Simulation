namespace WspółbieżneBlazor;

public sealed class UploadQueue : IDisposable
{
    private readonly Lock _lock = new();
    private Timer? _uploadTimer;
    private int _totalTicks;
    private int _currentTick;
    private long _currentClientId;
    private int _currentFileSize;
    private bool _disposed;

    private const int TimerIntervalMs = 100;

    public int FileSize
    {
        get { lock (_lock) return _currentFileSize; }
    }

    public long ClientId
    {
        get { lock (_lock) return _currentClientId; }
    }

    public float Progress { get; private set; }

    public bool IsUploading { get; private set; }

    public event EventHandler<UploadFinishedEventArgs>? FinishedUpload;
    public event EventHandler<float>? ProgressChanged;

    public void UploadFile(Client client, UploadServer server)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(server);

        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(UploadQueue));

            if (IsUploading)
            {
                return;
            }

            if (client.Files.Count == 0)
            {
                return;
            }

            IsUploading = true;
            _currentFileSize = client.Files[0];
            _currentClientId = client.Id;
            _totalTicks = _currentFileSize;
            _currentTick = 0;
            Progress = 0;

            client.StartTime ??= DateTime.Now;

            client.Files.RemoveAt(0);

            if (client.Files.Count == 0)
            {
                server.RemoveClient(client);
            }

            _uploadTimer?.Dispose();
            _uploadTimer = new Timer(UploadCallback, null, 0, TimerIntervalMs);
        }
    }

    private void UploadCallback(object? state)
    {
        bool uploadFinished = false;
        long finishedClientId = -1;
        int finishedFileSize = -1;

        lock (_lock)
        {
            if (_disposed || !IsUploading)
            {
                _uploadTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                return;
            }

            _currentTick++;
            Progress = _totalTicks > 0 ? (float)_currentTick / _totalTicks : 1.0f;

            if (_currentTick >= _totalTicks)
            {
                uploadFinished = true;
                finishedClientId = _currentClientId;
                finishedFileSize = _currentFileSize;

                _uploadTimer?.Change(Timeout.Infinite, Timeout.Infinite);

                Progress = 0;
                _currentClientId = -1;
                _currentFileSize = -1;
                IsUploading = false;
            }
        }

        if (uploadFinished)
        {
            FinishedUpload?.Invoke(this, new UploadFinishedEventArgs(finishedClientId, finishedFileSize));
        }
        else
        {
            ProgressChanged?.Invoke(this, Progress);
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _uploadTimer?.Dispose();
            _uploadTimer = null;
        }
    }

    public sealed class UploadFinishedEventArgs(long clientId, int fileSize) : EventArgs
    {
        public long ClientId { get; init; } = clientId;
        public int FileSize { get; init; } = fileSize;
    }
}
