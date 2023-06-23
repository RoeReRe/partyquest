using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum GameEventCodes : byte {
    GAMESTART,
    PLAYERINITSTATS, // To Host and Player
    PLAYERSYNCSTATS, // To Host
    PLAYERSTARTTURN, // To Player
    PLAYERMOVE, // To Host
    PLAYERCANENDTURN, // To Player
    BATTLESTART, // To Player
    RESUMEBOARD, // To Player
    PLAYERENDTURN, // To Host
    NOTIFYPLAYER, // To Player
    PLAYERNOTIFLEFT, // To Host
    PLAYERNOTIFRIGHT, // To Host
    PLAYERSTATUSCHANGE, // To Player
    PLAYERBATTLEWAIT, // To Player
    PLAYERBATTLEACTION, // To Player
    SENDBATTLEACTION, // To Host and Player
    REFRESHTARGETLIST, // To Player
}

public class GameManager : MonoBehaviour
{
    public ScriptableObject sessionInfo;
}
