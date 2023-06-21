using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class EnemyBattleBehaviour : MonoBehaviour
{
    protected BattleManager battleManager;

    // UI
    protected TMP_Text unitName;
    protected Image shieldImage;
    protected TMP_Text shieldCount;
    
    // Enemy status
    public abstract int health { get; set; }
    public abstract int physicalAttack { get; set; }
    public abstract int magicalAttack { get; set; }
    public abstract int physicalDefence { get; set; }
    public abstract int magicalDefence  { get; set; }
    public abstract int shield { get; set; }
    public abstract BattleCodes weakness { get; set; }

    // Action List
    public List<Action> actionList = new List<Action>();

    // Common FX
    [SerializeField] ParticleSystem shieldHitEffect;
    [SerializeField] ParticleSystem shieldBreakEffect;

    public virtual void Awake() {
        battleManager = FindObjectOfType<BattleManager>();
        unitName = this.gameObject.transform.Find("EnemyUICanvas/Name").gameObject.GetComponent<TMP_Text>();
        shieldImage = this.gameObject.transform.Find("EnemyUICanvas/Shield").gameObject.GetComponent<Image>();
        shieldCount = this.gameObject.transform.Find("EnemyUICanvas/Shield/ShieldCount").gameObject.GetComponent<TMP_Text>();
        unitName.text = this.gameObject.name.Replace("(Clone)", "").Trim();
        UpdateShieldUI();
    }

    private void UpdateShieldUI() {
        shieldCount.text = shield.ToString();
        if (shield <= 0) {
            shieldImage.gameObject.SetActive(false);
        } else {
            shieldImage.gameObject.SetActive(true);
        }
    }

    public void SendAction(Dictionary<BattleCodes, object> actionInfo) {
        object[] pkg = new object[] { CustomType.SerializeBattleCode(actionInfo) };
        battleManager.ReceiveAction(pkg, this.name);
    }

    public void ReceiveAction(Dictionary<BattleCodes, object> actionInfo, string sender) {
        switch ((BattleCodes) actionInfo[BattleCodes.ACTION_TYPE]) {
            case BattleCodes.ATTACK:
                ReceiveAttack(actionInfo, sender);
                break;
        }
    }

    public void ReceiveAttack(Dictionary<BattleCodes, object> actionInfo, string sender) {
        // Should abstract and rmb max
        int receivedDamage;
        if (shield <= 0) {
            receivedDamage = Math.Max((int) actionInfo[BattleCodes.DAMAGE_NUMBER] - physicalDefence, 0);
        } else {
            receivedDamage = Math.Max(((int) actionInfo[BattleCodes.DAMAGE_NUMBER] * 25 / 100) - physicalDefence, 0);
        }
        health -= receivedDamage;

        if ((BattleCodes) actionInfo[BattleCodes.DAMAGE_TYPE] == weakness && shield > 0) {
            BreakShield((int) actionInfo[BattleCodes.HIT_COUNT]);
        }

        Debug.Log(String.Format("{0} received {1} raw damage, which took {2} HP. Current HP: {3}.",
            this.gameObject.name,
            (int) actionInfo[BattleCodes.DAMAGE_NUMBER],
            receivedDamage,
            health));
    }

    public void BreakShield(int count) {
        shield = Math.Max(shield - count, 0);
        if (shield > 0) {
            ParticleSystem effect = Instantiate(shieldHitEffect, shieldImage.transform.position, Quaternion.identity);
            effect.Play();
        } else {
            ParticleSystem effect = Instantiate(shieldBreakEffect, shieldImage.transform.position, Quaternion.identity);
            effect.Play();
        }
        UpdateShieldUI();
    }

    public abstract void OnAction();
}
