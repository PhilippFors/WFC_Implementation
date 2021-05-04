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
        [SerializeField] protected int gridSize = 1;
        public Vector3Int size = new Vector3Int(10, 1, 10);
        public bool backTrack;
        public int backTrackDepth = 5;

        [Tooltip("Only useful when you have an existing input")]
        public bool periodicIN;
        public bool periodicOUT;
        protected GameObject outputObject;
        protected int[,,] modelOutput;
        public int maxRetries = 10;
        protected int retries;
        protected GridTopology topology;
        protected TilePropagator propagator;
        protected Coroutine mainCoroutine;

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
            var t = transform.Find("output-" + gameObject.name);
            if (t != null)
            {
                outputObject = t.gameObject;
            }
            if (outputObject != null)
            {
                if (Application.isPlaying) { Destroy(outputObject); } else { DestroyImmediate(outputObject); }
                outputObject = new GameObject("output-" + gameObject.name);
                outputObject.transform.parent = transform;
                outputObject.transform.position = this.gameObject.transform.position;
                outputObject.transform.rotation = this.gameObject.transform.rotation;
                outputObject.transform.localScale = Vector3.one;
            }
            else
            {
                Transform ot = transform.Find("output-" + gameObject.name);
                if (ot != null) { outputObject = ot.gameObject; }

                if (Application.isPlaying) { Destroy(outputObject); } else { DestroyImmediate(outputObject); }

                if (outputObject == null)
                {
                    outputObject = new GameObject("output-" + gameObject.name);
                    outputObject.transform.parent = transform;
                    outputObject.transform.position = this.gameObject.transform.position;
                    outputObject.transform.rotation = this.gameObject.transform.rotation;
                    outputObject.transform.localScale = Vector3.one;
                }
            }
        }

        private float timeout = 5f;
        /// <summary>
        /// Runs the model. Retries generation if it fails.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator RunModel()
        {
            for (int i = 0; i < maxRetries; i++)
            {
                var timeoutCoroutine = StartCoroutine(TimeOut());

                propagator.Clear();
                
                ApplyConstraints();

                DeBroglie.Resolution status = DeBroglie.Resolution.Undecided;

                while (status == DeBroglie.Resolution.Undecided)
                {
                    status = propagator.Step();
                }

                modelOutput = propagator.ToValueArray<int>().ToArray3d();

                if (status != DeBroglie.Resolution.Contradiction)
                {
                    StopCoroutine(timeoutCoroutine);
                    yield break;
                }

                yield return null;

                StopCoroutine(timeoutCoroutine);
            }
            Debug.Log("Generation not successful");
        }

        private IEnumerator TimeOut()
        {
            float time = 0;
            while (time < timeout)
            {
                time += Time.deltaTime;
                yield return null;
            }
            if (mainCoroutine != null)
            {
                StopCoroutine(mainCoroutine);
            }
            Debug.Log("Timeout!");
        }

        protected virtual void RunModelSingle()
        {
            for (int i = 0; i < maxRetries; i++)
            {
                var status = propagator.Run();
                modelOutput = propagator.ToValueArray<int>().ToArray3d();

                if (status != DeBroglie.Resolution.Contradiction)
                {
                    return;
                }
            }
        }
        protected abstract void ApplyConstraints();
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

