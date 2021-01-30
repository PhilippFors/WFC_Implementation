using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace MyWFC
{
    public class MyTile : MonoBehaviour
    {
        public int ID;
        public int weight;
        public int rotationDeg;
        public bool hasRotation;

        public Vector2Int coords;
        public Vector3Int size = new Vector3Int(1, 1, 1);

        [Tooltip("Can be left alone if you use a sample input.")]
        public List<TileSide> sides = new List<TileSide>();

        [SerializeField] bool showGizmo;

        private void OnDrawGizmos()
        {
            if (ID == 0)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(transform.position, new Vector3(size.x - 1f, size.y - 1f, size.z - 1f));
            }
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            if (sides.Count > 0 && showGizmo)
                foreach (TileSide s in sides)
                {
                    switch (s.side)
                    {
                        case Sides.Left:
                            for (int i = 0; i < s.subSides.Count; i++)
                            {
                                ConnectionColor(s.subSides[i].connection);
                                Gizmos.DrawCube(new Vector3(-size.x / 2, 0, (-1 + i) * size.x / 3), Vector3.one);
                            }
                            // ConnectionColor(s.connection);
                            // Gizmos.DrawCube(new Vector3(-size.x / 2, 0, 0), Vector3.one / 4f);

                            break;
                        case Sides.Right:
                            for (int i = 0; i < s.subSides.Count; i++)
                            {
                                ConnectionColor(s.subSides[i].connection);
                                Gizmos.DrawCube(new Vector3(size.x / 2, 0, (-1 + i) * size.x / 3), Vector3.one);
                            }
                            // ConnectionColor(s.connection);
                            // Gizmos.DrawCube(new Vector3(size.x / 2, 0, 0), Vector3.one / 4f);
                            break;
                        case Sides.Back:
                            for (int i = 0; i < s.subSides.Count; i++)
                            {
                                ConnectionColor(s.subSides[i].connection);
                                Gizmos.DrawCube(new Vector3((-1 + i) * size.z / 3, 0, -size.z / 2), Vector3.one);
                            }
                            // ConnectionColor(s.connection);
                            // Gizmos.DrawCube(new Vector3(0, 0, -size.z / 2), Vector3.one / 4f);
                            break;
                        case Sides.Front:
                            for (int i = 0; i < s.subSides.Count; i++)
                            {
                                ConnectionColor(s.subSides[i].connection);
                                Gizmos.DrawCube(new Vector3((-1 + i) * size.z / 3, 0, size.z / 2), Vector3.one);
                            }
                            // ConnectionColor(s.connection);
                            // Gizmos.DrawCube(new Vector3(0, 0, size.z / 2), Vector3.one / 4f);
                            break;
                    }
                }
        }
        void ConnectionColor(Connections c)
        {
            switch (c)
            {
                case Connections.A:
                    Gizmos.color = Color.red;
                    break;
                case Connections.B:
                    Gizmos.color = new Color(0.5f, 1f, 0.5f, 1);
                    break;
                case Connections.C:
                    Gizmos.color = Color.blue;
                    break;
                case Connections.D:
                    Gizmos.color = Color.black;
                    break;
                case Connections.E:
                    Gizmos.color = Color.cyan;
                    break;
                case Connections.F:
                    Gizmos.color = Color.green;
                    break;
                case Connections.G:
                    Gizmos.color = Color.gray;
                    break;
                case Connections.H:
                    Gizmos.color = Color.magenta;
                    break;
                case Connections.I:
                    Gizmos.color = Color.yellow;
                    break;
                case Connections.J:
                    Gizmos.color = Color.white;
                    break;
            }
        }
    }
    [System.Serializable]
    public class ConnectionGroup
    {
        public Connections connectionA;
        public Connections connectionB;
        public bool canConnectToSelf = true;
    }

    [System.Serializable]
    public class TileSide
    {
        public Sides side;
        public Connections connection;
        public List<SubSide> subSides = new List<SubSide>();
    }
    [System.Serializable]
    public class SubSide
    {
        public Sides side;
        public Connections connection;
    }


    public enum Sides
    {
        Left = 180, Right = 0, Front = 270, Back = 90, Middle = 4, Upper = 5, Lower = -5
    }
    public enum Connections
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J
    }

    [CustomEditor(typeof(MyTile))]
    [CanEditMultipleObjects]
    public class TileEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MyTile t = (MyTile)target;
            DrawDefaultInspector();
            if (t.sides.Count == 0)
            {
                t.sides.Add(new TileSide() { side = Sides.Left, connection = Connections.A });
                t.sides.Add(new TileSide() { side = Sides.Right, connection = Connections.A });
                t.sides.Add(new TileSide() { side = Sides.Front, connection = Connections.A });
                t.sides.Add(new TileSide() { side = Sides.Back, connection = Connections.A });

                foreach (TileSide s in t.sides)
                {
                    s.subSides.Add(new SubSide() { side = Sides.Lower, connection = Connections.A });
                    s.subSides.Add(new SubSide() { side = Sides.Middle, connection = Connections.A });
                    s.subSides.Add(new SubSide() { side = Sides.Upper, connection = Connections.A });
                }
            }

            if (GUI.changed)
                EditorUtility.SetDirty(t);
        }
    }
}
