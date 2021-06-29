using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeBroglie;
using DeBroglie.Models;
using DeBroglie.Topo;

namespace MyWFC
{
    [AddComponentMenu("My Overlap WFC")]
    public class OverlapWFC : ModelImplementation
    {
        public int n = 2;

        // protected DeBroglie.Models.OverlappingModel model;
        public InputSampler inputSampler;
        protected ITopoArray<Tile> sample;

        public override Coroutine Generate(bool useCoroutine)
        {
            retries = 0;
            
#if UNITY_EDITOR
            inputSampler.Train();
#endif
            
            if (useCoroutine)
                return StartCoroutine(StartGenerate());
            else
                StartCoroutine(StartGenerate());

            return null;
        }

        protected IEnumerator StartGenerate()
        {
            CreateOutputObject();

            sample = TopoArray.Create<Tile>(inputSampler.Sample, periodicIN);

            var model = GetModel(sample);
            var topology = GetTopology();

            TilePropagatorOptions options = new TilePropagatorOptions();
            options.BackTrackDepth = backTrackDepth;
            options.PickHeuristicType = PickHeuristicType.MinEntropy;

            var propagator = new TilePropagator(model, topology, options);

            yield return StartCoroutine(RunModel(propagator));
            DrawOutput();
        }

        private OverlappingModel GetModel(ITopoArray<Tile> sample)
        {
            var m = new DeBroglie.Models.OverlappingModel(n);
            m.AddSample(sample);
            return m;
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

            if ((int) tile < inputSampler.RuntimeTiles.Length)
            {
                Vector3 pos = new Vector3(x * gridSize, 0, z * gridSize);
                var obj = inputSampler.RuntimeTiles[(int) tile].obj.gameObject;
                if (obj != null)
                {
                    MyTile newTile = Instantiate(obj, new Vector3(), Quaternion.identity).GetComponent<MyTile>();
                    newTile.coords = new Vector2Int(x, z);
                    Vector3 fscale = newTile.transform.localScale;
                    newTile.transform.parent = outputObject.transform;
                    newTile.transform.localPosition = pos;
                    newTile.transform.localEulerAngles =
                        new Vector3(0, inputSampler.RuntimeTiles[(int) tile].rotation, 0);
                    newTile.transform.localScale = fscale;
                }
            }
        }

        protected override void ApplyConstraints(TilePropagator propagator)
        {
        }
    }
}