using System;
using Module.SoundConverter.AudioFormats;

namespace Module.SoundConverter.ConversionScheduler;

public record ConvertedAudio
{
    public required Guid Guid { get; init; } 
    public double Progress { get; set; }
    public bool IsFinished { get; set; }
    public IAudio? OutputAudio { get; set; }
}