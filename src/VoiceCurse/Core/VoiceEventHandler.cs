using System.Collections.Generic;
using VoiceCurse.Events;

namespace VoiceCurse.Core;

public class VoiceEventHandler(VoiceCurseConfig config) {
    private readonly List<IVoiceEvent> _events = new() {
        new InstantDeathEvent(config),
        new AfflictionEvent(config)
    };

    public void HandleSpeech(string text) {
        if (string.IsNullOrWhiteSpace(text)) return;

        string lowerText = text.ToLowerInvariant();
        foreach (IVoiceEvent evt in _events) {
            evt.TryExecute(lowerText, lowerText);
        }
    }
}