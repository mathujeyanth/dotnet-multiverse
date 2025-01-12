using DotnetMultiverse.Startup.AudioFormats;

namespace DotnetMultiverse.Startup.AudioHandlers;

public interface IAudioHandler
{
    public Task<IAudio> TryCreateAudio(Stream audioStream);
}