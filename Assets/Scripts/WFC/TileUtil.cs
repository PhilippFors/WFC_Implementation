using DeBroglie;
using MyWFC;
using System.Collections.Generic;
using UnityEngine;

public static class WFCUtil
{
    /// <summary>
    /// Finds a random position on the topology of the propagator.
    /// </summary>
    /// <param name="propagator"></param>
    /// <returns></returns>
    public static Vector3Int RandomPoint(TilePropagator propagator)
    {
        var xMax = propagator.Topology.Width;
        var yMax = propagator.Topology.Height;
        var zMax = propagator.Topology.Depth;

        return new Vector3Int(Random.Range(0, xMax), Random.Range(0, yMax), Random.Range(0, zMax));
    }
    /// <summary>
    /// Finds a single tile with a ID and specific rotation.
    /// </summary>
    /// <param name="tileSet"></param>
    /// <param name="ID"></param>
    /// <param name="rot"></param>
    /// <returns></returns>
    public static Tile FindTile(RuntimeTile[] tileSet, int ID, int rot = 0)
    {
        int index = -1;
        for (int j = 0; j < tileSet.Length; j++)
        {
            if (tileSet[j].ID == ID)
                index = j;
        }
        return new Tile(index);
    }

    /// <summary>
    /// Finds Tiles of all rotations with the given ID
    /// </summary>
    /// <param name="tileSet"></param>
    /// <param name="ID"></param>
    /// <returns></returns>
    public static Tile[] FindTilesArr(RuntimeTile[] tileSet, int ID)
    {
        List<Tile> l = new List<Tile>();
        for (int j = 0; j < tileSet.Length; j++)
            if (tileSet[j].ID == ID)
                l.Add(new Tile(j));

        Tile[] t = new Tile[l.Count];
        for (int i = 0; i < t.Length; i++)
            t[i] = l[i];
        return t;
    }

    /// <summary>
    /// Finds all Tiles of all rotations within the ID array
    /// </summary>
    /// <param name="tileSet"></param>
    /// <param name="ID"></param>
    /// <returns></returns>
    public static Tile[] FindTilesArr(RuntimeTile[] tileSet, int[] ID)
    {
        List<Tile> l = new List<Tile>();

        for (int i = 0; i < tileSet.Length; i++)
            for (int j = 0; j < ID.Length; j++)
                if (tileSet[i].ID == ID[j])
                    l.Add(new Tile(i));

        Tile[] t = new Tile[l.Count];
        for (int i = 0; i < t.Length; i++)
            t[i] = l[i];
        return t;
    }


    /// <summary>
    /// Finds Tiles of all rotations with the given ID
    /// </summary>
    /// <param name="tileSet"></param>
    /// <param name="ID"></param>
    /// <returns></returns>
    public static IEnumerable<Tile> FindTileList(RuntimeTile[] tileSet, int ID)
    {
        List<Tile> l = new List<Tile>();
        for (int j = 0; j < tileSet.Length; j++)
        {
            if (tileSet[j].ID == ID)
                l.Add(new Tile(j));
        }
        return l;
    }

    /// <summary>
    /// Finds all Tiles of all rotations within the ID array
    /// </summary>
    /// <param name="tileSet"></param>
    /// <param name="ID"></param>
    /// <returns></returns>
    public static IEnumerable<Tile> FindTilesList(RuntimeTile[] tileSet, int[] ID)
    {
        List<Tile> l = new List<Tile>();
        for (int i = 0; i < tileSet.Length; i++)
        {
            for (int j = 0; j < ID.Length; j++)
            {
                if (tileSet[i].ID == ID[j])
                    l.Add(new Tile(i));
            }

        }
        return l;
    }
    /// <summary>
    /// Finds all Tiles of all rotations within the Tile List
    /// </summary>
    /// <param name="tileSet"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    public static IEnumerable<Tile> FindTilesList(RuntimeTile[] tileSet, List<MyTile> list)
    {
        List<Tile> l = new List<Tile>();
        for (int i = 0; i < tileSet.Length; i++)
        {
            for (int j = 0; j < list.Count; j++)
            {
                if (tileSet[i].ID == list[j].ID)
                    l.Add(new Tile(i));
            }

        }
        return l;
    }
}