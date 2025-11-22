using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using UnityEngine;
using Vosk;

namespace VoiceCurse.Core {
    public class VoiceRecognizer : IVoiceRecognizer {
        private readonly Model _model;
        private readonly VoskRecognizer _recognizer;
        private readonly ConcurrentQueue<short[]> _audioQueue = new();
        private readonly ConcurrentQueue<(string text, bool isPartial)> _resultQueue = new();
        
        private Thread? _workerThread;
        private volatile bool _isRunning;

        public event Action<string>? OnPhraseRecognized;
        public event Action<string>? OnPartialResult;

        public VoiceRecognizer(string modelPath) {
            if (!Directory.Exists(modelPath)) {
                throw new DirectoryNotFoundException("Vosk model not found at: " + modelPath);
            }

            try {
                _model = new Model(modelPath);
                _recognizer = new VoskRecognizer(_model, 48000.0f);
                _recognizer.SetMaxAlternatives(0);
                _recognizer.SetWords(true);
            } catch (Exception e) {
                Debug.LogError("Failed to initialize Vosk: " + e.Message);
                throw;
            }
        }

        public void Update() {
            while (_resultQueue.TryDequeue(out (string text, bool isPartial) result)) {
                if (result.isPartial) {
                    OnPartialResult?.Invoke(result.text);
                } else {
                    OnPhraseRecognized?.Invoke(result.text);
                }
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
                        ExtractAndQueue(jsonResult, isPartial: false);
                    } else {
                        string partialJson = _recognizer.PartialResult();
                        ExtractAndQueue(partialJson, isPartial: true);
                    }
                } else {
                    Thread.Sleep(10);
                }
            }
        }
        
        private void ExtractAndQueue(string json, bool isPartial) {
            if (string.IsNullOrEmpty(json)) return;
            
            string key = isPartial ? "\"partial\"" : "\"text\"";
            
            int keyIndex = json.IndexOf(key, StringComparison.Ordinal);
            if (keyIndex == -1) return;
            
            int colonIndex = json.IndexOf(':', keyIndex + key.Length);
            if (colonIndex == -1) return;
            
            int startQuote = json.IndexOf('"', colonIndex);
            if (startQuote == -1) return;
            
            int endQuote = json.IndexOf('"', startQuote + 1);
            if (endQuote == -1) return;

            if (endQuote > startQuote + 1) {
                string text = json.Substring(startQuote + 1, endQuote - startQuote - 1);
                if (!string.IsNullOrWhiteSpace(text)) {
                    _resultQueue.Enqueue((text, isPartial));
                }
            }
        }

        public void Dispose() {
            Stop();
            _recognizer.Dispose();
            _model.Dispose();
        }
    }
}