using System.Collections.Generic;
using Photon.Pun;

namespace VoiceCurse.Events;

public class DeathEvent(Config config) : VoiceEventBase(config) {
    private readonly HashSet<string> _deathKeywords = [
        "die", "death", "dead", "suicide", "kill", "deceased", "skeleton", 
        "skull", "bones", "perish", "demise", "expire", 
        "fatal", "mortality", "mortal", "slain", "dying",
        "corpse", "cadaver", "lifeless", "cease", "extinct", "eliminate",
        "terminate", "execute", "obliterate", "annihilate", "eradicate",
        "end", "finish", "doom", "grave", "burial",
        "coffin", "casket", "tomb", "crypt", "reaper", "grim"
    ];

    protected override IEnumerable<string> GetKeywords() => _deathKeywords;

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (player.data.dead) return false;
        player.photonView.RPC("RPCA_Die", RpcTarget.All, player.Center);
        return true;
    }
}