using System.IO;
using System.Threading.Tasks;
using MJ.Module.SoundConverter.AudioFormats;

namespace MJ.Module.SoundConverter.IAudioCreators;

public interface IAudioCreator
{
    public Task<IAudio> TryCreateAudio(Stream audioStream);
}