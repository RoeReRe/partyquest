using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuinBehaviour : TileBehaviour
{   
    public override string exhaustTitle => "Ancient Ruins";
    public override string exhaustBody => "Would you like to explore the ruins?";

    public override void openTile(BoardManager context, string playerName)
    {
        context.gameUIManager.logInfo(string.Format("[Board] {0} has discovered some ancient ruins.", playerName));
        StartCoroutine(context.promptExhaustTile(playerName, this));
    }

    public override void exhaustTile(BoardManager context, string playerName)
    {
        context.gameUIManager.logInfo(string.Format("[Board] {0} gained 200XP and 100 gold from exploring the ruins.", playerName));
        Dictionary<string, int> change = new Dictionary<string, int> {
            { "xp", 200 },
            { "gold", 100 },   
        };
        
        context.playerManager.changeStatus(
            new string[] { playerName },
            change,
            StatusChangeType.CHANGESTAT,
            StatusChangeType.ABSOLUTE,
            0f
        );
    }
}
