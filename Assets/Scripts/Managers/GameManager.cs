using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum GameEventCodes : byte {
    GAMESTART,
    PLAYERINITSTATS,
    PLAYERSYNCSTATS,
    PLAYERSTARTTURN,
    PLAYERMOVE,
    PLAYERENDTURN,
}

public class GameManager : MonoBehaviour
{
    public ScriptableObject sessionInfo;
}
