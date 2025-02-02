using System;
using System.IO;
using System.Threading.Tasks;
using DotnetMultiverse.Shared.SoundConverter.AudioFormats;
using DotnetMultiverse.Shared.SoundConverter.AudioHandlers;
using Microsoft.AspNetCore.Components.Forms;

namespace DotnetMultiverse.Shared.SoundConverter;

public class AudioHandler(Mp3Handler mp3Handler)
{
    private const long MaxFileSize = 2L * 1024L * 1024L * 1024L; // 2GB
    public async Task<IAudio> ValidateAndCreateAudio(IBrowserFile browserFile)
    {
        await using var audioStream = browserFile.OpenReadStream(maxAllowedSize: MaxFileSize);
        var newAudioStream = new MemoryStream(new byte[audioStream.Length]);
        await audioStream.CopyToAsync(newAudioStream);
        return browserFile.ContentType switch
        {
            "audio/mpeg" => await mp3Handler.TryCreateAudio(newAudioStream),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public async Task<IAudio> ToOgg(IAudio audio, IProgress<double> progress)
    {
        return audio switch
        {
            Mp3Audio mp3Audio => await mp3Audio.Mp3ToOgg(progress),
            _ => throw new ArgumentOutOfRangeException(nameof(audio), audio, null)
        };
    }
}