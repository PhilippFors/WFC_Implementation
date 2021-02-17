using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace MyWFC
{
    [CustomEditor(typeof(MyTile))]
    [CanEditMultipleObjects]
    public class TileEditor : Editor
    {
        bool init = false;
        public override void OnInspectorGUI()
        {
            MyTile t = (MyTile)target;
            DrawDefaultInspector();
            if (GUILayout.Button("Fill sides"))
            {
                foreach (Cell c in t.cells)
                {
                    c.side1 = new TileSide() { side = Sides.Left, connection = Connections.A };
                    c.side2 = new TileSide() { side = Sides.Right, connection = Connections.A };
                    c.side3 = new TileSide() { side = Sides.Front, connection = Connections.A };
                    c.side4 = new TileSide() { side = Sides.Back, connection = Connections.A };
                }
                init = true;
            }

            t.cells[0].center = t.center;

            if (GUILayout.Button("Tile editor"))
            {
                TileEditorWindow w = EditorWindow.GetWindow<TileEditorWindow>();
                w.tile = t;
                w.Show();
                
            }
            if (GUI.changed)
                EditorUtility.SetDirty(t);
        }
    }
}