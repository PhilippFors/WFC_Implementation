using MyWFC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class Evaluations : MonoBehaviour
{
    public Coroutine coroutine;
    public abstract void StartEvaluation();
}

public class SingleGeneratorEval : Evaluations
{
    [SerializeField] private AdjacentWFC generator;
    [SerializeField] private List<float> generateTimes;
    [SerializeField] private int iterations;
    [SerializeField] private Vector3Int size = new Vector3Int(10, 1, 10);
    [SerializeField] private bool useConstraints;


    public override void StartEvaluation()
    {
        generator.size = size;
        generateTimes.Clear();

        var constraints = generator.GetComponents<CustomConstraint>();

        foreach (CustomConstraint c in constraints)
        {
            c.useConstraint = useConstraints;

            if (c is FixedTileCostraint)
            {
                var f = (FixedTileCostraint)c;
                f.pointList[0].point = new Vector3Int(size.x / 2, 0, 0);
                f.pointList[1].point = new Vector3Int(size.x / 2, 0, size.z - 1);
            }

            if (c is PathConstraint)
            {
                var p = (PathConstraint)c;
                p.endPoints[0].point = new Vector3Int(size.x / 2, 0, 0);
                p.endPoints[1].point = new Vector3Int(size.x / 2, 0, size.z - 1);
            }
        }

        coroutine = StartCoroutine(Eval());
    }

    private IEnumerator Eval()
    {
        for (int i = 0; i < iterations; i++)
        {
            yield return new WaitForSeconds(0.1f);
            
            var startTime = Time.realtimeSinceStartup;

            yield return generator.Generate(true);

            var endTime = (Time.realtimeSinceStartup - startTime) * 1000f;

            generateTimes.Add(endTime);
        }

        GenerateAverage();
    }

    private void GenerateAverage()
    {
        float sum = 0;
        foreach (float f in generateTimes)
        {
            sum += f;
        }
        Debug.Log("Generation takes on average " + sum / generateTimes.Count + " ms for " + iterations + " iterations, with a generator size of " + size);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Evaluations), true)]
public class EvalEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Evaluations e = (Evaluations)target;
        if (GUILayout.Button("Start Evaluation"))
        {
            e.StartEvaluation();
        }
        if (GUILayout.Button("Stop"))
        {
            if (e.coroutine != null)
            {
                e.StopCoroutine(e.coroutine);
            }
        }
        DrawDefaultInspector();
    }
}
#endif
