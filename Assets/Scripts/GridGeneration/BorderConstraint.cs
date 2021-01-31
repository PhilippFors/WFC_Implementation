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
            List<int> l = new List<int>();
            foreach (MyTile t in borderTiles)
                l.Add(t.ID);

            var Tiles = WFCUtil.FindTilesArr(tileSet, l.ToArray()),
            // var constraint = new DeBroglie.Constraints.BorderConstraint()
            // {
            //     Tiles = WFCUtil.FindTilesArr(tileSet, l.ToArray()),
            //     Sides = BorderSides.XMax | BorderSides.XMin | BorderSides.YMin | BorderSides.YMax,
            // };
            for (int i = 0; i < propagator.Topology.Width; i++)
                for (int j = 0; j < propagator.Topology.Height; i++)
                {
                    foreach (Tile t in Tiles)
                        if (!propagator.IsSelected(i, j, 0, t))
                        {

                        }
                }


            // if (useConstraint)
            //     constraint.Init(propagator);
        }
    }
}
