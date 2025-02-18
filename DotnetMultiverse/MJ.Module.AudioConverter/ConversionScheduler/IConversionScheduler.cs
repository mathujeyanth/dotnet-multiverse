using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;

namespace MJ.Module.AudioConverter.ConversionScheduler;

public interface IConversionScheduler
{
    public ConvertedAudio AddToQueue(IBrowserFile file);
    public void StartConverting();
    public event Func<ConvertedAudio, Task>? OnProgressAsync;
}