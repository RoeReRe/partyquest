using UnityEngine;

public enum TileState : byte {
    UNEXPLORED,
    OPENED,
    EXHAUSTED,
}

public abstract class TileBehaviour : MonoBehaviour
{
    public TileState currentState = TileState.UNEXPLORED;
    public Sprite openSprite;
    public abstract void openTile();
    public abstract void exhaustTile();
    public void changeState(TileState newState) {
        currentState = newState;
    }
}
