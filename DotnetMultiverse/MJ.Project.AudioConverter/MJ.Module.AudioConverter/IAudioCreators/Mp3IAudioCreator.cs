using System.IO;
using System.Threading.Tasks;
using MJ.Module.AudioConverter.AudioFormats;
using NAudio.Wave;
using NLayer.NAudioSupport;

namespace MJ.Module.AudioConverter.IAudioCreators;

public class Mp3IAudioCreator : IAudioCreator
{
    public async Task<IAudio> TryCreateAudio(Stream audioStream)
    {
        audioStream.Seek(0, SeekOrigin.Begin);
        await using var mp3FileReaderBase = new Mp3FileReaderBase(audioStream, wf => new Mp3FrameDecompressor(wf));
        return new Mp3Audio
        {
            AudioStream = audioStream,
            Duration = mp3FileReaderBase.TotalTime,
            SampleRate = mp3FileReaderBase.WaveFormat.SampleRate
        };
    }
}