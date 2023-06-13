using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class Launcher : MonoBehaviourPunCallbacks, IOnEventCallback
{   
    public static Launcher instance;

    [Header("Menu UI")]
    public GameObject loadingScreen;
    public TMP_Text loadingText;
    public GameObject menuButtons;
    public GameObject nameScreen;
    public GameObject joinRoomScreen;
    public GameObject roomScreen;
    public GameObject errorScreen;
    
    
    [Header("Lobby UI")]
    public SessionInfo sessionInfo;
    public GameObject playerRoomDisplay;
    public GameObject scrollViewContent;
    public GameObject hostUI;
    public GameObject playerUI;
    public TMP_Dropdown raceSelector;
    public TMP_Dropdown classSelector;
    public TMP_Dropdown avatarSelector;
    public Toggle readyToggle;
    private List<Player> playerList = new List<Player>();

    public enum EventCodes : byte {
        RaceChange,
        ClassChange,
        AvatarChange,
        ReadyChange,
    }
    
    private void Awake() {
        instance = this;
    }

    void Start() {
        CloseMenus();
        loadingText.text = "Connecting to PUN master server";
        loadingScreen.SetActive(true);
        
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() {
        loadingText.text = "Joining PUN lobby";
        
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true; // Sync scene across clients
    }

    public override void OnJoinedLobby() {
        nameScreen.SetActive(true);
        if (PlayerPrefs.HasKey("playerName")) {
            nameScreen.GetComponentInChildren<TMP_InputField>().text = PlayerPrefs.GetString("playerName");
        }
    }

    public void ConfirmNickname() {
        TMP_InputField inputField = nameScreen.GetComponentInChildren<TMP_InputField>();
        string name = inputField.text;

        if (!string.IsNullOrWhiteSpace(name)) {
            PhotonNetwork.NickName = name;
            PlayerPrefs.SetString("playerName", name);
            PlayerPrefs.Save();

            CloseMenus();
            menuButtons.SetActive(true);
        }
    }

    public void CreateRoom() {
        RoomOptions roomOptions = new RoomOptions() {
            MaxPlayers = 5,
            PlayerTtl = 1000,
            EmptyRoomTtl = 1000,
            CleanupCacheOnLeave = false,
        };
        int roomCode = UnityEngine.Random.Range(100000, 1000000);
        PhotonNetwork.CreateRoom(roomCode.ToString(), roomOptions);

        CloseMenus();
        loadingText.text = "Creating room";
        loadingScreen.SetActive(true);
    }

    public void JoinRoomFromMenu() {
        CloseMenus();
        joinRoomScreen.SetActive(true);
    }

    public void JoinRoomWithCode() {
        TMP_InputField inputField = joinRoomScreen.GetComponentInChildren<TMP_InputField>();
        string roomCode = inputField.text;

        CloseMenus();
        loadingText.text = "Joining room";
        loadingScreen.SetActive(true);

        PhotonNetwork.JoinRoom(roomCode);
    }

    public override void OnJoinedRoom() {
        CloseMenus();
        TMP_Text roomText = roomScreen.GetComponentInChildren<TMP_Text>();
        roomText.text = PhotonNetwork.CurrentRoom.Name;
        roomScreen.SetActive(true);
        
        switch (PhotonNetwork.IsMasterClient) {
            case (true):
                hostUI.SetActive(true);
                playerUI.SetActive(false);
                break;
            case (false):
                hostUI.SetActive(false);
                playerUI.SetActive(true);
                populateSelection();
                break;
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message) {
        CloseMenus();
        errorScreen.SetActive(true);
        TMP_Text errorText = GameObject.Find("Error Text (TMP)").GetComponent<TMP_Text>();
        errorText.text = message;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        ListAllPlayers();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ListAllPlayers();
    }

    private void RefreshPlayerList() {
        playerList.Clear();
        playerList.AddRange(PhotonNetwork.PlayerList);
    }

    private void ListAllPlayers() {
        if (!PhotonNetwork.IsMasterClient) {
            return;
        }
        
        RefreshPlayerList();
        
        // Housekeeping
        foreach(Transform child in scrollViewContent.transform) {
            Destroy(child.gameObject);
        }

        foreach (Player player in playerList) {
            if (!player.IsMasterClient) {
                GameObject temp = Instantiate(playerRoomDisplay, Vector3.zero, Quaternion.identity, scrollViewContent.transform);
                temp.GetComponentInChildren<TMP_Text>(false).text = player.NickName;

                if (sessionInfo.isPlayerReady(player.NickName)) {
                    GameObject bottom = temp.transform.Find("Bottom").gameObject;
                    bottom.GetComponentInChildren<TMP_Text>().text = sessionInfo.getRaceClass(player.NickName);
                    bottom.GetComponentInChildren<Image>().sprite = sessionInfo.getAvatar(player.NickName);
                    bottom.SetActive(true);
                }
            }
        }
    }

    private void populateSelection()
    {
        raceSelector.AddOptions(sessionInfo.getRaceOptions());
        classSelector.AddOptions(sessionInfo.getClassOptions());
        avatarSelector.AddOptions(sessionInfo.getAvatarOptions());
    }

    private void playerReadyCheck() {
        readyToggle.isOn = false;
        if (raceSelector.value < 0 ||
            classSelector.value < 0 ||
            avatarSelector.value < 0) {
                readyToggle.interactable = false;
            } else {
                readyToggle.interactable = true;
            }
    }

    public void OnEvent(EventData photonEvent) {
        if (photonEvent.Code < 200) {
            EventCodes eventCode = (EventCodes) photonEvent.Code;
            object[] eventData = (object[]) photonEvent.CustomData;
            string sender = PhotonNetwork.CurrentRoom.GetPlayer(photonEvent.Sender).NickName;

            switch (eventCode) {
                case EventCodes.RaceChange:
                    sessionInfo.ReceiveRaceChange(eventData, sender);
                    break;
                case EventCodes.ClassChange:
                    sessionInfo.ReceiveClassChange(eventData, sender);
                    break;
                case EventCodes.AvatarChange:
                    sessionInfo.ReceiveAvatarChange(eventData, sender);
                    break;
                case EventCodes.ReadyChange:
                    sessionInfo.ReceiveReadyChange(eventData, sender);
                    ListAllPlayers();
                    startButtonCondition();
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
    
    public void OnRaceChange() {
        playerReadyCheck();

        object[] pkg = new object[] { raceSelector.value };
        PhotonNetwork.RaiseEvent(
            (byte) EventCodes.RaceChange,
            pkg,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
        );
    }

    public void OnClassChange() {
        playerReadyCheck();

        object[] pkg = new object[] { classSelector.value };
        PhotonNetwork.RaiseEvent(
            (byte) EventCodes.ClassChange,
            pkg,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
        );
    }
    
    public void OnAvatarChange() {
        playerReadyCheck();

        object[] pkg = new object[] { avatarSelector.value };
        PhotonNetwork.RaiseEvent(
            (byte) EventCodes.AvatarChange,
            pkg,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
        );
    }

    public void OnReadyChange() {
        raceSelector.interactable = !readyToggle.isOn;
        classSelector.interactable = !readyToggle.isOn;
        avatarSelector.interactable = !readyToggle.isOn;

        object[] pkg = new object[] { readyToggle.isOn };
        PhotonNetwork.RaiseEvent(
            (byte) EventCodes.ReadyChange,
            pkg,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
        );
    }

    public void startButtonCondition() {
        Button startButton = hostUI.GetComponentInChildren<Button>();
        foreach (Player player in playerList) {
            if (!player.IsMasterClient && !sessionInfo.isPlayerReady(player.NickName)) {
                startButton.interactable = false;
                return;
            }
        }
        startButton.interactable = true;
    }
    
    public void StartGame() {
        PhotonNetwork.LoadLevel("PlayerUI");
    }

    public void CloseMenus() {
        loadingScreen.SetActive(false);
        menuButtons.SetActive(false);
        nameScreen.SetActive(false);
        roomScreen.SetActive(false);
        joinRoomScreen.SetActive(false);
        errorScreen.SetActive(false);
    }
}