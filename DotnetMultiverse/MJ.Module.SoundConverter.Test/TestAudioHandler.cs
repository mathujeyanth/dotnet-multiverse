using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using MJ.Module.SoundConverter.IAudioCreators;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace MJ.Module.SoundConverter.Test;

#pragma warning disable CS9113
public class TestAudioHandler(ITestOutputHelper testOutputHelper)
#pragma warning restore CS9113
{
    [Fact]
    public async Task Should_ConvertToOgg_When_PassingValidMp3File()
    {
        // ARRANGE
        var audioHandler = new AudioHandler.AudioHandler(new Mp3IAudioCreator());
        var browserFile = Substitute.For<IBrowserFile>();
        // Sample rate : 44.100 kHz
        // Channels : 2
        // Bit rate : 128 kbps
        // TODO: Test with others. Mono does not work I think.
        var file = await File.ReadAllBytesAsync("Resources/test.mp3");
        browserFile.Name.Returns("test.mp3");
        browserFile.ContentType.Returns("audio/mpeg");
        browserFile.Size.Returns(file.Length);
        browserFile.OpenReadStream().ReturnsForAnyArgs(new MemoryStream(file));
        var audio = await audioHandler.ValidateAndCreateAudio(browserFile);

        var progress = Substitute.For<IProgress<double>>();

        // ACT
        var convertedAudio = await audioHandler.ToOgg(audio, progress);

        // ASSERT
        convertedAudio.Extension.Should().Be("ogg");
        convertedAudio.Duration.Should().BeCloseTo(audio.Duration, TimeSpan.FromSeconds(3));
    }
}