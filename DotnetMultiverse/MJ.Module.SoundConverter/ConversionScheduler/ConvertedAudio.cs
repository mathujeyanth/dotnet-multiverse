using System;
using MJ.Module.SoundConverter.AudioFormats;

namespace MJ.Module.SoundConverter.ConversionScheduler;

public record ConvertedAudio
{
    public required Guid Guid { get; init; }
    public double Progress { get; set; }
    public bool IsFinished { get; set; }
    public IAudio? OutputAudio { get; set; }
}