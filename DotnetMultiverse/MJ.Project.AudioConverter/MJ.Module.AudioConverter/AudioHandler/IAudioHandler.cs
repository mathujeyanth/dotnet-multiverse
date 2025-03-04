using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using MJ.Module.AudioConverter.AudioFormats;

namespace MJ.Module.AudioConverter.AudioHandler;

public interface IAudioHandler
{
    public Task<IAudio> ValidateAndCreateAudio(IBrowserFile browserFile);

    public Task<IAudio> ToOgg(IAudio audio, IProgress<double> progress, CancellationToken cancellationToken = default);
}