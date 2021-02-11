using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace MyWFC
{
    public class MyTile : MonoBehaviour
    {
        /// <summary>
        /// Unique ID for the tile
        /// </summary>
        public int ID;
        public bool use = true;

        /// <summary>
        /// The frequenzy of the tile placement
        /// </summary>
        public int weight;

        public int rotationDeg;
        public bool hasRotation;

        public Vector2Int coords;
        public Vector3Int size = new Vector3Int(1, 1, 1);

        [Tooltip("Can be left alone if you use a sample input.")]
        [SerializeField] TileSide side1 = new TileSide();
        [SerializeField] TileSide side2 = new TileSide();
        [SerializeField] TileSide side3 = new TileSide();
        [SerializeField] TileSide side4 = new TileSide();

        public TileSide Side1
        {
            set
            {
                side1 = value;
            }
        }
        public TileSide Side2
        {
            set
            {
                side2 = value;
            }
        }
        public TileSide Side3
        {
            set
            {
                side3 = value;
            }
        }
        public TileSide Side4
        {
            set
            {
                side4 = value;
            }
        }

        [HideInInspector]
        public List<TileSide> sides
        {
            get
            {
                List<TileSide> arr = new List<TileSide>();
                arr.Add(side1);
                arr.Add(side2);
                arr.Add(side3);
                arr.Add(side4);
                return arr;
            }
        }

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
    public class TileSide
    {
        public Sides side;
        public Connections connection;

        public List<SubSide> subSides = new List<SubSide>(){new SubSide(){side = UnderSides.Lower, connection = Connections.A},
                                                            new SubSide(){side = UnderSides.Middle, connection = Connections.A},
                                                            new SubSide(){side = UnderSides.Upper, connection = Connections.A}};
    }
    [System.Serializable]
    public class SubSide
    {
        public UnderSides side;
        public Connections connection;
    }


    public enum Sides
    {
        Left = 180, Right = 0, Front = 270, Back = 90
    }
    public enum UnderSides
    {
        Upper, Lower, Middle
    }
    public enum Connections
    {
        A = 0,
        B = 1,
        C = 2,
        D = 3,
        E = 4,
        F = 5,
        G = 6,
        H = 7,
        I = 8,
        J = 9
    }

#if UNITY_EDITOR
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
                t.Side1 = new TileSide() { side = Sides.Left, connection = Connections.A };
                t.Side2 = new TileSide() { side = Sides.Right, connection = Connections.A };
                t.Side3 = new TileSide() { side = Sides.Front, connection = Connections.A };
                t.Side4 = new TileSide() { side = Sides.Back, connection = Connections.A };
                init = true;
            }
            if (GUI.changed)
                EditorUtility.SetDirty(t);
        }
    }
#endif
}
