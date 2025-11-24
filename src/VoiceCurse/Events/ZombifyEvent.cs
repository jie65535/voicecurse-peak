using System.Collections.Generic;
using Photon.Pun;

namespace VoiceCurse.Events;

public class ZombifyEvent(Config config) : VoiceEventBase(config) {
    private readonly HashSet<string> _deathKeywords = [
        "zombie", "zombify", "zombified", "walker", "ghoul", 
        "bitten", "bite", "brain", "rot", "decay",
        "infected", "infection", "plague", "pandemic", "virus", "outbreak",
        "cannibal", "flesh", "meat", "undead", "risen", "horde", "apocalypse",
        "reanimate", "reanimated", "lurker", "creeper", "crawler",
        "groaning", "groan", "moan", "moaning", "growl", "snarl"
    ];

    protected override IEnumerable<string> GetKeywords() => _deathKeywords;

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (player.data.dead || player.data.zombified) return false;
        player.photonView.RPC("RPCA_Zombify", RpcTarget.All, player.Center);
        return true;
    }
}