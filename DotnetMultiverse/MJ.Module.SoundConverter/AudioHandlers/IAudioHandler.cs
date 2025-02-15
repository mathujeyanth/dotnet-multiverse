using System.IO;
using System.Threading.Tasks;
using MJ.Module.SoundConverter.AudioFormats;

namespace MJ.Module.SoundConverter.AudioHandlers;

public interface IAudioHandler
{
    public Task<IAudio> TryCreateAudio(Stream audioStream);
}