using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;
using TMPro;

public class PlayerUIManager : PlayerBoardStateMachine, IOnEventCallback
{
    public GameObject playerDisplay;
    public GameObject playerButtons;
    public GameObject playerBattleButtons;
    public GameObject loadingScreen;
    public NotificationHandler playerNotificationScreen;
    public Button endTurnButton;
    
    private PlayerStatus playerStatus;
    public PlayerBattle playerBattle;
    public GameObject targetScreen;
    public GameObject targetButtonPrefab;
    public string[] allies;
    public string[] enemies;
    public Action<string> currentAction;

    private void Start() {
        BoardUIState(false);
        BattleUIState(false);
        loadingScreen.SetActive(true);

        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.CurrentRoom.PlayerTtl = 5 * 60 * 1000;

        if (PhotonNetwork.IsMasterClient) {
            PhotonNetwork.LoadLevel("MainGame");
        }

        playerStatus = FindObjectOfType<PlayerStatus>();
        playerBattle = FindObjectOfType<PlayerBattle>();
    }

    public void BoardUIState(bool state) {
        playerDisplay.SetActive(state);
        playerButtons.SetActive(state);
    }

    public void BattleUIState(bool state) {
        targetScreen.SetActive(state);
        playerBattleButtons.SetActive(state);
    }

    public void ButtonState(bool state) {
        Button[] buttons = playerButtons.GetComponentsInChildren<Button>();
        foreach (Button button in buttons) {
            button.interactable = state;
        }
    }

    public void BattleButtonState(bool state) {
        Button[] buttons = playerBattleButtons.GetComponentsInChildren<Button>();
        foreach (Button button in buttons) {
            button.interactable = state;
        }
    }

    public void EndTurnState(bool state) {
        endTurnButton.interactable = state;
    }

    public void OnMove() {
        EndTurnState(false);
        currentState.OnMove();
    }

    public void OnEndTurn() {
        currentState.OnEndTurn();
    }

    public void OnAttack() {
        currentAction = battleState.OnAttack;
        ShowTargets(false);
    }

    public void ShowTargets(bool showAllies) {
        foreach (Transform child in targetScreen.transform) {
            GameObject.Destroy(child.gameObject);
        }
        
        if (showAllies) {
            foreach (string playerName in allies) {
                GameObject temp = Instantiate(targetButtonPrefab, Vector3.zero, Quaternion.identity, targetScreen.transform);
                temp.name = playerName;
                temp.GetComponentInChildren<TMP_Text>().text = playerName;
                temp.GetComponent<Button>().onClick.AddListener(OnTargetSelect);
            }
        }

        foreach (string enemyName in enemies) {
            GameObject temp = Instantiate(targetButtonPrefab, Vector3.zero, Quaternion.identity, targetScreen.transform);
            temp.name = enemyName;
            temp.GetComponentInChildren<TMP_Text>().text = enemyName;
            temp.GetComponent<Button>().onClick.AddListener(OnTargetSelect);
        }

        targetScreen.SetActive(true);
    }

    public void OnTargetSelect() {
        string currentTarget = EventSystem.current.currentSelectedGameObject.name;
        currentAction.Invoke(currentTarget);
        targetScreen.SetActive(false);
    }

    public void OnEvent(EventData photonEvent) { 
        if (photonEvent.Code < 200) {
            GameEventCodes eventCode = (GameEventCodes) photonEvent.Code;
            object[] eventData = (object[]) photonEvent.CustomData;
            string sender = PhotonNetwork.CurrentRoom.GetPlayer(photonEvent.Sender).NickName;

            switch (eventCode) {
                case GameEventCodes.GAMESTART:
                    ReceiveGameStart();
                    break;
                case GameEventCodes.PLAYERSTARTTURN:
                    ReceiveStartTurn();
                    break;
                case GameEventCodes.NOTIFYPLAYER:
                    ReceiveNotification(eventData);
                    break;
                case GameEventCodes.PLAYERCANENDTURN:
                    ReceiveCanEndTurn();
                    break;
                case GameEventCodes.BATTLESTART:
                    ReceiveStartBattle();
                    break;
                case GameEventCodes.RESUMEBOARD:
                    ReceiveResumeBoard();
                    break;
                case GameEventCodes.PLAYERBATTLEWAIT:
                    ReceiveBattleWait();
                    break;
                case GameEventCodes.PLAYERBATTLEACTION:
                    ReceiveBattleAction();
                    break;
                case GameEventCodes.REFRESHTARGETLIST:
                    ReceiveTargetList(eventData);
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

    public void ReceiveGameStart() {
        currentState = new BoardOtherTurnState(this);
        currentState.OnEnter();
        loadingScreen.SetActive(false);

        object[] pkg = new object[0];
        PhotonNetwork.RaiseEvent(
            (byte) GameEventCodes.GAMESTART,
            pkg,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
        );
    }

    public void ReceiveStartTurn() {
        ChangeState(new BoardPlayerTurnState(this));
    }

    public void ReceiveNotification(object[] eventData) {
        // [Title, body, leftText, leftState, rightText, rightState]
        Tuple<string, bool>[] buttonSettings;
        if (eventData.Length == 4) {
            buttonSettings = new Tuple<string, bool>[1];
        } else {
            buttonSettings = new Tuple<string, bool>[2];
            buttonSettings[1] = new Tuple<string, bool>((string) eventData[4], (bool) eventData[5]);
        }
        
        buttonSettings[0] = new Tuple<string, bool>((string) eventData[2], (bool) eventData[3]);

        playerNotificationScreen.initNotification(
            (string) eventData[0],
            (string) eventData[1],
            buttonSettings
        );
    }

    public void ReceiveCanEndTurn() {
        EndTurnState(true);
    }

    public void ReceiveStartBattle() {
        battleState = new OtherBattleTurnState(this);
        battleState.OnEnter();
    } 

    public void ReceiveResumeBoard() {
        battleState = null;
        playerBattleButtons.SetActive(false);
        playerButtons.SetActive(true);
    }

    public void ReceiveBattleWait() {
        ChangeBattleState(new OtherBattleTurnState(this));
    }

    public void ReceiveBattleAction() {
        ChangeBattleState(new PlayerBattleTurnState(this));
    }

    public void ReceiveTargetList(object[] eventData) {
        this.allies = (string[]) eventData[0];
        this.enemies = (string[]) eventData[1];
    }
}
