using System;
using Photon.Voice;
using VoiceCurse.Interfaces;

namespace VoiceCurse.Voice;

public class VoiceProcessor(IVoiceRecognizer recognizer, int inputSampleRate) : IProcessor<float> {
    private const int TargetSampleRate = 16000;

    public float[]? Process(float[]? buf) {
        if (buf == null || buf.Length == 0) return buf;
        
        if (Math.Abs(inputSampleRate - TargetSampleRate) < 100) {
            ProcessDirect(buf);
            return buf;
        }
        
        float ratio = (float)inputSampleRate / TargetSampleRate;
        int targetLength = (int)(buf.Length / ratio);
        
        if (targetLength == 0) return buf;

        short[] pcmBuffer = new short[targetLength];

        for (int i = 0; i < targetLength; i++) {
            float sourceIndex = i * ratio;
            int index = (int)sourceIndex;
            float frac = sourceIndex - index;

            if (index >= buf.Length - 1) {
                pcmBuffer[i] = FloatToShort(buf[^1]);
                continue;
            }

            float sample1 = buf[index];
            float sample2 = buf[index + 1];
            float interpolated = sample1 + (sample2 - sample1) * frac;

            pcmBuffer[i] = FloatToShort(interpolated);
        }
        
        recognizer.FeedAudio(pcmBuffer, pcmBuffer.Length);
        return buf;
    }

    private void ProcessDirect(float[] buf) {
        short[] pcmBuffer = new short[buf.Length];
        for (int i = 0; i < buf.Length; i++) {
            pcmBuffer[i] = FloatToShort(buf[i]);
        }
        recognizer.FeedAudio(pcmBuffer, pcmBuffer.Length);
    }

    private static short FloatToShort(float sample) {
        sample = sample switch {
            > 1.0f => 1.0f,
            < -1.0f => -1.0f,
            _ => sample
        };
        
        return (short)(sample * 32767);
    }

    public void Dispose() { }
}