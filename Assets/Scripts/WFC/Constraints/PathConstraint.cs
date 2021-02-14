using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeBroglie;
using DeBroglie.Constraints;

namespace MyWFC
{
    public class PathConstraint : CustomConstraint
    {
        public List<MyTilePoint> endPoints = new List<MyTilePoint>();
        public List<MyTile> pathTiles = new List<MyTile>();

        public DeBroglie.Constraints.PathConstraint pathConstraint;
        public override void SetConstraint(TilePropagator propagator, RuntimeTile[] tileSet)
        {
            DeBroglie.Point[] points = new DeBroglie.Point[endPoints.Count];
            HashSet<Tile> endpointTiles = new HashSet<Tile>();
            for (int i = 0; i < points.Length; i++)
            {
                // if (endPoints[i].tile != null)
                // {
                //     var allTiles = WFCUtil.FindTilesArr(tileSet, endPoints[i].tile.ID);
                //     foreach (Tile t in allTiles)
                //         endpointTiles.Add(t);
                // }

                points[i] = new DeBroglie.Point(endPoints[i].point.x, endPoints[i].point.y, endPoints[i].point.z);
            }

            HashSet<Tile> paths = new HashSet<Tile>();
            foreach (MyTile tile in pathTiles)
            {
                var allTiles = WFCUtil.FindTilesArr(tileSet, tile.gameObject.GetHashCode());
                foreach (Tile t in allTiles)
                    paths.Add(t);
            }

            pathConstraint = new DeBroglie.Constraints.PathConstraint(paths, points);
            if (endpointTiles.Count > 0)
                pathConstraint.EndPointTiles = endpointTiles;

        }
    }
}
