using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit {
    private GameObject enemy;

    public EnemyUnit(BattleManager context, GameObject enemy, float priority) {
        this.priority = priority;
        this.battleManager = context;
        this.enemy = enemy;
        this.portrait = context.createPortrait(enemy.GetComponentInChildren<SpriteRenderer>().sprite);
    }

    public override void OnAction()
    {
        enemy.GetComponent<EnemyBattleBehaviour>().OnAction();
    }

    public override void SenderAction(Dictionary<BattleCodes, object> actionInfo)
    {
        battleManager.unitList[enemy.name].SetPriority((float) actionInfo[BattleCodes.WAIT_TIME]);
    }

    public override void ReceiveAction(object[] eventData, Dictionary<BattleCodes, object> actionInfo, string sender) {
        enemy.GetComponent<EnemyBattleBehaviour>().ReceiveAction(actionInfo, sender);
    }
}