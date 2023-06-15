using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class NotificationHandler : MonoBehaviour
{
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text bodyText;
    [SerializeField] Button leftButton;
    [SerializeField] Button rightButton;
    private Image image;
    private Button[] buttonArray = new Button[2];
    private bool leftResponse;
    private bool rightResponse;
    
    public void initNotification(string title, string body, Tuple<string, bool>[] buttons) {
        buttonArray[0] = leftButton;
        buttonArray[1] = rightButton;
        
        setTitle(title);
        setBody(body);
        setButtons(buttons);

        if (buttons.Length == 1) {
            leftButton.gameObject.SetActive(true);
            rightButton.gameObject.SetActive(false);
        } else {
            leftButton.gameObject.SetActive(true);
            rightButton.gameObject.SetActive(true);
        }

        this.gameObject.SetActive(true);
    }

    public void initNotification(string title, string body) {
        setTitle(title);
        setBody(body);

        this.gameObject.SetActive(true);
    }

    public void initNotification(string title, string body, Sprite image) {
        this.image.sprite = image;
        initNotification(title, body);
    }
    
    public void setTitle(string titleText) {
        this.titleText.text = titleText;
    }

    public void setBody(string bodyText) {
        this.bodyText.text = bodyText;
    }

    public void setButtons(Tuple<string, bool>[] buttonSettings) {
        for (int i = 0; i < buttonSettings.Length; i++) {
            buttonArray[i].GetComponentInChildren<TMP_Text>().text = buttonSettings[i].Item1;
            
            if (i == 0) {
                leftResponse = buttonSettings[i].Item2;
            } else {
                rightResponse = buttonSettings[i].Item2;
            }
        }
    }

    public void OnLeftClick() {
        if (leftResponse) {
            object[] pkg = new object[0];
            PhotonNetwork.RaiseEvent(
                (byte) GameEventCodes.PLAYERNOTIFLEFT,
                pkg,
                new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
                new SendOptions { Reliability = true }
            );
        }
        this.gameObject.SetActive(false);
    }

    public void OnRightClick() {
        if (rightResponse) {
            object[] pkg = new object[0];
            PhotonNetwork.RaiseEvent(
                (byte) GameEventCodes.PLAYERNOTIFRIGHT,
                pkg,
                new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
                new SendOptions { Reliability = true }
            );  
        }
        this.gameObject.SetActive(false);
    }
}
