using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeBroglie;
using DeBroglie.Models;
namespace MyWFC
{
    public static class AdjacentUtil
    {
        public static void CheckAdjacencies(List<RuntimeTile> tileset, AdjacentModel model, List<ConnectionGroup> connectionGroups)
        {
            for (int i = 0; i < tileset.Count; i++)
            {
                for (int j = i; j < tileset.Count; j++)
                {
                    var rotA = tileset[i].rotation;
                    var rotB = tileset[j].rotation;

                    var tileA = tileset[i].sides; //Reference Tile
                    var tileB = tileset[j].sides; // Tile to check against
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

        static bool CompareSides(TileSide sideA, int rotA, Sides side, TileSide sideB, int rotB, List<ConnectionGroup> connectionGroups)
        {
            SubSide[] arrayA = new SubSide[3];
            SubSide[] arrayB = new SubSide[3];
            if ((rotA == 90 || rotA == 180) && (side == Sides.Left || side == Sides.Right))
            {
                arrayA[0] = sideA.subSides[2];
                arrayA[1] = sideA.subSides[1];
                arrayA[2] = sideA.subSides[0];
                // Debug.Log(arrayA[1].connection.ToString() + ", " + arrayA[1].side.ToString());
            }
            else if ((rotA == 180 || rotA == 270) && (side == Sides.Front || side == Sides.Back))
            {
                arrayA[0] = sideA.subSides[2];
                arrayA[1] = sideA.subSides[1];
                arrayA[2] = sideA.subSides[0];
                // Debug.Log(arrayA[1].connection.ToString() + ", " + arrayA[1].side.ToString());
            }
            else
            {
                arrayA = sideA.subSides.ToArray();
                // Debug.Log(arrayA[1].connection.ToString() + ", " + arrayA[1].side.ToString());
            }

            if ((rotB == 90 || rotB == 180) && (side == Sides.Left || side == Sides.Right))
            {
                arrayB[0] = sideB.subSides[2];
                arrayB[1] = sideB.subSides[1];
                arrayB[2] = sideB.subSides[0];
                // Debug.Log(arrayB[0].connection.ToString() + ", " + arrayB[0].side.ToString());
            }
            else if ((rotB == 180 || rotB == 270) && (side == Sides.Front || side == Sides.Back))
            {
                arrayB[0] = sideB.subSides[2];
                arrayB[1] = sideB.subSides[1];
                arrayB[2] = sideB.subSides[0];
                // Debug.Log(arrayB[0].connection.ToString() + ", " + arrayB[0].side.ToString());
            }
            else
            {
                arrayB = sideB.subSides.ToArray();
                // Debug.Log(arrayB[0].connection.ToString() + ", " + arrayB[0].side.ToString());
            }
            List<ConnectionGroup> group = new List<ConnectionGroup>();

            for (int i = 0; i < arrayA.Length; i++)
            {
                if (connectionGroups != null && connectionGroups.Count > 0)
                    group = FindConnectionGroups(arrayA[i].connection, connectionGroups);

                if (!CheckConnections(arrayA[i], arrayB[i], group))
                    return false;
            }

            return true;
        }

        static bool CheckConnections(SubSide sideA, SubSide sideB, List<ConnectionGroup> groups)
        {
            if (groups != null && groups.Count > 0)
            {
                if (sideA.connection == sideB.connection)
                {
                    foreach (ConnectionGroup group in groups)
                    {
                        if (sideA.connection != group.connectionA || sideA.connection != group.connectionB || sideB.connection != group.connectionA || sideB.connection != group.connectionB)
                            continue;

                        if (group.connectionA.Equals(group.connectionB) && group.canConnectToSelf)
                            return true;
                    }
                }
                else
                {
                    foreach (ConnectionGroup group in groups)
                    {
                        if (group.connectionA != group.connectionB)
                        {
                            var otherConnection = sideA.connection == group.connectionA ? group.connectionB : group.connectionA;
                            if (sideB.connection.Equals(otherConnection))
                                return true;
                            otherConnection = sideB.connection == group.connectionA ? group.connectionB : group.connectionB;

                            if (sideA.connection.Equals(otherConnection))
                                return true;
                        }
                    }
                }
                return false;
            }
            else
            {
                return sideA.connection.Equals(sideB.connection);
            }
        }

        static List<ConnectionGroup> FindConnectionGroups(Connections c, List<ConnectionGroup> connectionGroups)
        {
            List<ConnectionGroup> list = connectionGroups.FindAll(x => x.connectionA.Equals(c) || x.connectionB.Equals(c));
            return list != null && list.Count > 0 ? list : null;
        }
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
