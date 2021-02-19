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

        //used for big tiles
        [HideInInspector] public int bID;
        [HideInInspector] public List<int> runtimeIDs = new List<int>();

        public bool use = true;

        /// <summary>
        /// The frequenzy of the tile placement
        /// </summary>
        public int weight;

        public int rotationDeg;
        public bool hasRotation;

        public Vector2Int coords;
        public Vector3Int size = new Vector3Int(1, 1, 1);
        public Vector3 center = Vector3.zero;
        public List<Cell> cells = new List<Cell>() { new Cell() };

        public bool showGizmo;
        [SerializeField] float gizmoSize = 0.5f;
        private void OnDrawGizmos()
        {
            if (!GetComponent<MeshRenderer>() && GetComponentsInChildren<MeshRenderer>() == null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(transform.position, new Vector3(size.x - 0.2f, size.y - 0.2f, size.z - 0.2f));
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            if (cells.Count > 0)
                foreach (Cell c in cells)
                    if (c.sides.Count > 0 && showGizmo)
                        foreach (TileSide s in c.sides)
                        {
                            switch (s.side)
                            {
                                case Sides.Left:
                                    for (int i = 0; i < s.sideInfo.GetLength(0); i++)
                                        for (int j = s.sideInfo.GetLength(1) - 1; j >= 0; j--)
                                        {
                                            ConnectionColor(s.sideInfo[j, i]);

                                            Gizmos.DrawCube(new Vector3((float)(-size.x) / 2 + 0.05f, i * (float)size.y / 3, (1 - j) * (float)size.z / 3) + c.center, new Vector3(gizmoSize, gizmoSize, gizmoSize));
                                        }
                                    // ConnectionColor(s.connection);
                                    // Gizmos.DrawCube(new Vector3(-size.x / 2, 0, 0), Vector3.one / 4f);

                                    break;
                                case Sides.Right:
                                    for (int i = 0; i < s.sideInfo.GetLength(0); i++)
                                        for (int j = 0; j < s.sideInfo.GetLength(1); j++)
                                        {
                                            ConnectionColor(s.sideInfo[j, i]);
                                            Gizmos.DrawCube(new Vector3((float)size.x / 2 - 0.05f, i * (float)size.y / 3, (-1 + j) * (float)size.z / 3) + c.center, new Vector3(gizmoSize, gizmoSize, gizmoSize));
                                        }
                                    // ConnectionColor(s.connection);
                                    // Gizmos.DrawCube(new Vector3(size.x / 2, 0, 0), Vector3.one / 4f);
                                    break;
                                case Sides.Back:
                                    for (int i = 0; i < s.sideInfo.GetLength(0); i++)
                                        for (int j = 0; j < s.sideInfo.GetLength(1); j++)
                                        {
                                            ConnectionColor(s.sideInfo[j, i]);
                                            Gizmos.DrawCube(new Vector3((-1 + j) * (float)size.x / 3, i * (float)size.y / 3, -(float)size.z / 2 + 0.05f) + c.center, new Vector3(gizmoSize, gizmoSize, gizmoSize));
                                        }
                                    // ConnectionColor(s.connection);
                                    // Gizmos.DrawCube(new Vector3(0, 0, -size.z / 2), Vector3.one / 4f);
                                    break;
                                case Sides.Front:
                                    for (int i = 0; i < s.sideInfo.GetLength(0); i++)
                                        for (int j = s.sideInfo.GetLength(1) - 1; j >= 0; j--)
                                        {
                                            ConnectionColor(s.sideInfo[j, i]);
                                            Gizmos.DrawCube(new Vector3((1 - j) * (float)size.x / 3, i * (float)size.y / 3, (float)size.z / 2 - 0.05f) + c.center, new Vector3(gizmoSize, gizmoSize, gizmoSize));
                                        }
                                    // ConnectionColor(s.connection);
                                    // Gizmos.DrawCube(new Vector3(0, 0, size.z / 2), Vector3.one / 4f);
                                    break;
                                case Sides.Top:
                                    for (int i = 0; i < s.sideInfo.GetLength(0); i++)
                                        for (int j = 0; j < s.sideInfo.GetLength(1); j++)
                                        {
                                            ConnectionColor(s.sideInfo[j, i]);
                                            Gizmos.DrawCube(new Vector3((-1 + j) * (float)size.x / 3, (float)(size.y) - 0.5f, (-1 + i) * (float)size.z / 3) + c.center, new Vector3(gizmoSize, gizmoSize, gizmoSize));
                                        }
                                    break;
                                case Sides.Bottom:
                                    for (int i = 0; i < s.sideInfo.GetLength(0); i++)
                                        for (int j = s.sideInfo.GetLength(1) - 1; j >= 0; j--)
                                        {
                                            ConnectionColor(s.sideInfo[j, i]);
                                            Gizmos.DrawCube(new Vector3((1 - j) * (float)size.x / 3, -0.25f, (-1 + i) * (float)size.z / 3) + c.center, new Vector3(gizmoSize, gizmoSize, gizmoSize));
                                        }
                                    break;
                            }
                        }
        }


        void ConnectionColor(Connections c)
        {
            switch (c)
            {
                case Connections.None:
                    Gizmos.color = new Color(1, 1, 1, 0.1f);
                    break;
                case Connections.BIDYES:
                    Gizmos.color = new Color(0.5f, 0.5f, 0.5f);
                    break;
                case Connections.BID:
                    Gizmos.color = new Color(0.2f, 0.2f, 0.2f);
                    break;
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
#endif
    }

    [System.Serializable]
    public class Cell
    {
        public Vector3 center = Vector3.zero;
        public TileSide right = new TileSide() { side = Sides.Right };
        public TileSide left = new TileSide() { side = Sides.Left };
        public TileSide front = new TileSide() { side = Sides.Front };
        public TileSide back = new TileSide() { side = Sides.Back };
        public TileSide top = new TileSide() { side = Sides.Top };
        public TileSide bottom = new TileSide() { side = Sides.Bottom };

        [HideInInspector]
        public List<TileSide> sides
        {
            get
            {
                List<TileSide> arr = new List<TileSide>();
                arr.Add(right);
                arr.Add(left);
                arr.Add(front);
                arr.Add(back);
                arr.Add(top);
                arr.Add(bottom);
                return arr;
            }
        }
    }
    [System.Serializable]
    public class TileSide
    {
        public Sides side;
        public Connections connection;

        public Connections left;
        public Connections middle;
        public Connections right;
        public Connections topLeft;
        public Connections topMiddle;
        public Connections topRight;
        public Connections bottomLeft;
        public Connections bottomMiddle;
        public Connections bottomRight;

        public List<Connections> all
        {
            get
            {
                List<Connections> l = new List<Connections>();
                l.Add(left);
                l.Add(middle);
                l.Add(right);
                return l;
            }
            set
            {
                left = value[0];
                middle = value[1];
                right = value[2];
            }
        }

        public Connections[,] sideInfo
        {
            get
            {
                Connections[,] arr = new Connections[3, 3];
                arr[0, 0] = bottomLeft;
                arr[1, 0] = bottomMiddle;
                arr[2, 0] = bottomRight;
                arr[0, 1] = left;
                arr[1, 1] = middle;
                arr[2, 1] = right;
                arr[0, 2] = topLeft;
                arr[1, 2] = topMiddle;
                arr[2, 2] = topRight;
                return arr;
            }
        }

        public bool Equals(Connections c)
        {
            foreach (Connections conn in sideInfo)
            {
                if (!conn.Equals(c))
                    return false;
            }
            return true;
        }
        public static int c = 0;
        public bool Equals(TileSide sideB, ConnectionGroups connectionGroups, int[] bIDS = null, int rotationYA = 0, int rotationYB = 0)
        {
            c++;

            if (this.Equals(Connections.None) || sideB.Equals(Connections.None))
                return false;

            // Connections[] arrayA = this.all.ToArray();
            // Connections[] arrayB = sideB.all.ToArray();

            Connections[,] a = RotateMatrix(this.sideInfo, rotationYA, this.side);
            Connections[,] b = RotateMatrix(sideB.sideInfo, rotationYB, sideB.side);

            for (int x = 0; x < a.GetLength(0); x++)
                for (int i = 0, j = a.GetLength(1) - 1; i < a.GetLength(0); i++, j--)
                {
                    if (!AdjacentUtil.CheckConnections(a[i, x], b[j, x], connectionGroups))
                        return false;
                }

            // if (bIDS[0] != -1 && bIDS[1] != -1)
            // {
            //     for (int i = 0, j = arrayA.Length - 1; i < arrayA.Length; i++, j--)
            //     {
            //         if (!AdjacentUtil.CheckConnections(arrayA[i], arrayB[j], connectionGroups) || bIDS[0] != bIDS[1])
            //             return false;
            //     }
            // }
            // else
            // {
            //     for (int i = 0, j = arrayA.Length - 1; i < arrayA.Length; i++, j--)
            //     {
            //         if (!AdjacentUtil.CheckConnections(arrayA[i], arrayB[j], connectionGroups))
            //             return false;
            //     }
            // }
            return true;
        }

        public Connections[,] RotateMatrix(Connections[,] connections, int rot, Sides side)
        {
            Connections[,] newArr = connections;

            if (rot != 0 && (side.Equals(Sides.Top) || side.Equals(Sides.Bottom)))
            {
                int amount = rot / 90;
                Debug.Log(rot);
                Debug.Log(amount);
                for (int i = 0; i < amount; i++)
                {
                    Transpose(ref newArr);
                    Reverse(ref newArr);
                }
            }
            return newArr;
        }

        void Transpose(ref Connections[,] connections)
        {
            for (int x = 0; x < connections.GetLength(0); x++)
                for (int z = x; z < connections.GetLength(1); z++)
                {
                    var temp = connections[x, z];
                    connections[x, z] = connections[z, x];
                    connections[z, x] = temp;
                }
        }

        void Reverse(ref Connections[,] connections)
        {
            var start = 0;
            var end = connections.GetLength(1) - 1;

            for (int i = 0; i < connections.Length; i++)
            {
                while (start < end)
                {
                    var temp = connections[i, start];
                    connections[i, start] = connections[i, end];
                    connections[i, end] = temp;
                    start++;
                    end--;
                }
            }
        }
    }

    public enum Sides
    {
        Left = 180, Right = 0, Front = 270, Back = 90, Top, Bottom
    }
    public enum Connections
    {
        None = -3,
        BIDYES = -2,
        BID = -1,
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
}
