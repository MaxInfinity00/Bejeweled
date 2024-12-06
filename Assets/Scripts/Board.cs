using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

namespace Bejeweled
{
    
public class Board : MonoBehaviour
{
    public Vector2 size;
    public Tile[,] tiles;
    public float TileSideLength = 0.42f;
    
    private Gem CurrentlySelectedGem;
    private Gem GemToSwap;
    private List<Gem> GemsToDestroy = new List<Gem>();
    private List<Gem> GemsToTween = new List<Gem>();
    private List<Tile> GemsToCreate = new List<Tile>();
    private bool inControl = true;

    [SerializeField] private GameObject Marker;
    

    private void Start()
    {
        Bejeweled.BejeweledControls controls = new Bejeweled.BejeweledControls();
        controls.Enable();
        controls.BejeweledActionMap.MouseClick.performed += ctx => OnMouseClick();
        SetGrid();
        PopulateGrid();
    }
    
    void SetGrid()
    {
        float estimatedYBegin = TileSideLength * -3.5f;
        float estimatedXBegin = 0.5f + estimatedYBegin;
        tiles = new Tile[(int)size.x, (int)size.y];
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                Tile tile = new Tile(estimatedXBegin + i*TileSideLength, estimatedYBegin + j*TileSideLength,i,j);
                tiles[i, j] = tile;
            }
        }
    }
    
    void PopulateGrid()
    {
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                GemPool.Instance.CreateGem(tiles[i, j]);
            }
        }
        CheckGrid();
    }

    void OnMouseClick()
    {
        if (!inControl) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        if(hit.collider != null)
        {
            Gem gem = hit.collider.GetComponent<Gem>();
            if (gem != null)
            {
                Debug.Log("Gem clicked");
                if(CurrentlySelectedGem == null)
                {
                    CurrentlySelectedGem = gem;
                    UpdateMarker();
                }
                else if(CurrentlySelectedGem != gem && CurrentlySelectedGem.type != gem.type)
                {
                    int result = CheckValidMove(CurrentlySelectedGem.tile, gem.tile);
                    switch (result)
                    {
                        case 0:
                            Debug.Log("Not neighbours");
                            break;
                        case 1:
                            Debug.Log("Neighbours but no potential match");
                            CurrentlySelectedGem = null;
                            UpdateMarker();
                            break;
                        case 2:
                            Debug.Log("Neighbours and potential match");
                            inControl = false;
                            SwapGems(CurrentlySelectedGem, gem);
                            CurrentlySelectedGem = null;
                            UpdateMarker();
                            CheckGrid();
                            break;
                    }
                }
                
            }
            else
            {
                Debug.Log("Something else clicked");
            }
        }
        else
        {
            Debug.Log("Nothing clicked");
        }
    }
    
    int CheckValidMove(Tile tile1, Tile tile2)
    {
        // results: 0 - aren't neighbours, 1 - are neighbours but no potential match, 2 - are neighbours and potential match)
        
        if (!CheckNeighbours(tile1, tile2))
        {
            return 0;
        }
        else if(CheckPotentialMatch(tile1, tile2))
        {
            return 2;
        }
        else
        {
            return 1;
        }

    }
    
    bool CheckNeighbours(Tile tile1, Tile tile2)
    {
        return (Mathf.Abs(tile1.tileIndexX - tile2.tileIndexX) + Mathf.Abs(tile1.tileIndexY - tile2.tileIndexY) == 1);
    }

    bool CheckPotentialMatch(Tile tile1, Tile tile2)
    {
        // return false;
        // Tile 1 Checks
        for(int i = Mathf.Max(tile1.tileIndexX-2,0), result = 0; i <= Mathf.Min(size.x - 1,tile1.tileIndexX+2); i++)
        {
            Tile tileinquestion = tiles[i, tile1.tileIndexY];
            if ((tileinquestion == tile1 || tileinquestion.gem.type == tile2.gem.type) && tileinquestion != tile2)
            {
                result++;
            }
            else result = 0;
            if (result == 3)
            {
                return true;
            }
        }
        for(int i = Mathf.Max(tile1.tileIndexY-2,0), result = 0; i <= Mathf.Min(size.y - 1,tile1.tileIndexY+2); i++)
        {
            Tile tileinquestion = tiles[tile1.tileIndexX, i];
            if ((tileinquestion == tile1 || tileinquestion.gem.type == tile2.gem.type) && tileinquestion != tile2)
            {
                result++;
            }
            else result = 0;
            if (result == 3)
            {
                return true;
            }
        }
        // Tile 2 Checks
        for(int i = Mathf.Max(tile2.tileIndexX-2,0), result = 0; i <= Mathf.Min(size.x - 1,tile2.tileIndexX+2); i++)
        {
            Tile tileinquestion = tiles[i, tile2.tileIndexY];
            if((tileinquestion == tile2 || tileinquestion.gem.type == tile1.gem.type) && tileinquestion != tile1)
            {
                result++;
            }
            else result = 0;
            if (result == 3)
            {
                return true;
            }
        }
        for(int i = Mathf.Max(tile2.tileIndexY-2,0), result = 0; i <= Mathf.Min(size.y - 1,tile2.tileIndexY+2); i++)
        {
            Tile tileinquestion = tiles[tile2.tileIndexX, i];
            if((tileinquestion == tile2 || tileinquestion.gem.type == tile1.gem.type) && tileinquestion != tile1)
            {
                result++;
            }
            else result = 0;
            if (result == 3)
            {
                return true;
            }
        }

        return false;
    }
    
    void SwapGems(Gem gem1, Gem gem2)
    {
        (gem1.tile, gem2.tile) = (gem2.tile, gem1.tile);
        gem1.transform.position = gem1.tile.position;
        gem2.transform.position = gem2.tile.position;
        gem1.tile.gem = gem1;
        gem2.tile.gem = gem2;
    }

    void TransferGem(Tile tile1, Tile emptyTile)
    {
        emptyTile.gem = tile1.gem;
        tile1.gem.tile = emptyTile;
        tile1.gem = null;
        emptyTile.gem.tile = emptyTile;
        GemsToTween.Add(emptyTile.gem);
        // emptyTile.gem.transform.position = emptyTile.position;
    }
    
    void CheckGrid()
    {
        //Check for vertical matches
        for (int i = 0; i < size.x; i++)
        {
            int lastGemType = -1;
            int consecutiveGemcount = 0;
            for (int j = 0; j < size.y; j++)
            {
                Tile tile = tiles[i, j];
                Gem gem = tile.gem;
                if (gem.type == lastGemType)
                {
                    consecutiveGemcount++;
                    if (j == size.y - 1 && consecutiveGemcount >= 3)
                    {
                        for (int k = 0; k < consecutiveGemcount; k++)
                        {
                            Gem gemToAdd = tiles[i, j - k].gem;
                            if(!GemsToDestroy.Contains(gemToAdd))
                                GemsToDestroy.Add(gemToAdd);
                        }
                    }
                }
                else
                {
                    if (consecutiveGemcount >= 3)
                    {
                        for (int k = 1; k <= consecutiveGemcount; k++)
                        {
                            Gem gemToAdd = tiles[i, j - k].gem;
                            if(!GemsToDestroy.Contains(gemToAdd))
                                GemsToDestroy.Add(gemToAdd);
                        }
                    }
                    consecutiveGemcount = 1;
                    lastGemType = gem.type;
                }
            }
        }
        //Check for horizontal matches
        for (int i = 0; i < size.y; i++)
        {
            int lastGemType = -1;
            int consecutiveGemcount = 0;
            for (int j = 0; j < size.x; j++)
            {
                Tile tile = tiles[j, i];
                Gem gem = tile.gem;
                if (gem.type == lastGemType)
                {
                    consecutiveGemcount++;
                    if(j == size.x - 1 && consecutiveGemcount >= 3)
                    {
                        for (int k = 0; k < consecutiveGemcount; k++)
                        {
                            Gem gemToAdd = tiles[j - k, i].gem;
                            if(!GemsToDestroy.Contains(gemToAdd))
                                GemsToDestroy.Add(gemToAdd);
                        }
                    }
                }
                else
                {
                    if (consecutiveGemcount >= 3)
                    {
                        for (int k = 1; k <= consecutiveGemcount; k++)
                        {
                            Gem gemToAdd = tiles[j - k, i].gem;
                            if(!GemsToDestroy.Contains(gemToAdd))
                                GemsToDestroy.Add(gemToAdd);
                        }
                    }
                    consecutiveGemcount = 1;
                    lastGemType = gem.type;
                }
            }
        }
        if(GemsToDestroy.Count > 0)
        {
            StartCoroutine(RemoveGems());
        }
        else
        {
            inControl = true;
        }
    }

    IEnumerator RemoveGems()
    {
        yield return new WaitForSeconds(1f);
        foreach (var gem in GemsToDestroy)
        {
            GemPool.Instance.RemoveGem(gem);
        }
        GemsToDestroy.Clear();
        FillBoard();
    }

    void FillBoard()
    {
        for (int i = 0; i < size.x; i++)
        {
            int index1 = 0;
            for (int j = 0; j < size.y; j++)
            {
                Tile tile = tiles[i, j];
                if(tile.gem == null)
                {
                    
                    for(;index1 < 8; index1++)
                    {
                        Tile checkTile = tiles[i, index1];
                        if(checkTile.gem != null)
                        {
                            TransferGem(checkTile, tile);
                            break;
                        }
                    }
                    if (index1 >= 8)
                    {
                        GemsToCreate.Add(tile);
                        continue;
                    }
                }
                index1++;
                
            }

        }
        StartCoroutine(GemFall());
    }
    
    IEnumerator GemFall()
    {
        yield return new WaitForSeconds(0.5f);
        foreach (Gem gem in GemsToTween)
        {
            LeanTween.move(gem.gameObject, gem.tile.position, 1.5f).setEase(LeanTweenType.easeOutQuint);
        }
        yield return new WaitForSeconds(1.5f);
        foreach (Tile tile in GemsToCreate)
        {
            GemPool.Instance.CreateGem(tile);
        }
        yield return new WaitForSeconds(0.25f);
        GemsToCreate.Clear();
        GemsToTween.Clear();
        CheckGrid();
    }
    
    private void UpdateMarker()
    {
        if (CurrentlySelectedGem != null)
        {
            Marker.SetActive(true);
            Marker.transform.position = CurrentlySelectedGem.transform.position;
        }
        else
        {
            Marker.SetActive(false);
        }
    }
    void OnDrawGizmos()
    {
        float estimatedYBegin = TileSideLength * -3.5f;
        float estimatedXBegin = 0.5f + estimatedYBegin;
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                Gizmos.DrawWireCube(new Vector3(estimatedXBegin + i*TileSideLength, estimatedYBegin + j*TileSideLength, 0), Vector3.one * TileSideLength);
            }
        }
    }
}
}

