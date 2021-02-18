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
        public TileSet tileSet;
        public ConnectionGroups ConnectionGroups;

        [Tooltip("Keep rotations to 1 if you have a complete tileset with all rotations")]
        [HideInInspector] public int rotations = 4;
        [HideInInspector] public List<RuntimeTile> runtimeTiles;
        public DeBroglie.Wfc.ModelConstraintAlgorithm modelConstraintAlgorithm;
        AdjacentModel model;
        int bIDCounter = 0;

        public override Coroutine Generate(bool multithread = true)
        {
            if (multithread)
                return StartCoroutine(StartGenerate());
            else
                StartCoroutine(StartGenerate());

            return null;
        }

        protected IEnumerator StartGenerate()
        {
            retries = 0;
            CreateOutputObject();
            PrepareModel();
            PreparePropagator();

            yield return StartCoroutine(RunModel());

            DrawOutput();
        }

        protected void PrepareModel()
        {
            bIDCounter = 0;
            runtimeTiles = new List<RuntimeTile>();

            model = new AdjacentModel();
            model.SetDirections(DirectionSet.Cartesian3d);
            
            topology = new GridTopology(size.x, size.y, size.z, periodicOUT);
        
            CreateTileRotations();
        }

        /// <summary>
        /// Creates a unique verison of a tile in the tileset for each rotational axis.
        /// </summary>
        void CreateTileRotations()
        {
            for (int i = 0; i < tileSet.tiles.Count; i++)
            {
                MyTile t = tileSet.tiles[i].GetComponent<MyTile>();
                if (tileSet.tileUse[i])
                    if (t.hasRotation)
                        for (int j = 0; j < rotations; j++)
                        {
                            for (int z = 0; z < t.cells.Count; z++)
                            {
                                int rot = j * 90;
                                if (rot == 0)
                                {
                                    if (t.cells.Count > 1)
                                        t.runtimeIDs.Add(runtimeTiles.Count);

                                    runtimeTiles.Add(new RuntimeTile(t.gameObject.GetHashCode(), t.cells.Count > 1 ? bIDCounter : -1, rot, tileSet.frequenzies[i], false, t.gameObject, t.cells[z].sides));
                                }
                                else
                                {
                                    List<TileSide> l = AdjacentUtil.TileSideCopy(t.cells[z].sides);

                                    //Hacky way for sides to keep their global orientation so adjacencies are much easier to compare later. See AdjacencyUtil class.
                                    for (int sideIndex = 0; sideIndex < t.cells[z].sides.Count; sideIndex++)
                                    {
                                        int s = (int)l[sideIndex].side + rot;
                                        if (s >= 360)
                                            s -= 360;

                                        l[sideIndex].side = (Sides)s;
                                    }

                                    if (t.cells.Count > 1)
                                        t.runtimeIDs.Add(runtimeTiles.Count);

                                    runtimeTiles.Add(new RuntimeTile(t.gameObject.GetHashCode(), t.cells.Count > 1 ? bIDCounter : -1, rot, tileSet.frequenzies[i], false, t.gameObject, l));
                                }
                            }
                            if (t.cells.Count > 1)
                                bIDCounter++;
                        }
                    else
                    {
                        for (int j = 0; j < t.cells.Count; j++)
                        {
                            if (t.cells.Count > 1)
                                t.runtimeIDs.Add(runtimeTiles.Count);

                            runtimeTiles.Add(new RuntimeTile(t.gameObject.GetHashCode(), (int)t.gameObject.transform.eulerAngles.y, tileSet.frequenzies[j], false, t.gameObject, t.cells[j].sides));
                        }
                    }
            }

            AdjacentUtil.StartAdjacencyCheck(runtimeTiles, model, ConnectionGroups);
            SetFrequencies();
        }

        #region unimportant
        // void BigTileCategorize(MyTile t)
        // {
        //     if (t.hasRotation)
        //     {
        //         for (int j = 0; j < rotations; j++)
        //         {
        //             for (int i = 0; i < t.cells.Count; i++)
        //             {
        //                 int rot = j * 90;
        //                 if (rot == 0)
        //                 {
        //                     t.runtimeIDs.Add(runtimeTiles.Count);
        //                     runtimeTiles.Add(new RuntimeTile(t.ID, bIDCounter, rot, t.weight, false, t.gameObject, t.cells[i].sides));
        //                 }
        //                 else
        //                 {
        //                     List<TileSide> l = AdjacentUtil.TileSideCopy(t.cells[i].sides);

        //                     //Hacky way for sides to keep their global orientation so adjacencies much easier to compare.
        //                     for (int sideIndex = 0; sideIndex < t.cells[i].sides.Count; sideIndex++)
        //                     {
        //                         int s = (int)l[sideIndex].side + rot;
        //                         if (s >= 360)
        //                             s -= 360;

        //                         l[sideIndex].side = (Sides)s;
        //                     }

        //                     t.runtimeIDs.Add(runtimeTiles.Count);
        //                     runtimeTiles.Add(new RuntimeTile(t.ID, bIDCounter, rot, t.weight, false, t.gameObject, l));
        //                 }
        //             }

        //         }
        //     }
        //     else
        //     {
        //         for (int i = 0; i < t.cells.Count; i++)
        //         {
        //             t.runtimeIDs.Add(runtimeTiles.Count);
        //             runtimeTiles.Add(new RuntimeTile(t.ID, bIDCounter, 0, t.weight, false, t.gameObject, t.cells[i].sides));
        //         }

        //     }
        //     bIDCounter++;

        // }
        #endregion

        void SetFrequencies()
        {
            for (int i = 0; i < runtimeTiles.Count; i++)
                model.SetFrequency(new Tile(i), WFCUtil.FindTileFrequenzy(runtimeTiles.ToArray(), tileSet, i));
        }

        protected void PreparePropagator()
        {
            TilePropagatorOptions options = new TilePropagatorOptions();
            options.BackTrackDepth = backTrack ? backTrackDepth : 0;
            options.PickHeuristicType = PickHeuristicType.MinEntropy;
            options.ModelConstraintAlgorithm = modelConstraintAlgorithm;
            
            //DeBroglie constraints should be added into the propagator via constructor
            var p = GetPathConstraint();
            if (p != null)
                options.Constraints = new[] { p };

            propagator = new TilePropagator(model, topology, options);

            //Most custom constraints only call Select or Ban on the propagator and don't need to be passed into the constructor
            this.ApplyConstraints();

            this.ApplyMask();
        }

        protected void ApplyConstraints()
        {
            CustomConstraint[] constraints = GetComponents<CustomConstraint>();
            if (constraints != null)
                foreach (CustomConstraint c in constraints)
                {
                    if (c.useConstraint)
                        c.SetConstraint(propagator, runtimeTiles.ToArray());
                }
        }

        DeBroglie.Constraints.ITileConstraint GetPathConstraint()
        {
            var pathC = GetComponent<MyWFC.PathConstraint>();
            if (pathC != null && pathC.useConstraint)
            {
                pathC.SetConstraint(propagator, runtimeTiles.ToArray());
                return pathC.ReturnConstraint();
            }
            else
                return null;
        }

        protected void ApplyMask()
        {
            if (maskGenerator != null && maskGenerator.maskOutput != null && useMask)
            {
                for (int x = 0; x < size.x; x++)
                    for (int y = 0; y < size.y; y++)
                        for (int z = 0; z < size.z; z++)
                            if (!maskGenerator.maskOutput[x, y, z])
                                propagator.Select(x, y, z, WFCUtil.FindTileList(runtimeTiles.ToArray(), tileSet.empty.GetHashCode()));

                // foreach (MaskArea area in maskGenerator.allAreas)
                foreach (Point p in maskGenerator.areaEdges)
                {
                    // if (!propagator.IsSelected(p.x, p.y, 0, WFCUtil.FindTile(runtimeTiles.ToArray(), tileSet.empty.GetHashCode())))
                    propagator.Select(p.x, p.y, p.z, WFCUtil.FindTileList(runtimeTiles.ToArray(), tileSet.empty.GetHashCode()));
                }
            }
        }

        public void DrawOutput()
        {
            for (int x = 0; x < modelOutput.GetLength(0); x++)
                for (int y = 0; y < modelOutput.GetLength(1); y++)
                    for (int z = 0; z < modelOutput.GetLength(2); z++)
                    {
                        if (IsBigTile(x, y, z))
                            DrawBigTile(x, y, z);
                        else
                            DrawSingleTile(x, y, z);
                    }
        }


        /// <summary>
        /// Instantiates a single tile at position x, y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        void DrawSingleTile(int x, int y, int z)
        {
            var tile = modelOutput[x, y, z];

            Vector3 pos = new Vector3(x * gridSize, y * gridSize, z * gridSize);
            GameObject fab = runtimeTiles[(int)tile].obj;
            if (fab != null)
            {
                GameObject newTile = Instantiate(fab, new Vector3(), Quaternion.identity) as GameObject;
                Vector3 fscale = newTile.transform.localScale;
                newTile.transform.parent = output.transform;
                newTile.transform.localPosition = pos;

                newTile.transform.localEulerAngles = new Vector3(0, runtimeTiles[(int)tile].rotation, 0);

                newTile.transform.localScale = fscale;
            }
        }

        void DrawBigTile(int x, int y, int z)
        {
            Debug.Log("BigTile at: " + x + ", " + y + ", Tile: " + modelOutput[x, y, z]);
            // var tileID = modelOutput[x, y];
            // var firstTile = WFCUtil.FindRuntimeTile(runtimeTiles.ToArray(), modelOutput[x, y]);
            // int bID = firstTile.bID;
            // int ID = firstTile.ID;
            // var myTile = firstTile.obj.GetComponent<MyTile>();

            // List<int> bigTileParts = new List<int>();
            // // float smallestX = int.MaxValue;
            // // float smallestY = int.MaxValue;

            // float biggestX = int.MinValue;
            // float biggestY = int.MinValue;

            // for (int i = x - 1; i < x + 1; i++)
            //     for (int j = y - 1; j < y + 1; j++)
            //     {
            //         if (IsInMap(i, j))
            //             if (myTile.runtimeIDs.Contains(modelOutput[i, j]))
            //             {
            //                 if (i > biggestX)
            //                     biggestX = i;
            //                 if (j > biggestY)
            //                     biggestY = j;
            //                 // if (i < smallestX)
            //                 //     smallestX = i;
            //                 // if (j < smallestY)
            //                 //     smallestY = j;
            //                 bigTileParts.Add(modelOutput[i, j]);
            //             }
            //     }

            // // if (myTile.runtimeIDs.Count == bigTileParts.Count)
            // // {
            // Vector3 pos = new Vector3(biggestX - 0.5f * gridSize, 0, biggestY - 0.5f * gridSize);
            // // GameObject fab = GetTilePrefab(tile) as GameObject;

            // GameObject newTile = Instantiate(firstTile.obj, new Vector3(), Quaternion.identity) as GameObject;
            // Vector3 fscale = newTile.transform.localScale;
            // newTile.transform.parent = output.transform;
            // newTile.transform.localPosition = pos;

            // if (useSample && inputSampler != null)
            //     newTile.transform.localEulerAngles = new Vector3(0, inputSampler.runtimeTiles[(int)tileID].rotation, 0);
            // else
            //     newTile.transform.localEulerAngles = new Vector3(0, runtimeTiles[(int)tileID].rotation, 0);

            // newTile.transform.localScale = fscale;
            // // }

        }

        bool IsInMap(int x, int y, int z)
        {
            return !(x < 0 || x >= modelOutput.GetLength(0) || y < 0 || y >= modelOutput.GetLength(1) || z < 0 || z >= modelOutput.GetLength(2));
        }
        bool IsBigTile(int x, int y, int z)
        {
            return WFCUtil.FindRuntimeTile(runtimeTiles.ToArray(), modelOutput[x, y, z]).bID != -1;
        }
    }
}