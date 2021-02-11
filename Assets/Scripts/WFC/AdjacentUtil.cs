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
        public static void CheckAdjacencies(List<RuntimeTile> tileset, AdjacentModel model, ConnectionGroups connectionGroups)
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
                            switch (sideA.side)
                            {
                                case Sides.Left:
                                    if (sideB.side.Equals(Sides.Right) && CompareSides(sideA, rotA, Sides.Left, sideB, rotB, connectionGroups))
                                    {
                                        model.AddAdjacency(new Tile(i), new Tile(j), -1, 0, 0);
                                    }
                                    break;
                                case Sides.Right:
                                    if (sideB.side.Equals(Sides.Left) && CompareSides(sideA, rotA, Sides.Right, sideB, rotB, connectionGroups))
                                    {
                                        model.AddAdjacency(new Tile(i), new Tile(j), 1, 0, 0);
                                    }
                                    break;
                                case Sides.Front:
                                    if (sideB.side.Equals(Sides.Back) && CompareSides(sideA, rotA, Sides.Front, sideB, rotB, connectionGroups))
                                    {
                                        model.AddAdjacency(new Tile(i), new Tile(j), 0, 1, 0);
                                    }
                                    break;
                                case Sides.Back:
                                    if (sideB.side.Equals(Sides.Front) && CompareSides(sideA, rotA, Sides.Back, sideB, rotB, connectionGroups))
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
        static bool CompareSides(TileSide sideA, int rotA, Sides side, TileSide sideB, int rotB, ConnectionGroups connectionGroups)
        {
            SubSide[] arrayA = ShuffleArray(sideA, rotA, side);
            SubSide[] arrayB = ShuffleArray(sideB, rotB, side);

            for (int i = 0; i < arrayA.Length; i++)
            {
                if (!CheckConnections(arrayA[i], arrayB[i], connectionGroups))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Due to rotation garbage, the subsides need to be swapped up for proper comparison. Does not impact the original data.
        /// Idk if there is a better way.
        /// </summary>
        /// <param name="tileSide"></param>
        /// <param name="rot"></param>
        /// <param name="side"></param>
        /// <returns></returns>
        static SubSide[] ShuffleArray(TileSide tileSide, int rot, Sides side)
        {
            SubSide[] arr = new SubSide[3];
            if ((rot == 90 || rot == 180) && (side == Sides.Left || side == Sides.Right))
            {
                arr[0] = tileSide.subSides[2];
                arr[1] = tileSide.subSides[1];
                arr[2] = tileSide.subSides[0];
            }
            else if ((rot == 180 || rot == 270) && (side == Sides.Front || side == Sides.Back))
            {
                arr[0] = tileSide.subSides[2];
                arr[1] = tileSide.subSides[1];
                arr[2] = tileSide.subSides[0];
            }
            else
            {
                arr = tileSide.subSides.ToArray();
            }
            return arr;
        }

        /// <summary>
        /// Iterates through the connectiongroup and checks if two subsides are equal.
        /// </summary>
        /// <param name="subSideA"></param>
        /// <param name="subSideB"></param>
        /// <param name="connectionGroups"></param>
        /// <returns></returns>
        static bool CheckConnections(SubSide subSideA, SubSide subSideB, ConnectionGroups connectionGroups)
        {
            for (int x = 0; x < connectionGroups.groups.Length; x++)
            {
                if ((int)subSideA.connection == x)
                {
                    for (int y = 0; y < connectionGroups.groups.Length; y++)
                    {
                        if ((int)subSideB.connection == y && connectionGroups.groups[x].v[y])
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

                List<SubSide> subList = new List<SubSide>();
                for (int j = 0; j < sides[i].subSides.Count; j++)
                {
                    SubSide newSubside = new SubSide();
                    s = (int)sides[i].subSides[j].side;
                    newSubside.side = (UnderSides)s;
                    c = (int)sides[i].subSides[j].connection;
                    newSubside.connection = (Connections)c;
                    subList.Add(newSubside);
                }
                newSide.subSides = subList;
                list.Add(newSide);
            }
            return list;
        }
    }
}
