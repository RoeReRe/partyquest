using UnityEngine;

public abstract class TileBehaviour : MonoBehaviour
{
    protected Sprite openSprite;
    protected Sprite exhaustedSprite;

    public abstract void openTile();
    public abstract void exhaustTile();
}
