using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bejeweled{
public class GemPool : MonoBehaviour
{
    List<Gem> InactiveGems = new List<Gem>();
    List<Gem> ActiveGems = new List<Gem>();
    
    public List<Sprite> gemSprites;
    
    public GameObject gemPrefab;
    
    public static GemPool Instance;
    
    void Awake()
    {
        Instance = this;
    }
    
    public Gem CreateGem(Tile tile)
    {
        Gem gem;
        if (InactiveGems.Count > 0)
        {
            gem = InactiveGems[0];
            InactiveGems.RemoveAt(0);
        }
        else
        {
            gem = Instantiate(gemPrefab, tile.position, Quaternion.identity).GetComponent<Gem>();
        }
        int type = Random.Range(0, gemSprites.Count);
        gem.type = type;
        gem.GetComponent<SpriteRenderer>().sprite = gemSprites[type];
        gem.transform.position = tile.position;
        gem.tile = tile;
        tile.gem = gem;
        gem.gameObject.SetActive(true);
        ActiveGems.Add(gem);

        return gem;
    }
    
    public void RemoveGem(Gem gem)
    {
        gem.gameObject.SetActive(false);
        ActiveGems.Remove(gem);
        gem.tile.gem = null;
        gem.tile = null;
        gem.gameObject.SetActive(false);
        InactiveGems.Add(gem);
    }
    
}
}