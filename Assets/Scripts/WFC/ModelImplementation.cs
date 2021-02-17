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
        public int gridSize = 1;
        public Vector3Int size = new Vector3Int(10, 1, 10);

        public bool backTrack;
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

        public TilePropagator p_Propagator
        {
            get
            {
                return propagator;
            }
        }

        /// <summary>
        /// Starting generation of WFC
        /// </summary>
        /// <param name="multithread"></param>
        /// <returns></returns>
        public abstract Coroutine Generate(bool multithread = true);

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

