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
    private BoardManager boardManager;
    private PlayerManager playerManager;
    public GameObject playerInfoParent;
    public GameObject playerInfoPrefab;
    public GameObject chatContent;
    public TMP_Text chatText;
    private Queue<TMP_Text> chatQueue = new Queue<TMP_Text>();
    public GameObject loadingScreen;
    public NotificationHandler notificationScreen;
    public GameObject rewardScreen;
    
    [Header("Battle")]
    public Animator sceneTransition;
    public GameObject battlePriorityBar;
    public GameObject portrait;

    private Dictionary<string, GameObject> playerDisplays = new Dictionary<string, GameObject>();

    private void Awake() {
        playerManager = FindObjectOfType<PlayerManager>();
        boardManager = FindObjectOfType<BoardManager>();
        loadingScreen.SetActive(true);
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

    public void setLoadingScreen(bool state) {
        loadingScreen.SetActive(state);
    }

    public void closeNotification() {
        notificationScreen.gameObject.SetActive(false);
    }

    public void logInfo(string info) {
        TMP_Text temp = Instantiate(chatText, Vector3.zero, Quaternion.identity, chatContent.transform);
        temp.text = info;
        chatQueue.Enqueue(temp);

        if (chatQueue.Count > 30) {
            TMP_Text oldest = chatQueue.Dequeue();
            GameObject.Destroy(oldest.gameObject);
        }
    }

    public void screenWipeIn() {
        sceneTransition.SetTrigger("ScreenWipeIn");
    }

    public void screenFadeOut() {
        sceneTransition.SetTrigger("ScreenFadeOut");
    }

    public void setRewardScreen(int gold, int xp) {
        rewardScreen.GetComponentInChildren<TMP_Text>().text = string.Format("Victory\nGold: {0}\nXP: {1}", gold, xp);
    }

    public void OnEvent(EventData photonEvent) { 
        if (photonEvent.Code < 200) {
            GameEventCodes eventCode = (GameEventCodes) photonEvent.Code;
            object[] eventData = (object[]) photonEvent.CustomData;
            string sender = PhotonNetwork.CurrentRoom.GetPlayer(photonEvent.Sender).NickName;

            switch (eventCode) {
                case GameEventCodes.PLAYERINITSTATS:
                    ReceiveInitStats(eventData, sender);
                    break;
                case GameEventCodes.PLAYERSYNCSTATS:
                    ReceiveSyncStats(eventData, sender);
                    break;
            }
        }
    }

    public void ReceiveInitStats(object[] eventData, string sender) {
        ReceiveSyncStats(eventData, sender);
        boardManager.ReceiveGameStart();
    }

    public void ReceiveSyncStats(object[] eventData, string sender) {
        GameObject player = playerDisplays[sender];
        Slider health = player.transform.Find("Health").gameObject.GetComponentInChildren<Slider>();
        Slider mana = player.transform.Find("Mana").gameObject.GetComponentInChildren<Slider>();
        TMP_Text level = player.transform.Find("Level").gameObject.GetComponent<TMP_Text>();

        health.maxValue = (int) eventData[1];
        health.value = (int) eventData[0];
        mana.maxValue = (int) eventData[3];
        mana.value = (int) eventData[2];
        level.text = "Level: " + ((int) eventData[4]).ToString();
    }
}
