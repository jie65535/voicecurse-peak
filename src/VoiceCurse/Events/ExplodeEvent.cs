using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using VoiceCurse.Core;

namespace VoiceCurse.Events {
    public class ExplodeEvent(VoiceCurseConfig config) : IVoiceEvent {
        private readonly HashSet<string> _keywords = new() { 
            "explosion", "explode", "blowing", "blew", "blow", "boom" 
        };

        public bool TryExecute(string spokenWord, string fullSentence) {
            if (!_keywords.Contains(spokenWord)) return false;

            Character localChar = Character.localCharacter;
            if (localChar == null || localChar.data.dead) return false;

            if (config.EnableDebugLogs.Value) {
                Debug.Log($"[VoiceCurse] Explosion triggered by '{spokenWord}'");
            }
            
            GameObject dynamiteObj = PhotonNetwork.Instantiate("Dynamite", localChar.Center, Quaternion.identity);
            
            if (dynamiteObj is not null) {
                localChar.StartCoroutine(ExplodeRoutine(dynamiteObj));
            }

            return true;
        }

        private IEnumerator ExplodeRoutine(GameObject? dynamiteObj) {
            yield return null;
            if (dynamiteObj is null) yield break;
            
            PhotonView view = dynamiteObj.GetComponent<PhotonView>();
            view?.RPC("RPC_Explode", RpcTarget.All);
            yield return new WaitForSeconds(0.1f);

            if (dynamiteObj.GetComponent<PhotonView>().IsMine) {
                PhotonNetwork.Destroy(dynamiteObj);
            }
        }
    }
}