using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using MJ.Module.SoundConverter.AudioFormats;
using MJ.Module.SoundConverter.IAudioCreators;

namespace MJ.Module.SoundConverter.AudioHandler;

public class AudioHandler(Mp3IAudioCreator mp3IAudioCreator) : IAudioHandler
{
    private const long MaxFileSize = 2L * 1024L * 1024L * 1024L; // 2GB
    public async Task<IAudio> ValidateAndCreateAudio(IBrowserFile browserFile)
    {
        await using var audioStream = browserFile.OpenReadStream(maxAllowedSize: MaxFileSize);
        var newAudioStream = new MemoryStream(new byte[audioStream.Length]);
        await audioStream.CopyToAsync(newAudioStream);
        return browserFile.ContentType switch
        {
            "audio/mpeg" => await mp3IAudioCreator.TryCreateAudio(newAudioStream),
            _ => throw new ArgumentOutOfRangeException(browserFile.ContentType)
        };
    }

    public async Task<IAudio> ToOgg(IAudio audio, IProgress<double> progress, CancellationToken cancellationToken = default)
    {
        return audio switch
        {
            Mp3Audio mp3Audio => await mp3Audio.Mp3ToOgg(progress, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(audio), audio, null)
        };
    }
}