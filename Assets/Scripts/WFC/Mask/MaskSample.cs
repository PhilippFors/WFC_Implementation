using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MyWFC
{
    [CreateAssetMenu(menuName = "MaskSample", fileName = "New MaskSample")]
    public class MaskSample : ScriptableObject
    {
        [HideInInspector] public int sizex = 4;
        [HideInInspector] public int sizey = 4;
        [HideInInspector] public IntArrayHelper[] arr;
        public int[,] sample
        {
            get
            {
                int[,] n = new int[arr.Length, arr[0].v.Length];
                for (int i = 0; i < arr.Length; i++)
                    for (int j = 0; j < arr[0].v.Length; j++)
                        n[i, j] = arr[i].v[j];

                return n;
            }
        }
    }


    [CustomEditor(typeof(MaskSample))]
    public class SampleEditor : Editor
    {
        int tempX;
        int tempY;
        InputSampler maskInput;
        public override void OnInspectorGUI()
        {
            MaskSample t = (MaskSample)target;

            if (t.arr == null)
            {
                t.arr = new IntArrayHelper[t.sizex];
                for (int i = 0; i < t.sizex; i++)
                {
                    t.arr[i] = new IntArrayHelper(t.sizey);
                }
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Size X");
            t.sizex = EditorGUILayout.IntField(t.sizex);
            GUILayout.Label("Size Y");
            t.sizey = EditorGUILayout.IntField(t.sizey);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Set Size"))
            {
                IntArrayHelper[] temp = t.arr;
                t.arr = new IntArrayHelper[t.sizex];
                for (int i = 0; i < t.sizex; i++)
                {
                    t.arr[i] = new IntArrayHelper(t.sizey);
                }

                for (int i = 0; i < (temp.Length < t.sizex ? temp.Length : t.sizex); i++)
                    for (int j = 0; j < (temp[i].v.Length < t.sizey ? temp[i].v.Length : t.sizey); j++)
                        t.arr[i].v[j] = temp[i].v[j];
            }

            if (t.arr != null && t.arr.Length != 0)
                for (int i = t.arr[0].v.Length - 1; i >= 0; i--)
                {
                    GUILayout.BeginHorizontal();
                    for (int j = 0; j < t.arr.Length; j++)
                    {
                        GUILayout.Label(j + ", " + i, GUILayout.MaxWidth(30f));
                        t.arr[j].v[i] = EditorGUILayout.IntField(t.arr[j].v[i], GUILayout.MaxWidth(25f));
                    }
                    GUILayout.EndHorizontal();
                }

            maskInput = (InputSampler)EditorGUILayout.ObjectField(maskInput, typeof(InputSampler), true);
            if (maskInput != null)
                if (GUILayout.Button("Translate Input"))
                {
                    maskInput.Train();
                    t.arr = new IntArrayHelper[maskInput.sample.GetLength(0)];
                    for (int i = 0; i < maskInput.sample.GetLength(0); i++)
                    {
                        t.arr[i] = new IntArrayHelper(maskInput.sample.GetLength(1));
                    }

                    for (int i = 0; i < maskInput.sample.GetLength(0); i++)
                        for (int j = 0; j < maskInput.sample.GetLength(1); j++)
                            t.arr[i].v[j] = (int)maskInput.sample[i, j].Value == 0 ? 1 : 0;
                }

            if (GUI.changed)
                EditorUtility.SetDirty(t);

            if (GUILayout.Button("Save"))
                AssetDatabase.SaveAssets();

        }
    }
}