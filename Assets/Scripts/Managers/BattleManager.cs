using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Linq;

public class BattleManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    internal GameUIManager gameUIManager;
    internal PlayerManager playerManager;
    internal BoardManager boardManager;
    private bool isTimeFrozen = true;
    private Vector3 startPositionZero = new Vector3(-0.85f, 0.45f, 0);
    private Vector3 startPositionFour = new Vector3(-3.85f, -2.2f, 0);
    public Dictionary<string, Unit> unitList = new Dictionary<string, Unit>();
    List<string> allies = new List<string>();
    List<string> enemies = new List<string>();
    private Vector3 priorityBarLeftLocal;
    private Vector3 priorityBarRightLocal;

    private Vector3 ENEMY_POS_0 = new Vector3(4.6f, 0.55f, 0f);
    private Vector3 ENEMY_POS_1 = new Vector3(6.85f, -0.7f, 0f); 
    private Vector3 ENEMY_POS_2 = new Vector3(2.95f, -1.9f, 0f);

    private int goldReward;
    private int xpReward;
    
    private void Awake() {
        gameUIManager = FindObjectOfType<GameUIManager>();
        playerManager = FindObjectOfType<PlayerManager>();
        boardManager = FindObjectOfType<BoardManager>();

        Vector3[] localCorners = new Vector3[4];
        gameUIManager.battlePriorityBar.GetComponent<RectTransform>().GetLocalCorners(localCorners);
        priorityBarLeftLocal = (localCorners[0] + localCorners[1]) / 2;
        priorityBarRightLocal = (localCorners[2] + localCorners[3]) / 2;
    }
    
    public IEnumerator StartBattleScene(int battlePosition, string[] players, GameObject[] enemies, GameObject background) {
        gameUIManager.logInfo("[Battle] Battle started.");
        
        gameUIManager.screenWipeIn();
        
        object[] pkg = new object[0];
        PhotonNetwork.RaiseEvent(
            (byte) GameEventCodes.BATTLESTART,
            pkg,
            new RaiseEventOptions { Receivers = ReceiverGroup.Others },
            new SendOptions { Reliability = true }
        );

        yield return new WaitForSecondsRealtime(1f);
        Instantiate(background, background.transform.position, Quaternion.identity, this.transform);
        goldReward = 0;
        xpReward = 0;
        unitList.Clear();
        gameUIManager.battlePriorityBar.SetActive(true);
        isTimeFrozen = true;

        spawnEnemies(enemies);

        gameUIManager.screenFadeOut();
        
        // Spawn players, set scale
        foreach (string playerName in players) {
            GameObject player = playerManager.getPlayer(playerName);
            player.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            player.transform.position = getPlayerStartPost(playerName) - new Vector3(UnityEngine.Random.Range(5f, 10f), 0f, 0f);
            runPlayerTo(playerName, getPlayerStartPost(playerName));
            unitList.Add(playerName, new PlayerUnit(this, player, calculateStartPriority(player, battlePosition)));
        }

        RefreshTargetList();

        yield return new WaitForSecondsRealtime(2f);
        isTimeFrozen = false;
    }

    private void Update() {
        if (!isTimeFrozen) {
            foreach (Unit unit in unitList.Values.ToList()) {
                if (unit.priority > 0) {
                    unit.priority -= Time.deltaTime;
                } else if (unit.isIdle) {
                    unit.isIdle = false;
                    unit.OnAction();
                }
                unit.portrait.transform.localPosition = getLocalPriorityPosition(unit.priority);
            }
        }
    }

    public Vector3 getPlayerStartPost(string playerName) {
        if (playerManager.getTotal() == 1) {
            return startPositionZero;
        }

        Vector3 interval = (startPositionFour - startPositionZero) / (playerManager.getTotal() - 1);
        return startPositionZero + (interval * playerManager.getID(playerName));
    }

    public void runPlayerTo(string playerName, Vector3 destination) {
        StartCoroutine(playerManager.getPlayer(playerName).GetComponent<PlayerMovement>().runTo(destination));
    }

    private float calculateStartPriority(GameObject player, int battlePosition) {
        int maxTiles = (boardManager.boardSize * 4) - 4;
        int playerPosition = player.GetComponent<PlayerMovement>().getPosition();
        int forwardDistance = Mathf.Abs(battlePosition - playerPosition);
        int backDistance = Mathf.Abs(maxTiles - battlePosition + playerPosition);
        return linearPriorityFunction(Mathf.Min(forwardDistance, backDistance));
    }

    private float linearPriorityFunction(float x) {
        return Mathf.Clamp(0.75f * x, 0f, 15f);
    }

    private Vector3 getLocalPriorityPosition(float priority) {
        float maxValue = 60f;
        return priorityBarLeftLocal + ((priorityBarRightLocal - priorityBarLeftLocal) * priority / maxValue); 
    }

    public GameObject createPortrait(Sprite avatar) {
        GameObject temp = Instantiate(gameUIManager.portrait, Vector3.forward, Quaternion.identity, gameUIManager.battlePriorityBar.transform);
        temp.transform.Find("Mask/Avatar")
            .gameObject
            .GetComponent<Image>()
            .sprite = avatar;
        return temp;
    }

    public void spawnEnemies(GameObject[] enemies) {
        if (enemies.Length == 1) {
            spawnEnemyAtPosition(enemies[0], 1);
        } else if (enemies.Length == 2) {
            spawnEnemyAtPosition(enemies[0], 0);
            spawnEnemyAtPosition(enemies[0], 2);
        } else {
            for (int i = 0; i < enemies.Length; i++) {
                spawnEnemyAtPosition(enemies[i], i);
            }
        }
    }

    public void spawnEnemyAtPosition(GameObject enemy, int pos) {
        Vector3 temp;
        if (pos == 0) {
            temp = ENEMY_POS_0;
        } else if (pos == 1) {
            temp = ENEMY_POS_1;
        } else {
            temp = ENEMY_POS_2;
        }
        GameObject newEnemy = Instantiate(enemy, temp, Quaternion.identity, this.transform);
        newEnemy.name = enemy.name;
        
        float startPriority = newEnemy.GetComponent<EnemyBattleBehaviour>().startPriority;
        startPriority = startPriority + Mathf.Min(UnityEngine.Random.Range(0f, startPriority), 15f);
        
        unitList.Add(enemy.name, new EnemyUnit(this, newEnemy, startPriority));
    }

    public void RefreshTargetList() {
        allies.Clear();
        enemies.Clear();

        foreach (string unitName in unitList.Keys.ToList()) {
            if (unitList[unitName].GetType() == typeof(PlayerUnit)) {
                allies.Add(unitName);
            } else {
                enemies.Add(unitName);
            }
        }

        object[] pkg = new object[] { allies.ToArray(), enemies.ToArray() };
        PhotonNetwork.RaiseEvent(
            (byte) GameEventCodes.REFRESHTARGETLIST,
            pkg,
            new RaiseEventOptions { Receivers = ReceiverGroup.Others },
            new SendOptions { Reliability = true }
        );
    }

    public void setDead(string unitName, int gold, int xp) {
        goldReward += gold;
        xpReward += xp;
        unitList.Remove(unitName);
        RefreshTargetList();

        if (!enemies.Any()) {
            StartCoroutine(ChangeToVictoryState());
        }
    }

    private IEnumerator ChangeToVictoryState() {
        yield return new WaitForRealSeconds(2f);
        
        isTimeFrozen = true;
        
        // Disable controls
        PhotonNetwork.RaiseEvent(
            (byte) GameEventCodes.PLAYERBATTLEWAIT,
            new object[0],
            new RaiseEventOptions { Receivers = ReceiverGroup.Others },
            new SendOptions { Reliability = true }
        );
        
        // Show rewards
        gameUIManager.rewardScreen.SetActive(true);
        gameUIManager.setRewardScreen(goldReward, xpReward);

        // Give rewards
        playerManager.changeStatus(
            playerManager.getAllPlayerNames().ToArray(),
            new Dictionary<string, int> {
                { "gold", goldReward },
                { "xp", xpReward }
            },
            StatusChangeType.CHANGESTAT,
            StatusChangeType.ABSOLUTE,
            0f
        );

        yield return new WaitForRealSeconds(5f);
        gameUIManager.screenWipeIn();
        yield return new WaitForSecondsRealtime(1f);

        gameUIManager.rewardScreen.SetActive(false);
        gameUIManager.battlePriorityBar.SetActive(false);
        
        foreach (Transform child in this.gameObject.transform) {
            Destroy(child.gameObject);
        }

        // Respawn players back in board
        foreach (string playerName in playerManager.getAllPlayerNames()) {
            GameObject player = playerManager.getPlayer(playerName);
            player.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            player.transform.position = GameObject.Find("Tile " + player.GetComponent<PlayerMovement>().getPosition().ToString())
                .GetComponent<TileProperty>()
                .getPosition(playerManager.getID(player.name));    
        }

        // Allow controls
        PhotonNetwork.RaiseEvent(
            (byte) GameEventCodes.RESUMEBOARD,
            new object[0],
            new RaiseEventOptions { Receivers = ReceiverGroup.Others },
            new SendOptions { Reliability = true }
        );

        gameUIManager.screenFadeOut();
    }

    public void OnEvent(EventData photonEvent) {
        if (photonEvent.Code < 200) {
            GameEventCodes eventCode = (GameEventCodes) photonEvent.Code;
            object[] eventData = (object[]) photonEvent.CustomData;
            string sender = PhotonNetwork.CurrentRoom.GetPlayer(photonEvent.Sender).NickName;

            switch (eventCode) {
                case GameEventCodes.SENDBATTLEACTION:
                    ReceiveAction(eventData, sender);
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

    public void ReceiveAction(object[] eventData, string sender) {
        Dictionary<BattleCodes, object> actionInfo = CustomType.DeserializeBattleCode((Dictionary<byte, object>) eventData[0]);
        unitList[sender].SenderAction(actionInfo);
        unitList[(string) actionInfo[BattleCodes.TARGET_NAME]].ReceiveAction(eventData, actionInfo, sender);
    }
}

public enum BattleCodes : byte {
    NONE,    
    
    ACTION_TYPE,
    ATTACK,
    GUARD,
    SKILL,
    ITEM,
    RUN,

    DAMAGE_TYPE,
    DAMAGE_PHYSICAL,
    DAMAGE_MAGICAL,
    DAMAGE_AMOUNT,

    TARGET_NAME,
    DAMAGE_NUMBER,
    HIT_COUNT,
    COOL_DOWN, 
    WAIT_TIME,
    PLAYER_RETURN_TO_POS,

    SKILL_NAME,
    CAST_TIME,

    EFFECT_WAIT_TIME,
}

public abstract class Unit {
    public BattleManager battleManager;
    public float priority;
    public GameObject portrait;
    public bool isIdle = true;
    public abstract void OnAction();
    public abstract void SenderAction(Dictionary<BattleCodes, object> actionInfo);
    public abstract void ReceiveAction(object[] eventData, Dictionary<BattleCodes, object> actionInfo, string sender);
    public void SetPriority(float priority) {
        this.priority = priority;
        isIdle = true;
    }
}
