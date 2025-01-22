namespace DotnetMultiverse.Startup.AudioFormats;

public interface IAudio : IDisposable
{
    public Stream AudioStream { get; init; }
    public TimeSpan Duration { get; init; }
    public int SampleRate { get; init; }
    public string Extension { get; }
}