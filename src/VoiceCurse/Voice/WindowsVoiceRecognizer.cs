using System;
using UnityEngine;
using UnityEngine.Windows.Speech;
using VoiceCurse.Interfaces;

namespace VoiceCurse.Voice;

public class WindowsVoiceRecognizer : IVoiceRecognizer {
    private DictationRecognizer? _dictationRecognizer;

    public event Action<string>? OnPhraseRecognized;
    public event Action<string>? OnPartialResult;

    public WindowsVoiceRecognizer() {
        if (Application.platform != RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.WindowsEditor) {
            throw new PlatformNotSupportedException("Windows Speech is only available on Windows.");
        }
    }

    public void Start() {
        if (_dictationRecognizer != null) return;

        try {
            _dictationRecognizer = new DictationRecognizer();
            
            _dictationRecognizer.DictationResult += (text, _) => {
                OnPhraseRecognized?.Invoke(text);
            };
            
            _dictationRecognizer.DictationHypothesis += text => {
                OnPartialResult?.Invoke(text);
            };

            _dictationRecognizer.DictationError += (error, hresult) => {
                Debug.LogError($"[VoiceCurse] Windows Speech Error: {error} (Code: {hresult})");
            };

            _dictationRecognizer.Start();
            Debug.Log("[VoiceCurse] Windows Native Speech Initialized.");
        }
        catch (Exception e) {
            Debug.LogError($"[VoiceCurse] Failed to start Windows Speech: {e.Message}");
        }
    }

    public void FeedAudio(short[] pcmData, int length) { }

    public void Stop() {
        if (_dictationRecognizer is { Status: SpeechSystemStatus.Running }) {
            _dictationRecognizer.Stop();
        }
    }

    public void Dispose() {
        Stop();
        if (_dictationRecognizer == null) return;
        _dictationRecognizer.Dispose();
        _dictationRecognizer = null;
    }
}