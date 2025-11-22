using System;
using System.Collections.Concurrent;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using VoiceCurse.Core;
using VoiceCurse.Audio;

namespace VoiceCurse;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin {
    internal static ManualLogSource Log { get; set; } = null!;
    private IVoiceRecognizer? _recognizer;
    private AudioStreamTapper? _tapper;
    private readonly ConcurrentQueue<Action> _mainThreadActions = new();

    private void Awake() {
        Log = Logger;
        Log.LogInfo($"Plugin {Name} is loading...");

        SetupVoiceRecognition();
    }

    private void SetupVoiceRecognition() {
        string modelPath = Path.Combine(Paths.PluginPath, "Ryocery-VoiceCurse", "model-en-us");

        if (!Directory.Exists(modelPath)) {
            Log.LogError($"Vosk model not found! Please create folder: {modelPath}");
            return;
        }

        try {
            _recognizer = new VoiceRecognizer(modelPath);
            _recognizer.OnPhraseRecognized += OnPhraseRecognized;
            _recognizer.Start();

            Log.LogInfo("Voice Recognizer started successfully.");
        } catch (Exception ex) {
            Log.LogError($"Failed to start Voice Recognizer: {ex.Message}");
        }
    }

    private void Update() {
        if (_tapper is not null) return;

        AudioListener listener = FindFirstObjectByType<AudioListener>();

        if (listener is null) return;

        Log.LogInfo("Found AudioListener, attaching Tapper...");
        _tapper = listener.gameObject.AddComponent<AudioStreamTapper>();
        if (_recognizer != null) _tapper.Initialize(_recognizer);
    }

    private void OnPhraseRecognized(string text) {
        _mainThreadActions.Enqueue(() => {
            Log.LogInfo($"[SPEECH DETECTED]: {text}");
        });
    }


    private void OnDestroy() {
        _recognizer?.Stop();
        _recognizer?.Dispose();
    }
}