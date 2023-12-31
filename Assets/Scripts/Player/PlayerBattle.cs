using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

public class PlayerBattle : MonoBehaviourPunCallbacks, IOnEventCallback
{
    private PlayerStatus playerStatus;
    private PlayerUIManager playerUIManager;
    
    private void Awake() {
        playerStatus = GetComponent<PlayerStatus>();
        playerUIManager = GetComponent<PlayerUIManager>();
    } 

    public void Attack(string targetName) {
        int damage = StatFunction.StrengthToPhysical(playerStatus.strength);
        int hitCount = 1;
        float waitTime = 7f;
        
        Dictionary<BattleCodes, object> actionInfo = new Dictionary<BattleCodes, object> {
            { BattleCodes.ACTION_TYPE, BattleCodes.ATTACK },
            { BattleCodes.TARGET_NAME, targetName },
            { BattleCodes.DAMAGE_NUMBER, damage },
            { BattleCodes.DAMAGE_TYPE, BattleCodes.DAMAGE_PHYSICAL },
            { BattleCodes.HIT_COUNT, hitCount},
            { BattleCodes.WAIT_TIME, waitTime },
            { BattleCodes.PLAYER_RETURN_TO_POS, true }
        };
        
        SendAction(actionInfo);
    }

    public void Skill(string targetName, Skill skill) {
        Dictionary<BattleCodes, object> actionInfo = new Dictionary<BattleCodes, object> {
            { BattleCodes.ACTION_TYPE, BattleCodes.SKILL },
            { BattleCodes.SKILL_NAME, skill.skillName },
            { BattleCodes.TARGET_NAME, targetName }
        };
        
        ActiveSkill active = (ActiveSkill) skill;
        foreach (KeyValuePair<BattleCodes, object> kvp in active.GetSkillCode()) {
            actionInfo.Add(kvp.Key, kvp.Value);
        }

        if ((BattleCodes) actionInfo[BattleCodes.DAMAGE_TYPE] != BattleCodes.NONE) {{
            if ((BattleCodes) actionInfo[BattleCodes.DAMAGE_TYPE] == BattleCodes.DAMAGE_PHYSICAL) {
                actionInfo.Add(BattleCodes.DAMAGE_NUMBER, damageCalculator(skill, StatFunction.StrengthToPhysical(playerStatus.strength)));
            } else {
                actionInfo.Add(BattleCodes.DAMAGE_NUMBER, damageCalculator(skill, StatFunction.IntelligenceToMagic(playerStatus.intelligence)));
            }
        }}

        List<Tuple<string, int>> changeList = new List<Tuple<string, int>> { new Tuple<string, int>("mana", -active.MP) };
        StartCoroutine(playerStatus.changeStatus(changeList, 0f));
        StartCoroutine(playerUIManager.SetCooldown(skill, (float) actionInfo[BattleCodes.COOL_DOWN]));

        SendAction(actionInfo);
    }

    public int damageCalculator(Skill skill, int damage) {
        ActiveSkill active = (ActiveSkill) skill;
        damage = (int) Math.Round(damage * active.bonusDamageMultiplier);
        switch (active.damageAmount) {
            case DamageAmount.Light:
                break;
            case DamageAmount.Medium:
                damage *= 150 / 100;
                break;
            case DamageAmount.Heavy:
                damage *= 2;
                break;
            case DamageAmount.Severe:
                damage *= 250 / 100;
                break;
            case DamageAmount.Colossal:
                damage *= 3;
                break;
        }
        return damage;
    }

    public void OnEvent(EventData photonEvent) {
        if (photonEvent.Code < 200) {
            GameEventCodes eventCode = (GameEventCodes) photonEvent.Code;
            object[] eventData = (object[]) photonEvent.CustomData;
            string sender = PhotonNetwork.CurrentRoom.GetPlayer(photonEvent.Sender).NickName;

            switch (eventCode) {
                case GameEventCodes.SENDBATTLEACTION:
                    ReceiveAction(eventData, sender);
                    break;
            }
        }
    }

    public void SendAction(Dictionary<BattleCodes, object> actionInfo) {
        object[] pkg = new object[] { CustomType.SerializeBattleCode(actionInfo) };
        PhotonNetwork.RaiseEvent(
            (byte) GameEventCodes.SENDBATTLEACTION,
            pkg,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
        );
    }

    public void ReceiveAction(object[] eventData, string sender) {
        Dictionary<BattleCodes, object> actionInfo = CustomType.DeserializeBattleCode((Dictionary<byte, object>) eventData[0]);
        
        switch ((BattleCodes) actionInfo[BattleCodes.ACTION_TYPE]) {
            case BattleCodes.ATTACK:
                ReceiveAttack(actionInfo, sender);
                break;
        }
    }

    public void ReceiveAttack(Dictionary<BattleCodes, object> actionInfo, string sender) {
        int rawDamage = (int) actionInfo[BattleCodes.DAMAGE_NUMBER];
        int damageTaken = StatFunction.PhysicalToHP(rawDamage, playerStatus.vitality, playerStatus.strength);
        List<Tuple<string, int>> changeList = new List<Tuple<string, int>> { new Tuple<string, int>("health", -damageTaken) };
        StartCoroutine(playerStatus.changeStatus(changeList, 0f));
    }
}
