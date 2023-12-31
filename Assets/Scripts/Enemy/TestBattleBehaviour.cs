using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBattleBehaviour : EnemyBattleBehaviour
{
    public override float startPriority { get; set; } = 10f;
    public override int health { get; set; } = 300;
    public override int physicalAttack { get; set; } = 20;
    public override int magicalAttack { get; set; } = 5;
    public override int physicalDefence { get; set; } = 10;
    public override int magicalDefence { get; set; } = 5;
    public override int shield { get; set; } = 5;
    public override BattleCodes weakness { get; set; } = BattleCodes.DAMAGE_MAGICAL;
    public override int gold { get; set; } = 200;
    public override int xp { get; set; } = 500;

    public override void Awake() {
        base.Awake();
    }

    public override void OnAction()
    {
         if (shield == 0) {
            shield = 5;
            StartCoroutine(PlayShieldRecoverAnimation());
            UpdateShieldUI();
            battleManager.unitList[unitName.text].SetPriority(15f);
            return;
         }
         
         Dictionary<BattleCodes, object> actionInfo = new Dictionary<BattleCodes, object> {
            { BattleCodes.ACTION_TYPE, BattleCodes.ATTACK },
            { BattleCodes.TARGET_NAME, battleManager.playerManager.getName(0) },
            { BattleCodes.DAMAGE_NUMBER, 90 },
            { BattleCodes.DAMAGE_TYPE, BattleCodes.DAMAGE_PHYSICAL },
            { BattleCodes.HIT_COUNT, 1},
            { BattleCodes.WAIT_TIME, 15f }
        };
        SendAction(actionInfo);
    }

    public override void OnShieldBreak()
    {
        battleManager.unitList[unitName.text].SetPriority(15f);
    }
}
