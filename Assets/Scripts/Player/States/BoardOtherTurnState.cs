using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardOtherTurnState : PlayerBoardState
{
    public BoardOtherTurnState(PlayerUIManager context) : base(context)
    {
    }

    public override void OnEnter() {
        context.BoardUIState(true);
        context.ButtonState(false);
    }

    public override void OnExit() {
        return;
    }
}
