using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MJ.Module.SoundConverter.AudioFormats;
using MJ.Module.SoundConverter.ConversionScheduler;
using MudBlazor;
using MudBlazor.Services;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace MJ.BlazorComponent.SoundConverter.Test;

public sealed class ComponentTest : TestContext
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IConversionScheduler _conversionScheduler;
    private readonly IRenderedComponent<SoundConverterComponent> _cut;

    public ComponentTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _conversionScheduler = Substitute.For<IConversionScheduler>();
        Services.AddMudServices();
        Services.AddSingleton(_conversionScheduler);
        JSInterop.Mode = JSRuntimeMode.Loose;
        _cut = RenderComponent<SoundConverterComponent>();
    }

    [Fact]
    public async Task Should_HaveDownloadableFile_When_StartedUploadedFileAndClickedConvert()
    {
        // ARRANGE
        // Setup mocks.
        var browserFile = Substitute.For<IBrowserFile>();
        const string fileName = "test.mp3";
        browserFile.Name.ReturnsForAnyArgs(fileName);
        browserFile.ContentType.ReturnsForAnyArgs("audio/mpeg");
        browserFile.Size.ReturnsForAnyArgs(512);
        browserFile.OpenReadStream().ReturnsForAnyArgs(new MemoryStream("MJ"u8.ToArray()));

        var outputAudio = Substitute.For<IAudio>();
        outputAudio.AudioStream.ReturnsForAnyArgs(new MemoryStream("MJM"u8.ToArray()));
        outputAudio.Extension.ReturnsForAnyArgs(".cool");
        var guid = Guid.NewGuid();
        _conversionScheduler.AddToQueue(Arg.Any<IBrowserFile>()).ReturnsForAnyArgs(new ConvertedAudio { Guid = guid });

        // Fetch components.
        var fileUploadC = _cut.FindComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>();
        var convertC = _cut.FindComponents<MudButton>()
            .Single(x => x.Instance.StartIcon == Icons.Material.Filled.DoubleArrow);

        // Initial state check.
        _cut.FindComponents<MudTd>().Count(x => x.Instance.DataLabel == "Download").Should().Be(5);

        // Simulate file uploaded.
        await fileUploadC.InvokeAsync(() => fileUploadC.Instance.FilesChanged.InvokeAsync([browserFile]));
        await fileUploadC.InvokeAsync(() => fileUploadC.Instance.Validate());
        await Task.Delay(TimeSpan.FromSeconds(1));

        convertC.Instance.Disabled.Should().BeFalse();

        await fileUploadC.InvokeAsync(() => convertC.Instance.OnClick.InvokeAsync());
        _cut.FindComponents<MudTd>().Count(x => x.Instance.DataLabel == "Download").Should().Be(1);
        _cut.FindComponents<MudIconButton>().Single(x => x.Instance.Icon == Icons.Material.Filled.ArrowDownward)
            .Instance.Disabled
            .Should()
            .BeTrue();

        // Simulate conversion finished.
        var convertedAudio = new ConvertedAudio
            { Guid = guid, Progress = 100, IsFinished = true, OutputAudio = outputAudio };
        _conversionScheduler.OnProgressAsync +=
            Raise.Event<Func<ConvertedAudio, Task>?>(convertedAudio);
        await Task.Delay(TimeSpan.FromSeconds(1));
        _cut.FindComponent<MudTableBase>().Instance.Loading.Should().BeFalse();

        var downloadButtonC = _cut.FindComponents<MudIconButton>()
            .Single(x => x.Instance.Icon == Icons.Material.Filled.ArrowDownward);

        using var streamRef = new DotNetStreamReference(outputAudio.AudioStream);
        var newFileName = $"{Path.GetFileNameWithoutExtension(fileName)} - converted.{outputAudio.Extension}";

        // ACT
        // Initiate download.
        await fileUploadC.InvokeAsync(() => downloadButtonC.Instance.OnClick.InvokeAsync());

        // ASSERT
        downloadButtonC.Instance.Disabled.Should().BeFalse();
        var jsRuntimeInvocation = JSInterop.VerifyInvoke("jsDownloadFileFromStream");
        jsRuntimeInvocation.Arguments.Count.Should().Be(2);
        jsRuntimeInvocation.Arguments[0].Should().Be(newFileName);
        jsRuntimeInvocation.Arguments[1].Should().BeEquivalentTo(streamRef);
    }
}