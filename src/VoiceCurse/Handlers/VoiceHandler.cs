using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using BepInEx;
using BepInEx.Logging;
using Photon.Pun;
using Photon.Voice;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine;
using VoiceCurse.Interfaces;
using VoiceCurse.Voice;
using Vosk;

namespace VoiceCurse.Handlers;

public class VoiceHandler : IDisposable {
    private readonly ManualLogSource _log;
    private readonly Config _config;

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool SetDllDirectory(string lpPathName);

    private IVoiceRecognizer? _recognizer;
    private EventHandler? _eventHandler;
    private NetworkHandler? _networker;
    private Model? _voskModel;
    
    private VoiceHook? _activeHook;
        
    private readonly ConcurrentQueue<Action> _mainThreadActions = new();
    private volatile string _lastPartialText = "";

    public VoiceHandler(ManualLogSource logger, Config config, string pluginDir) {
        _log = logger;
        _config = config;

        Initialize(pluginDir);
    }

    private void Initialize(string pluginDir) {
        _log.LogInfo("Initializing VoiceCurse Manager...");
        
        if (Directory.Exists(pluginDir)) {
            SetDllDirectory(pluginDir);
        }

        _eventHandler = new EventHandler(_config);
        _networker = new NetworkHandler();
        PhotonNetwork.AddCallbackTarget(_networker);
        
        string modelPath = Path.Combine(Paths.PluginPath, "VoiceCurse", "model-en-us");
        if (Directory.Exists(modelPath)) {
            try {
                _voskModel = new Model(modelPath);
                _log.LogInfo("Vosk Model loaded successfully.");
            } catch (Exception e) {
                _log.LogError($"Failed to load Vosk Model: {e.Message}");
            }
        } else {
            _log.LogError($"Vosk model not found at: {modelPath}");
        }
    }

    public void Update() {
        while (_mainThreadActions.TryDequeue(out Action action)) {
            action.Invoke();
        }

        if (!_activeHook && Character.localCharacter) {
            TryHookIntoPhotonVoice();
        }
    }

    private void TryHookIntoPhotonVoice() {
        PhotonVoiceView voiceView = Character.localCharacter.GetComponent<PhotonVoiceView>();
        
        if (!voiceView || !voiceView.RecorderInUse) return;
        Recorder recorder = voiceView.RecorderInUse;

        VoiceHook hook = recorder.gameObject.AddComponent<VoiceHook>();
        hook.Initialize(this, recorder);
            
        _activeHook = hook;
        _log.LogInfo($"[VoiceCurse] Hooked into Recorder on: {recorder.gameObject.name}");
    }

    public void OnPhotonVoiceReady(Recorder recorder, LocalVoice voice) {
        int samplingRate = 48000; 
        if (voice.Info.SamplingRate > 0) {
            samplingRate = voice.Info.SamplingRate;
        }

        _log.LogInfo($"[VoiceCurse] Photon Voice Ready. Using Stream Rate: {samplingRate} Hz");

        SetupVoiceRecognition(samplingRate);
        
        if (_recognizer is VoskVoiceRecognizer && voice is LocalVoiceAudio<float> floatVoice) {
            floatVoice.AddPostProcessor(new VoiceProcessor(_recognizer));
            _log.LogInfo("[VoiceCurse] Audio Processor Injected successfully!");
        } 
        else if (_recognizer is not VoskVoiceRecognizer) {
            _log.LogInfo($"[VoiceCurse] Processor skipped for {_recognizer?.GetType().Name} (Native handling).");
        }
        else {
            _log.LogWarning($"[VoiceCurse] LocalVoice type mismatch: {voice.GetType().Name}");
        }
    }

    private void SetupVoiceRecognition(int sampleRate) {
        if (_recognizer != null) return;
        bool forceVosk = false; 

        if (!forceVosk && Application.platform == RuntimePlatform.WindowsPlayer) {
            try {
                _log.LogInfo("[VoiceCurse] Attempting to start Windows Native Speech...");
                WindowsVoiceRecognizer winRecognizer = new();
                
                winRecognizer.OnPhraseRecognized += HandleRecognized;
                winRecognizer.OnPartialResult += HandlePartial;
                
                winRecognizer.Start();
                
                _recognizer = winRecognizer;
                return;
            }
            catch (Exception e) {
                _log.LogWarning($"[VoiceCurse] Windows Speech failed to start: {e.Message}");
                _log.LogWarning("[VoiceCurse] Falling back to Vosk...");
                _recognizer = null;
            }
        }

        if (_voskModel == null) {
            _log.LogError("[VoiceCurse] Cannot fall back to Vosk: Model is null.");
            return;
        }
        
        try {
            _recognizer = new VoskVoiceRecognizer(_voskModel, sampleRate);
            AttachEvents();
            _recognizer.Start();
            _log.LogInfo($"[VoiceCurse] Vosk fallback initialized at {sampleRate} Hz.");
        }
        catch (Exception ex) {
            _log.LogError($"Failed to start Vosk Recognizer: {ex.Message}");
        }
    }
    
    private void AttachEvents() {
        if (_recognizer == null) return;
        _recognizer.OnPhraseRecognized += HandleRecognized;
        _recognizer.OnPartialResult += HandlePartial;
    }

    private void HandleRecognized(string text) {
        _lastPartialText = "";
        _mainThreadActions.Enqueue(() => {
            _log.LogInfo($"[Recognized]: {text}");
            _eventHandler?.HandleSpeech(text, true);
        });
    }

    private void HandlePartial(string text) {
        if (string.IsNullOrWhiteSpace(text) || text.Length < 2) return;
        if (_lastPartialText == text) return;

        _lastPartialText = text;
        string captured = text;
        _mainThreadActions.Enqueue(() => {
            _log.LogInfo($"[Partial]: {captured}");
            _eventHandler?.HandleSpeech(captured, false);
        });
    }

    public void Dispose() {
        if (_networker != null) PhotonNetwork.RemoveCallbackTarget(_networker);
        _recognizer?.Stop();
        _recognizer?.Dispose();
        _voskModel?.Dispose();
        _log.LogInfo("VoiceCurse Manager disposed.");
    }
}