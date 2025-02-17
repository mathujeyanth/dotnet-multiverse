using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using MJ.Module.SoundConverter.AudioFormats;
using MJ.Module.SoundConverter.AudioHandler;

namespace MJ.Module.SoundConverter.ConversionScheduler;

public class ConversionScheduler(IAudioHandler audioHandler, ILogger<ConversionScheduler> logger)
    : IConversionScheduler
{
    public event Func<ConvertedAudio, Task>? OnProgressAsync;
    private const int MaxThreads = 3;
    private int _threadCount;

    private readonly ConcurrentQueue<(Guid guid, IBrowserFile file)> _concurrentQueue = new();

    public ConvertedAudio AddToQueue(IBrowserFile file)
    {
        var guid = Guid.NewGuid();
        _concurrentQueue.Enqueue((guid, file));
        return new ConvertedAudio
        {
            Guid = guid
        };
    }

    private async Task Converter(CancellationTokenSource cancellationTokenSource)
    {
        while (!_concurrentQueue.IsEmpty)
        {
            if (!_concurrentQueue.TryDequeue(out var result)) throw new ApplicationException("Queue is empty");

            logger.LogInformation($"Converting {result.file.Name}");
            var inputAudio = await audioHandler.ValidateAndCreateAudio(result.file);

            var progressHandler = new Progress<double>(progress =>
            {
                try
                {
                    UpdateProgress(result.guid, progress);
                }
                catch (ApplicationException e)
                {
                    logger.LogError(e, "Error in progress handler");
                    cancellationTokenSource.Cancel();
                }
            });
            try
            {
                var audio = await audioHandler.ToOgg(inputAudio, progressHandler, cancellationTokenSource.Token);
                UpdateProgress(result.guid, 1, true, audio);
                logger.LogInformation($"Converted {result.file.Name}");
            }
            catch (Exception e) when (e is ApplicationException or OperationCanceledException)
            {
                logger.LogError(e, $"Failed converting {result.file.Name}");
                return;
            }
        }
    }

    private void UpdateProgress(Guid guid, double progress, bool isFinished = false, IAudio? audio = null)
    {
        if (OnProgressAsync is null) throw new ApplicationException("OnProgressAsync is null");
        OnProgressAsync.Invoke(new ConvertedAudio
        {
            Guid = guid,
            Progress = progress,
            IsFinished = isFinished,
            OutputAudio = audio
        });
    }

    public void StartConverting()
    {
        if (_threadCount >= MaxThreads) return;
        var maxThreadCount = int.Min(_concurrentQueue.Count, MaxThreads);
        for (var i = 0; i < maxThreadCount; i++)
        {
            _threadCount++;
            new Thread(async void () =>
            {
                try
                {
                    using var tokenSource = new CancellationTokenSource();
                    await Converter(tokenSource);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Terminating thread due to error");
                }
                finally
                {
                    _threadCount--;
                }
            }).Start();
        }
    }
}