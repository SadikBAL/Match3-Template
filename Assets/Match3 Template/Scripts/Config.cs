using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName ="Config/Create Config")]
public class Config : ScriptableObject
{
    [Range(5, 100)]
    public int Width;
    [Range(5, 100)]
    public int Height;
    [Range(0.1f, 3f)]
    public float SlideSpeed;
    [Range(0.1f, 3f)]
    public float ExplosionSpeed;
    [Range(0.1f, 3f)]
    public float SwapSpeed;
    public GameObject[] Prefabs = new GameObject[TileType.GetNames(typeof(TileType)).Length];
    public GameObject TileBack;
    public List<Vector2> DisabledTiles = new List<Vector2>();
    

    [Range(1, 100)]
    public int[] SpawnRates = new int[TileType.GetNames(typeof(TileType)).Length];
    public List<Sprite> Sprites = new List<Sprite>();
    
}
