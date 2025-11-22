using UnityEngine;
using VoiceCurse.Core;

namespace VoiceCurse.Audio;

public class AudioStreamTapper : MonoBehaviour {
    private IVoiceRecognizer? _recognizer;
    private bool _isActive;

    public void Initialize(IVoiceRecognizer recognizer) {
        _recognizer = recognizer;
        _isActive = true;
    }

    private void OnAudioFilterRead(float[] data, int channels) {
        if (!_isActive) return;

        int samples = data.Length / channels;
        short[] pcmBuffer = new short[samples];

        for (int i = 0; i < samples; i++) {
            float sample = 0;

            for (int c = 0; c < channels; c++) {
                sample += data[i * channels + c];
            }
            sample /= channels;

            sample = sample switch {
                > 1.0f => 1.0f,
                < -1.0f => -1.0f,
                _ => sample
            };

            pcmBuffer[i] = (short)(sample * 32767);
        }

        _recognizer?.FeedAudio(pcmBuffer, pcmBuffer.Length);
    }
}