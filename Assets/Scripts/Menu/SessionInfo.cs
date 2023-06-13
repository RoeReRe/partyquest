using UnityEngine;
using TMPro;
using System.Collections.Generic;

[CreateAssetMenu]
public class SessionInfo : ScriptableObject
{
    public List<Race> playerRaces = new List<Race>();
    public List<Class> playerClasses = new List<Class>();
    public List<PlayerMovement> playerPrefabs = new List<PlayerMovement>();
    public Dictionary<string, PlayerInfo> playerInfoMap = new Dictionary<string, PlayerInfo>();
    
    public void ReceiveRaceChange(object[] pkg, string playerName) {
        Race toRace = playerRaces[(int) pkg[0]];
        if (playerInfoMap.ContainsKey(playerName)) {
            playerInfoMap[playerName].playerRace = toRace;
        } else {
            PlayerInfo temp = new PlayerInfo();
            temp.playerRace = toRace;
            playerInfoMap.Add(playerName, temp);
        }
    }

    public void ReceiveClassChange(object[] pkg, string playerName) {
        Class toClass = playerClasses[(int) pkg[0]];
        if (playerInfoMap.ContainsKey(playerName)) {
            playerInfoMap[playerName].playerClass = toClass;
        } else {
            PlayerInfo temp = new PlayerInfo();
            temp.playerClass = toClass;
            playerInfoMap.Add(playerName, temp);
        }
    }

    public void ReceiveAvatarChange(object[] pkg, string playerName) {
        PlayerMovement toPrefab = playerPrefabs[(int) pkg[0]];
        if (playerInfoMap.ContainsKey(playerName)) {
            playerInfoMap[playerName].playerPrefab = toPrefab;
        } else {
            PlayerInfo temp = new PlayerInfo();
            temp.playerPrefab = toPrefab;
            playerInfoMap.Add(playerName, temp);
        }
    }

    public void ReceiveReadyChange(object[] pkg, string playerName) {
        playerInfoMap[playerName].isReady = (bool) pkg[0];
    }

    public List<TMP_Dropdown.OptionData> getRaceOptions() {
        List<TMP_Dropdown.OptionData> raceOptions = new List<TMP_Dropdown.OptionData>();
        foreach (Race race in playerRaces) {
            raceOptions.Add(new TMP_Dropdown.OptionData(race.nounName));
        }
        return raceOptions;
    }

    public List<TMP_Dropdown.OptionData> getClassOptions() {
        List<TMP_Dropdown.OptionData> classOptions = new List<TMP_Dropdown.OptionData>();
        foreach (Class job in playerClasses) {
            classOptions.Add(new TMP_Dropdown.OptionData(job.nounName));
        }
        return classOptions;
    }

    public List<TMP_Dropdown.OptionData> getAvatarOptions() {
        List<TMP_Dropdown.OptionData> avatarOptions = new List<TMP_Dropdown.OptionData>();
        foreach (PlayerMovement prefab in playerPrefabs) {
            Sprite prefabSprite = prefab.GetComponent<SpriteRenderer>().sprite;
            avatarOptions.Add(new TMP_Dropdown.OptionData(prefab.name, prefabSprite));
        }
        return avatarOptions;
    }

    public bool isPlayerReady(string playerName) {
        if (!playerInfoMap.ContainsKey(playerName)) {
            return false;
        }
        return playerInfoMap[playerName].isReady;
    }

    public string getRaceClass(string playerName) {
        return playerInfoMap[playerName].playerRace.adjectiveName
            + " "
            + playerInfoMap[playerName].playerClass.nounName;
    }

    public Sprite getAvatar(string playerName) {
        return playerInfoMap[playerName].playerPrefab.GetComponent<SpriteRenderer>().sprite;
    }

    public Sprite getAvatar(int index) {
        return playerPrefabs[index].GetComponent<SpriteRenderer>().sprite;
    }
    
    public int totalPlayers() {
        return playerInfoMap.Count;
    }

    public Race getRace(string playerName) {
        return playerInfoMap[playerName].playerRace;
    }

    public Race getRace(int index) {
        return playerRaces[index];
    }

    public Class getClass(string playerName) {
        return playerInfoMap[playerName].playerClass;
    }

    public Class getClass(int index) {
        return playerClasses[index];
    }
    
    public PlayerMovement getPrefab(string playerName) {
        return playerInfoMap[playerName].playerPrefab;
    }

    public int getIndex(Race race) {
        return playerRaces.IndexOf(race);
    }

    public int getIndex(Class job) {
        return playerClasses.IndexOf(job);
    }

    public int getIndex(PlayerMovement prefab) {
        return playerPrefabs.IndexOf(prefab);
    }
}

public class PlayerInfo {
    public Race playerRace { get; set; }
    public Class playerClass { get; set; }
    public PlayerMovement playerPrefab { get; set; }
    public bool isReady { get; set; } = false;
}
