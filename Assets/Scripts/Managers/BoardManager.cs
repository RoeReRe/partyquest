using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class BoardManager : StateMachine, IOnEventCallback
{
    public GameUIManager gameUIManager;
    public PlayerManager playerManager;
    private TileSpawner tileSpawner;
    private BattleManager battleManager;
    public int boardSize = 11;
    public int currentPlayer = 0;

    private bool playerNotifLeft;
    private bool playerNotifRight;
    
    public GameObject[] enemyList;

    private void Awake() {
        battleManager = FindObjectOfType<BattleManager>();
        tileSpawner = FindObjectOfType<TileSpawner>();
        tileSpawner.startBoard(boardSize);

        enemyList = Resources.LoadAll<GameObject>("Enemies/Base");
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

    public IEnumerator movePlayerTo(PlayerMovement player, int tileIndex) {
        yield return StartCoroutine(movePlayerToRoutine(player, tileIndex));
    }

    public IEnumerator movePlayerTo(string playerName, int tileIndex) {
        yield return StartCoroutine(movePlayerTo(playerManager.getPlayer(playerName).GetComponent<PlayerMovement>(), tileIndex));
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
        gameUIManager.logInfo(String.Format("[Board] {0}'s turn to act.", playerManager.getName(playerID)));
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

    public void allowPlayerEndTurn(string playerName) {
        object[] pkg = new object[0];
        PhotonNetwork.RaiseEvent(
            (byte) GameEventCodes.PLAYERCANENDTURN,
            pkg,
            new RaiseEventOptions { TargetActors = new int[] { playerManager.getActor(playerName) } },
            new SendOptions { Reliability = true }
        );
    } 

    private IEnumerator waitForPlayerResponse(Action leftFunc, Action rightFunc) {
        yield return new WaitUntil(() => playerNotifLeft || playerNotifRight);
        
        if (playerNotifLeft) {
            playerNotifLeft = false;
            playerNotifRight = false;
            leftFunc.Invoke();
        } else {
            playerNotifLeft = false;
            playerNotifRight = false;
            rightFunc.Invoke();
        }
    }

    public IEnumerator promptExploreTile(string playerName, TileBehaviour tile) {
        object[] pkg = new object[] {
            "Unexplored Tile",
            "Would you like to explore the tile?",
            "Yes", true,
            "No", true
        };
        PhotonNetwork.RaiseEvent(
            (byte) GameEventCodes.NOTIFYPLAYER,
            pkg,
            new RaiseEventOptions { TargetActors = new int[] { playerManager.getActor(playerName) }},
            new SendOptions { Reliability = true }
        );

        yield return StartCoroutine(waitForPlayerResponse(() => tile.openTileSequence(this, playerName), () => allowPlayerEndTurn(playerName)));
    }
    
    public IEnumerator promptExhaustTile(string playerName, TileBehaviour tile) {
        gameUIManager.notificationScreen.initNotification(tile.exhaustTitle, tile.exhaustBody, tile.openSprite);
        
        object[] pkg = new object[] {
            tile.exhaustTitle,
            tile.exhaustBody,
            "Yes", true,
            "No", true
        };
        PhotonNetwork.RaiseEvent(
            (byte) GameEventCodes.NOTIFYPLAYER,
            pkg,
            new RaiseEventOptions { TargetActors = new int[] { playerManager.getActor(playerName) }},
            new SendOptions { Reliability = true }
        );

        yield return StartCoroutine(waitForPlayerResponse(() => tile.exhaustTileSequence(this, playerName), () => { return; } ));

        gameUIManager.closeNotification();
        allowPlayerEndTurn(playerName);
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
                    StartCoroutine(ReceivePlayerMove(sender));
                    break;
                case GameEventCodes.PLAYERENDTURN:
                    ReceivePlayerEndTurn();
                    break;
                case GameEventCodes.PLAYERNOTIFLEFT:
                    playerNotifLeft = true;
                    break;
                case GameEventCodes.PLAYERNOTIFRIGHT:
                    playerNotifRight = true;
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
        // GameUI also calls this when loading of stats are done
        if (currentPlayer >= 2 * playerManager.getTotal()) {
            currentPlayer = 0;
            gameUIManager.setLoadingScreen(false);
            gameUIManager.logInfo("[Board] Game started.");
            startPlayerTurn(0);
            StartCoroutine(testStartBattle());
        }
    }

    public GameObject background;
    IEnumerator testStartBattle() {
        yield return new WaitForRealSeconds(5f);
        StartCoroutine(battleManager.StartBattleScene(0, playerManager.getAllPlayerNames().ToArray(), enemyList, background));
    }

    public IEnumerator ReceivePlayerMove(string playerName) {
        int steps = UnityEngine.Random.Range(0, 12);
        gameUIManager.logInfo(String.Format("[Board] {0} rolled {1}.", playerName, steps.ToString()));
        
        int maxIndex = (boardSize * 4) - 4;
        int initPos = playerManager.getPlayer(playerName).GetComponent<PlayerMovement>().getPosition();
        int finalPos = (initPos + steps) % maxIndex;
        yield return StartCoroutine(movePlayerTo(playerName, finalPos));

        TileBehaviour tile = GameObject.Find("Tile " + finalPos).gameObject.GetComponent<TileBehaviour>();

        switch (tile.currentState) {
            case TileState.UNEXPLORED:
                StartCoroutine(promptExploreTile(playerName, tile));
                break;
            case TileState.OPENED:
                StartCoroutine(promptExhaustTile(playerName, tile));
                break;
            case TileState.EXHAUSTED:
                allowPlayerEndTurn(playerName);
                break;
            case TileState.SPONTANEOUS:
                tile.exhaustTileSequence(this, playerName);
                break;
        }
    }
    
    public void ReceivePlayerEndTurn() {
        incrementNextPlayer();
        startPlayerTurn(currentPlayer);
    }
}
