using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class BoardPlayerTurnState : PlayerBoardState
{
    public BoardPlayerTurnState(PlayerUIManager context) : base(context)
    {
    }

    public override void OnEnter() {
        context.BoardUIState(true);
        context.ButtonState(true);
    }

    public override void OnExit() {
        context.ButtonState(false);
    }

    public override void OnMove() {
        Button moveButton = context.playerButtons
            .transform.Find("Utility/Move Button").gameObject
            .GetComponent<Button>();
        moveButton.interactable = false;

        object[] pkg = new object[0];
        PhotonNetwork.RaiseEvent(
            (byte) GameEventCodes.PLAYERMOVE,
            pkg,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
        );
    }

    public override void OnEndTurn() {
        object[] pkg = new object[0];
        PhotonNetwork.RaiseEvent(
            (byte) GameEventCodes.PLAYERENDTURN,
            pkg,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
        );

        context.ChangeState(new BoardOtherTurnState(context));
    }
}
