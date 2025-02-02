using System.IO;
using System.Threading.Tasks;
using DotnetMultiverse.Shared.SoundConverter.AudioFormats;

namespace DotnetMultiverse.Shared.SoundConverter.AudioHandlers;

public interface IAudioHandler
{
    public Task<IAudio> TryCreateAudio(Stream audioStream);
}