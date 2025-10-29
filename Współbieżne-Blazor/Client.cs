namespace WspółbieżneBlazor;

public sealed class Client
{
    public required long Id { get; init; }
    public DateTime? StartTime { get; set; }
    public required List<int> Files { get; init; }
    public double PriorityScore { get; set; }
    public TimeSpan TimeWaiting => StartTime.HasValue ? DateTime.Now - StartTime.Value : TimeSpan.Zero;
}
