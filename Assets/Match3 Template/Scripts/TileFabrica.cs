using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileFabrica
{
    List<Tile> tileList;
    GameManager gameManager;
    private Vector3 prefabsStartLocation = new Vector3(-100,-100,10);
    public TileFabrica(GameManager gameManager)
    {
        int boardSize = gameManager.config.Width * gameManager.config.Height;
        this.gameManager = gameManager;
        GameObject tempGameObject;
        Tile tempTile;
        tileList = new List<Tile>();
        foreach(TileType type in TileType.GetValues(typeof(TileType)))
        {
            for(int i = 0; i< boardSize; i++)
            {
                tempGameObject = GameObject.Instantiate(gameManager.config.Prefabs[(int)type - 1], prefabsStartLocation, Quaternion.identity);
                tempTile = tempGameObject.GetComponent<Tile>();
                tempTile.x = -1;
                tempTile.y = -1;
                tempTile.type = type;
                tempTile.tile = tempGameObject;
                tempTile.gameManager = gameManager;
                tempTile.tile.SetActive(false);
            }
        }
    }
    public Tile PopTile(TileType type)
    {
        foreach(Tile t in tileList)
        {
            if(t.type == type)
            {
                t.isExplosion = 0;
                tileList.Remove(t);
                return t;
            }
        }
        GameObject tempGameObject;
        Tile tempTile;
        tempGameObject = GameObject.Instantiate(gameManager.config.Prefabs[(int)type - 1], prefabsStartLocation, Quaternion.identity);
        tempTile = tempGameObject.GetComponent<Tile>();
        tempTile.x = -1;
        tempTile.y = -1;
        tempTile.isExplosion = 0;
        tempTile.type = type;
        tempTile.tile = tempGameObject;
        tempTile.gameManager = gameManager;
        tempTile.tile.SetActive(false);
        return tempTile;
    }
    public Tile PopRandomTile()
    {
        int randomBonus = Random.Range(1, 100);
        if(randomBonus < 5)
        {
            return this.PopTile(TileType.Bonus);
        }
        else
        {
            int random = Random.Range(1, TileType.GetNames(typeof(TileType)).Length - 1);
            return this.PopTile((TileType)random);
        }

    }
    public void PushTile(Tile t)
    {
        t.isExplosion = 0;
        tileList.Add(t);
        t.tile.SetActive(false);
    }
}
