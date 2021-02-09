using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
namespace MyWFC
{
    [CreateAssetMenu(fileName = "new Connectiongroup", menuName = "WFC/Connection Group")]
    public class ConnectionGroups : ScriptableObject
    {
        public BoolArrayHelper[] groups;

        public void FillAr()
        {
            groups = new BoolArrayHelper[Enum.GetValues(typeof(Connections)).Length];
            var enums = Enum.GetValues(typeof(Connections));

            for (int i = 0; i < groups.Length; i++)
            {
                groups[i] = new BoolArrayHelper(groups.Length);
            }

            for (int i = 0; i < groups.Length; i++)
            {
                groups[i].v[i] = true;
            }
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(ConnectionGroups))]
    public class ConnectionGroupsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ConnectionGroups g = (ConnectionGroups)target;
            if (g.groups == null)
                g.FillAr();

            var enums = Enum.GetValues(typeof(Connections));
            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            foreach (Connections c in enums)
            {
                GUILayout.Label(c.ToString(), GUILayout.MaxWidth(20f));
            }
            GUILayout.EndHorizontal();
            for (int y = 0; y < g.groups.Length; y++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(enums.GetValue(y).ToString(), GUILayout.MaxWidth(20f));
                for (int x = 0; x < g.groups.Length; x++)
                {
                    bool temp = g.groups[x].v[y];
                    g.groups[x].v[y] = EditorGUILayout.Toggle(g.groups[x].v[y], GUILayout.MaxWidth(20f));
                    if (temp != g.groups[x].v[y])
                    {
                        g.groups[y].v[x] = !temp;
                    }
                }
                GUILayout.EndHorizontal();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(g);
                AssetDatabase.SaveAssets();
            }
        }


    }
#endif
}