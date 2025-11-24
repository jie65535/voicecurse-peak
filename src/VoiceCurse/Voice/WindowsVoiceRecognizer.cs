using System;
using UnityEngine;
using UnityEngine.Windows.Speech;
using VoiceCurse.Interfaces;

namespace VoiceCurse.Voice;

public class WindowsVoiceRecognizer : IVoiceRecognizer {
    private DictationRecognizer? _dictationRecognizer;
    private bool _isStopping;

    public event Action<string>? OnPhraseRecognized;
    public event Action<string>? OnPartialResult;

    public WindowsVoiceRecognizer() {
        if (Application.platform != RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.WindowsEditor) {
            throw new PlatformNotSupportedException("Windows Speech is only available on Windows.");
        }
    }

    public void Start() {
        if (_dictationRecognizer != null) return;

        _dictationRecognizer = new DictationRecognizer();
        _isStopping = false;

        _dictationRecognizer.DictationResult += (text, _) => {
            OnPhraseRecognized?.Invoke(text);
        };
        
        _dictationRecognizer.DictationHypothesis += text => {
            OnPartialResult?.Invoke(text);
        };
        
        _dictationRecognizer.DictationComplete += cause => {
            if (_isStopping) return;
            if (cause == DictationCompletionCause.Canceled) {
                Debug.LogError("[VoiceCurse] Windows Speech Canceled. This usually means microphone access was denied or the device is unavailable.");
                return;
            }

            if (cause != DictationCompletionCause.Complete) {
                Debug.LogWarning($"[VoiceCurse] Dictation stopped: {cause}. Restarting...");
            }
            
            _dictationRecognizer.Start();
        };

        _dictationRecognizer.DictationError += (error, hresult) => {
            Debug.LogError($"[VoiceCurse] Windows Speech Error: {error} (Code: {hresult})");
        };

        _dictationRecognizer.Start();
        Debug.Log("[VoiceCurse] Windows Native Speech Initialized.");
    }

    public void FeedAudio(short[] pcmData, int length) { }

    public void Stop() {
        _isStopping = true;
        if (_dictationRecognizer != null && _dictationRecognizer.Status == SpeechSystemStatus.Running) {
            _dictationRecognizer.Stop();
        }
    }

    public void Dispose() {
        Stop();
        if (_dictationRecognizer != null) {
            _dictationRecognizer.Dispose();
            _dictationRecognizer = null;
        }
    }
}