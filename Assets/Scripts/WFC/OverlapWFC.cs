using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeBroglie;
using DeBroglie.Topo;

namespace MyWFC
{
    [AddComponentMenu("My Overlap WFC")]
    public class OverlapWFC : ModelImplementation
    {
        public int n = 2;
        protected DeBroglie.Models.OverlappingModel model;

        public override void Generate(bool multithread)
        {
            maxRoutines = 0;
            inputSampler.Train();
            if (multithread)
                StartCoroutine(StartGenerate());
            else
                StartGenerateSingle();
        }

        protected override IEnumerator StartGenerate()
        {
            CreateOutputObject();
            PrepareModel();
            PreparePropagator();

            yield return StartCoroutine(RunModel());
            // Debug.Log("Finished, " + maxRoutines);
            Draw();
        }

        protected override void PrepareModel()
        {
            sample = TopoArray.Create<Tile>(inputSampler.sample, periodicIN);

            model = new DeBroglie.Models.OverlappingModel(n);
            model.AddSample(sample);

            topology = new GridTopology(size.x, size.z, periodicOUT);

            if (maskGenerator != null && useMask)
            {
                maskGenerator.size = size;
                maskGenerator.Generate(false);
                topology = topology.WithMask(TopoArray.Create<bool>(maskGenerator.mask, topology));
            }
        }


        protected override void PreparePropagator()
        {
            TilePropagatorOptions options = new TilePropagatorOptions();
            options.BackTrackDepth = backTrackDepth;
            options.PickHeuristicType = PickHeuristicType.MinEntropy;

            propagator = new TilePropagator(model, topology, options);
            if (maskGenerator != null && useMask)
            {
                foreach (Point p in maskGenerator.areaEdges)
                {
                    propagator.Select(p.x, p.y, p.z, WFCUtil.FindTile(inputSampler.runtimeTiles, 1));
                }
            }
            AddConstraints();
            ApplyMask();
        }
        
        public override void Draw()
        {
            for (int x = 0; x < modelOutput.GetLength(0); x++)
            {
                // for (int y = 0; y < modelOutput.GetLength(1) - 1; y++)
                for (int z = 0; z < modelOutput.GetLength(1); z++)
                {
                    if (useMask)
                    {
                        if (maskGenerator != null && maskGenerator.mask[x, z])
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

            if ((int)tile < inputSampler.runtimeTiles.Length)
            {
                Vector3 pos = new Vector3(x * gridSize, 0, z * gridSize);
                GameObject fab = GetTilePrefab(tile) as GameObject;
                if (fab != null)
                {
                    MyTile newTile = Instantiate(fab, new Vector3(), Quaternion.identity).GetComponent<MyTile>();
                    newTile.coords = new Vector2Int(x, z);
                    Vector3 fscale = newTile.transform.localScale;
                    newTile.transform.parent = output.transform;
                    newTile.transform.localPosition = pos;
                    newTile.transform.localEulerAngles = new Vector3(0, inputSampler.runtimeTiles[(int)tile].rotation, 0);
                    newTile.transform.localScale = fscale;
                }
            }
        }
        protected GameObject GetTilePrefab(Tile tile)
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
        protected GameObject GetTilePrefab(int tile)
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
    }

}