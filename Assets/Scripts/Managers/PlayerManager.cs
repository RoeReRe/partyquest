using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlayerManager : MonoBehaviour
{
    public SessionInfo sessionInfo;
    public Dictionary<string, GameObject> playerList = new Dictionary<string, GameObject>();
    public Dictionary<string, int> playerID = new Dictionary<string, int>();
    public Dictionary<string, int> playerActor = new Dictionary<string, int>();

    private GameUIManager gameUIManager;

    private void Awake() {
        gameUIManager = FindObjectOfType<GameUIManager>();
        
        int idx = 0;
        
        foreach (Player player in PhotonNetwork.PlayerList) {
            if (!player.IsMasterClient) {
                GameObject playerObject = createPlayer(player.NickName);
                playerID.Add(player.NickName, idx++);
                playerActor.Add(player.NickName, player.ActorNumber);
                playerList.Add(player.NickName, playerObject);
                gameUIManager.createPlayer(player.NickName);

                object[] pkg = new object[] {
                    sessionInfo.getIndex(sessionInfo.getRace(player.NickName)),
                    sessionInfo.getIndex(sessionInfo.getClass(player.NickName)),
                    sessionInfo.getIndex(sessionInfo.getPrefab(player.NickName)),
                };
                PhotonNetwork.RaiseEvent(
                    (byte) GameEventCodes.PLAYERINITSTATS,
                    pkg,
                    new RaiseEventOptions { TargetActors = new int[] { player.ActorNumber } },
                    new SendOptions { Reliability = true }
                );
            }
        }
    }

    private GameObject createPlayer(string playerName) {
        GameObject temp = Instantiate(sessionInfo.getPrefab(playerName),
                Vector3.zero,
                Quaternion.identity)
                .gameObject;
        temp.name = playerName;

        return temp;
    }

    public List<string> getAllPlayerNames() {
        return playerList.Keys.ToList();
    }

    public List<GameObject> getAllPlayers() {
        return playerList.Values.ToList();
    }
    
    public int getID(string playerName) {
        return playerID[playerName];
    }

    public string getName(int id) {
        foreach (string playerName in playerID.Keys) {
            if (playerID[playerName] == id) {
                return playerName;
            }
        }
        return "";
    }

    public GameObject getPlayer(string playerName) {
        return playerList[playerName];
    }

    public int getTotal() {
        return playerList.Count;
    }

    public int getActor(string playerName) {
        return playerActor[playerName];
    }
}
