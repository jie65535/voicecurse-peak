using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Photon.Pun;
using Random = UnityEngine.Random;

namespace VoiceCurse.Events;

public class TransmuteEvent : VoiceEventBase {
    private readonly List<(string Name, string[] Triggers, string[] Targets, Func<bool> IsEnabled)> _definitions;
    private readonly Dictionary<string, (string Name, string[] Targets, Func<bool> IsEnabled)> _transmuteLookup = new();
    private readonly Dictionary<string, Item?> _itemCache = new();

    private static readonly Regex NameCleaner = new(@"\s*\((\d+|Clone)\)", RegexOptions.Compiled);

    public TransmuteEvent(Config config) : base(config) {
        _definitions = [
            ("奶",     ["milk", "calcium", "奶", "钙"], ["Fortified Milk", "奶白金"], () => config.TransmuteMilkEnabled.Value),
            ("仙人掌",   ["cactus", "cacti", "仙人掌"], ["Cactus", "仙人球"], () => config.TransmuteCactusEnabled.Value),
            ("椰子",  ["coconut", "椰子"], ["Coconut", "椰子"], () => config.TransmuteCoconutEnabled.Value),
            ("水果",    ["apple", "berry", "苹果", "浆果"], ["Red Crispberry", "Yellow Crispberry", "Green Crispberry", "红脆莓", "黄脆莓", "绿脆莓"], () => config.TransmuteAppleEnabled.Value),
            ("香蕉",   ["banana", "香蕉"], ["Berrynana Peel Yellow", "莓蕉皮"], () => config.TransmuteBananaEnabled.Value),
            ("鸡蛋",      ["egg", "鸡蛋"], ["Egg", "煎蛋"], () => config.TransmuteEggEnabled.Value),
            ("水果",    ["fruit", "水果"], ["Red Crispberry", "Yellow Crispberry", "Green Crispberry", "Kingberry Purple", "Kingberry Yellow", "Kingberry Green", "Berrynana Brown", "Berrynana Yellow", "Berrynana Pink", "Berrynana Blue", "红脆莓", "黄脆莓", "绿脆莓", "紫荔莓", "黄荔莓", "青荔莓", "棕莓蕉", "黄莓蕉", "粉莓蕉", "蓝莓蕉"], () => config.TransmuteFruitEnabled.Value),
            ("蘑菇", ["fungus", "mushroom", "fungi", "funghi", "shroom", "蘑菇", "真菌"], ["Mushroom Normie", "蘑菇"], () => config.TransmuteMushroomEnabled.Value)
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

        ExecutionDetail = ruleName;
        TransmuteInventory(player, targets);
        player.photonView.RPC("RPCA_Die", RpcTarget.All, player.Center);
        return true;
    }

    private void TransmuteInventory(Character player, string[] possibleTargets) {
        int countToSpawn = 1;
        Vector3 voidPosition = new(0, -5000, 0);

        for (byte i = 0; i < 3; i++) {
            ItemSlot slot = player.player.GetItemSlot(i);
            if (slot == null || slot.IsEmpty()) continue;

            countToSpawn++;
            player.refs.items.photonView.RPC("DropItemFromSlotRPC", RpcTarget.All, i, voidPosition);
        }

        ItemSlot backpackSlot = player.player.GetItemSlot(3);
        if (backpackSlot != null && !backpackSlot.IsEmpty()) {
            if (backpackSlot.data.TryGetDataEntry(DataEntryKey.BackpackData, out BackpackData backpackData)) {
                countToSpawn += backpackData.FilledSlotCount();
            }

            countToSpawn++;
            player.refs.items.photonView.RPC("DropItemFromSlotRPC", RpcTarget.All, (byte)3, voidPosition);
        }

        Vector3 spawnOrigin = player.Center;

        for (int i = 0; i < countToSpawn; i++) {
            string selectedTargetName = possibleTargets[Random.Range(0, possibleTargets.Length)];
            Item? targetItem = GetOrFindItem(selectedTargetName);
            if (!targetItem) continue;

            Vector3 pos = spawnOrigin + Random.insideUnitSphere * 0.5f;
            pos.y = spawnOrigin.y + 0.5f;

            string cleanName = NameCleaner.Replace(targetItem.name, "").Trim();
            string prefabPath = "0_Items/" + cleanName;

            GameObject obj = PhotonNetwork.Instantiate(prefabPath, pos, Quaternion.identity);
            if (obj && obj.TryGetComponent(out PhotonView pv)) {
                pv.RPC("SetKinematicRPC", RpcTarget.All, false, pos, Quaternion.identity);
            }
        }

        player.refs.afflictions.UpdateWeight();
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