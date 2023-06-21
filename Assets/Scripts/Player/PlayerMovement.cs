using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerManager playerManager;
    private Animator animatorObject;
    private int playerPosition;
    [SerializeField] float moveSpeed = 1f;

    private void Start() {
        playerManager = FindObjectOfType<PlayerManager>();
        animatorObject = GetComponent<Animator>();
        playerPosition = 0;
    }

    // Linear movement; Board movement controlled by BoardManager;
    public IEnumerator moveTo(int tileIndex) {
        Vector3 destination = GameObject.Find("Tile " + tileIndex.ToString())
            .GetComponent<TileProperty>()
            .getPosition(playerManager.getID(this.name));
        this.playerPosition = tileIndex;
        yield return StartCoroutine(moveTo(destination));
    }    

    IEnumerator moveTo(Vector3 destination) {
        animatorObject.SetBool("isWalking", true);
        yield return StartCoroutine(translateTo(destination));
        animatorObject.SetBool("isWalking", false);
    }

    public IEnumerator runTo(Vector3 destination) {
        animatorObject.SetBool("isRunning", true);
        yield return StartCoroutine(translateTo(destination));
        animatorObject.SetBool("isRunning", false);
    }

    IEnumerator translateTo(Vector3 destination) {
        SpriteRenderer thisSpriteRenderer = GetComponent<SpriteRenderer>();
        if (this.transform.position.x > destination.x) {
            thisSpriteRenderer.flipX = true;
        }

        while (this.transform.position != destination) {
            this.transform.position = Vector3.MoveTowards(this.transform.position, destination, moveSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        thisSpriteRenderer.flipX = false;
    }

    public int getPosition() {
        return this.playerPosition;
    }
}
