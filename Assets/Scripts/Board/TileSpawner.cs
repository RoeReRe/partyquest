using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    public GameObject parent;
    private GameObject[] tileList;
    private GameObject[] tileSet;

    public void startBoard(int size) {
        // Retrieve tile asset
        tileList = Resources.LoadAll<GameObject>("Tiles");

        // Generate tile set
        generateTileSet(size);

        makeBoard(size);
    }

    private void generateTileSet(int size) {
        int totalTiles = (size * 4) - 4;
        tileSet = new GameObject[totalTiles];

        for (int i = 0; i < totalTiles; i++) {
            tileSet[i] = (GameObject) tileList[UnityEngine.Random.Range(0, tileList.Length)];
        }
    }

    private void makeBoard(int size) {
        float tileWidth = tileSet[0].GetComponent<SpriteRenderer>().bounds.size.x;
        float tileHeight = tileSet[0].GetComponent<SpriteRenderer>().bounds.size.y;

        float xMax = tileWidth * (size / 2);
        float yMax = tileHeight * (size / 2);
        float xPos = -xMax;
        float yPos = yMax;
        
        GameObject temp;
        int index = 0;
        
        for (int i = 0; i < size; i++) {
            temp = Instantiate(tileSet[index], parent.transform, false);
            temp.transform.localPosition = new Vector3(xPos, yPos, 0);
            temp.name = "Tile " + index.ToString();
            index++;
            xPos += tileWidth;
        }
        xPos = xMax;
        yPos -= tileHeight;
        for (int i = 0; i < size - 1; i++) {
            temp = Instantiate(tileSet[index], parent.transform, false);
            temp.transform.localPosition = new Vector3(xPos, yPos, 0);
            temp.name = "Tile " + index.ToString();
            index++;
            yPos -= tileHeight;
        }
        xPos -= tileWidth;
        yPos = -yMax;
        for (int i = 0; i < size - 1; i++) {
            temp = Instantiate(tileSet[index], parent.transform, false);
            temp.transform.localPosition = new Vector3(xPos, yPos, 0);
            temp.name = "Tile " + index.ToString();
            index++;
            xPos -= tileWidth;
        }
        xPos = -xMax;
        yPos += tileHeight;
        for (int i = 0; i < size - 2; i++) {
            temp = Instantiate(tileSet[index], parent.transform, false);
            temp.transform.localPosition = new Vector3(xPos, yPos, 0);
            temp.name = "Tile " + index.ToString();
            index++;
            yPos += tileHeight;
        }
    }
}
