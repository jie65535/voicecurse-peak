using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using BepInEx.Logging;
using Photon.Pun;
using Photon.Voice;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
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
        string modelPath = Path.Combine(pluginDir, "model-zh-cn");

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
        int streamSampleRate = 48000;
        
        if (voice.Info.SamplingRate > 0) {
            streamSampleRate = voice.Info.SamplingRate;
        }

        _log.LogInfo($"[VoiceCurse] Photon Input Rate: {streamSampleRate} Hz. Initializing Vosk at 16000 Hz.");
        SetupVoiceRecognition();
        
        if (_recognizer is VoiceRecognizer && voice is LocalVoiceAudio<float> floatVoice) {
            floatVoice.AddPostProcessor(new VoiceProcessor(_recognizer, streamSampleRate));
            _log.LogInfo($"[VoiceCurse] Audio Processor attached. Resampling {streamSampleRate} -> 16000 Hz.");
        } else {
            _log.LogWarning($"[VoiceCurse] Could not attach processor. Voice type: {voice.GetType().Name}");
        }
    }

    private void SetupVoiceRecognition() {
        if (_recognizer != null) return;
        if (_voskModel == null) return;
        
        try {
            _recognizer = new VoiceRecognizer(_voskModel, 16000);
            
            _recognizer.OnPhraseRecognized += (text) => {
                _lastPartialText = "";
                _mainThreadActions.Enqueue(() => {
                    _log.LogInfo($"[Recognized]: {text}");
                    _eventHandler?.HandleSpeech(text, true);
                });
            };
            
            _recognizer.OnPartialResult += text => {
                if (string.IsNullOrWhiteSpace(text) || text.Length < 2) return;
                if (_lastPartialText == text) return;

                _lastPartialText = text;
                string captured = text;
                _mainThreadActions.Enqueue(() => {
                    _log.LogInfo($"[Partial]: {captured}"); 
                    _eventHandler?.HandleSpeech(captured, false);
                });
            };

            _recognizer.Start();
            _log.LogInfo("[VoiceCurse] Vosk Recognizer started.");
        }
        catch (Exception ex) {
            _log.LogError($"Failed to start Vosk Recognizer: {ex.Message}");
        }
    }

    public void Dispose() {
        if (_networker != null) PhotonNetwork.RemoveCallbackTarget(_networker);
        _recognizer?.Stop();
        _recognizer?.Dispose();
        _voskModel?.Dispose();
        _log.LogInfo("VoiceCurse Manager disposed.");
    }
}