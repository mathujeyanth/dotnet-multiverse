using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using DotnetMultiverse.Shared.SoundConverter;
using Microsoft.AspNetCore.Components.Forms;

namespace DotnetMultiverse.PWA.SoundConverter;

public class ConversionScheduler(AudioHandler audioHandler) : IConversionScheduler
{
    public event Func<Task> OnChangeAsync = null!;
    private Thread? _thread;
        
    private readonly ConcurrentQueue<(IBrowserFile file, ConvertedAudio audio)> _concurrentQueue = new();

    public ConvertedAudio AddToQueue(IBrowserFile file, string convertToFormat)
    {
        var convertedAudio = new ConvertedAudio
        {
            Guid = Guid.NewGuid(),
            To = convertToFormat,
            FileName = file.Name,
            OutputAudio = null
        };
        _concurrentQueue.Enqueue((file, convertedAudio));
        return convertedAudio;
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
            result.audio.From = inputAudio.Extension;
            var progressHandler = new Progress<double>(progress =>
            {
                result.audio.Progress = progress;
                OnChangeAsync.Invoke();
            });
                    
            result.audio.OutputAudio = await audioHandler.ToOgg(inputAudio, progressHandler);
            result.audio.Progress = 1;
            result.audio.IsFinished = true;
            _ = OnChangeAsync.Invoke();
        }
    }

    public void StartConverting()
    {
        if (_thread is not null && _thread.IsAlive) return;
        _thread = new Thread(async void () => await Converter());
        _thread.Start();
    }
}