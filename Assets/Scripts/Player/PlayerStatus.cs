using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatus : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public SessionInfo sessionInfo;
    
    // Visual status
    [System.NonSerialized] public Sprite avatar;
    [System.NonSerialized] public int health;
    [System.NonSerialized] public int maxHealth;
    [System.NonSerialized] public int mana;
    [System.NonSerialized] public int maxMana;
    [System.NonSerialized] public int xp = 0;
    [System.NonSerialized] public int gold;

    // Stats
    [System.NonSerialized] public int vitality;
    [System.NonSerialized] public int mind;
    [System.NonSerialized] public int strength;
    [System.NonSerialized] public int intelligence;
    [System.NonSerialized] public int dexterity;
    [System.NonSerialized] public int endurance;

    // Cached
    [System.NonSerialized] public Race playerRace;
    [System.NonSerialized] public Class playerClass;
    [System.NonSerialized] public Sprite playerSprite;

    // Display
    private PlayerUIManager playerUIManager;
    [SerializeField] Image avatarSprite;
    [SerializeField] Slider healthSlider;
    [SerializeField] TMP_Text healthText;
    [SerializeField] Slider manaSlider;
    [SerializeField] TMP_Text manaText;
    [SerializeField] TMP_Text usernameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text goldText;
    
    public void ReceiveInitStats(object[] eventData) {
        this.playerRace = sessionInfo.getRace((int) eventData[0]);
        this.playerClass = sessionInfo.getClass((int) eventData[1]);
        this.playerSprite = sessionInfo.getAvatar((int) eventData[2]);
        this.playerUIManager = this.gameObject.GetComponent<PlayerUIManager>();

        foreach (PropertyInfo property in typeof(Race).GetProperties()) {
            FieldInfo propertyField = this.GetType().GetField(property.Name);
            
            if (propertyField != null) {
                propertyField.SetValue(this, property.GetValue(this.playerRace)); 
            }
        }

        avatarSprite.sprite = this.playerSprite;
        usernameText.text = PhotonNetwork.LocalPlayer.NickName;
        
        health = StatFunction.statToHP(vitality);
        mana = StatFunction.statToMP(mind);
        gold = 100;

        updateDisplay();
    }

    private void updateDisplay() {
        // HP Calculation
        maxHealth = StatFunction.statToHP(vitality);
        healthSlider.maxValue = maxHealth;
        healthText.text = health.ToString() + " / " + maxHealth.ToString();

        // MP Calculation
        maxMana = StatFunction.statToMP(mind);
        manaSlider.maxValue = maxMana;
        manaText.text = mana.ToString() + " / " + maxMana.ToString();

        // Level Calculation
        int level = xp / 1000;
        levelText.text = string.Format("Level: {0} ({1} XP)", level, xp);

        // Gold
        goldText.text = "Gold: " + gold.ToString();

        // Send to Master
        object[] pkg = new object[] {
            health,
            maxHealth,
            mana,
            maxMana,
            level
        };
        PhotonNetwork.RaiseEvent(
            (byte) GameEventCodes.PLAYERSYNCSTATS,
            pkg,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
        );
    }

    public void OnEvent(EventData photonEvent) { 
        if (photonEvent.Code < 200) {
            GameEventCodes eventCode = (GameEventCodes) photonEvent.Code;
            object[] eventData = (object[]) photonEvent.CustomData;
            string sender = PhotonNetwork.CurrentRoom.GetPlayer(photonEvent.Sender).NickName;

            switch (eventCode) {
                case GameEventCodes.PLAYERINITSTATS:
                    ReceiveInitStats(eventData);
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
}
