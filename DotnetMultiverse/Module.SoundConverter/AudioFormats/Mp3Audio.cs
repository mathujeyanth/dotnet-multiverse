using System;
using System.IO;

namespace Module.SoundConverter.AudioFormats;

public record Mp3Audio : IAudio
{
    public required Stream AudioStream { get; init; }
    public required TimeSpan Duration { get; init; }
    public required int SampleRate { get; init; }
    public string Extension => "mp3";
    public void Dispose()
    {
        AudioStream.Dispose();
    }
}