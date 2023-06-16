using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum GameEventCodes : byte {
    GAMESTART,
    PLAYERINITSTATS, // To Player
    PLAYERSYNCSTATS, // To Host
    PLAYERSTARTTURN, // To Player
    PLAYERMOVE, // To Host
    PLAYERCANENDTURN, // To Player
    PLAYERENDTURN, // To Host
    NOTIFYPLAYER, // To Player
    PLAYERNOTIFLEFT, // To Host
    PLAYERNOTIFRIGHT, // To Host
    PLAYERSTATUSCHANGE, // To Player
}

public class GameManager : MonoBehaviour
{
    public ScriptableObject sessionInfo;
}
