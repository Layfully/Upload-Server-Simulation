using System.Reactive.Linq;
using System.Reactive.Subjects;
using static WspółbieżneBlazor.UploadQueue;

namespace WspółbieżneBlazor;

public sealed class UploadServer : IDisposable
{
    private readonly Subject<List<UploadQueue>> _uploadQueuesSubject = new();
    private readonly Subject<List<Client>> _clientQueueSubject = new();
    private readonly List<UploadQueue> _uploadQueues = [];
    private readonly ClientComparer _clientComparer = new();
    private readonly List<Client> _clientQueue = [];
    private readonly Lock _lock = new();
    private bool _disposed;

    public UploadServer()
    {
        _uploadQueues.AddRange(Enumerable.Range(0, 5).Select(_ => new UploadQueue()));

        foreach (UploadQueue uploadQueue in _uploadQueues)
        {
            uploadQueue.FinishedUpload += OnUploadFinished;
            uploadQueue.ProgressChanged += OnProgressChanged;
        }

        _uploadQueuesSubject.OnNext(_uploadQueues);
        _clientQueueSubject.OnNext([]);
    }

    private void OnUploadFinished(object? sender, UploadFinishedEventArgs e)
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            ProcessPendingUploads();
            _uploadQueuesSubject.OnNext(_uploadQueues);
        }
    }

    private void OnProgressChanged(object? sender, float progress)
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            _uploadQueuesSubject.OnNext(_uploadQueues);
        }
    }

    public void AddClient(Client client)
    {
        ArgumentNullException.ThrowIfNull(client);

        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(UploadServer));

            Console.WriteLine("Adding Client");
            _clientQueue.Add(client);
            client.StartTime = DateTime.Now;

            UpdatePriorityScoresInternal();
            ProcessPendingUploads();

            _uploadQueuesSubject.OnNext(_uploadQueues);
            _clientQueueSubject.OnNext(_clientQueue.ToList());
        }
    }

    public void RemoveClient(Client client)
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            bool removed = _clientQueue.Remove(client);
            if (!removed)
            {
                return;
            }

            Console.WriteLine($"Removed client {client.Id}");
            UpdatePriorityScoresInternal();
            _clientQueueSubject.OnNext(_clientQueue.ToList());
        }
    }

    private void ProcessPendingUploads()
    {
        // Must be called within lock
        foreach (UploadQueue uploadQueue in _uploadQueues.Where(q => !q.IsUploading))
        {
            Client? client = _clientQueue.FirstOrDefault();
            if (client is null)
            {
                break;
            }

            uploadQueue.UploadFile(client, this);
        }
    }

    private void UpdatePriorityScoresInternal()
    {
        // Must be called within lock
        _clientComparer.ClientsWaiting = _clientQueue.Count;
        _clientQueue.Sort(_clientComparer);
    }

    public IObservable<List<Client>> GetClientQueue() =>
        _clientQueueSubject
            .AsObservable()
            .StartWith(_clientQueue.ToList());

    public IObservable<List<UploadQueue>> GetUploadQueues() =>
        _uploadQueuesSubject
            .AsObservable()
            .StartWith(_uploadQueues);

    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
        }

        foreach (UploadQueue uploadQueue in _uploadQueues)
        {
            uploadQueue.FinishedUpload -= OnUploadFinished;
            uploadQueue.ProgressChanged -= OnProgressChanged;
            uploadQueue.Dispose();
        }

        _uploadQueuesSubject.Dispose();
        _clientQueueSubject.Dispose();
    }
}
