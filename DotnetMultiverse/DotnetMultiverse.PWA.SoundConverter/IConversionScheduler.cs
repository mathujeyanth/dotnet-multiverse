using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;

namespace DotnetMultiverse.PWA.SoundConverter;

public interface IConversionScheduler
{
    public ConvertedAudio AddToQueue(IBrowserFile file, string convertToFormat);
    public void StartConverting();
    public event Func<Task> OnChangeAsync;
}