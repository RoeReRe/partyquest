using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileProperty : MonoBehaviour
{
    public Sprite unexploredSprite;
    public ParticleSystem openAnimation;
    private PlayerManager playerManager;

    private void Awake() {
        playerManager = GameObject.FindObjectOfType<PlayerManager>();
    }

    public Vector3 getPosition() {
        return this.transform.position;
    }
    
    public Vector3 getPosition(int playerID) {
        SpriteRenderer spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
        float x = spriteRenderer.bounds.size.x / 2;
        float y = spriteRenderer.bounds.size.y / 2;
        Vector3 offset = new Vector3(0.1f, 0.25f, 0f);
        Vector3 bottomLeft = transform.position - new Vector3(x, y, 0) + offset;
        Vector3 diagonal = (2 * new Vector3(x, y, 0)) - (2 * offset);
        
        int totalPlayer = playerManager.getTotal();
        if (totalPlayer == 1) {
            return getPosition();
        } else if (totalPlayer == 2) {
            return playerID == 0 ? bottomLeft : bottomLeft + diagonal;
        } else {
            Vector3 interval = diagonal / (totalPlayer - 1);
            return bottomLeft + (playerID * interval);
        }
    }
    
    public void exhaustTile() {
        GetComponent<SpriteRenderer>().sprite = null;
        GetComponentsInChildren<SpriteRenderer>()[1].color = new Color(1f, 1f, 1f, 0.75f);
    }
}
