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
                if (t.cells == null || t.cells.Count == 0)
                    t.cells.Add(new Cell());

                foreach (Cell c in t.cells)
                {
                    c.right = new TileSide() { side = Sides.Left };
                    c.left = new TileSide() { side = Sides.Right };
                    c.front = new TileSide() { side = Sides.Front };
                    c.back = new TileSide() { side = Sides.Back };
                    c.top = new TileSide() { side = Sides.Top };
                    c.bottom = new TileSide() { side = Sides.Bottom };
                }
            }

            t.cells[0].center = t.center;

            if (GUILayout.Button("Tile editor"))
            {
                TileEditorWindow w = EditorWindow.GetWindow<TileEditorWindow>();
                w.tile = t;
                w.Show();
                t.showGizmo = true;
            }
            
            if (GUI.changed)
                EditorUtility.SetDirty(t);
        }
    }
}