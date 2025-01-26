using System.IO;
using System.Threading.Tasks;
using DotnetMultiverse.SoundConverter.AudioFormats;

namespace DotnetMultiverse.SoundConverter.AudioHandlers;

public interface IAudioHandler
{
    public Task<IAudio> TryCreateAudio(Stream audioStream);
}