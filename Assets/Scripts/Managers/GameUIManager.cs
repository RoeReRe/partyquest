using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;

public class GameUIManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public SessionInfo sessionInfo;
    private PlayerManager playerManager;
    public GameObject playerInfoParent;
    public GameObject playerInfoPrefab;

    private Dictionary<string, GameObject> playerDisplays = new Dictionary<string, GameObject>();

    private void Awake() {
        playerManager = FindObjectOfType<PlayerManager>();
    }

    public void createPlayer(string playerName) {
        GameObject temp = Instantiate(playerInfoPrefab, Vector3.zero, Quaternion.identity, playerInfoParent.transform);
        temp.name = playerName;
        playerDisplays.Add(temp.name, temp);

        temp.transform.Find("Avatar/Mask/Image")
            .gameObject
            .GetComponent<Image>()
            .sprite = sessionInfo.getAvatar(playerName);

        temp.transform.Find("Username")
            .gameObject
            .GetComponent<TMP_Text>()
            .text = playerName;
    }
    
    public void OnEvent(EventData photonEvent) { 
        if (photonEvent.Code < 200) {
            GameEventCodes eventCode = (GameEventCodes) photonEvent.Code;
            object[] eventData = (object[]) photonEvent.CustomData;
            string sender = PhotonNetwork.CurrentRoom.GetPlayer(photonEvent.Sender).NickName;

            switch (eventCode) {
                case GameEventCodes.PLAYERSYNCSTATS:
                    ReceiveSyncStats(eventData, sender);
                    break;
            }
        }
    }

    public override void OnEnable() {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable() {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void ReceiveSyncStats(object[] eventData, string sender) {
        GameObject player = playerDisplays[sender];
        Slider health = player.transform.Find("Health").gameObject.GetComponentInChildren<Slider>();
        Slider mana = player.transform.Find("Mana").gameObject.GetComponentInChildren<Slider>();
        TMP_Text level = player.transform.Find("Level").gameObject.GetComponent<TMP_Text>();

        health.value = (int) eventData[0];
        health.maxValue = (int) eventData[1];
        mana.value = (int) eventData[2];
        mana.maxValue = (int) eventData[3];
        level.text = "Level: " + ((int) eventData[4]).ToString();
    }
}