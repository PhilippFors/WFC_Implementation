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

                    var tileA = tileset[i].tileSides; //Reference Tile
                    var tileB = tileset[j].tileSides; // Tile to check against
                    foreach (TileSide sideA in tileA)
                    {
                        foreach (TileSide sideB in tileB)
                        {

                            switch (sideA.side)
                            {
                                case Sides.Left:
                                    if (sideB.side.Equals(Sides.Right) && CompareSides(sideA, rotA, sideB, rotB, Direction.LEFT, connectionGroups))
                                    {
                                        model.AddAdjacency(new Tile(i), new Tile(j), -1, 0, 0);
                                    }
                                    break;
                                case Sides.Right:
                                    if (sideB.side.Equals(Sides.Left) && CompareSides(sideA, rotA, sideB, rotB, Direction.RIGHT, connectionGroups))
                                    {
                                        model.AddAdjacency(new Tile(i), new Tile(j), 1, 0, 0);
                                    }
                                    break;
                                case Sides.Front:
                                    if (sideB.side.Equals(Sides.Back) && CompareSides(sideA, rotA, sideB, rotB, Direction.UP, connectionGroups))
                                    {
                                        model.AddAdjacency(new Tile(i), new Tile(j), 0, 1, 0);
                                    }
                                    break;
                                case Sides.Back:
                                    if (sideB.side.Equals(Sides.Front) && CompareSides(sideA, rotA, sideB, rotB, Direction.DOWN, connectionGroups))
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
        static ConnectionGroup FindConnectionGroup(Connections c, List<ConnectionGroup> connectionGroups)
        {
            return connectionGroups.Find(x => x.connectionA.Equals(c) || x.connectionB.Equals(c));
        }
        static bool CompareSides(TileSide sideA, int rotA, TileSide sideB, int rotB, Direction dir, List<ConnectionGroup> connectionGroups)
        {
            ConnectionGroup group = null;
            bool result = false;
            var smallerrot = rotA < rotB ? rotA : rotB;
            var biggerrot = rotA < rotB ? rotA : rotB;
            var difference = smallerrot - biggerrot;

            if (difference != 0)
            {
                if (smallerrot == 0)
                {
                    if (biggerrot == 90 && dir == Direction.LEFT || dir == Direction.RIGHT)
                        CheckReverse(sideA, sideB, connectionGroups);
                    else if (biggerrot == 180 && dir == Direction.LEFT || dir == Direction.RIGHT || dir == Direction.UP || dir == Direction.DOWN)
                        CheckReverse(sideA, sideB, connectionGroups);
                    else if (biggerrot == 270 && dir == Direction.UP || dir == Direction.DOWN)
                        CheckReverse(sideA, sideB, connectionGroups);
                }
                else if (smallerrot == 90)
                {
                    if (biggerrot == 180 && dir == Direction.UP || dir == Direction.DOWN)
                        CheckReverse(sideA, sideB, connectionGroups);
                    else if (biggerrot == 270 && dir == Direction.LEFT || dir == Direction.RIGHT || dir == Direction.UP || dir == Direction.DOWN)
                        CheckReverse(sideA, sideB, connectionGroups);
                }
                else if (smallerrot == 180)
                {
                    if (biggerrot == 270 && dir == Direction.UP || dir == Direction.DOWN)
                        CheckReverse(sideA, sideB, connectionGroups);
                }
            }
            foreach (SubSide subsideA in sideA.subSides)
            {
                if (connectionGroups != null && connectionGroups.Count > 0)
                    group = FindConnectionGroup(subsideA.connection, connectionGroups);

                foreach (SubSide subsideB in sideB.subSides)
                {
                    if (subsideA.side.Equals(subsideB.side))
                        if (!CheckConnections(subsideA, subsideB, group))
                            return false;
                }
            }
            return result;
        }

        static bool CheckReverse(TileSide sideA, TileSide sideB, List<ConnectionGroup> connectionGroups)
        {
            ConnectionGroup group = null;
            foreach (SubSide subsideA in sideA.subSides)
            {
                if (connectionGroups != null && connectionGroups.Count > 0)
                    group = FindConnectionGroup(subsideA.connection, connectionGroups);

                foreach (SubSide subsideB in sideB.subSides)
                {
                    if (subsideA.side == Sides.Middle && subsideB.side == Sides.Middle)
                        if (!CheckConnections(subsideA, subsideB, group))
                            return false;

                    if ((int)subsideA.side == (int)subsideB.side * -1)
                        if (!CheckConnections(subsideA, subsideB, group))
                            return false;
                }
            }
            return true;
        }

        static bool CheckConnections(SubSide a, SubSide b, ConnectionGroup group)
        {
            bool result = false;
            if (group != null)
            {
                var connectionGroup = group.connectionA.Equals(a.connection) ? group.connectionB : group.connectionA;
                if (!group.connectionA.Equals(group.connectionB))
                {
                    if ((a.connection.Equals(b.connection) && group.canConnectToSelf) || b.connection.Equals(connectionGroup))
                    {
                        result = true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (a.connection.Equals(b.connection) && group.canConnectToSelf)
                    {
                        result = true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (a.connection.Equals(b.connection))
                {
                    result = true;
                }
                else
                {
                    return false;
                }
            }
            return result;
        }
    }
}
