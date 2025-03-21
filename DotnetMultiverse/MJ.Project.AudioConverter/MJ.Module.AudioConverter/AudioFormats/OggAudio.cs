using System;
using System.IO;

namespace MJ.Module.AudioConverter.AudioFormats;

public record OggAudio : IAudio
{
    public required Stream AudioStream { get; init; }
    public required TimeSpan Duration { get; init; }
    public required int SampleRate { get; init; }
    public string Extension => "ogg";

    public void Dispose()
    {
        AudioStream.Dispose();
    }
}