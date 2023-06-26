using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using ExitGames.Client.Photon;

public class PlayerUnit : Unit {
    private GameObject player;
    private PlayerMovement playerMovement;
    
    public PlayerUnit(BattleManager context, GameObject player, float priority) {
        this.priority = priority;
        this.player = player;
        this.playerMovement = player.GetComponent<PlayerMovement>();
        this.battleManager = context;
        this.portrait = context.createPortrait(player.GetComponent<SpriteRenderer>().sprite);
    }

    public override void OnAction() {
        Vector3 actionPosition = battleManager.getPlayerStartPost(player.name) + new Vector3(2.5f, 0f, 0f);
        if (player.transform.position != actionPosition) {
            battleManager.runPlayerTo(player.name, actionPosition);
        }
        
        object[] pkg = new object[0];
        PhotonNetwork.RaiseEvent(
            (byte) GameEventCodes.PLAYERBATTLEACTION,
            pkg,
            new RaiseEventOptions { TargetActors = new int[] { battleManager.playerManager.getActor(player.name) }},
            new SendOptions { Reliability = true }
        );
    }

    public override void SenderAction(Dictionary<BattleCodes, object> actionInfo) {
        playerMovement.playAnimation(actionInfo);
    }

    public void VisualCallback(Dictionary<BattleCodes, object> actionInfo) {
        battleManager.unitList[player.name].SetPriority((float) actionInfo[BattleCodes.WAIT_TIME]);
        
        if ((bool) actionInfo[BattleCodes.PLAYER_RETURN_TO_POS]) {
            battleManager.runPlayerTo(player.name, battleManager.getPlayerStartPost(player.name));
        }
    }

    public override void ReceiveAction(object[] eventData, Dictionary<BattleCodes, object> actionInfo, string sender) {
        PhotonNetwork.RaiseEvent(
            (byte) GameEventCodes.SENDBATTLEACTION,
            eventData,
            new RaiseEventOptions { TargetActors = new int[] { battleManager.playerManager.getActor(player.name) }},
            new SendOptions { Reliability = true }
        );
    }
}