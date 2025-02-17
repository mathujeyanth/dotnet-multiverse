using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using MJ.Module.SoundConverter.AudioFormats;
using MJ.Module.SoundConverter.AudioHandler;
using MJ.Module.SoundConverter.ConversionScheduler;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace MJ.Module.SoundConverter.Test;

public class TestConversionScheduler(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task Should_QueueAndFinishedConverting3Audios_When_Passing10AudiosWithLargeDelayAfterThird()
    {
        // ARRANGE
        var audioHandler = Substitute.For<IAudioHandler>();
        var logger = Substitute.For<ILogger<ConversionScheduler.ConversionScheduler>>();
        var conversionScheduler = new ConversionScheduler.ConversionScheduler(audioHandler, logger);
        var convertedAudioDict = new ConcurrentDictionary<Guid, ConvertedAudio>();

        Task Update(ConvertedAudio convertedAudio)
        {
            convertedAudioDict[convertedAudio.Guid] = convertedAudio;
            return Task.CompletedTask;
        }

        conversionScheduler.OnProgressAsync += Update;

        const int maxFiles = 5;
        const int maxAllowedFiles = 3;
        var guids = Enumerable.Range(0, maxFiles).Select(_ => Guid.NewGuid()).ToList();
        var files = guids.Select(x => Substitute.For<IBrowserFile>()).ToList();
        var allowedGuids = guids.Take(maxAllowedFiles).ToList();
        foreach (var (guid, file) in guids.Zip(files))
        {
            file.Name.ReturnsForAnyArgs(guid.ToString());
            if (allowedGuids.Contains(guid))
            {
                var audioResult = Substitute.For<IAudio>();
                audioResult.Extension.Returns(guid.ToString());
                audioHandler
                    .ValidateAndCreateAudio(
                        Arg.Is<IBrowserFile>(x => allowedGuids.Contains(Guid.Parse(x.Name))))
                    .Returns(Task.FromResult(audioResult));
                audioHandler
                    .ToOgg(
                        Arg.Is<IAudio>(x => x.Extension == guid.ToString()),
                        Arg.Any<Progress<double>>())
                    .Returns(Task.FromResult(audioResult));
                continue;
            }

            async Task<IAudio> LargeDelay()
            {
                await Task.Delay(TimeSpan.MaxValue);
                throw new Exception("Should never get here.");
            }

            audioHandler
                .ValidateAndCreateAudio(
                    Arg.Is<IBrowserFile>(x => !allowedGuids.Contains(Guid.Parse(x.Name))))
                .Returns(LargeDelay());
        }

        // ACT
        foreach (var file in files) conversionScheduler.AddToQueue(file);

        conversionScheduler.StartConverting();
        await Task.Delay(TimeSpan.FromSeconds(1));

        // ASSERT
        convertedAudioDict.Values.Count.Should().Be(maxAllowedFiles);
        convertedAudioDict.Values.Should().OnlyContain(x => x.IsFinished);
    }
}