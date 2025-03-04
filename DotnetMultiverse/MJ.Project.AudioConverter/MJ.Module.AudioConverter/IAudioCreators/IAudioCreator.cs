using System.IO;
using System.Threading.Tasks;
using MJ.Module.AudioConverter.AudioFormats;

namespace MJ.Module.AudioConverter.IAudioCreators;

public interface IAudioCreator
{
    public Task<IAudio> TryCreateAudio(Stream audioStream);
}