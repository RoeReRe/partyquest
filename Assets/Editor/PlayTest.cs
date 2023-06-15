using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlayTest : EditorWindow {

    public string tileIndex = "";
    public string playerID = "";

    [MenuItem("Party Quest/PlayTest")]
    private static void ShowWindow() {
        var window = GetWindow<PlayTest>();
        window.titleContent = new GUIContent("PlayTest");
        window.Show();
    }

    private void OnGUI() {
        BoardManager board = FindObjectOfType<BoardManager>();
        PlayerManager playerManager = FindObjectOfType<PlayerManager>();

        GUILayout.Box("Player ID");
        playerID = GUILayout.TextField(playerID);
        GUILayout.Box("Move to");
        tileIndex = GUILayout.TextField(tileIndex);
        // if (GUILayout.Button("Run")) {
        //     board.movePlayerTo(playerManager.getName(int.Parse(playerID)) , int.Parse(tileIndex)); 
        // }
    }
}
