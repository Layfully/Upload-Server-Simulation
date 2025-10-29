namespace WspółbieżneBlazor;

public sealed class ClientComparer : IComparer<Client>
{
    public int ClientsWaiting { get; set; }

    public int Compare(Client? x, Client? y) => (x, y) switch
    {
        (null, null) => 0,
        (null, _) => -1,
        (_, null) => 1,
        _ => CalculatePriority(y).CompareTo(CalculatePriority(x))
    };

    private double CalculatePriority(Client client)
    {
        double t = client.TimeWaiting.TotalSeconds;
        int s = client.Files[0];
        int k = ClientsWaiting;
        double priority = t * t + k / s;
        client.PriorityScore = priority;
        return priority;
    }
}
