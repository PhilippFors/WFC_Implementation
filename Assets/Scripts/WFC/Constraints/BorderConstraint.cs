using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeBroglie.Constraints;
using DeBroglie;
namespace MyWFC
{
    [System.Flags]
    public enum Borders
    {
        None = 0,
        XMin = 0x01,
        XMax = 0x02,
        YMin = 0x04,
        YMax = 0x08,
        ZMin = 0x10,
        ZMax = 0x20,
        All = 0x3F,
    }
    public class BorderConstraint : CustomConstraint
    {
        public List<MyTile> borderTiles = new List<MyTile>();
        public Borders sides;
        public bool respectFixedConstraint = true;
        public bool ban = false;
        public override void SetConstraint(TilePropagator propagator, RuntimeTile[] tileSet)
        {
            MyWFC.FixedTileCostraint c = GetComponent<MyWFC.FixedTileCostraint>();

            List<int> l = new List<int>();
            foreach (MyTile t in borderTiles)
                l.Add(t.gameObject.GetHashCode());

            var Tiles = WFCUtil.FindTilesArr(tileSet, l.ToArray());
            // var constraint = new DeBroglie.Constraints.BorderConstraint()
            // {
            //     Tiles = WFCUtil.FindTilesArr(tileSet, l.ToArray()),
            //     Sides = BorderSides.XMax | BorderSides.XMin | BorderSides.YMin | BorderSides.YMax,
            // };
            for (int x = 0; x < propagator.Topology.Width; x++)
            {
                var xmin = x == 0;
                var xmax = x == propagator.Topology.Width - 1;

                for (int y = 0; y < propagator.Topology.Height; y++)
                {
                    var ymin = y == 0;
                    var ymax = y == propagator.Topology.Height - 1;

                    for (int z = 0; z < propagator.Topology.Depth; z++)
                    {
                        var zmin = z == 0;
                        var zmax = z == propagator.Topology.Depth - 1;

                        bool cont = false;

                        if (c != null && respectFixedConstraint && c.useConstraint)
                        {
                            foreach (MyTilePoint p in c.pointList)
                            {
                                if (x == p.point.x && y == p.point.y && z == p.point.z)
                                {
                                    cont = true;
                                }
                            }
                        }
                        
                        if (cont)
                        {
                            continue;
                        }
                        else
                        {
                            if (Match(sides, xmin, xmax, ymin, ymax, zmin, zmax))
                            {
                                if (ban)
                                {
                                    bool isBanned;
                                    bool isSelected;
                                    propagator.GetBannedSelected(x, y, z, Tiles, out isBanned, out isSelected);

                                    if (!isSelected)
                                    {
                                        propagator.Ban(x, y, z, Tiles);
                                    }
                                }
                                else
                                {
                                    bool isBanned;
                                    bool isSelected;
                                    propagator.GetBannedSelected(x, y, z, Tiles, out isBanned, out isSelected);

                                    // if (!isBanned)
                                    // {
                                    propagator.Select(x, y, z, Tiles);
                                    // }
                                }

                            }
                        }
                    }
                }
            }
        }
        private bool Match(Borders sides, bool xmin, bool xmax, bool ymin, bool ymax, bool zmin, bool zmax)
        {
            return
                xmin && sides.HasFlag(Borders.XMin) ||
                xmax && sides.HasFlag(Borders.XMax) ||
                ymin && sides.HasFlag(Borders.YMin) ||
                ymax && sides.HasFlag(Borders.YMax) ||
                zmin && sides.HasFlag(Borders.ZMin) ||
                zmax && sides.HasFlag(Borders.ZMax);
        }
    }
}
