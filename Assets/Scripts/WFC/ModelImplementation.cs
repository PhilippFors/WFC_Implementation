using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DeBroglie;
using DeBroglie.Topo;

namespace MyWFC
{
    public abstract class ModelImplementation : MonoBehaviour
    {
        public MaskGenerator maskGenerator;
        public bool useMask;
        public InputSampler inputSampler;
        protected ITopoArray<Tile> sample;
        public int gridSize = 1;
        public Vector3Int size = new Vector3Int(10, 1, 10);
        public int backTrackDepth = 5;

        [Tooltip("Only useful when you have an existing input")]
        public bool periodicIN;
        public bool periodicOUT;
        protected int[,] rendering;
        protected GameObject output;
        protected int[,] modelOutput;
        protected int maxRoutines;
        protected GridTopology topology;
        protected TilePropagator propagator;


        /// <summary>
        /// Starting generation of WFC
        /// </summary>
        /// <param name="multithread"></param>
        /// <returns></returns>
        public virtual Coroutine Generate(bool multithread = true)
        {
            if (multithread)
                return StartCoroutine(StartGenerate());
            else
                StartGenerateSingle();

            return null;
        }

        protected void StartGenerateSingle()
        {
            maxRoutines = 0;
            CreateOutputObject();
            PrepareModel();
            PreparePropagator();
            RunModelSingle();
            Draw();
        }

        protected virtual IEnumerator StartGenerate()
        {
            maxRoutines = 0;
            CreateOutputObject();
            PrepareModel();
            PreparePropagator();
            yield return StartCoroutine(RunModel());
            Draw();
        }

        /// <summary>
        /// Creates the output gameObject under which the output is going to be parented.
        /// </summary>
        protected virtual void CreateOutputObject()
        {
            if (output != null)
            {
                if (Application.isPlaying) { Destroy(output); } else { DestroyImmediate(output); }
                output = new GameObject("output-" + gameObject.name);
                output.transform.parent = transform;
                output.transform.position = this.gameObject.transform.position;
                output.transform.rotation = this.gameObject.transform.rotation;
                output.transform.localScale = Vector3.one;
            }
            else
            {
                Transform ot = transform.Find("output-" + gameObject.name);
                if (ot != null) { output = ot.gameObject; }

                if (Application.isPlaying) { Destroy(output); } else { DestroyImmediate(output); }

                if (output == null)
                {
                    output = new GameObject("output-" + gameObject.name);
                    output.transform.parent = transform;
                    output.transform.position = this.gameObject.transform.position;
                    output.transform.rotation = this.gameObject.transform.rotation;
                    output.transform.localScale = Vector3.one;
                }
            }
        }

        /// <summary>
        /// Prepares the WFC Model of choice.
        /// </summary>
        protected abstract void PrepareModel();

        /// <summary>
        /// Sets up the Propagator.
        /// </summary>
        protected abstract void PreparePropagator();

        /// <summary>
        /// Checks for any constraint components that were added to the gameObject and applies them to the propagator.
        /// </summary>
        protected virtual void AddConstraints()
        {
            CustomConstraint[] constraints = GetComponents<CustomConstraint>();
            if (constraints != null)
                foreach (CustomConstraint c in constraints)
                    c.SetConstraint(propagator, inputSampler.runtimeTiles);
        }
        protected virtual void ApplyMask()
        {
            if (maskGenerator != null && useMask)
            {
                foreach (Point p in maskGenerator.areaEdges)
                {

                    if (maskGenerator.mask[p.x, p.y])
                    {
                        propagator.Select(p.x, p.y, p.z, WFCUtil.FindTilesList(inputSampler.runtimeTiles, maskGenerator.borderTiles));
                    }

                }
                foreach (Point p in maskGenerator.passageEdges)
                {

                    if (maskGenerator.mask[p.x, p.y])
                    {
                        propagator.Ban(p.x, p.y, p.z, WFCUtil.FindTilesList(inputSampler.runtimeTiles, maskGenerator.borderTiles));
                    }
                }
            }
        }

        /// <summary>
        /// Runs the model. Retries generation if it fails.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator RunModel()
        {
            if (maxRoutines > 10)
            {
                yield break;
            }

            var status = propagator.Run();
            modelOutput = propagator.ToValueArray<int>().ToArray2d();

            if (status == DeBroglie.Resolution.Contradiction || status == DeBroglie.Resolution.Undecided)
            {
                maxRoutines++;
                yield return StartCoroutine(RunModel());
            }
        }

        protected virtual void RunModelSingle()
        {
            if (maxRoutines > 10)
            {
                return;
            }

            var status = propagator.Run();
            modelOutput = propagator.ToValueArray<int>().ToArray2d();

            if (status == DeBroglie.Resolution.Contradiction || status == DeBroglie.Resolution.Undecided)
            {
                maxRoutines++;
                RunModel();
            }
        }

        /// <summary>
        /// Instantiates Tiles from the tileset according to the modeloutput.
        /// </summary>
        public abstract void Draw();

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(new Vector3(size.x * gridSize / 2f - gridSize * 0.5f, size.y * gridSize / 2f, size.z * gridSize / 2f - gridSize * 0.5f),
                                new Vector3(size.x * gridSize, size.y * gridSize, size.z * gridSize));
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(ModelImplementation), true)]
    public class WFCEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ModelImplementation t = (ModelImplementation)target;

            if (GUILayout.Button("Generate"))
            {
                t.Generate(true);
            }

            if (GUILayout.Button("Stop"))
            {
                t.StopAllCoroutines();
            }
            DrawDefaultInspector();
        }
    }
#endif

}

