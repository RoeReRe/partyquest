using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class BoardManager : StateMachine, IOnEventCallback
{
    public PlayerManager playerManager;
    private TileSpawner tileSpawner;
    public int boardSize = 11;
    public int currentPlayer = 0;
    
    private void Awake() {
        TileSpawner tileSpawner = FindObjectOfType<TileSpawner>();
        tileSpawner.startBoard(boardSize);
    }

    private void Start() {
        spawnPlayers();

        object[] pkg = new object[0];
        PhotonNetwork.RaiseEvent(
            (byte) GameEventCodes.GAMESTART,
            pkg,
            new RaiseEventOptions { Receivers = ReceiverGroup.Others },
            new SendOptions { Reliability = true }
        );
    }

    private void spawnPlayers() {
        TileProperty startTile = GameObject.Find("Tile 0").GetComponent<TileProperty>();
        
        foreach (GameObject playerObject in playerManager.getAllPlayers()) {
            Vector3 startPosition = startTile.getPosition(playerManager.getID(playerObject.name));
            playerObject.transform.position = startPosition;
        }
    }

    public void movePlayerTo(PlayerMovement player, int tileIndex) {
        StartCoroutine(movePlayerToRoutine(player, tileIndex));
    }

    public void movePlayerTo(string playerName, int tileIndex) {
        movePlayerTo(playerManager.getPlayer(playerName).GetComponent<PlayerMovement>(), tileIndex);
    }

    IEnumerator movePlayerToRoutine(PlayerMovement player, int tileIndex) {
        int start = player.getPosition();
        int steps = tileIndex - start;

        if (steps < 0) {
            steps = (boardSize * 4) - 4 - start + tileIndex;
        }
        
        while (steps > 0) {
            int tempStep = boardSize - 1 - start % (boardSize - 1);
            tempStep = Math.Min(tempStep, steps);
            
            start += tempStep;
            if (start > (boardSize * 4) - 5) {
                start = 0;
            }

            yield return StartCoroutine(player.moveTo(start));
            steps -= tempStep;
        }
    }

    public void startPlayerTurn(int playerID) {
        int actorID = playerManager.getActor(playerManager.getName(playerID));

        object[] pkg = new object[0];
        PhotonNetwork.RaiseEvent(
            (byte) GameEventCodes.PLAYERSTARTTURN,
            pkg,
            new RaiseEventOptions { TargetActors = new int[] { actorID } },
            new SendOptions { Reliability = true }
        );
    }

    private void incrementNextPlayer() {
        currentPlayer = ++currentPlayer % playerManager.getTotal();
    }
    
    public void OnEvent(EventData photonEvent) {
        if (photonEvent.Code < 200) {
            GameEventCodes eventCode = (GameEventCodes) photonEvent.Code;
            object[] eventData = (object[]) photonEvent.CustomData;
            string sender = PhotonNetwork.CurrentRoom.GetPlayer(photonEvent.Sender).NickName;

            switch (eventCode) {
                case GameEventCodes.GAMESTART:
                    ReceiveGameStart();
                    break;
                case GameEventCodes.PLAYERMOVE:
                    ReceivePlayerMove(sender);
                    break;
                case GameEventCodes.PLAYERENDTURN:
                    ReceivePlayerEndTurn();
                    break;
            }
        }
    }
    
    public override void OnEnable() {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable() {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    
    public void ReceiveGameStart() {
        currentPlayer++;
        if (currentPlayer >= playerManager.getTotal()) {
            currentPlayer = 0;
            startPlayerTurn(0);
        }
    }

    public void ReceivePlayerMove(string playerName) {
        int maxIndex = (boardSize * 4) - 4;
        int steps = UnityEngine.Random.Range(0, 12);
        int initPos = playerManager.getPlayer(playerName).GetComponent<PlayerMovement>().getPosition();
        int finalPos = (initPos + steps) % maxIndex;
        movePlayerTo(playerName, finalPos);
    }
    
    public void ReceivePlayerEndTurn() {
        incrementNextPlayer();
        startPlayerTurn(currentPlayer);
    }
}
