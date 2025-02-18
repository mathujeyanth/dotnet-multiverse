using System;
using MJ.Module.AudioConverter.AudioFormats;

namespace MJ.Module.AudioConverter.ConversionScheduler;

public record ConvertedAudio
{
    public required Guid Guid { get; init; }
    public double Progress { get; set; }
    public bool IsFinished { get; set; }
    public IAudio? OutputAudio { get; set; }
}