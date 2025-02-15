using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MJ.Module.SoundConverter.AudioFormats;
using NAudio.Wave;
using NLayer.NAudioSupport;
using NVorbis;
using OggVorbisEncoder;

namespace MJ.Module.SoundConverter.AudioHandlers;

internal static class Mp3ToOggConverter
{
    private const int WriteBufferSize = 1024 * 1024;

    public static async Task<OggAudio> Mp3ToOgg(this Mp3Audio mp3Audio,
        IProgress<double> progress, CancellationToken cancellationToken = default)
    {
        mp3Audio.AudioStream.Seek(0, SeekOrigin.Begin);
        await using var mp3FileReaderBase = new Mp3FileReaderBase(mp3Audio.AudioStream, wf => new Mp3FrameDecompressor(wf));
        var outputStream = new MemoryStream();
        var pcmChannels = mp3FileReaderBase.WaveFormat.Channels;
        var pcmSampleRate = mp3FileReaderBase.WaveFormat.SampleRate;
        var outputChannels = pcmChannels;
        var outputSampleRate = pcmSampleRate;

        // if (pcmChannels != outputChannels || pcmSampleRate != outputSampleRate) throw new Exception();

        InitOggStream(outputSampleRate, outputChannels, out var oggStream, out var processingState);

        var pcmSampleSize = mp3FileReaderBase.WaveFormat.BitsPerSample / 8; // == 4

        var status = 0;

        var pcm = new byte[WriteBufferSize];

        var outSamples = new float[outputChannels][];
        var numOutputSamples = WriteBufferSize / pcmSampleSize / pcmChannels;
        for (var ch = 0; ch < outputChannels; ch++)
        {
            outSamples[ch] = new float[numOutputSamples];
        }

        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
            var chunkSize = mp3FileReaderBase.Read(pcm, 0, WriteBufferSize);

            status += chunkSize;
            progress.Report((double) status / mp3FileReaderBase.Length);

            if (chunkSize <= 0) break;

            for (var sampleNumber = 0; sampleNumber < numOutputSamples; sampleNumber++)
            {
                var sampleIndex = sampleNumber * pcmChannels * pcmSampleSize;

                outSamples[0][sampleNumber] = BitConverter.ToSingle(pcm, sampleIndex);
                outSamples[1][sampleNumber] = BitConverter.ToSingle(pcm, sampleIndex + pcmSampleSize);
            }

            await FlushPages(oggStream, outputStream, false, cancellationToken);
            ProcessChunk(outSamples, processingState, oggStream, numOutputSamples, cancellationToken);
        }

        await FlushPages(oggStream, outputStream, true, cancellationToken);
        
        using var vorbis = new VorbisReader(outputStream, closeOnDispose: false);
        return new OggAudio
        {
            AudioStream = outputStream,
            Duration = vorbis.TotalTime,
            SampleRate = vorbis.SampleRate
        };
    }

    private static async Task FlushPages(OggStream oggStream, Stream output, bool force, CancellationToken cancellationToken = default)
    {
        while (oggStream.PageOut(out var page, force))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
            await output.WriteAsync(page.Header.AsMemory(0, page.Header.Length), cancellationToken);
            await output.WriteAsync(page.Body.AsMemory(0, page.Body.Length), cancellationToken);
        }
    }

    private static void InitOggStream(int sampleRate, int channels, out OggStream oggStream,
        out ProcessingState processingState)
    {
        // Stores all the static vorbis bitstream settings
        var info = VorbisInfo.InitVariableBitRate(channels, sampleRate, 0.5f);

        // set up our packet->stream encoder
        var serial = new Random().Next();
        oggStream = new OggStream(serial);

        // Header.
        var infoPacket = HeaderPacketBuilder.BuildInfoPacket(info);
        var commentsPacket = HeaderPacketBuilder.BuildCommentsPacket(new Comments());
        var booksPacket = HeaderPacketBuilder.BuildBooksPacket(info);

        oggStream.PacketIn(infoPacket);
        oggStream.PacketIn(commentsPacket);
        oggStream.PacketIn(booksPacket);

        // Audio body.
        processingState = ProcessingState.Create(info);
    }

    private static void ProcessChunk(float[][] floatSamples, ProcessingState processingState, OggStream oggStream,
        int writeBufferSize, CancellationToken cancellationToken = default)
    {
        processingState.WriteData(floatSamples, writeBufferSize);

        while (!oggStream.Finished && processingState.PacketOut(out var packet))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
            oggStream.PacketIn(packet);
        }
    }
}