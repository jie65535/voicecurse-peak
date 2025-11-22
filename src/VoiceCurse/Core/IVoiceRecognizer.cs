using System;

namespace VoiceCurse.Core;

public interface IVoiceRecognizer : IDisposable {
    event Action<string> OnPhraseRecognized;
    void FeedAudio(short[] pcmData, int length);

    void Start();
    void Stop();
}