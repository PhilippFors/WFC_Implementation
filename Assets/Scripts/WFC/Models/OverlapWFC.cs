using System.Collections;
using UnityEngine;
using DeBroglie;
using DeBroglie.Models;
using DeBroglie.Topo;

namespace MyWFC
{
    [AddComponentMenu("Overlap WFC")]
    public class OverlapWFC : ModelImplementation
    {
        [SerializeField] private int n = 2;
        [SerializeField] private InputSampler inputSampler;
        private ITopoArray<Tile> sample;

        public override Coroutine Generate(bool useCoroutine)
        {
            retries = 0;

            if (useCoroutine)
                return StartCoroutine(StartGenerate());
            else
                StartGenerate();

            return null;
        }

        private IEnumerator StartGenerate()
        {
            CreateOutputObject();

            sample = TopoArray.Create<Tile>(inputSampler.Sample, periodicIN);

            var model = GetModel(sample);
            var topology = new GridTopology(size.x, size.y, size.z, periodicOUT);
            topology.Directions = DirectionSet.Cartesian3d;

            TilePropagatorOptions options = new TilePropagatorOptions();
            options.BackTrackDepth = backTrackDepth;
            options.PickHeuristicType = PickHeuristicType.MinEntropy;

            var propagator = new TilePropagator(model, topology, options);

            yield return StartCoroutine(RunModel(propagator));
            DrawOutput();
        }

        private OverlappingModel GetModel(ITopoArray<Tile> sample)
        {
            var m = new OverlappingModel(n);
            m.AddSample(sample);
            return m;
        }

        private void DrawOutput()
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

        private void DrawTile(int x, int y, int z)
        {
            var tile = modelOutput[x, y, z];

            if (tile != -1)
            {
                Vector3 pos = new Vector3(x * gridSize, 0, z * gridSize);
                
                // Get the correct gameobject based on the id in the Tiles
                var obj = inputSampler.RuntimeTiles[(int) tile].obj.gameObject;

                if (obj != null)
                {
                    MyTile newTile = Instantiate(obj, new Vector3(), Quaternion.identity).GetComponent<MyTile>();

                    newTile.transform.parent = outputObject.transform;
                    newTile.transform.localPosition = pos;
                    newTile.transform.localEulerAngles =
                        new Vector3(0, inputSampler.RuntimeTiles[(int) tile].rotation, 0);
                }
            }
        }

        protected override void ApplyConstraints(TilePropagator propagator)
        {
        }
    }
}