namespace WspółbieżneBlazor;

public sealed class Simulation : IDisposable
{
    private readonly UploadServer _uploadServer;
    private long _newClientId = 1;
    private CancellationTokenSource? _cancellationTokenSource;

    public Simulation(UploadServer server)
    {
        _uploadServer = server;
    }

    public Task Start()
    {
        if (_cancellationTokenSource is not null)
        {
            return Task.CompletedTask;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = _cancellationTokenSource.Token;

        Task.Run(() => SimulationLoop(token), token);

        return Task.CompletedTask;
    }

    private async Task SimulationLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            int randomInterval = Random.Shared.Next(200, 2000);
            await Task.Delay(randomInterval, token);

            if (token.IsCancellationRequested)
            {
                break;
            }

            CreateClient();
        }
    }

    public Task Stop()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;

        return Task.CompletedTask;
    }

    private void CreateClient()
    {
        int fileCount = Random.Shared.Next(3, 10);
        var files = Enumerable.Range(0, fileCount)
                              .Select(_ => Random.Shared.Next(1, 100))
                              .OrderBy(x => x)
                              .ToList();

        Client newClient = new()
        {
            Id = _newClientId++,
            Files = files,
            StartTime = DateTime.Now
        };

        _uploadServer.AddClient(newClient);
    }

    public void Dispose()
    {
        Stop();
    }
}
