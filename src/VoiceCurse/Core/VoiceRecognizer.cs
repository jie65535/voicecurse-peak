using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using UnityEngine;
using Vosk;

namespace VoiceCurse.Core;

public class VoiceRecognizer : IVoiceRecognizer {
    private readonly Model _model;
    private readonly VoskRecognizer _recognizer;
    private readonly ConcurrentQueue<short[]> _audioQueue = new();
    private Thread? _workerThread;
    private volatile bool _isRunning;

    public event Action<string>? OnPhraseRecognized;

    public VoiceRecognizer(string modelPath) {
        if (!Directory.Exists(modelPath)) {
            throw new DirectoryNotFoundException("Vosk model not found at: " + modelPath);
        }

        Vosk.Vosk.SetLogLevel(-1);

        try {
            _model = new Model(modelPath);
            _recognizer = new VoskRecognizer(_model, 48000.0f);
        } catch (Exception e) {
            Debug.LogError("Failed to initialize Vosk: " + e.Message);
            throw;
        }
    }

    public void Start() {
        if (_isRunning) return;

        _isRunning = true;
        _workerThread = new Thread(ProcessAudioLoop);
        _workerThread.IsBackground = true;
        _workerThread.Name = "VoiceCurse Worker";
        _workerThread.Start();
    }

    public void Stop() {
        _isRunning = false;
        if (_workerThread is { IsAlive: true }) {
            _workerThread.Join(500);
        }
    }

    public void FeedAudio(short[] pcmData, int length) {
        if (_isRunning) {
            _audioQueue.Enqueue(pcmData);
        }
    }

    private void ProcessAudioLoop() {
        while (_isRunning) {
            if (_audioQueue.TryDequeue(out short[] data)) {
                if (_recognizer.AcceptWaveform(data, data.Length)) {
                    string jsonResult = _recognizer.Result();
                    ExtractAndFire(jsonResult);
                }
            } else {
                Thread.Sleep(10);
            }
        }
    }

    private void ExtractAndFire(string json) {
        if (string.IsNullOrEmpty(json)) return;

        int textIndex = json.IndexOf("\"text\" :", StringComparison.Ordinal);
        if (textIndex != -1) {
            int start = json.IndexOf("\"", textIndex + 8, StringComparison.Ordinal) + 1;
            int end = json.LastIndexOf("\"", StringComparison.Ordinal);

            if (end > start) {
                string text = json.Substring(start, end - start);
                if (!string.IsNullOrWhiteSpace(text)) {
                    OnPhraseRecognized?.Invoke(text);
                }
            }
        }
    }

    public void Dispose() {
        Stop();
        _recognizer.Dispose();
        _model.Dispose();
    }
}