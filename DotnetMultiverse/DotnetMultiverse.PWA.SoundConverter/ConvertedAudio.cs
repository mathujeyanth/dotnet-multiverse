using System;
using DotnetMultiverse.Shared.SoundConverter.AudioFormats;

namespace DotnetMultiverse.PWA.SoundConverter;

public record ConvertedAudio
{
    public required Guid Guid { get; init; } 
    public string? From { get; set; }
    public required string To { get; set; }
    public required string FileName { get; init; }
    public double Progress { get; set; }
    public bool IsFinished { get; set; }
    public IAudio? OutputAudio { get; set; }
}