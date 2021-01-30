using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaGenerator : MonoBehaviour
{
    [SerializeField] GameObject generatorPrefab;

    [SerializeField] List<GameObject> tilePrefabs = new List<GameObject>();
    public TileSet tileSet;
    public List<Area> areaList = new List<Area>();
    public List<Tessera.TesseraGenerator> generators = new List<Tessera.TesseraGenerator>();
    public bool loaded = false;
    private void Start()
    {
        FindPrefabs();
    }
    public void FindPrefabs()
    {
        // if (generatorPrefab == null)
        // {
        //     generatorPrefab = Resources.Load<GameObject>("GeneratorRoot");
        // }
        generatorPrefab = Resources.Load<GameObject>("GeneratorRoot");
    }
    public void StartAreaGenerator(GridTile[] tile)
    {
        generators.Clear();
        if (areaList.Count != 0)
            foreach (Area a in areaList)
                if (Application.isPlaying) Destroy(a.parent); else DestroyImmediate(a.parent);

        areaList.Clear();

        foreach (GridTile t in tile)
        {
            GenerateArea(t);
        }
        StartCoroutine(StartGenerator());
        StartCoroutine(CheckForFinsihed());
    }

    IEnumerator CheckForFinsihed()
    {
        while (!CheckList())
        {
            yield return null;
        }

        Debug.Log("All Areas Generated!");
    }

    bool CheckList()
    {
        int i = 0;
        foreach (Area a in areaList)
        {
            if (a.generated)
                i++;
        }
        return i == areaList.Count;
    }
    void GenerateArea(GridTile tile)
    {
        Area a = new Area(new GameObject("Area " + tile.index), tile);
        areaList.Add(a);

        GameObject newGeneratorObj = Instantiate(generatorPrefab, Vector3.zero, Quaternion.identity);
        Tessera.TesseraGenerator generator = newGeneratorObj.GetComponentInChildren<Tessera.TesseraGenerator>();
        newGeneratorObj.transform.position = tile.position;
        newGeneratorObj.transform.parent = a.parent.transform;

        List<Tessera.TileEntry> tesseraTiles = new List<Tessera.TileEntry>();
        generator.tiles = new List<Tessera.TileEntry>();
        foreach (GameObject g in tileSet.tiles)
        {
            Tessera.TesseraTile t = g.GetComponent<Tessera.TesseraTile>();
            Tessera.TileEntry tileEntry = new Tessera.TileEntry();
            tileEntry.tile = t;
            tileEntry.weight = g.GetComponent<MyWFC.MyTile>().weight;

            generator.tiles.Add(tileEntry);
        }

        generator.size = new Vector3Int(tile.width / (int)generator.tileSize.x, 1, tile.height / (int)generator.tileSize.z);
        generators.Add(generator);
    }

    IEnumerator StartGenerator()
    {
        Tessera.TesseraGenerateOptions t = new Tessera.TesseraGenerateOptions();
        t.multithreaded = true;
        for (int i = 0; i < generators.Count; i++)
        {
            yield return generators[i].StartGenerate(t);

            areaList[i].generated = true;
        }
    }
}

[System.Serializable]
public class Area
{
    public bool generated;
    public GameObject parent;
    public GridTile tileInfo;
    public Area(GameObject g, GridTile tile)
    {
        parent = g;
        tileInfo = tile;
        parent.transform.position = tile.position;
    }
}
