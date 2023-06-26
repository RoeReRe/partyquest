using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PassiveSkill : Skill
{
    public abstract void Effect();

    public virtual int duration { get; }
    public virtual int CD { get; }
    public virtual bool activateCondition() { return true; }
}
