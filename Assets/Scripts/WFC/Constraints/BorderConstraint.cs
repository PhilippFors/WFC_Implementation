using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeBroglie.Constraints;
using DeBroglie;
namespace MyWFC
{
    public class BorderConstraint : CustomConstraint
    {
        [SerializeField] List<MyTile> borderTiles = new List<MyTile>();

        public override void SetConstraint(TilePropagator propagator, RuntimeTile[] tileSet)
        {
            MyWFC.FixedTileCostraint c = GetComponent<MyWFC.FixedTileCostraint>();

            List<int> l = new List<int>();
            foreach (MyTile t in borderTiles)
                l.Add(t.ID);

            var Tiles = WFCUtil.FindTilesArr(tileSet, l.ToArray());
            // var constraint = new DeBroglie.Constraints.BorderConstraint()
            // {
            //     Tiles = WFCUtil.FindTilesArr(tileSet, l.ToArray()),
            //     Sides = BorderSides.XMax | BorderSides.XMin | BorderSides.YMin | BorderSides.YMax,
            // };
            for (int i = 0; i < propagator.Topology.Width; i++)
                for (int j = 0; j < propagator.Topology.Height; j++)
                {
                    bool cont = false;
                    if (c != null && c.useConstraint)
                        foreach (MyTilePoint p in c.pointList)
                            if (i == p.point.x && j == p.point.y)
                                cont = true;
                    if (cont)
                    {
                        continue;
                    }
                    else
                    {
                        if (i == 0 || i == propagator.Topology.Width - 1 || j == 0 || j == propagator.Topology.Height - 1)
                        {
                            propagator.Select(i, j, 0, Tiles);
                        }
                    }

                }


            // if (useConstraint)
            //     constraint.Init(propagator);
        }
    }
}
