using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using MJ.Module.AudioConverter.IAudioCreators;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace MJ.Module.AudioConverter.Test;

#pragma warning disable CS9113
public class TestAudioHandler(ITestOutputHelper testOutputHelper)
#pragma warning restore CS9113
{
    [Theory]
    [InlineData("test_chanel=stereo_sample-rate=44100kHz_bitrate=128kbps.mp3")]
    [InlineData("test_channel=mono.mp3")]
    public async Task Should_ConvertToOgg_When_PassingValidMp3File(string fileName)
    {
        // ARRANGE
        var audioHandler = new AudioHandler.AudioHandler(new Mp3IAudioCreator());
        var browserFile = Substitute.For<IBrowserFile>();

        var file = await File.ReadAllBytesAsync($"Resources/{fileName}");
        browserFile.Name.Returns("test.mp3");
        browserFile.ContentType.Returns("audio/mpeg");
        browserFile.Size.Returns(file.Length);
        browserFile.OpenReadStream().ReturnsForAnyArgs(new MemoryStream(file));
        var audio = await audioHandler.ValidateAndCreateAudio(browserFile);
        var progress = Substitute.For<IProgress<double>>();

        // ACT
        var convertedAudio = await audioHandler.ToOgg(audio, progress);

        // ASSERT
        convertedAudio.AudioStream.Should().NotBeNull();
        convertedAudio.Extension.Should().Be("ogg");
        convertedAudio.Duration.Should().BeCloseTo(audio.Duration, TimeSpan.FromSeconds(1));
    }
}