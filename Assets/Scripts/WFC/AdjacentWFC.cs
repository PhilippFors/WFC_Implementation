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

        [Tooltip("Keep rotations to 1 if you have a complete tileset with all rotations")]
        public int rotations = 4;
        public List<RuntimeTile> runtimeTiles;

        public ConnectionGroups ConnectionGroups;
        public bool useSample;
        AdjacentModel model;

        protected override IEnumerator StartGenerate()
        {
            maxRoutines = 0;
            if (useSample && inputSampler == null)
            {
                Debug.LogError("No sample to analyze.");
                yield break;
            }
            CreateOutputObject();
            PrepareModel();
            PreparePropagator();

            yield return StartCoroutine(RunModel());

            Draw();
        }

        protected override void PrepareModel()
        {
            runtimeTiles = new List<RuntimeTile>();

            model = new AdjacentModel();
            model.SetDirections(DirectionSet.Cartesian2d);

            CreateTileRotations();

            topology = new GridTopology(size.x, size.z, periodicOUT);

            this.CreateMask();
            // CheckAdjacencies();
            AdjacentUtil.CheckAdjacencies(runtimeTiles, model, ConnectionGroups);
            SetFrequencies();
        }

        void CreateTileRotations()
        {
            if (!useSample)
            {
                if (rotations > 1)
                    for (int i = 0; i < tileSet.tiles.Count; i++)
                    {
                        MyTile t = tileSet.tiles[i].GetComponent<MyTile>();

                        if (t.use)
                            if (t.hasRotation)
                                for (int j = 0; j < rotations; j++)
                                {
                                    int rot = j * 90;
                                    if (rot == 0)
                                    {
                                        runtimeTiles.Add(new RuntimeTile(t.ID, rot, t.weight, false, tileSet.tiles[i], t.sides));
                                    }
                                    else
                                    {
                                        List<TileSide> l = AdjacentUtil.TileSideCopy(t.sides);
                                        for (int sideIndex = 0; sideIndex < t.sides.Count; sideIndex++)
                                        {
                                            int s = (int)l[sideIndex].side + rot;
                                            if (s >= 360)
                                                s -= 360;

                                            l[sideIndex].side = (Sides)s;
                                        }
                                        runtimeTiles.Add(new RuntimeTile(t.ID, rot, t.weight, false, tileSet.tiles[i], l));
                                    }
                                }
                            else
                            {
                                runtimeTiles.Add(new RuntimeTile(t.ID, 0, t.weight, false, tileSet.tiles[i], t.sides));
                            }

                    }
                else
                {
                    foreach (GameObject o in tileSet.tiles)
                    {
                        var t = o.GetComponent<MyTile>();
                        runtimeTiles.Add(new RuntimeTile(t.ID, t.rotationDeg, t.weight, false, o));
                    }
                }
            }
            else
            {
                sample = TopoArray.Create<Tile>(inputSampler.sample, periodicIN);
                model.AddSample(sample);
            }
        }

        void SetFrequencies()
        {
            for (int i = 0; i < runtimeTiles.Count; i++)
                model.SetFrequency(new Tile(i), runtimeTiles[i].weight);
        }

        protected override void PreparePropagator()
        {
            TilePropagatorOptions options = new TilePropagatorOptions();
            options.BackTrackDepth = backTrackDepth;
            options.PickHeuristicType = PickHeuristicType.MinEntropy;

            var p = GetPathConstraint();
            if (p != null)
                options.Constraints = new[] { p };

            propagator = new TilePropagator(model, topology, options);

            this.ApplyMask();

            this.AddConstraints();
        }
        DeBroglie.Constraints.PathConstraint GetPathConstraint()
        {
            var constraints = GetComponent<MyWFC.PathConstraint>();
            if (constraints != null && constraints.useConstraint)
            {
                constraints.SetConstraint(propagator, runtimeTiles.ToArray());
                return constraints.pathConstraint;
            }
            return null;
        }
        protected override void AddConstraints()
        {
            CustomConstraint[] constraints = GetComponents<CustomConstraint>();
            if (constraints != null)
                foreach (CustomConstraint c in constraints)
                {
                    if (c.useConstraint)
                        if (useSample)
                            c.SetConstraint(propagator, inputSampler.runtimeTiles);
                        else
                            c.SetConstraint(propagator, runtimeTiles.ToArray());
                }
        }

        protected void CreateMask()
        {
            if (maskGenerator != null && useMask)
            {
                maskGenerator.size = size;
                maskGenerator.Generate(false);
                // topology = topology.WithMask(TopoArray.Create<bool>(maskGenerator.mask, topology));
            }
        }
        protected override void ApplyMask()
        {
            if (maskGenerator != null && useMask)
            {
                foreach (Point p in maskGenerator.areaEdges)
                {
                    if (useSample)
                    {
                        propagator.Select(p.x, p.y, p.z, WFCUtil.FindTilesList(inputSampler.runtimeTiles, maskGenerator.borderTiles));
                    }
                    else
                    {
                        propagator.Select(p.x, p.y, p.z, WFCUtil.FindTilesList(runtimeTiles.ToArray(), maskGenerator.borderTiles));
                    }
                }
                for (int i = 0; i < size.x; i++)
                    for (int j = 0; j < size.z; j++)
                        if (!maskGenerator.mask[i, j])
                            propagator.Select(i, j, 0, WFCUtil.FindTile(runtimeTiles.ToArray(), 0));
                // foreach (Point p in maskGenerator.passageEdges)
                // {
                //     if (useSample)
                //         propagator.Ban(p.x, p.y, p.z, WFCUtil.FindTile(inputSampler.runtimeTiles, 1));
                //     else
                //     {
                //         propagator.Ban(p.x, p.y, p.z, WFCUtil.FindTile(runtimeTiles.ToArray(), 1));
                //     }
                // }
            }
        }

        public override void Draw()
        {
            for (int x = 0; x < modelOutput.GetLength(0); x++)
            {
                for (int z = 0; z < modelOutput.GetLength(1); z++)
                {
                    if (useMask)
                    {
                        if (maskGenerator != null)
                        {
                            DrawTile(x, z);
                        }
                    }
                    else
                    {
                        DrawTile(x, z);
                    }
                }
            }
        }

        void DrawTile(int x, int z)
        {
            var tile = modelOutput[x, z];

            Vector3 pos = new Vector3(x * gridSize, 0, z * gridSize);
            GameObject fab = GetTilePrefab(tile) as GameObject;
            if (fab != null)
            {
                GameObject newTile = Instantiate(fab, new Vector3(), Quaternion.identity) as GameObject;
                Vector3 fscale = newTile.transform.localScale;
                newTile.transform.parent = output.transform;
                newTile.transform.localPosition = pos;

                if (useSample && inputSampler != null)
                    newTile.transform.localEulerAngles = new Vector3(0, inputSampler.runtimeTiles[(int)tile].rotation, 0);
                else
                    newTile.transform.localEulerAngles = new Vector3(0, runtimeTiles[(int)tile].rotation, 0);

                newTile.transform.localScale = fscale;
            }
        }

        protected GameObject GetTilePrefab(Tile tile)
        {
            if (useSample)
            {
                var runtimeTile = inputSampler.runtimeTiles[(int)tile.Value];
                foreach (GameObject obj in inputSampler.availableTiles)
                {
                    var wfc = obj.GetComponent<MyTile>();
                    if (wfc.ID == runtimeTile.ID)
                    {
                        return obj;
                    }
                }
                return null;
            }
            else
            {
                var runtimeTile = runtimeTiles[(int)tile.Value];
                foreach (GameObject obj in tileSet.tiles)
                {
                    var wfc = obj.GetComponent<MyTile>();
                    if (wfc.ID == runtimeTile.ID)
                    {
                        return obj;
                    }
                }
                return null;
            }
        }
        protected GameObject GetTilePrefab(int tile)
        {
            if (useSample)
            {
                var runtimeTile = inputSampler.runtimeTiles[tile];
                foreach (GameObject obj in inputSampler.availableTiles)
                {
                    var wfc = obj.GetComponent<MyTile>();
                    if (wfc.ID == runtimeTile.ID)
                    {
                        return obj;
                    }
                }
                return null;
            }
            else
            {
                var runtimeTile = runtimeTiles[tile];
                foreach (GameObject obj in tileSet.tiles)
                {
                    var wfc = obj.GetComponent<MyTile>();
                    if (wfc.ID == runtimeTile.ID)
                    {
                        return obj;
                    }
                }
                return null;
            }
        }
    }
}