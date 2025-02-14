using System.IO;
using System.Threading.Tasks;
using Module.SoundConverter.AudioFormats;

namespace Module.SoundConverter.AudioHandlers;

public interface IAudioHandler
{
    public Task<IAudio> TryCreateAudio(Stream audioStream);
}