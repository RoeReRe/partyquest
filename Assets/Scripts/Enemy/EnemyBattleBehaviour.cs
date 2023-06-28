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
    public abstract float startPriority { get; set; }
    public abstract int health { get; set; }
    public abstract int physicalAttack { get; set; }
    public abstract int magicalAttack { get; set; }
    public abstract int physicalDefence { get; set; }
    public abstract int magicalDefence  { get; set; }
    public abstract int shield { get; set; }
    public abstract BattleCodes weakness { get; set; }
    public abstract int gold { get; set; }
    public abstract int xp { get; set; }

    // Action List
    public List<Action> actionList = new List<Action>();

    // Common FX
    [SerializeField] ParticleSystem attackImpactEffect;
    [SerializeField] ParticleSystem shieldHitEffect;
    [SerializeField] ParticleSystem shieldBreakEffect;
    [SerializeField] ParticleSystem shieldRecoverEffect;
    [SerializeField] GameObject damageNumber;

    public virtual void Awake() {
        battleManager = FindObjectOfType<BattleManager>();
        unitName = this.gameObject.transform.Find("EnemyUICanvas/Name").gameObject.GetComponent<TMP_Text>();
        shieldImage = this.gameObject.transform.Find("EnemyUICanvas/Shield").gameObject.GetComponent<Image>();
        shieldCount = this.gameObject.transform.Find("EnemyUICanvas/Shield/ShieldCount").gameObject.GetComponent<TMP_Text>();
        unitName.text = this.gameObject.name.Replace("(Clone)", "").Trim();
        UpdateShieldUI();
    }

    public void UpdateShieldUI() {
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
            case BattleCodes.SKILL:
                ReceiveSkill(actionInfo, sender);
                break;
        }
    }

    public void ReceiveAttack(Dictionary<BattleCodes, object> actionInfo, string sender) {
        int receivedDamage;
        if ((BattleCodes) actionInfo[BattleCodes.DAMAGE_TYPE] == BattleCodes.DAMAGE_PHYSICAL) {
            receivedDamage = StatFunction.EnemyDamageToHP((int) actionInfo[BattleCodes.DAMAGE_NUMBER], physicalDefence, shield > 0);
        } else {
            receivedDamage = StatFunction.EnemyDamageToHP((int) actionInfo[BattleCodes.DAMAGE_NUMBER], magicalDefence, shield > 0);
        }
        StartCoroutine(PlayImpactAnimation(receivedDamage));
        health -= receivedDamage;

        if (health <= 0) {
            StartCoroutine(OnDeath());
            return;
        }

        if ((BattleCodes) actionInfo[BattleCodes.DAMAGE_TYPE] == weakness && shield > 0) {
            StartCoroutine(BreakShield((int) actionInfo[BattleCodes.HIT_COUNT]));
        }
    }

    public void ReceiveSkill(Dictionary<BattleCodes, object> actionInfo, string sender) {
        string path = string.Format("Skills/{0}", (string) actionInfo[BattleCodes.SKILL_NAME]);
        Skill skill = (Skill) Resources.Load(path);
        StartCoroutine(useSkillCoroutine(skill));
        ReceiveAttack(actionInfo, sender);
    }

    private IEnumerator useSkillCoroutine(Skill skill) {
        skill.ReceiverAction(this.gameObject);
        yield return new WaitForEndOfFrame();
    }

    public IEnumerator PlayImpactAnimation(int damage) {
        yield return new WaitForSecondsRealtime(0.25f);
        ParticleSystem impact = Instantiate(attackImpactEffect, this.transform.position, Quaternion.identity);
        var emission = impact.emission;
        emission.rateOverTime = Math.Clamp(damage, 150, 1000);
        impact.Play(true);

        GameObject number = Instantiate(damageNumber, this.transform, false);
        number.GetComponentInChildren<TMP_Text>().text = damage.ToString();
    }

    public IEnumerator BreakShield(int count) {
        yield return new WaitForSecondsRealtime(0.25f);
        shield = Math.Max(shield - count, 0);
        if (shield > 0) {
            ParticleSystem effect = Instantiate(shieldHitEffect, shieldImage.transform.position, Quaternion.identity);
            effect.Play();
        } else {
            ParticleSystem effect = Instantiate(shieldBreakEffect, shieldImage.transform.position, Quaternion.identity);
            effect.Play();
            OnShieldBreak();
        }
        UpdateShieldUI();
    }

    public IEnumerator PlayShieldRecoverAnimation() {
        ParticleSystem effect = Instantiate(shieldRecoverEffect, shieldImage.transform.position, Quaternion.identity);
        effect.Play(true);
        yield return new WaitForEndOfFrame();
    }

    public IEnumerator OnDeath() {
        battleManager.setDead(this.gameObject.name, gold, xp);
        Destroy(this.gameObject, 1f);
        yield return new WaitForEndOfFrame();
    }

    public abstract void OnAction();
    public abstract void OnShieldBreak();
}
