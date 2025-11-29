using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

namespace VoiceCurse.Events;

public class SlipEvent(Config config) : VoiceEventBase(config) {
    private readonly HashSet<string> _keywords = ParseKeywords(config.SlipKeywords.Value);
    private static AudioClip? _cachedTripSound;
    
    private Item? _cachedBananaItem;
    private static readonly Regex NameCleaner = new(@"\s*\((\d+|Clone)\)", RegexOptions.Compiled);

    protected override IEnumerable<string> GetKeywords() {
        return Config.SlipEnabled.Value ? _keywords : Enumerable.Empty<string>();
    }

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (!Config.SlipEnabled.Value) return false;
        if (player.data.dead || player.data.fullyPassedOut) return false;
        
        if (Config.BananaBombEnabled.Value) {
            if (Random.value <= Config.BananaBombChance.Value) {
                ExecutionDetail = "BananaBomb";
            }
        }
        
        player.Fall(Config.SlipStunDuration.Value); 
        
        Vector3 lookDir = player.data.lookDirection_Flat;
        ApplyForceToPart(player, BodypartType.Foot_R, (lookDir + Vector3.up) * 200f);
        ApplyForceToPart(player, BodypartType.Foot_L, (lookDir + Vector3.up) * 200f);
        ApplyForceToPart(player, BodypartType.Hip, Vector3.up * 1500f);
        ApplyForceToPart(player, BodypartType.Head, lookDir * -300f);

        return true;
    }

    private static void ApplyForceToPart(Character player, BodypartType partType, Vector3 force) {
        Rigidbody rb = player.GetBodypartRig(partType);
        if (rb) {
            rb.AddForce(force, ForceMode.Impulse);
        }
    }

    public override void PlayEffects(Character origin, Vector3 position, string detail) {
        base.PlayEffects(origin, position, detail);

        if (detail == "BananaBomb") {
            SpawnBananaBomb(position);
        }
    }

    public override void PlayEffects(Vector3 position) {
        AudioClip? clip = GetTripSound();
        if (clip) {
            AudioSource.PlayClipAtPoint(clip, position);
        }
    }
    
    private void SpawnBananaBomb(Vector3 origin) {
        if (!PhotonNetwork.IsMasterClient) return;

        Item? bananaItem = GetBananaItem();
        if (!bananaItem) {
            if (Config.EnableDebugLogs.Value) Debug.LogWarning("[VoiceCurse] Could not find 'Berrynana Peel Yellow' for Banana Bomb.");
            return;
        }
        
        string cleanName = NameCleaner.Replace(bananaItem.name, "").Trim();
        string prefabPath = "0_Items/" + cleanName;

        int amount = Config.BananaBombAmount.Value;
        
        for (int i = 0; i < amount; i++) {
            Vector3 spawnPos = origin + Random.insideUnitSphere * 3.0f; 
            spawnPos.y = origin.y + 1.5f;
            GameObject banana = PhotonNetwork.Instantiate(prefabPath, spawnPos, Quaternion.identity);

            if (!banana || !banana.TryGetComponent(out PhotonView pv)) continue;
            pv.RPC("SetKinematicRPC", RpcTarget.All, false, spawnPos, Quaternion.identity);
                
            Vector3 launchDir = GetRandomUpwardDirection();
            float force = Random.Range(25f, 200f);
            
            if (!banana.TryGetComponent(out Rigidbody rb)) continue;
            rb.isKinematic = false; 
            rb.AddForce(launchDir * force, ForceMode.Impulse);
        }
    }
    
    private static Vector3 GetRandomUpwardDirection() {
        Vector3 direction = Random.onUnitSphere;
        direction.y = Mathf.Abs(direction.y);
        if (direction.y < 0.2f) direction.y = 0.5f; 
        return direction.normalized;
    }

    private Item? GetBananaItem() {
        if (_cachedBananaItem) return _cachedBananaItem;
        _cachedBananaItem = Resources.FindObjectsOfTypeAll<Item>()
            .FirstOrDefault(i => i.name.Contains("Berrynana Peel Yellow"));
            
        return _cachedBananaItem;
    }

    private static AudioClip? GetTripSound() {
        if (!_cachedTripSound) {
            _cachedTripSound = Resources.FindObjectsOfTypeAll<AudioClip>()
                .FirstOrDefault(c => c.name == "Au_Slip1");
        }
        return _cachedTripSound;
    }
}