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
        public InputSampler inputSampler;
        protected ITopoArray<Tile> sample;

        public override Coroutine Generate(bool multithread)
        {
            retries = 0;
            inputSampler.Train();
            if (multithread)
                return StartCoroutine(StartGenerate());
            else
                StartCoroutine(StartGenerate());

            return null;
        }

        protected IEnumerator StartGenerate()
        {
            CreateOutputObject();
            PrepareModel();
            PreparePropagator();

            yield return StartCoroutine(RunModel());
            DrawOutput();
        }

        protected virtual void PrepareModel()
        {
            sample = TopoArray.Create<Tile>(inputSampler.sample, periodicIN);

            model = new DeBroglie.Models.OverlappingModel(n);
            model.AddSample(sample);

            topology = new GridTopology(size.x, size.y, size.z, periodicOUT);
        }

        protected virtual void PreparePropagator()
        {
            TilePropagatorOptions options = new TilePropagatorOptions();
            options.BackTrackDepth = backTrackDepth;
            options.PickHeuristicType = PickHeuristicType.MinEntropy;

            propagator = new TilePropagator(model, topology, options);
        }

        public virtual void DrawOutput()
        {
            for (int x = 0; x < modelOutput.GetLength(0); x++)
            {
                for (int y = 0; y < modelOutput.GetLength(1); y++)
                {
                    for (int z = 0; z < modelOutput.GetLength(2); z++)
                    {
                        DrawTile(x, y, z);
                    }
                }
            }
        }

        void DrawTile(int x, int y, int z)
        {
            var tile = modelOutput[x, y, z];

            if ((int)tile < inputSampler.runtimeTiles.Length)
            {
                Vector3 pos = new Vector3(x * gridSize, 0, z * gridSize);
                GameObject fab = inputSampler.runtimeTiles[(int)tile].obj;
                if (fab != null)
                {
                    MyTile newTile = Instantiate(fab, new Vector3(), Quaternion.identity).GetComponent<MyTile>();
                    newTile.coords = new Vector2Int(x, z);
                    Vector3 fscale = newTile.transform.localScale;
                    newTile.transform.parent = outputObject.transform;
                    newTile.transform.localPosition = pos;
                    newTile.transform.localEulerAngles = new Vector3(0, inputSampler.runtimeTiles[(int)tile].rotation, 0);
                    newTile.transform.localScale = fscale;
                }
            }
        }

        protected override void ApplyConstraints()
        {
            throw new System.NotImplementedException();
        }
    }
}