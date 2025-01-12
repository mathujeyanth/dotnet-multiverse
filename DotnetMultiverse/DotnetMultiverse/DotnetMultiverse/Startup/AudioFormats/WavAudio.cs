namespace DotnetMultiverse.Startup.AudioFormats;

public record WavAudio : IAudio
{
    public required Stream AudioStream { get; init; }
    public required TimeSpan Duration { get; init; }
    public required int SampleRate { get; init; }
    public void Dispose()
    {
        AudioStream.Dispose();
    }
}