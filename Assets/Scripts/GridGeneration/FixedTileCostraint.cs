using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeBroglie;
using DeBroglie.Constraints;

namespace MyWFC
{
    [System.Serializable]
    public class MyTilePoint
    {
        public Vector3Int point;
        public bool useRandomPoint;
        public MyTile tile;
    }
    public class FixedTileCostraint : CustomConstraint
    {
        [Tooltip("Enter a list of tiles and a fixed point")]
        public List<MyTilePoint> pointList = new List<MyTilePoint>();
        public override void SetConstraint(TilePropagator propagator, RuntimeTile[] tileSet)
        {
            if (useConstraint)
                foreach (MyTilePoint p in pointList)
                {
                    var tile = WFCUtil.FindTile(tileSet, p.tile.ID);
                    if (p.useRandomPoint)
                    {
                        p.point = WFCUtil.RandomPoint(propagator);
                    }
                    if (!propagator.IsSelected(p.point.x, p.point.y, p.point.z, tile) || !propagator.IsBanned(p.point.x, p.point.y, p.point.z, tile))
                    {
                        propagator.Select(p.point.x, p.point.y, p.point.z, tile);
                    }
                }
        }

    }
}
