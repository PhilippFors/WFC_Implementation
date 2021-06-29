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
        [SerializeField] protected Vector3Int size = new Vector3Int(10, 1, 10);
        [SerializeField] protected bool backTrack;
        [SerializeField] protected int backTrackDepth = 5;
        [SerializeField] protected int maxRetries = 10;
        [SerializeField] protected bool periodicIN;
        [SerializeField] protected bool periodicOUT;
        
        protected GameObject outputObject;
        protected int[,,] modelOutput;
        protected int retries;
        protected Coroutine mainCoroutine;

        /// <summary>
        /// Starting generation of WFC
        /// </summary>
        /// <param name="useCoroutine"></param>
        /// <returns></returns>
        public abstract Coroutine Generate(bool useCoroutine = true);
        
        protected abstract void ApplyConstraints(TilePropagator propagator);
        
        /// <summary>
        /// Creates the output gameObject under which the output is going to be parented.
        /// </summary>
        protected void CreateOutputObject()
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

        protected GridTopology GetTopology()
        {
            return new GridTopology(size.x, size.y, size.z, periodicOUT);
        }
        
        private float timeout = 5f;
        /// <summary>
        /// Runs the model. Retries generation if it fails.
        /// </summary>
        /// <returns></returns>
        protected IEnumerator RunModel(TilePropagator propagator)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                propagator.Clear();
                
                ApplyConstraints(propagator);

                DeBroglie.Resolution status = DeBroglie.Resolution.Undecided;

                while (status == DeBroglie.Resolution.Undecided)
                {
                    status = propagator.Step();
                }

                modelOutput = propagator.ToValueArray<int>().ToArray3d();

                if (status != DeBroglie.Resolution.Contradiction)
                {
                    yield break;
                }

                yield return null;
            }
            
            Debug.Log("Generation not successful");
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

