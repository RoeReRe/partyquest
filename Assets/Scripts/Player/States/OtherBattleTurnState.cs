using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherBattleTurnState : PlayerBoardState
{
    public OtherBattleTurnState(PlayerUIManager context) : base(context)
    {
    }

    public override void OnEnter()
    {
        context.playerButtons.SetActive(false);
        context.playerBattleButtons.SetActive(true);
        context.BattleButtonState(false);
    }

    public override void OnExit()
    {
        
    }
}
