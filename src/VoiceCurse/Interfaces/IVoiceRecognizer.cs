using System;

namespace VoiceCurse.Interfaces;

public interface IVoiceRecognizer : IDisposable {
    event Action<string> OnPhraseRecognized;
    event Action<string> OnPartialResult;
        
    void FeedAudio(short[] pcmData, int length);
    void Start();
    void Stop();
}