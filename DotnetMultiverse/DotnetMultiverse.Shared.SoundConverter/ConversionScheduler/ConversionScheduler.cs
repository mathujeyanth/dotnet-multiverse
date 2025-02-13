using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using DotnetMultiverse.Shared.SoundConverter.AudioFormats;
using Microsoft.AspNetCore.Components.Forms;

namespace DotnetMultiverse.Shared.SoundConverter.ConversionScheduler;

public class ConversionScheduler(AudioHandler audioHandler) : IConversionScheduler
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
            if (!_concurrentQueue.TryDequeue(out var result))
            {
                throw new ApplicationException("Queue is empty");
            }
            Console.WriteLine($"Converting {result.file.Name}");
            var inputAudio = await audioHandler.ValidateAndCreateAudio(result.file);
            
            var progressHandler = new Progress<double>(progress =>
            {
                try
                {
                    UpdateProgress(result.guid, progress);
                }
                catch (ApplicationException e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("In progress handler \n\n");
                    cancellationTokenSource.Cancel();
                }
            });
            try
            {
                var audio = await audioHandler.ToOgg(inputAudio, progressHandler, cancellationTokenSource.Token);
                UpdateProgress(result.guid, 1, true, audio);
                Console.WriteLine($"Converted {result.file.Name}");
            }
            catch (Exception e) when (e is ApplicationException or OperationCanceledException)
            {
                Console.WriteLine("In converter\n\n");
                Console.WriteLine($"Failed converting {result.file.Name}");
                Console.WriteLine(e);
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
        for (int i = 0; i < maxThreadCount; i++)
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
                    Console.WriteLine("Exiting");
                    Console.WriteLine(e);
                }
                finally
                {
                    _threadCount--;
                }

            }).Start();
        }
    }
}