using UnityEngine;

public enum TileState : byte {
    UNEXPLORED,
    OPENED,
    EXHAUSTED,
    SPONTANEOUS,
}

public abstract class TileBehaviour : MonoBehaviour
{
    public TileState currentState = TileState.UNEXPLORED;
    public Sprite openSprite;
    public virtual string exhaustTitle { get; }
    public virtual string exhaustBody  { get; }

    public abstract void openTile(BoardManager context, string playerName);
    public abstract void exhaustTile(BoardManager context, string playerName);
    
    public void changeState(TileState newState) {
        currentState = newState;
    }

    public void openTileSequence(BoardManager context, string playerName) {
        Instantiate(this.gameObject.GetComponent<TileProperty>().openAnimation, this.transform.position, Quaternion.identity);
        this.gameObject.GetComponent<SpriteRenderer>().sprite = openSprite;
        this.currentState = TileState.OPENED;
        openTile(context, playerName);
    }

    public void exhaustTileSequence(BoardManager context, string playerName) {
        this.gameObject.GetComponent<TileProperty>().exhaustTile();
        this.currentState = TileState.EXHAUSTED;
        exhaustTile(context, playerName);
    }
}
