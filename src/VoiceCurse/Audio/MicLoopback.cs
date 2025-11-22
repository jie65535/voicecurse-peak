using BepInEx.Logging;
using UnityEngine;

namespace VoiceCurse.Audio;

public class MicLoopback : MonoBehaviour {
    private AudioSource? _source;
    private AudioStreamTapper? _tapper;

    public void StartLoopback(Core.IVoiceRecognizer recognizer, ManualLogSource log) {
        _source = gameObject.AddComponent<AudioSource>();
        _source.loop = true;
        _source.bypassEffects = true;
        _source.bypassListenerEffects = true;

        string? deviceName = null;

        log.LogInfo($"Starting Microphone Loopback on device: {deviceName ?? "Default"}");
        _source.clip = Microphone.Start(deviceName, true, 10, 48000);
        while (!(Microphone.GetPosition(deviceName) > 0)) { }

        _source.Play();
        _tapper = gameObject.AddComponent<AudioStreamTapper>();
        _tapper.Initialize(recognizer, muteOutput: true);

        log.LogInfo("Microphone Loopback Active.");
    }
}