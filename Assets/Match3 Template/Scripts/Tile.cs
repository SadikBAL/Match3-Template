using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public GameManager gameManager;
    public int x;
    public int y;
    public GameObject tile;
    public TileType type;
    public int isExplosion;
    private bool isEnabled = true;
    public void SetEnable(bool enable)
    {
        isEnabled = enable;
        tile.SetActive(isEnabled);
    }
    public bool IsEnable()
    {
        return isEnabled;
    }
    public void OnMouseDown()
    {
        if(gameManager.ActiveAnimationCount == 0 && this.type != TileType.None)
            gameManager.ClickedTile = this;
    }
    private void OnMouseUp()
    {
        gameManager.OnTile = null;
        gameManager.ClickedTile = null;
    }
    public void OnMouseEnter()
    {
        if(gameManager.ClickedTile != null && gameManager.ClickedTile != this && this.type != TileType.None)
        {
            gameManager.OnTile = this;
        }
    }
}
