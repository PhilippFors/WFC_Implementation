using DeBroglie;
using DeBroglie.Models;
using System.Collections.Generic;
using UnityEngine;

namespace MyWFC
{
    public static class AdjacentUtil
    {
        private static int iterationCounter = 0;
        /// <summary>
        /// Iterates through the tileset and the subsequent tilesides of each tile to check legal adjacencies.
        /// </summary>
        /// <param name="tileset"></param>
        /// <param name="model"></param>
        /// <param name="connectionGroups"></param>
        public static void StartAdjacencyCheck(List<RuntimeTile> tileset, AdjacentModel model, ConnectionGroups connectionGroups)
        {
            iterationCounter = 0;
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
                            // if (sideA.Equals(sideB, connectionGroups, new[] { tileset[i].bID, tileset[j].bID }, rotA, rotB))
                            switch (sideA.side)
                            {
                                case Sides.Left:
                                    if (sideB.side.Equals(Sides.Right) && CompairSides(sideA, sideB, connectionGroups))
                                    {
                                        model.AddAdjacency(new Tile(i), new Tile(j), -1, 0, 0);
                                    }
                                    break;
                                case Sides.Right:
                                    if (sideB.side.Equals(Sides.Left) && CompairSides(sideA, sideB, connectionGroups))
                                    {
                                        model.AddAdjacency(new Tile(i), new Tile(j), 1, 0, 0);
                                    }
                                    break;
                                case Sides.Front:
                                    if (sideB.side.Equals(Sides.Back) && CompairSides(sideA, sideB, connectionGroups))
                                    {
                                        model.AddAdjacency(new Tile(i), new Tile(j), 0, 0, 1);
                                    }
                                    break;
                                case Sides.Back:
                                    if (sideB.side.Equals(Sides.Front) && CompairSides(sideA, sideB, connectionGroups))
                                    {
                                        model.AddAdjacency(new Tile(i), new Tile(j), 0, 0, -1);
                                    }
                                    break;
                                case Sides.Top:
                                    if (sideB.side.Equals(Sides.Bottom) && CompairSides(sideA, sideB, connectionGroups))
                                    {
                                        model.AddAdjacency(new Tile(i), new Tile(j), 0, -1, 0);
                                    }
                                    break;
                                case Sides.Bottom:
                                    if (sideB.side.Equals(Sides.Top) && CompairSides(sideA, sideB, connectionGroups))
                                    {
                                        model.AddAdjacency(new Tile(i), new Tile(j), 0, 1, 0);
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            Debug.Log(iterationCounter);
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

        private static bool CompairSides(TileSide sideA, TileSide sideB, ConnectionGroups connectionGroups, int[] bIDS = null, int rotationYA = 0, int rotationYB = 0)
        {
            iterationCounter++;

            if (sideA.Equals(Connections.None) || sideB.Equals(Connections.None))
            {
                return false;
            }

            Connections[,] a = RotateMatrix(sideA.sideInfo, rotationYA, sideA.side);
            Connections[,] b = RotateMatrix(sideB.sideInfo, rotationYB, sideB.side);

            for (int x = 0; x < a.GetLength(0); x++)
            {
                for (int i = 0, j = a.GetLength(1) - 1; i < a.GetLength(0); i++, j--)
                {
                    if (!AdjacentUtil.CheckConnections(a[i, x], b[j, x], connectionGroups))
                        return false;
                }
            }
            return true;
        }

        private static Connections[,] RotateMatrix(Connections[,] connections, int rot, Sides side)
        {
            Connections[,] newArr = connections;

            if (rot != 0 && (side.Equals(Sides.Top) || side.Equals(Sides.Bottom)))
            {
                int amount = rot / 90;
                Debug.Log(rot);
                Debug.Log(amount);
                for (int i = 0; i < amount; i++)
                {
                    Transpose(ref newArr);
                    Reverse(ref newArr);
                }
            }
            return newArr;
        }

        private static void Transpose(ref Connections[,] connections)
        {
            for (int x = 0; x < connections.GetLength(0); x++)
                for (int z = x; z < connections.GetLength(1); z++)
                {
                    var temp = connections[x, z];
                    connections[x, z] = connections[z, x];
                    connections[z, x] = temp;
                }
        }

        private static void Reverse(ref Connections[,] connections)
        {
            var start = 0;
            var end = connections.GetLength(1) - 1;

            for (int i = 0; i < connections.Length; i++)
            {
                while (start < end)
                {
                    var temp = connections[i, start];
                    connections[i, start] = connections[i, end];
                    connections[i, end] = temp;
                    start++;
                    end--;
                }
            }
        }

        /// <summary>
        /// Iterates through the connectiongroup and checks if two connections can connect.
        /// </summary>
        /// <param name="connA"></param>
        /// <param name="connB"></param>
        /// <param name="connectionGroups"></param>
        /// <returns></returns>
        private static bool CheckConnections(Connections connA, Connections connB, ConnectionGroups connectionGroups)
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

                newSide.left = sides[i].left;
                newSide.middle = sides[i].middle;
                newSide.right = sides[i].right;
                newSide.topLeft = sides[i].topLeft;
                newSide.topMiddle = sides[i].topMiddle;
                newSide.topRight = sides[i].topRight;
                newSide.bottomLeft = sides[i].bottomLeft;
                newSide.bottomMiddle = sides[i].bottomMiddle;
                newSide.bottomRight = sides[i].bottomRight;

                list.Add(newSide);
            }
            return list;
        }
    }
}
