using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActiveSkill : Skill
{
    [Header("Active Properties")]
    public int WT;
    public int CD;
    public int castTime;
    public bool returnToPos;
    public BattleCodes damageType = BattleCodes.NONE;
    public DamageAmount damageAmount = DamageAmount.NONE;
    public int hitCount { get; } = 0;
    public virtual bool useCondition() { return true; }

    public Dictionary<BattleCodes, object> GetSkillCode() {
        Dictionary<BattleCodes, object> code = new Dictionary<BattleCodes, object> {
            { BattleCodes.WAIT_TIME, (float) WT },
            { BattleCodes.COOL_DOWN, (float) CD },
            { BattleCodes.PLAYER_RETURN_TO_POS, returnToPos },
            { BattleCodes.CAST_TIME, (float) castTime }
        };

        if (damageType != BattleCodes.NONE) {
            code.Add(BattleCodes.DAMAGE_TYPE, damageType);
        }

        if (damageAmount != DamageAmount.NONE) {
            code.Add(BattleCodes.DAMAGE_AMOUNT, damageAmount);
        }
        
        if (hitCount != 0) {
            code.Add(BattleCodes.HIT_COUNT, hitCount);
        }

        return code;
    }
}
