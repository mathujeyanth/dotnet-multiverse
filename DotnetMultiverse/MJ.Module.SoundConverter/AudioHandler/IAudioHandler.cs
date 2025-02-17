using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using MJ.Module.SoundConverter.AudioFormats;

namespace MJ.Module.SoundConverter.AudioHandler;

public interface IAudioHandler
{
    public Task<IAudio> ValidateAndCreateAudio(IBrowserFile browserFile);

    public Task<IAudio> ToOgg(IAudio audio, IProgress<double> progress, CancellationToken cancellationToken = default);
}