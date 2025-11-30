using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

namespace VoiceCurse.Events;

[Serializable]
public class TransmutePayload {
    public string? ruleName;
    public bool isDeath;
    public int spawnCount;
    public Vector3 deathPosition;
}

public class TransmuteEvent : VoiceEventBase {
    private readonly List<(string Name, string[] Triggers, string[] Targets, Func<bool> IsEnabled)> _definitions;
    private readonly Dictionary<string, (string Name, string[] Targets, Func<bool> IsEnabled)> _transmuteLookup = new();
    private readonly Dictionary<string, Item?> _itemCache = new();

    private static readonly Regex NameCleaner = new(@"\s*\((\d+|Clone)\)", RegexOptions.Compiled);

    public TransmuteEvent(Config config) : base(config) {
        _definitions = [
            ("奶",     ["milk", "calcium", "奶", "钙"], ["Fortified Milk"], () => config.TransmuteMilkEnabled.Value),
            ("仙人掌",   ["cactus", "cacti", "仙人掌"], ["Cactus"], () => config.TransmuteCactusEnabled.Value),
            ("椰子",  ["coconut", "椰子"], ["Coconut"], () => config.TransmuteCoconutEnabled.Value),
            ("苹果/浆果",    ["apple", "berry", "berries", "苹果", "浆果"], ["Red Crispberry", "Yellow Crispberry", "Green Crispberry"], () => config.TransmuteAppleEnabled.Value),
            ("香蕉",   ["banana", "香蕉"], ["Berrynana Peel Yellow"], () => config.TransmuteBananaEnabled.Value),
            ("鸡蛋",      ["egg", "鸡蛋"], ["Egg"], () => config.TransmuteEggEnabled.Value),
            ("水果",    ["fruit", "水果"], ["Red Crispberry", "Yellow Crispberry", "Green Crispberry", "Kingberry Purple", "Kingberry Yellow", "Kingberry Green", "Berrynana Brown", "Berrynana Yellow", "Berrynana Pink", "Berrynana Blue"], () => config.TransmuteFruitEnabled.Value),
            ("蘑菇", ["fungus", "mushroom", "fungi", "funghi", "shroom", "蘑菇", "真菌"], ["Mushroom Normie"], () => config.TransmuteMushroomEnabled.Value),
            ("球",     ["ball", "球"], ["Basketball"], () => config.TransmuteBallEnabled.Value)
        ];

        foreach ((string Name, string[] Triggers, string[] Targets, Func<bool> IsEnabled) def in _definitions) {
            foreach (string trigger in def.Triggers) {
                _transmuteLookup[trigger] = (def.Name, def.Targets, def.IsEnabled);
            }
        }
    }

    protected override IEnumerable<string> GetKeywords() {
        return Config.TransmuteEnabled.Value ? _definitions.Where(d => d.IsEnabled()).SelectMany(d => d.Triggers) : [];
    }

    protected override bool OnExecute(Character player, string spokenWord, string fullSentence, string matchedKeyword) {
        if (!Config.TransmuteEnabled.Value) return false;
        if (player.data.dead) return false;

        string[]? targets = null;
        string? ruleName = null;

        if (_transmuteLookup.TryGetValue(matchedKeyword, out (string Name, string[] Targets, Func<bool> IsEnabled) foundData)) {
            if (foundData.IsEnabled()) {
                ruleName = foundData.Name;
                targets = foundData.Targets;
            }
        }

        if (targets == null) {
            string? validKey = _transmuteLookup.Keys.FirstOrDefault(k =>
                fullSentence.Contains(k) && _transmuteLookup[k].IsEnabled());

            if (validKey != null) {
                (string Name, string[] Targets, Func<bool> IsEnabled) data = _transmuteLookup[validKey];
                ruleName = data.Name;
                targets = data.Targets;
            }
        }

        if (targets == null || targets.Length == 0) return false;

        bool deathEnabled = Config.TransmuteDeathEnabled.Value;

        TransmutePayload payload = new() {
            ruleName = ruleName,
            isDeath = deathEnabled,
            spawnCount = 0,
            deathPosition = player.Center
        };

        if (deathEnabled) {
            int countToSpawn = ClearInventoryForDeath(player);
            player.DieInstantly();
            payload.spawnCount = countToSpawn;
        }

        ExecutionDetail = ruleName;
        ExecutionPayload = JsonUtility.ToJson(payload);

        return true;
    }

    public override void PlayEffects(Character origin, Vector3 position, string detail) {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!origin) return;

        if (string.IsNullOrEmpty(detail)) return;

        TransmutePayload payload;
        try {
            payload = JsonUtility.FromJson<TransmutePayload>(detail);
        } catch {
            return;
        }

        if (payload == null || string.IsNullOrEmpty(payload.ruleName)) return;

        (string Name, string[] Triggers, string[] Targets, Func<bool> IsEnabled) definition = _definitions.FirstOrDefault(d => d.Name == payload.ruleName);
        if (definition.Targets == null || definition.Targets.Length == 0) return;

