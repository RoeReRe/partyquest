using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

public class PlayerUIManager : PlayerBoardStateMachine, IOnEventCallback
{
    public GameObject playerDisplay;
    public GameObject playerButtons;
    public GameObject loadingScreen;
    public NotificationHandler playerNotificationScreen;
    public Button endTurnButton;

    private void Start() {
        UIState(false);
        loadingScreen.SetActive(true);

        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.CurrentRoom.PlayerTtl = 5 * 60 * 1000;

        if (PhotonNetwork.IsMasterClient) {
            PhotonNetwork.LoadLevel("MainGame");
        }
    }

    public void UIState(bool state) {
        playerDisplay.SetActive(state);
        playerButtons.SetActive(state);
    }

    public void ButtonState(bool state) {
        Button[] buttons = playerButtons.GetComponentsInChildren<Button>();
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
}
