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
using System.Linq;

public enum StatusChangeType : byte {
    CHANGESTAT,
    SETSTAT,
    ABSOLUTE,
    PERCENTAGE,
}

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
        
        health = StatFunction.VitalityToHP(vitality);
        mana = StatFunction.MindToMP(mind);
        gold = 100;

        initDisplay();
    }

    private void initDisplay() {
        updateDisplay();

        // Send to Master
        object[] pkg = new object[] {
            health,
            maxHealth,
            mana,
            maxMana,
            xp / 1000
        };
        PhotonNetwork.RaiseEvent(
            (byte) GameEventCodes.PLAYERINITSTATS,
            pkg,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
        );
    }

    private void syncDisplay() {
        updateDisplay();
        
        // Send to Master
        object[] pkg = new object[] {
            health,
            maxHealth,
            mana,
            maxMana,
            xp / 1000
        };
        PhotonNetwork.RaiseEvent(
            (byte) GameEventCodes.PLAYERSYNCSTATS,
            pkg,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
        );
    }

    private void updateDisplay() {
        // HP Calculation
        maxHealth = StatFunction.VitalityToHP(vitality);
        healthSlider.maxValue = Math.Max(0, maxHealth);
        healthSlider.value = Math.Max(0, health);
        healthText.text = health.ToString() + " / " + maxHealth.ToString();

        // MP Calculation
        maxMana = StatFunction.MindToMP(mind);
        manaSlider.maxValue = Math.Max(0, maxMana);
        manaSlider.value = Math.Max(0, mana);
        manaText.text = mana.ToString() + " / " + maxMana.ToString();

        // Level Calculation
        int level = xp / 1000;
        levelText.text = string.Format("Level: {0} ({1} XP)", Math.Max(0, level), Math.Max(0, xp));

        // Gold
        goldText.text = "Gold: " + Math.Max(0, gold).ToString();
    }

    private int getPropertyValue(string propertyName) {
        FieldInfo propertyField = this.GetType().GetField(propertyName);
        return (int) propertyField.GetValue(this);
    }

    private void setPropertyValue(string propertyName, int value) {
        FieldInfo propertyField = this.GetType().GetField(propertyName);
        propertyField.SetValue(this, value);
    }

    public IEnumerator changeStatus(List<Tuple<string, int>> changeList, float timeSeconds) {
        foreach (Tuple<string, int> change in changeList) {
            setPropertyValue(change.Item1, getPropertyValue(change.Item1) + change.Item2);
        }
        syncDisplay();
        if (timeSeconds != 0f) {
            yield return new WaitForSecondsRealtime(timeSeconds);
            changeList = changeList.Select(change => new Tuple<string, int>(change.Item1, -change.Item2)).ToList();
            StartCoroutine(changeStatus(changeList, 0f));
        }
        yield return new WaitForEndOfFrame();
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
                case GameEventCodes.PLAYERSTATUSCHANGE:
                    ReceiveStatusChange(eventData);
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

    public void ReceiveStatusChange(object[] eventData) {
        // [Dictionary<string, int>, CHANGESTAT/SETSTAT, ABSOLUTE/PERCENTAGE, DurationInFloatSeconds]
        Dictionary<string, int> statChange = (Dictionary<string, int>) eventData[0];

        List<Tuple<string, int>> change = statChange.Keys.ToList()
            .Select(name => {
                return StatFunction.calculateChange(
                    name,
                    statChange[name],
                    getPropertyValue(name),
                    (StatusChangeType) eventData[1],
                    (StatusChangeType) eventData[2]
                );
            })
            .ToList();
        
        StartCoroutine(changeStatus(change, (float) eventData[3]));
    }
}
