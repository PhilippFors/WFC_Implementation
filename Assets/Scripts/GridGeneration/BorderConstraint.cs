using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeBroglie.Constraints;
using DeBroglie;
namespace MyWFC
{
    public class BorderConstraint : CustomConstraint
    {
        [SerializeField] MyTile borderTile;

        public override void SetConstraint(TilePropagator propagator, RuntimeTile[] tileSet)
        {
            var constraint = new DeBroglie.Constraints.BorderConstraint()
            {
                Tiles = WFCUtil.FindTilesArr(tileSet, borderTile.ID),
                Sides = BorderSides.XMax | BorderSides.XMin | BorderSides.YMin | BorderSides.YMax,
            };

            if (useConstraint)
                constraint.Init(propagator);
        }
    }
}
