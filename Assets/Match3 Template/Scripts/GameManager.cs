using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum TileType
{
    Type1 = 1,
    Type2 = 2,
    Type3 = 3,
    Type4 = 4,
    Type5 = 5,
    Type6 = 6,
    Bonus = 8,
    None = 7
}
public enum GameState
{
    Starting,
    Playable,
    CheckBoard,
    Explosion,
    Refill
}
public class GameManager : MonoBehaviour
{
    public Config config;
    public Tile[,] GameBoard;
    public SpriteRenderer Background;
    public GameObject[,] BoardBack;
    private Vector2 StartPosition = new Vector2();
    [HideInInspector]
    public int ActiveAnimationCount = 0;
    [HideInInspector]
    public Tile ClickedTile = null;
    [HideInInspector]
    public Tile OnTile = null;
    private TileFabrica tileFabrica;
    private GameState currentState = GameState.Starting;
    void Start()
    {
        Background.sprite = config.Sprites[Random.Range(0,config.Sprites.Count)];
        tileFabrica = new TileFabrica(this);
        GameBoard = new Tile[this.config.Width, this.config.Height];
        BoardBack = new GameObject[this.config.Width,this.config.Height];
        CalculateMaxOrthographicSize();
        CalculateStartPosition();
        InitBoard();
        currentState = GameState.Playable;
    }
    void Update()
    {
        if(ActiveAnimationCount == 0)
        {
           if(this.OnTile != null && this.ClickedTile != null)
            {
                SwapTile(this.ClickedTile,this.OnTile);
                this.ClickedTile = null;
                this.OnTile = null;
                return;
            }
            else if (currentState == GameState.Explosion)
            {
                Debug.Log("currentState == GameState.Explosion");
                ExploseTiles();
                currentState = GameState.Refill;
                return;
            }
            else if (currentState == GameState.Refill)
            {
                RefillTiles();
                currentState = GameState.CheckBoard;
                return;
            }
            else if (currentState == GameState.CheckBoard)
            {
                if(CheckBoard())
                {
                    currentState = GameState.Explosion;
                }
                else
                {
                    currentState = GameState.Playable;
                }
                return;
            }
           if(Input.GetKeyDown(KeyCode.Space) && this.ClickedTile != null)
            {
                int x = this.ClickedTile.x;
                int y = this.ClickedTile.y;
                tileFabrica.PushTile(GameBoard[x, y]);
                GameBoard[x, y] = tileFabrica.PopRandomTile();
                GameBoard[x, y].tile.transform.position = new Vector3(StartPosition.x + x, StartPosition.y - y, 10);
                GameBoard[x, y].tile.transform.localScale = new Vector3(1, 1, 1);
                GameBoard[x, y].x = x;
                GameBoard[x, y].y = y;
                GameBoard[x, y].tile.SetActive(true);
                this.ClickedTile = null;
                return;
            }
            if (Input.GetKeyDown(KeyCode.LeftControl) && this.ClickedTile != null)
            {
                int x = this.ClickedTile.x;
                int y = this.ClickedTile.y;
                tileFabrica.PushTile(GameBoard[x, y]);
                GameBoard[x, y] = tileFabrica.PopTile(TileType.Bonus);
                GameBoard[x, y].tile.transform.position = new Vector3(StartPosition.x + x, StartPosition.y - y, 10);
                GameBoard[x, y].tile.transform.localScale = new Vector3(1, 1, 1);
                GameBoard[x, y].x = x;
                GameBoard[x, y].y = y;
                GameBoard[x, y].tile.SetActive(true);
                this.ClickedTile = null;
                return;
            }
        }
        
    }
    private bool IsTilesSwapable(Tile tileClicked,Tile tileOn)
    {
        if(tileClicked.x + 1 == tileOn.x && tileClicked.y == tileOn.y)
        {
            return true;
        }
        else if (tileClicked.x - 1 == tileOn.x && tileClicked.y == tileOn.y)
        {
            return true;
        }
        else if (tileClicked.x == tileOn.x && tileClicked.y - 1 == tileOn.y)
        {
            return true;
        }
        else if (tileClicked.x == tileOn.x && tileClicked.y + 1 == tileOn.y)
        {
            return true;
        }
        return false;
    }
    private bool SlideTile(int posX, int posY)
    {
        for (int i =  posY-1; i >= 0; i--)
        {
            if(GameBoard[posX,i].isExplosion == 0 && GameBoard[posX,i].IsEnable())
            {
                Tile temp = GameBoard[posX, posY];
                GameBoard[posX, posY] = GameBoard[posX, i];
                GameBoard[posX, i] = temp;
                GameBoard[posX, posY].x = posX;
                GameBoard[posX, posY].y = posY;
                GameBoard[posX, i].x = posX;
                GameBoard[posX, i].y = i;
                return true;
            }
        }
        return false;
    }
    private void RefillTiles()
    {
        for (int x = 0; x < this.config.Width; x++)
        {
            for (int y = this.config.Height -1; y >= 0; y--)
            {
                if (GameBoard[x, y].isExplosion == 1)
                {
                    if (!SlideTile(x, y))
                    {
                        tileFabrica.PushTile(GameBoard[x, y]);
                        GameBoard[x, y] = tileFabrica.PopRandomTile();
                        GameBoard[x, y].tile.transform.position = new Vector3(StartPosition.x + x, StartPosition.y - y + 10, 10);
                        GameBoard[x, y].tile.transform.localScale = new Vector3(1, 1, 1);
                        GameBoard[x, y].x = x;
                        GameBoard[x, y].y = y;
                        GameBoard[x, y].tile.SetActive(true);                       
                    }
                    StartCoroutine(AnimateMove(GameBoard[x, y].tile, GameBoard[x, y].tile.transform.position, new Vector3(StartPosition.x + x, StartPosition.y - y, GameBoard[x, y].tile.transform.position.z), this.config.SlideSpeed));
                }
            }
        }
    }
    private void ExploseTiles()
    {
        for (int x = 0; x < this.config.Width; x++)
        {
            for (int y = 0; y < this.config.Height; y++)
            {
                if(GameBoard[x,y].isExplosion == 1)
                {
                    StartCoroutine(AnimateDestroy(GameBoard[x,y].tile, GameBoard[x, y].tile.transform.localScale,new Vector3(0.1f,0.1f,0.1f), this.config.ExplosionSpeed));
                }
            }
        }
    }
    private bool IsTileGoingToExplode(int posX, int posY, bool isExplosion)
    {
        bool hasExplode = false;
        if (!GameBoard[posX, posY].IsEnable())
            return hasExplode;
        TileType type = GameBoard[posX, posY].type;
        TileType bonusType = TileType.None;
        int counter = 0;
        for (int x = posX + 1; x < this.config.Width; x++)
        {
            if (type == TileType.Bonus && GameBoard[x, posY].type != TileType.None && GameBoard[x, posY].IsEnable())
            {
                if (GameBoard[x, posY].type == TileType.Bonus)
                {
                    counter++;
                }
                else if (bonusType != TileType.None)
                {
                    if (bonusType == GameBoard[x, posY].type)
                        counter++;
                    else
                        break;
                }
                else
                {
                    bonusType = GameBoard[x, posY].type;
                    counter++;
                }

            }
            else if ((type == GameBoard[x, posY].type || GameBoard[x, posY].type == TileType.Bonus) && GameBoard[x, posY].type != TileType.None && GameBoard[x, posY].IsEnable())
                counter++;
            else
                break;
        }
        if (counter >= 2)
        {
            GameBoard[posX, posY].isExplosion = 1;
            for (int i = 0; i < counter; i++)
            {
                if(isExplosion)
                    GameBoard[posX + 1 + i, posY].isExplosion = 1;
            }
            hasExplode = true;
        }
        counter = 0;
        for (int y = posY + 1; y < this.config.Height; y++)
        {
            if (type == TileType.Bonus && GameBoard[posX, y].type != TileType.None && GameBoard[posX, y].IsEnable())
            {
                if (GameBoard[posX, y].type == TileType.Bonus)
                {
                    counter++;
                }
                else if (bonusType != TileType.None)
                {
                    if (bonusType == GameBoard[posX, y].type)
                        counter++;
                    else
                        break;
                }
                else
                {
                    bonusType = GameBoard[posX, y].type;
                    counter++;
                }

            }
            else if ((type == GameBoard[posX, y].type || GameBoard[posX, y].type == TileType.Bonus) && GameBoard[posX, y].type != TileType.None && GameBoard[posX, y].IsEnable())
                counter++;
            else
                break;
        }
        if (counter >= 2)
        {
            GameBoard[posX, posY].isExplosion = 1;
            for (int i = 0; i < counter; i++)
            {
                if (isExplosion)
                    GameBoard[posX, posY + 1 + i].isExplosion = 1;
            }
            hasExplode = true;
        }
        counter = 0;
        for (int x = posX - 1; x >= 0; x--)
        {
            if (type == TileType.Bonus && GameBoard[x, posY].type != TileType.None && GameBoard[x, posY].IsEnable())
            {
                if (GameBoard[x, posY].type == TileType.Bonus)
                {
                    counter++;
                }
                else if (bonusType != TileType.None)
                {
                    if (bonusType == GameBoard[x, posY].type)
                        counter++;
                    else
                        break;
                }
                else
                {
                    bonusType = GameBoard[x, posY].type;
                    counter++;
                }

            }
            else if ((type == GameBoard[x, posY].type || GameBoard[x, posY].type == TileType.Bonus) && GameBoard[x, posY].type != TileType.None && GameBoard[x, posY].IsEnable())
                counter++;
            else
                break;
        }
        if (counter >= 2)
        {
            GameBoard[posX, posY].isExplosion = 1;
            for (int i = 0; i < counter; i++)
            {
                if (isExplosion)
                    GameBoard[posX - 1 - i, posY].isExplosion = 1;
            }
            hasExplode = true;
        }
        counter = 0;
        for (int y = posY - 1; y >= 0; y--)
        {
            if (type == TileType.Bonus && GameBoard[posX, y].type != TileType.None && GameBoard[posX, y].IsEnable())
            {
                if (GameBoard[posX, y].type == TileType.Bonus)
                {
                    counter++;
                }
                else if (bonusType != TileType.None)
                {
                    if (bonusType == GameBoard[posX, y].type)
                        counter++;
                    else
                        break;
                }
                else
                {
                    bonusType = GameBoard[posX, y].type;
                    counter++;
                }

            }
            else if ((type == GameBoard[posX, y].type || GameBoard[posX, y].type == TileType.Bonus) && GameBoard[posX, y].type != TileType.None && GameBoard[posX, y].IsEnable())
                counter++;
            else
                break;
        }
        if (counter >= 2)
        {
            GameBoard[posX, posY].isExplosion = 1;
            for (int i = 0; i < counter; i++)
            {
                if (isExplosion)
                    GameBoard[posX, posY - 1 - i].isExplosion = 1;
            }
            hasExplode = true;
        }
        return hasExplode;
    }
    private bool CheckBoard()
    {
        bool isThereExplosion = false;
        for (int x = 0; x < this.config.Width; x++)
        {
            for (int y = 0; y < this.config.Height; y++)
            {
                if (IsTileGoingToExplode(x, y,true))
                {
                    isThereExplosion = true;
                }

            }
        }
        return isThereExplosion;
    }
    private void SwapTile(Tile tileClicked,Tile tileOn)
    {
        if(IsTilesSwapable(tileClicked,tileOn))
        {
            int tempX = tileClicked.x;
            int tempY = tileClicked.y;
            tileClicked.x = tileOn.x;
            tileClicked.y = tileOn.y;
            tileOn.x = tempX;
            tileOn.y = tempY;
            GameBoard[tileClicked.x, tileClicked.y] = tileClicked;
            GameBoard[tileOn.x, tileOn.y] = tileOn;
            if (CheckBoard())
            {
                StartCoroutine(AnimateMove(GameBoard[tileClicked.x, tileClicked.y].tile, GameBoard[tileClicked.x, tileClicked.y].tile.transform.position, new Vector3(StartPosition.x + tileClicked.x, StartPosition.y - tileClicked.y, GameBoard[tileClicked.x, tileClicked.y].tile.transform.position.z), this.config.SwapSpeed));
                StartCoroutine(AnimateMove(GameBoard[tileOn.x, tileOn.y].tile, GameBoard[tileOn.x, tileOn.y].tile.transform.position, new Vector3(StartPosition.x + tileOn.x, StartPosition.y - tileOn.y, GameBoard[tileOn.x, tileOn.y].tile.transform.position.z), this.config.SwapSpeed));
                currentState = GameState.Explosion;
            }
            else
            {
                tempX = tileClicked.x;
                tempY = tileClicked.y;
                tileClicked.x = tileOn.x;
                tileClicked.y = tileOn.y;
                tileOn.x = tempX;
                tileOn.y = tempY;
                GameBoard[tileClicked.x, tileClicked.y] = tileClicked;
                GameBoard[tileOn.x, tileOn.y] = tileOn;
                StartCoroutine(AnimateSwap(GameBoard[tileClicked.x, tileClicked.y].tile, GameBoard[tileClicked.x, tileClicked.y].tile.transform.position, GameBoard[tileOn.x, tileOn.y].tile.transform.position, this.config.SwapSpeed));
                StartCoroutine(AnimateSwap(GameBoard[tileOn.x, tileOn.y].tile, GameBoard[tileOn.x, tileOn.y].tile.transform.position, GameBoard[tileClicked.x, tileClicked.y].tile.transform.position, this.config.SwapSpeed));
                currentState = GameState.Playable;
            }

        }

    }
    private void CalculateMaxOrthographicSize()
    {
        int hor = (this.config.Width / 2) + 1;
        int ver = (this.config.Height / 2) + 1;
        float aspactRatio = ((float)Screen.width / Screen.height);
        float res1 = (float)(hor / aspactRatio);
        float res2 = (float)(ver);
        if (res1 > res2)
            this.GetComponent<Camera>().orthographicSize = res1;
        else
            this.GetComponent<Camera>().orthographicSize = res2;
    }
    private void CalculateStartPosition()
    {
        if (this.config.Width % 2 == 0)
            this.StartPosition.x = (float)((this.config.Width / 2) - 0.5f) * -1;
        else
            this.StartPosition.x = (float)(this.config.Width / 2) * -1;
        if (this.config.Height % 2 == 0)
            this.StartPosition.y = (float)(this.config.Height / 2) - 0.5f;
        else
            this.StartPosition.y = (float)(this.config.Height / 2);
        Debug.Log(this.StartPosition);
    }
    private void InitBoard()
    {
       for(int x=0;x< this.config.Width;x++)
       {
            for(int y = 0; y< this.config.Height; y ++)
            {
                //int random = Random.Range(1, TileType.GetNames(typeof(TileType)).Length - 1);
                GameBoard[x, y] = tileFabrica.PopRandomTile();
                BoardBack[x,y] = GameObject.Instantiate(config.TileBack, new Vector3(StartPosition.x + x, StartPosition.y - y, GameBoard[x, y].tile.transform.position.z), Quaternion.identity);
            }
       }
       int counter = 0;
       bool isReady = false;
       while(!isReady)
       {
            counter++;
            Debug.Log("Sayac : " + counter);
            if (counter > 10)
                break;
            isReady = true;
            for (int x = 0; x < this.config.Width; x++)
            {
                for (int y = 0; y < this.config.Height; y++)
                {
                    if(IsTileGoingToExplode(x,y,false))
                    {
                        isReady = false;
                        tileFabrica.PushTile(GameBoard[x, y]);
                        //int random = Random.Range(1, TileType.GetNames(typeof(TileType)).Length - 1);
                        GameBoard[x, y] = tileFabrica.PopRandomTile();
                    }

                }
            }
       }
       for (int x = 0; x < this.config.Width; x++)
       {
            for (int y = 0; y < this.config.Height; y++)
            {
                GameBoard[x, y].tile.transform.position = new Vector3(StartPosition.x + x, StartPosition.y - y + 100, 10);
                GameBoard[x, y].tile.transform.localScale = new Vector3(1,1,1);
                GameBoard[x, y].x = x;
                GameBoard[x, y].y = y;
                GameBoard[x, y].tile.SetActive(true);
                StartCoroutine(AnimateMove(GameBoard[x, y].tile, GameBoard[x, y].tile.transform.position, new Vector3(StartPosition.x + x, StartPosition.y - y, GameBoard[x, y].tile.transform.position.z), this.config.SlideSpeed));
            }
       }
       Debug.Log("Disabled Tiles Count : " + config.DisabledTiles.Count);
        foreach(Vector2 tilePos in config.DisabledTiles)
        {
             Debug.Log("Disabled Pos : " + tilePos);
             if((tilePos.x >= 0 && tilePos.x < config.Width) && (tilePos.y >= 0 && tilePos.y < config.Height))
                 GameBoard[(int)tilePos.x, (int)tilePos.y].SetEnable(false);
        }
        for (int x = 0; x < this.config.Width; x++)
        {
            for (int y = 0; y < this.config.Height; y++)
            {
                BoardBack[x, y].SetActive(!GameBoard[x, y].IsEnable());
            }
        }
    }
    IEnumerator AnimateMove(GameObject obj,Vector3 start, Vector3 target, float duration)
    {
        ActiveAnimationCount++;
        //Debug.Log("Added : " + ActiveAnimationCount);
        float journey = 0f;
        while (journey <= duration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / duration);

            obj.transform.position = Vector3.Lerp(start, target, percent);

            yield return null;
        }
        ActiveAnimationCount--;
        //Debug.Log("Removed : " + ActiveAnimationCount);
    }
    IEnumerator AnimateSwap(GameObject obj, Vector3 start, Vector3 target, float duration)
    {
        ActiveAnimationCount++;
        //Debug.Log("Added : " + ActiveAnimationCount);
        float journey = 0f;
        while (journey <= duration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / duration);

            obj.transform.position = Vector3.Lerp(start, target, percent);

            yield return null;
        }
        journey = 0f;
        while (journey <= duration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / duration);

            obj.transform.position = Vector3.Lerp(target, start, percent);

            yield return null;
        }
        ActiveAnimationCount--;
        //Debug.Log("Removed : " + ActiveAnimationCount);
    }
    IEnumerator AnimateDestroy(GameObject obj, Vector3 start, Vector3 target, float duration)
    {
        ActiveAnimationCount++;
        //Debug.Log("Added : " + ActiveAnimationCount);
        float journey = 0f;
        while (journey <= duration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / duration);

            obj.transform.localScale = Vector3.Lerp(start, target, percent);

            yield return null;
        }
        obj.SetActive(false);
        obj.transform.localScale = start;
        ActiveAnimationCount--;
        //Debug.Log("Removed : " + ActiveAnimationCount);
    }
}
