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
    private Vector3 priorityBarLeftLocal;
    private Vector3 priorityBarRightLocal;

    private Vector3 ENEMY_POS_0 = new Vector3(4.6f, 0.55f, 0f);
    private Vector3 ENEMY_POS_1 = new Vector3(6.85f, -0.7f, 0f); 
    private Vector3 ENEMY_POS_2 = new Vector3(2.95f, -1.9f, 0f);
    
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
            (byte) GameEventCodes.PLAYERBATTLEWAIT,
            pkg,
            new RaiseEventOptions { Receivers = ReceiverGroup.Others },
            new SendOptions { Reliability = true }
        );

        yield return new WaitForSecondsRealtime(1f);
        Instantiate(background, background.transform.position, Quaternion.identity, this.transform);
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
            StartCoroutine(player.GetComponent<PlayerMovement>().runTo(getPlayerStartPost(playerName)));
            unitList.Add(playerName, new PlayerUnit(this, player, calculateStartPriority(player, battlePosition)));
        }

        RefreshTargetList();

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

    private Vector3 getPlayerStartPost(string playerName) {
        if (playerManager.getTotal() == 1) {
            return startPositionZero;
        }

        Vector3 interval = (startPositionFour - startPositionZero) / (playerManager.getTotal() - 1);
        return startPositionZero + (interval * playerManager.getID(playerName));
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
        GameObject temp = Instantiate(gameUIManager.portrait, Vector3.zero, Quaternion.identity, gameUIManager.battlePriorityBar.transform);
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
        unitList.Add(enemy.name, new EnemyUnit(this, newEnemy, UnityEngine.Random.Range(10f, 20f)));
    }

    public void RefreshTargetList() {
        List<string> allies = new List<string>();
        List<string> enemies = new List<string>();

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
        unitList[(string) actionInfo[BattleCodes.TARGET_NAME]].ReceiveAction(eventData, actionInfo, sender);
        unitList[sender].SetPriority((float) actionInfo[BattleCodes.WAIT_TIME]);
    }
}

public enum BattleCodes : byte {
    ACTION_TYPE,
    ATTACK,
    GUARD,
    SKILL,
    ITEM,
    RUN,

    DAMAGE_TYPE,
    DAMAGE_PHYSICAL,
    DAMAGE_MAGICAL,

    TARGET_NAME,
    DAMAGE_NUMBER,
    HIT_COUNT, 
    WAIT_TIME,
}

public abstract class Unit {
    public float priority;
    public GameObject portrait;
    public bool isIdle = true;
    public abstract void OnAction();
    public abstract void ReceiveAction(object[] eventData, Dictionary<BattleCodes, object> actionInfo, string sender);
    public void SetPriority(float priority) {
        this.priority = priority;
        isIdle = true;
    }
}

public class PlayerUnit : Unit {
    private BattleManager battleManager;
    private GameObject player;
    
    public PlayerUnit(BattleManager context, GameObject player, float priority) {
        this.priority = priority;
        this.player = player;
        this.battleManager = context;
        this.portrait = context.createPortrait(player.GetComponent<SpriteRenderer>().sprite);
    }

    public override void OnAction() {
        object[] pkg = new object[0];
        PhotonNetwork.RaiseEvent(
            (byte) GameEventCodes.PLAYERBATTLEACTION,
            pkg,
            new RaiseEventOptions { TargetActors = new int[] { battleManager.playerManager.getActor(player.name) }},
            new SendOptions { Reliability = true }
        );
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

public class EnemyUnit : Unit {
    private GameObject enemy;

    public EnemyUnit(BattleManager context, GameObject enemy, float priority) {
        this.priority = priority;
        this.enemy = enemy;
        this.portrait = context.createPortrait(enemy.GetComponentInChildren<SpriteRenderer>().sprite);
    }

    public override void OnAction()
    {
        enemy.GetComponent<EnemyBattleBehaviour>().OnAction();
    }

    public override void ReceiveAction(object[] eventData, Dictionary<BattleCodes, object> actionInfo, string sender) {
        enemy.GetComponent<EnemyBattleBehaviour>().ReceiveAction(actionInfo, sender);
    }
}
