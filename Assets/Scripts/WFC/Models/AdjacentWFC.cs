using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeBroglie;
using DeBroglie.Topo;
using DeBroglie.Models;

namespace MyWFC
{
    [AddComponentMenu("Adjacent WFC")]
    public class AdjacentWFC : ModelImplementation
    {
        [SerializeField] private TileSet tileSet;
        [SerializeField] private ConnectionGroups ConnectionGroups;

        [Tooltip("Keep rotations to 1 if you have a complete tileset with all rotations")] [SerializeField]
        private List<RuntimeTile> runtimeTiles;

        [SerializeField] private DeBroglie.Wfc.ModelConstraintAlgorithm modelConstraintAlgorithm;

        public override Coroutine Generate(bool useCoroutine = true)
        {
            if (useCoroutine)
            {
                return StartCoroutine(StartGenerate());
            }
            else
            {
                StartCoroutine(StartGenerate());
            }

            return null;
        }

        protected IEnumerator StartGenerate()
        {
            CreateOutputObject();

            retries = 0;
            gridSize = tileSet.GridSize;
            runtimeTiles = new List<RuntimeTile>();

            var model = GetModel();
            var topology = GetTopology();

            CreateTileRotations(model);

            TilePropagatorOptions options = new TilePropagatorOptions();
            options.BackTrackDepth = backTrack ? backTrackDepth : 0;
            options.PickHeuristicType = PickHeuristicType.MinEntropy;
            options.ModelConstraintAlgorithm = modelConstraintAlgorithm;

            // DeBroglie constraints should be added into the propagator via constructor
            var p = GetPathConstraint();
            if (p != null)
            {
                options.Constraints = new[] {p};
            }

            var propagator = new TilePropagator(model, topology, options);

            ApplyConstraints(propagator);

            mainCoroutine = StartCoroutine(RunModel(propagator));
            yield return mainCoroutine;

            DrawOutput();
        }

        private AdjacentModel GetModel()
        {
            var model = new AdjacentModel();
            model.SetDirections(DirectionSet.Cartesian3d);
            return model;
        }

        /// <summary>
        /// Creates a unique verison of a tile in the tileset for each rotational axis.
        /// </summary>
        private void CreateTileRotations(AdjacentModel model)
        {
            for (int i = 0; i < tileSet.tiles.Count; i++)
            {
                MyTile tile = tileSet.tiles[i].GetComponent<MyTile>();
                if (tileSet.tileUse[i])
                {
                    if (tile.hasRotation)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            var rot = j * 90;
                            
                            if (rot == 0)
                            {
                                var newTile = new RuntimeTile(tile.gameObject.GetHashCode(), rot, false, tile, tile.cells[0].sides);
                                runtimeTiles.Add(newTile);
                            }
                            else
                            {
                                List<TileSide> l = AdjacentUtil.TileSideCopy(tile.cells[0].sides);

                                //Hacky way for sides to keep their global orientation so adjacencies are much easier to compare later. See AdjacencyUtil class.
                                for (int sideIndex = 0; sideIndex < tile.cells[0].sides.Count; sideIndex++)
                                {
                                    int s = (int) l[sideIndex].side + rot;
                                    if (s >= 360)
                                    {
                                        s -= 360;
                                    }

                                    l[sideIndex].side = (Sides) s;
                                }

                                runtimeTiles.Add(new RuntimeTile(tile.gameObject.GetHashCode(), rot, false, tile, l));
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < tile.cells.Count; j++)
                        {
                            var newTile = new RuntimeTile(tile.gameObject.GetHashCode(), (int) tile.gameObject.transform.eulerAngles.y, false, tile, tile.cells[j].sides);
                            runtimeTiles.Add(newTile);
                        }
                    }
                }
            }

            AdjacentUtil.StartAdjacencyCheck(runtimeTiles, model, ConnectionGroups);
            SetFrequencies(model);
        }

        private void SetFrequencies(AdjacentModel model)
        {
            for (int i = 0; i < runtimeTiles.Count; i++)
            {
                model.SetFrequency(new Tile(i), WFCUtil.FindTileFrequenzy(runtimeTiles.ToArray(), tileSet, i));
            }
        }

        protected void PreparePropagator()
        {
            //Most custom constraints only call Select or Ban on the propagator and don't need to be passed into the constructor
        }

        protected override void ApplyConstraints(TilePropagator propagator)
        {
            CustomConstraint[] constraints = GetComponents<CustomConstraint>();
            if (constraints != null)
            {
                foreach (CustomConstraint c in constraints)
                {
                    if (c.useConstraint)
                        c.SetConstraint(runtimeTiles.ToArray(), propagator);
                }
            }
        }

        private DeBroglie.Constraints.ITileConstraint GetPathConstraint()
        {
            var pathC = GetComponent<MyWFC.PathConstraint>();
            if (pathC != null && pathC.useConstraint)
            {
                pathC.SetConstraint(runtimeTiles.ToArray());
                return pathC.GetConstraint();
            }
            else
            {
                return null;
            }
        }

        private void DrawOutput()
        {
            for (int x = 0; x < modelOutput.GetLength(0); x++)
            {
                for (int y = 0; y < modelOutput.GetLength(1); y++)
                {
                    for (int z = 0; z < modelOutput.GetLength(2); z++)
                    {
                        DrawSingleTile(x, y, z);
                    }
                }
            }
        }


        /// <summary>
        /// Instantiates a single tile at position x, y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        private void DrawSingleTile(int x, int y, int z)
        {
            var tile = modelOutput[x, y, z];

            Vector3 pos = new Vector3(x * gridSize, y * gridSize, z * gridSize);
            GameObject fab = runtimeTiles[(int) tile].obj.gameObject;
            
            if (fab != null)
            {
                GameObject newTile = Instantiate(fab, new Vector3(), Quaternion.identity) as GameObject;
                Vector3 fscale = newTile.transform.localScale;
                newTile.transform.parent = outputObject.transform;
                newTile.transform.localPosition = pos;

                newTile.transform.localEulerAngles = new Vector3(0, runtimeTiles[(int) tile].rotation, 0);

                newTile.transform.localScale = fscale;
            }
        }
    }
}