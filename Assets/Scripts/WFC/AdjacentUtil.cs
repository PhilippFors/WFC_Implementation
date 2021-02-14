using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeBroglie;
using DeBroglie.Models;
namespace MyWFC
{
    public static class AdjacentUtil
    {
        /// <summary>
        /// Iterates through the tileset and the subsequent tilesides of each tile to check legal adjacencies.
        /// </summary>
        /// <param name="tileset"></param>
        /// <param name="model"></param>
        /// <param name="connectionGroups"></param>
        public static void StartAdjacencyCheck(List<RuntimeTile> tileset, AdjacentModel model, ConnectionGroups connectionGroups)
        {
            for (int i = 0; i < tileset.Count; i++)
            {
                for (int j = i; j < tileset.Count; j++)
                {
                    var rotA = tileset[i].rotation;
                    var rotB = tileset[j].rotation;

                    var tileA = tileset[i].sides;
                    var tileB = tileset[j].sides;

                    foreach (TileSide sideA in tileA)
                    {
                        foreach (TileSide sideB in tileB)
                        {
                            if (CompareSides(sideA, sideB, connectionGroups, new[] { tileset[i].bID, tileset[j].bID }))
                                switch (sideA.side)
                                {
                                    case Sides.Left:
                                        if (sideB.side.Equals(Sides.Right))
                                        {
                                            model.AddAdjacency(new Tile(i), new Tile(j), -1, 0, 0);
                                        }
                                        break;
                                    case Sides.Right:
                                        if (sideB.side.Equals(Sides.Left))
                                        {
                                            model.AddAdjacency(new Tile(i), new Tile(j), 1, 0, 0);
                                        }
                                        break;
                                    case Sides.Front:
                                        if (sideB.side.Equals(Sides.Back))
                                        {
                                            model.AddAdjacency(new Tile(i), new Tile(j), 0, 1, 0);
                                        }
                                        break;
                                    case Sides.Back:
                                        if (sideB.side.Equals(Sides.Front))
                                        {
                                            model.AddAdjacency(new Tile(i), new Tile(j), 0, -1, 0);
                                        }
                                        break;
                                }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Comparing the sides for two tiles in more detail. Interrupts the check if a subside doesn't match.
        /// </summary>
        /// <param name="sideA"></param>
        /// <param name="rotA"></param>
        /// <param name="side"></param>
        /// <param name="sideB"></param>
        /// <param name="rotB"></param>
        /// <param name="connectionGroups"></param>
        /// <returns></returns>
        static bool CompareSides(TileSide sideA, TileSide sideB, ConnectionGroups connectionGroups, int[] bIDS)
        {
            Connections[] arrayA = sideA.all.ToArray();
            Connections[] arrayB = sideB.all.ToArray();

            if (sideA.Equals(Connections.BID) && sideB.Equals(Connections.BID))
            {
                for (int i = 0; i < arrayA.Length; i++)
                {
                    if (!CheckConnections(arrayA[i], arrayB[i], connectionGroups) || bIDS[0] != bIDS[1])
                        return false;
                }
            }
            else
            {
                for (int i = 0, j = arrayA.Length - 1; i < arrayA.Length; i++, j--)
                {
                    if (!CheckConnections(arrayA[i], arrayB[j], connectionGroups))
                        return false;
                }
            }
            return true;
        }

        #region unimportant
        /// <summary>
        /// Due to rotation garbage, the subsides need to be swapped up for proper comparison. Does not impact the original data.
        /// Idk if there is a better way.
        /// </summary>
        /// <param name="tileSide"></param>
        /// <param name="rot"></param>
        /// <param name="side"></param>
        /// <returns></returns>
        // static Connections[] ShuffleArray(TileSide tileSide, int rot, Sides side)
        // {
        //     Connections[] arr = new Connections[3];
        // if ((rot == 90 || rot == 180) && (side == Sides.Left || side == Sides.Right))
        // {
        // arr[0] = tileSide.all[2];
        // arr[1] = tileSide.all[1];
        // arr[2] = tileSide.all[0];
        // }
        // else if ((rot == 180 || rot == 270) && (side == Sides.Front || side == Sides.Back))
        // {
        //     arr[0] = tileSide.all[2];
        //     arr[1] = tileSide.all[1];
        //     arr[2] = tileSide.all[0];
        // }
        // else
        // {
        //     arr = tileSide.all.ToArray();
        // }
        //     return arr;
        // }
        #endregion

        /// <summary>
        /// Iterates through the connectiongroup and checks if two connections can connect.
        /// </summary>
        /// <param name="connA"></param>
        /// <param name="connB"></param>
        /// <param name="connectionGroups"></param>
        /// <returns></returns>
        static bool CheckConnections(Connections connA, Connections connB, ConnectionGroups connectionGroups)
        {
            for (int x = 0; x < connectionGroups.groups.Length; x++)
            {
                if ((int)connA == x)
                {
                    for (int y = 0; y < connectionGroups.groups.Length; y++)
                    {
                        if ((int)connB == y && connectionGroups.groups[x].v[y])
                            return true;
                    }
                    break;
                }
            }
            return false;
        }

        /// <summary>
        /// I have no idea
        /// </summary>
        /// <param name="sides"></param>
        /// <returns></returns>
        public static List<TileSide> TileSideCopy(List<TileSide> sides)
        {
            List<TileSide> list = new List<TileSide>();
            for (int i = 0; i < sides.Count; i++)
            {
                TileSide newSide = new TileSide();
                int s = (int)sides[i].side;
                newSide.side = (Sides)s;
                int c = (int)sides[i].connection;
                newSide.connection = (Connections)c;

                List<Connections> subList = new List<Connections>();
                for (int j = 0; j < sides[i].all.Count; j++)
                {
                    subList.Add(sides[i].all[j]);
                }
                newSide.all = subList;
                list.Add(newSide);
            }
            return list;
        }
    }
}
