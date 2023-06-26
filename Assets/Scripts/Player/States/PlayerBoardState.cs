using UnityEngine;

public abstract class PlayerBoardState : State {
    protected new PlayerUIManager context;

    public PlayerBoardState(PlayerUIManager context) : base(context)
    {
        this.context = context;
    }

    public virtual void OnMove() {}
    public virtual void OnSkill() {}
    public virtual void OnItem() {}
    public virtual void OnEquip() {}
    public virtual void OnEndTurn() {}

    public virtual void OnAttack(string targetName) {}
    public virtual void OnSkillChosen(string targetName, Skill skill) {}
    public virtual void OnGuard() {}
    public virtual void OnRun() {}
}