        string[] targets = definition.Targets;

        if (payload.isDeath) {
            Vector3 spawnPos = payload.deathPosition != Vector3.zero ? payload.deathPosition : origin.Center;
            SpawnTransmutedItems(spawnPos, payload.spawnCount, targets);
        } else {
            TransmuteInventoryAlive(origin, targets);
            float damage = Random.Range(Config.AfflictionMinPercent.Value, Config.AfflictionMaxPercent.Value);
            if (origin.refs.afflictions) {
                origin.refs.afflictions.AddStatus(CharacterAfflictions.STATUSTYPE.Injury, damage);
            }
        }
    }

    private static int ClearInventoryForDeath(Character player) {
        int count = 1;
        Vector3 voidPosition = new(0, -5000, 0);

        for (byte i = 0; i < 3; i++) {
            ItemSlot slot = player.player.GetItemSlot(i);
            if (slot == null || slot.IsEmpty()) continue;

            count++;
            player.refs.items.photonView.RPC("DropItemFromSlotRPC", RpcTarget.All, i, voidPosition);
        }

        ItemSlot backpackSlot = player.player.GetItemSlot(3);
        if (backpackSlot != null && !backpackSlot.IsEmpty()) {
            if (backpackSlot.data.TryGetDataEntry(DataEntryKey.BackpackData, out BackpackData backpackData)) {
                count += backpackData.FilledSlotCount();
            }

            count++;
            player.refs.items.photonView.RPC("DropItemFromSlotRPC", RpcTarget.All, (byte)3, voidPosition);
        }

        if (player.refs.afflictions) player.refs.afflictions.UpdateWeight();
        return count;
    }

    private void TransmuteInventoryAlive(Character player, string[] possibleTargets) {
        Vector3 voidPosition = new(0, -5000, 0);
        Vector3 spawnOrigin = player.Center;
        int totalItemsToSpawn = 0;

        for (byte i = 0; i < 3; i++) {
            ItemSlot slot = player.player.GetItemSlot(i);
            if (slot == null || slot.IsEmpty()) continue;

            player.refs.items.photonView.RPC("DropItemFromSlotRPC", RpcTarget.All, i, voidPosition);
            totalItemsToSpawn++;
        }

        ItemSlot backpackSlot = player.player.GetItemSlot(3);
        if (backpackSlot != null && !backpackSlot.IsEmpty()) {
            totalItemsToSpawn++;

            if (backpackSlot.data.TryGetDataEntry(DataEntryKey.BackpackData, out BackpackData backpackData)) {
                totalItemsToSpawn += backpackData.FilledSlotCount();
            }

            player.refs.items.photonView.RPC("DropItemFromSlotRPC", RpcTarget.All, (byte)3, voidPosition);
        }

        SpawnTransmutedItems(spawnOrigin, totalItemsToSpawn, possibleTargets);

        if (player.refs.afflictions) player.refs.afflictions.UpdateWeight();
    }

    private void SpawnTransmutedItems(Vector3 origin, int count, string[] targets) {
        for (int i = 0; i < count; i++) {
            SpawnItem(origin, targets, false, null);
        }
    }

    private void SpawnAndPickupItem(Character player, string[] targets, Vector3 origin) {
        SpawnItem(origin, targets, true, player);
    }

    private void SpawnItem(Vector3 origin, string[] targets, bool autoPickup, Character? picker) {
        string selectedTargetName = targets[Random.Range(0, targets.Length)];
        Item? targetItem = GetOrFindItem(selectedTargetName);
        if (!targetItem) return;

        Vector3 pos = origin + Random.insideUnitSphere * 0.5f;
        pos.y = origin.y + 0.5f;

        string cleanName = NameCleaner.Replace(targetItem.name, "").Trim();
        string prefabPath = "0_Items/" + cleanName;

        GameObject obj = PhotonNetwork.Instantiate(prefabPath, pos, Quaternion.identity);
        if (!obj || !obj.TryGetComponent(out PhotonView pv)) return;
        pv.RPC("SetKinematicRPC", RpcTarget.All, false, pos, Quaternion.identity);

        if (!autoPickup || !picker || !obj.TryGetComponent(out Item item)) return;
        if (picker.refs != null && picker.refs.items != null) {
            picker.refs.items.lastEquippedSlotTime = 0f;
        }

        item.Interact(picker);
    }

    private Item? GetOrFindItem(string searchName) {
        if (_itemCache.TryGetValue(searchName, out Item? cachedItem)) {
            if (cachedItem) return cachedItem;
            _itemCache.Remove(searchName);
        }

        Item? foundItem = Resources.FindObjectsOfTypeAll<Item>().FirstOrDefault(i => i.name.Contains(searchName) || (i.UIData != null && i.UIData.itemName.Contains(searchName)));
        _itemCache[searchName] = foundItem;

        if (!foundItem && Config.EnableDebugLogs.Value) {
            Debug.LogWarning($"[VoiceCurse] Could not find item matching '{searchName}'");
        }

        return foundItem;
    }
}