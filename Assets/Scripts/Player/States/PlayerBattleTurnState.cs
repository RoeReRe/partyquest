using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlayerBattleTurnState : PlayerBoardState
{
    public PlayerBattleTurnState(PlayerUIManager context) : base(context)
    {
    }

    public override void OnEnter()
    {
        context.playerButtons.SetActive(false);
        context.playerBattleButtons.SetActive(true);
        context.BattleButtonState(true);
    }

    public override void OnAttack(string targetName) {
        context.playerBattle.Attack(targetName);
        context.ChangeBattleState(new OtherBattleTurnState(context));
    }

    public override void OnSkillChosen(string targetName, Skill skill) {
        context.playerBattle.Skill(targetName, skill);
        context.ChangeBattleState(new OtherBattleTurnState(context));
    }

    public override void OnGuard() {
        
    }
    
    public override void OnExit()
    {
        
    }
}
