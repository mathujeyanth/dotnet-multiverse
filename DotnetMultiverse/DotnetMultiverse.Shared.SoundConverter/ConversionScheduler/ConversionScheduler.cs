using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;

namespace DotnetMultiverse.Shared.SoundConverter.ConversionScheduler;

public class ConversionScheduler(AudioHandler audioHandler) : IConversionScheduler
{
    public event Func<ConvertedAudio, Task> OnProgressAsync = null!;
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

    private async Task Converter()
    {
        while (!_concurrentQueue.IsEmpty)
        {
            if (!_concurrentQueue.TryDequeue(out var result))
            {
                throw new Exception();
            }

            var inputAudio = await audioHandler.ValidateAndCreateAudio(result.file);
            var progressHandler = new Progress<double>(progress =>
            {
                OnProgressAsync.Invoke(new ConvertedAudio
                {
                    Guid = result.guid,
                    Progress = progress,
                    IsFinished = false
                });
            });
                    
            var audio = await audioHandler.ToOgg(inputAudio, progressHandler);
            _ = OnProgressAsync.Invoke(new ConvertedAudio
            {
                Guid = result.guid,
                Progress = 1,
                IsFinished = true,
                OutputAudio = audio
            });
        }

        _threadCount--;
    }

    public void StartConverting()
    {
        if (_threadCount >= MaxThreads) return;
        var maxThreadCount = int.Min(_concurrentQueue.Count, MaxThreads);
        for (int i = 0; i < maxThreadCount; i++)
        {
            _threadCount++;
            new Thread(async void () => await Converter()).Start();
        }
    }
}