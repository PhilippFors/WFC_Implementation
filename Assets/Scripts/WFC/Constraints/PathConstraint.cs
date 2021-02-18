using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeBroglie;
using DeBroglie.Constraints;

namespace MyWFC
{
    public class PathConstraint : CustomConstraint
    {
        public enum PathConstraintType
        {
            Connected,
            Acyclic,
            Looped,
            None
        }

        public PathConstraintType pathConstraintType;
        public List<MyTilePoint> endPoints = new List<MyTilePoint>();
        public List<MyTile> pathTiles = new List<MyTile>();

        private DeBroglie.Constraints.ConnectedConstraint connectedConstraint = null;
        private DeBroglie.Constraints.AcyclicConstraint acyclicConstraint = null;
        private DeBroglie.Constraints.LoopConstraint loopConstraint = null;

        public override void SetConstraint(TilePropagator propagator, RuntimeTile[] tileSet)
        {
            var spec = new DeBroglie.Constraints.PathSpec();
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
                {
                    paths.Add(t);
                }

            }

            spec.Tiles = paths;
            spec.RelevantCells = points;
            spec.RelevantTiles = paths;
            
            switch (pathConstraintType)
            {
                case PathConstraintType.Looped:
                    LoopedConstrainConfig(propagator, spec);
                    break;
                case PathConstraintType.Acyclic:
                    AcyclicConstrainConfig(propagator, spec);
                    break;
                case PathConstraintType.Connected:
                    ConnectedConstrainConfig(propagator, spec);
                    break;
                case PathConstraintType.None:

                    break;
            }

        }

        public DeBroglie.Constraints.ITileConstraint ReturnConstraint()
        {
            if (pathConstraintType.Equals(PathConstraintType.Connected))
                return connectedConstraint;
            if (pathConstraintType.Equals(PathConstraintType.Acyclic))
                return acyclicConstraint;
            if (pathConstraintType.Equals(PathConstraintType.Looped))
                return loopConstraint;

            return null;
        }

        void ConnectedConstrainConfig(TilePropagator p, PathSpec spec)
        {
            connectedConstraint = new DeBroglie.Constraints.ConnectedConstraint();

            connectedConstraint.PathSpec = spec;
            // connectedConstraint.UsePickHeuristic = true;
        }
        void AcyclicConstrainConfig(TilePropagator p, PathSpec spec)
        {
            acyclicConstraint = new DeBroglie.Constraints.AcyclicConstraint();
            acyclicConstraint.PathSpec = spec;
        }
        void LoopedConstrainConfig(TilePropagator p, PathSpec spec)
        {
            loopConstraint = new DeBroglie.Constraints.LoopConstraint();
            loopConstraint.PathSpec = spec;
        }
    }
}
