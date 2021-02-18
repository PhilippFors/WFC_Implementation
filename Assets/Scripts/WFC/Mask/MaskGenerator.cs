using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeBroglie;
using DeBroglie.Topo;
namespace MyWFC
{
    public class MaskGenerator : OverlapWFC
    {
        public List<MaskSample> maskInputSamples;
        public int minAreaSize = 5;
        public int passageRadius = 1;
        public int minNeighbours = 3;
        public int smoothIterations = 1;
        ITopoArray<int> maskInput;
        public List<MaskArea> allAreas;
        [HideInInspector] public List<Point> areaEdges;
        [HideInInspector] public List<Point> passages;
        [HideInInspector] public List<Point> passageEdges;
        public bool[,,] maskOutput;
        bool[,,] checker;
        public GameObject full;
        public GameObject empty;

        private void Start()
        {
            full = (GameObject)Resources.Load("Full");
            empty = (GameObject)Resources.Load("Empty");
        }
        public override Coroutine Generate(bool multithread = false)
        {
            // inputSampler.Train();
            if (multithread)
                return StartCoroutine(StartGenerate());
            else
                StartCoroutine(StartGenerate());

            return null;
        }

        protected override void PrepareModel()
        {
            MaskSample r = maskInputSamples[Random.Range(0, maskInputSamples.Count)];
            maskInput = TopoArray.Create<int>(r.sample, periodicIN);

            model = new DeBroglie.Models.OverlappingModel(n);
            model.AddSample(maskInput.ToTiles<int>());

            topology = new GridTopology(size.x, size.z, periodicOUT);
        }
        protected override void PreparePropagator()
        {
            TilePropagatorOptions options = new TilePropagatorOptions();
            options.BackTrackDepth = backTrackDepth;
            options.PickHeuristicType = PickHeuristicType.MinEntropy;

            propagator = new TilePropagator(model, topology, options);

            AddConstraints();
        }

        protected void AddConstraints()
        {
            CustomConstraint[] constraints = GetComponents<CustomConstraint>();
            if (constraints != null)
                foreach (CustomConstraint c in constraints)
                {
                    if (c.useConstraint)
                        c.SetConstraint(propagator, inputSampler.runtimeTiles);
                }
        }
        public override void DrawOutput()
        {
            allAreas = new List<MaskArea>();
            areaEdges = new List<Point>();
            passages = new List<Point>();
            passageEdges = new List<Point>();
            checker = new bool[size.x, size.y, size.z];
            maskOutput = new bool[size.x, size.y, size.z];
            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                    for (int z = 0; z < size.z; z++)
                        maskOutput[x, y, z] = false;

            while (!AllTilesChecked())
            {
                for (int x = 0; x < size.x; x++)
                    for (int y = 0; x < size.y; y++)
                        for (int z = 0; z < size.z; z++)
                        {
                            if (modelOutput[x, y, z] == 1 && !PointExists(x, y, z))
                            {
                                List<Point> points = new List<Point>();
                                FloodFill(points, x, y, z);
                                MaskArea area = new MaskArea(points, allAreas.Count, modelOutput);
                                allAreas.Add(area);
                                checker[x, y, z] = true;
                            }
                            else
                            {
                                checker[x, y, z] = true;
                            }
                        }
            }
            UpdateModelOutput();

            Smooth();

            ProcessAreas();

            UpdateEdges();
            UpdateModelOutput();
            Draw();
        }

        void Draw()
        {
            for (int x = 0; x < modelOutput.GetLength(0); x++)
                for (int y = 0; y < modelOutput.GetLength(1); y++)
                    for (int z = 0; z < modelOutput.GetLength(2); z++)
                    {
                        var tile = modelOutput[x, y, z];

                        Vector3 pos = new Vector3(x * gridSize, y * gridSize, z * gridSize);
                        GameObject fab = null;
                        if (modelOutput[x, y, z] == 0)
                            fab = empty;
                        else
                            fab = full;

                        if (fab != null)
                        {
                            MyTile newTile = Instantiate(fab, new Vector3(), Quaternion.identity).GetComponent<MyTile>();
                            newTile.coords = new Vector2Int(x, z);
                            Vector3 fscale = newTile.transform.localScale;
                            newTile.transform.parent = output.transform;
                            newTile.transform.localPosition = pos;
                            newTile.transform.localEulerAngles = new Vector3(0, 0, 0);
                            newTile.transform.localScale = fscale;
                        }
                    }
        }

        public void Smooth()
        {
            for (int s = 0; s < smoothIterations; s++)
                for (int x = 0; x < size.x; x++)
                    for (int y = 0; y < size.y; y++)
                        for (int z = 0; z < size.z; y++)
                        {
                            int neighbours = GetNeighbours(x, y, z);
                            if (neighbours >= minNeighbours)
                                IsNearArea(x, y, z);
                        }
        }

        public void IsNearArea(int x, int y, int z)
        {
            foreach (MaskArea a in allAreas)
            {
                foreach (Point p in a.pointlist)
                {
                    for (int neighbourX = p.x - 1; neighbourX <= p.x + 1; neighbourX++)
                    {
                        for (int neighbourY = p.y - 1; neighbourY <= p.y + 1; neighbourY++)
                            for (int neighbourZ = p.z - 1; neighbourZ <= p.z + 1; neighbourZ++)
                            {
                                if (IsInMap(neighbourX, neighbourY, neighbourZ))
                                    if (neighbourX == x && y == neighbourY && neighbourZ != z)
                                    {
                                        if (!PointExists(neighbourX, neighbourX, neighbourZ))
                                        {
                                            a.AddPoint(new Point(neighbourX, neighbourY, neighbourZ));
                                            return;
                                        }
                                    }
                            }
                    }
                }
            }

            foreach (Point p in passages)
            {
                for (int neighbourX = p.x - 1; neighbourX <= p.x + 1; neighbourX++)
                {
                    for (int neighbourY = p.y - 1; neighbourY <= p.y + 1; neighbourY++)
                        for (int neighbourZ = p.z - 1; neighbourZ <= p.z + 1; neighbourZ++)
                        {
                            if (IsInMap(neighbourX, neighbourY, neighbourZ))
                                if (neighbourX == x || y == neighbourY || neighbourZ == z)
                                {
                                    if (!PointExists(neighbourX, neighbourX, neighbourZ))
                                    {
                                        passages.Add(new Point(neighbourX, neighbourY, neighbourZ));
                                        return;
                                    }
                                }
                        }
                }
            }
        }

        int GetNeighbours(int x, int y, int z)
        {
            int count = 0;
            for (int neighbourX = x - 1; neighbourX <= x + 1; neighbourX++)
            {
                for (int neighbourY = y - 1; neighbourY <= y + 1; neighbourY++)
                    for (int neighbourZ = z - 1; neighbourZ <= z + 1; neighbourZ++)
                    {
                        if (IsInMap(neighbourX, neighbourY, neighbourZ))
                            if (neighbourX != x || neighbourY != y || neighbourZ != x)
                                if (modelOutput[neighbourX, neighbourY, neighbourZ] == 1)
                                    count++;
                    }
            }
            return count;
        }
        public void UpdateEdges()
        {
            List<Point> pointList = new List<Point>();
            foreach (MaskArea a in allAreas)
            {
                a.FindEdges();
                foreach (Point p in a.edgePoints)
                {
                    areaEdges.Add(p);
                }
            }

            if (passages.Count > 0)
                foreach (Point p in passages)
                    for (int x = p.x - 1; x <= p.x + 1; x++)
                        for (int y = p.y - 1; y <= p.y + 1; y++)
                            for (int z = p.z - 1; z <= p.z + 1; z++)
                            {

                                var tempX = x;
                                var tempY = y;
                                var tempZ = z;

                                if (x < 0) tempX = 0;
                                if (x >= size.x) tempX = size.x - 1;

                                if (y < 0) tempY = 0;
                                if (y >= size.y) tempY = size.y - 1;

                                if (z < 0) tempZ = 0;
                                if (z >= size.z) tempZ = size.z - 1;

                                if (x == 0 || x == size.x - 1 || y == 0 || y == size.y - 1 || z == 0 || z == size.z - 1)
                                {
                                    if (modelOutput[tempX, tempY, tempZ] == 1)
                                        passageEdges.Add(p);
                                }

                                if (modelOutput[tempX, tempY, tempZ] == 0)
                                    passageEdges.Add(p);
                            }
        }

        void UpdateModelOutput()
        {
            foreach (MaskArea a in allAreas)
                foreach (Point p in a.pointlist)
                {
                    modelOutput[p.x, p.y, p.z] = 1;
                    maskOutput[p.x, p.y, p.z] = true;
                }

            if (passages.Count > 0)
                foreach (Point p in passages)
                {
                    modelOutput[p.x, p.y, p.z] = 1;
                    maskOutput[p.x, p.y, p.z] = true;
                }
        }

        void ProcessAreas()
        {
            List<MaskArea> rAreas = new List<MaskArea>();
            foreach (MaskArea area in allAreas)
            {
                if (area.pointlist.Count < minAreaSize)
                {
                    foreach (Point p in area.pointlist)
                    {
                        modelOutput[p.x, p.y, p.z] = 0;
                    }
                    rAreas.Add(area);
                }
            }

            for (int i = 0; i < rAreas.Count; i++)
            {
                allAreas.Remove(rAreas[i]);
            }

            allAreas.Sort();
            allAreas[0].isMainRoom = true;
            allAreas[0].isAccessibleFromMain = true;

            ConnectAreas();
        }

        void ConnectAreas(bool forceMainConnections = false)
        {
            List<MaskArea> areaListA = new List<MaskArea>();
            List<MaskArea> areaListB = new List<MaskArea>();
            if (forceMainConnections)
            {
                foreach (MaskArea area in allAreas)
                {
                    if (area.isAccessibleFromMain)
                        areaListB.Add(area);
                    else
                        areaListA.Add(area);
                }
            }
            else
            {
                areaListA = allAreas;
                areaListB = allAreas;
            }

            if (allAreas.Count <= 1)
                return;

            int bestDist = 0;
            Point bestPointA = new Point();
            Point bestPointB = new Point();
            MaskArea bestAreaA = new MaskArea();
            MaskArea bestAreaB = new MaskArea();
            bool possibleConnection = false;

            foreach (MaskArea a in areaListA)
            {
                if (!forceMainConnections)
                {
                    possibleConnection = false;
                    if (a.connectedAreas.Count > 0)
                    {
                        continue;
                    }
                }

                foreach (MaskArea b in areaListB)
                {
                    if (a == b || a.IsConnected(b))
                        continue;

                    for (int pIndexA = 0; pIndexA < a.edgePoints.Count; pIndexA++)
                    {
                        for (int pIndexB = 0; pIndexB < b.edgePoints.Count; pIndexB++)
                        {
                            Point pointA = a.edgePoints[pIndexA];
                            Point pointB = b.edgePoints[pIndexB];
                            int distanceBetweenRooms = (int)(Mathf.Pow(pointA.x - pointB.x, 2) + Mathf.Pow(pointA.y - pointB.y, 2) + Mathf.Pow(pointA.z - pointB.z, 2));

                            if (distanceBetweenRooms < bestDist || !possibleConnection)
                            {
                                bestDist = distanceBetweenRooms;
                                possibleConnection = true;

                                bestPointA = pointA;
                                bestPointB = pointB;
                                bestAreaA = a;
                                bestAreaB = b;
                            }
                        }
                    }
                }
                if (possibleConnection && !forceMainConnections)
                    CreateConnection(bestAreaA, bestAreaB, bestPointA, bestPointB);
            }

            if (possibleConnection && forceMainConnections)
            {
                CreateConnection(bestAreaA, bestAreaB, bestPointA, bestPointB);
                ConnectAreas(true);
            }
            if (!forceMainConnections)
            {
                ConnectAreas(true);
            }
        }

        void CreateConnection(MaskArea areaA, MaskArea areaB, Point pointA, Point pointB)
        {
            MaskArea.ConnectAreas(areaA, areaB);
            List<Point> line = GetLine(pointA, pointB);
            foreach (Point p in line)
                DrawCircle(areaA, p, passageRadius);

            Debug.DrawLine(transform.localPosition + new Vector3(pointA.x, pointA.y, pointA.z), transform.localPosition + new Vector3(pointB.x, pointB.y, pointB.z), Color.red, 10f);
        }

        void DrawCircle(MaskArea area, Point p, int r)
        {
            for (int x = -r; x <= r; x++)
                for (int y = -r; y <= r; y++)
                    for (int z = -r; z <= r; z++)
                        if (x * x + y * y + z * z <= r * r)
                        {
                            int realX = p.x + x;
                            int realY = p.y + y;
                            int realZ = p.z + z;
                            if (IsInMap(realX, realY, realZ))
                            {
                                if (!PointExists(realX, realY, realZ))
                                {
                                    passages.Add(new Point(realX, realY, realZ));
                                }
                            }
                        }
        }

        public bool IsInMap(int x, int y, int z)
        {
            return !(x < 0) && x < size.x && !(y < 0) && y < size.y && !(z < 0) && z < size.z;
        }
        List<Point> GetLine(Point from, Point to)
        {
            List<Point> list = new List<Point>();
            var x1 = from.x;
            var y1 = from.y;
            var z1 = from.z;

            var x2 = to.x;
            var y2 = to.y;
            var z2 = to.z;

            var dx = to.x - from.x;
            var dy = to.y - from.y;
            var dz = to.z - from.z;

            var xs = 0;
            var ys = 0;
            var zs = 0;

            if (from.x > to.x)
                xs = 1;
            else
                xs = -1;
            if (from.y > to.y)
                ys = 1;
            else
                ys = -1;
            if (from.z > to.z)
                zs = 1;
            else
                zs = -1;

            var p1 = 0;
            var p2 = 0;

            //Driving axis is X-Axis
            if (dx >= dy & dx >= dz)
            {
                p1 = 2 * dy - dx;
                p2 = 2 * dz - dx;
                while (x1 != x2)
                {
                    x1 += xs;
                    if (p1 >= 0)
                    {
                        y1 += ys;
                        p1 -= 2 * dx;
                    }
                    if (p2 >= 0)
                    {
                        z1 += zs;
                        p2 -= 2 * dx;
                    }
                    p1 += 2 * dy;
                    p2 += 2 * dz;
                    list.Add(new Point(x1, y1, z1));
                }
            }
            //Driving axis is Y-axis
            else if (dy >= dx & dy >= dz)
            {
                p1 = 2 * dx - dy;
                p2 = 2 * dz - dy;
                while (y1 != y2)
                {
                    y1 += ys;
                    if (p1 >= 0)
                    {
                        x1 += xs;
                        p1 -= 2 * dy;
                    }
                    if (p2 >= 0)
                    {
                        z1 += zs;
                        p2 -= 2 * dy;
                    }
                    p1 += 2 * dx;
                    p2 += 2 * dz;
                    list.Add(new Point(x1, y1, z1));
                }
            }
            //Driving axis is Z-axis
            else
            {
                p1 = 2 * dy - dz;
                p2 = 2 * dx - dz;
                while (z1 != z2)
                {
                    z1 += zs;
                    if (p1 >= 0)
                    {
                        y1 += ys;
                        p1 -= 2 * dz;
                    }
                    if (p2 >= 0)
                    {
                        x1 += xs;
                        p2 -= 2 * dz;
                    }
                    p1 += 2 * dy;
                    p2 += 2 * dx;
                    list.Add(new Point(x1, y1, z1));
                }
            }

            // int step = System.Math.Sign(dx);
            // int gradientStep = System.Math.Sign(dy);

            // int longest = Mathf.Abs(dx);
            // int middle = Mathf.Abs(dy);
            // int shortest = Mathf.Abs(dz);

            // if (longest < middle)
            // {
            //     var temp = longest;
            //     longest = middle;
            //     middle = temp;
            // }
            // if (longest < shortest)
            // {
            //     var temp = longest;
            //     longest = shortest;
            //     shortest = temp;
            // }

            // step = longest;
            // gradientStep = shortest;

            // if (longest < shortest)
            // {
            //     inverted = true;
            //     longest = Mathf.Abs(dy);
            //     shortest = Mathf.Abs(dx);
            //     step = System.Math.Sign(dy);
            //     gradientStep = System.Math.Sign(dx);
            // }

            // int gradientAccumulation = longest / 2;

            // for (int i = 0; i < longest; i++)
            // {
            //     list.Add(new Point(x, y));

            //     if (inverted)
            //         y += step;
            //     else
            //         x += step;

            //     gradientAccumulation += shortest;
            //     if (gradientAccumulation >= longest)
            //     {
            //         if (inverted)
            //             x += gradientStep;
            //         else
            //             y += gradientStep;
            //         gradientAccumulation -= longest;
            //     }
            // }

            return list;
        }

        bool AllTilesChecked()
        {
            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                    for (int z = 0; z < size.z; z++)
                        if (checker[x, y, z])
                            continue;
                        else
                            return checker[x, y, z];

            return true;
        }

        bool PointExists(int x, int y, int z)
        {
            if (allAreas.Count > 0)
                foreach (MaskArea area in allAreas)
                {
                    if (area.PointExists(x, y, z))
                    {
                        return true;
                    }
                }

            if (passages.Count > 0)
                return passages.Exists(p => p.x == x && p.y == y && p.z == z);

            return false;
        }

        void FloodFillUtil(List<Point> pointList, int x, int y, int z)
        {
            if (x < 0 || x >= size.x || y < 0 || y >= size.y || z < 0 || z >= size.z)
                return;
            if (pointList.Exists(p => (p.x == x) && (p.y == y) && (p.z == z)) || PointExists(x, y, z) || modelOutput[x, y, z] == 0)
                return;

            pointList.Add(new Point(x, y));

            FloodFillUtil(pointList, x + 1, y, z);
            FloodFillUtil(pointList, x - 1, y, z);
            FloodFillUtil(pointList, x, y + 1, z);
            FloodFillUtil(pointList, x, y - 1, z);
            FloodFillUtil(pointList, x, y, z + 1);
            FloodFillUtil(pointList, x, y, z - 1);
        }

        void FloodFill(List<Point> l, int x, int y, int z)
        {
            FloodFillUtil(l, x, y, z);
        }
    }
    [System.Serializable]
    public class Point
    {
        public Point() { }
        public Point(int x, int y, int z = 0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public int x;
        public int y;
        public int z;
    }

    [System.Serializable]
    public class MaskArea : System.IComparable<MaskArea>
    {
        public int size;
        int[,,] map;
        public int AreaID;
        public List<Point> pointlist;
        public List<Point> edgePoints;
        public List<MaskArea> connectedAreas;
        public bool isAccessibleFromMain;
        public bool isMainRoom;
        public MaskArea()
        {

        }
        public MaskArea(List<Point> l, int areaID, int[,,] map)
        {
            AreaID = areaID;
            pointlist = l;
            edgePoints = new List<Point>();
            connectedAreas = new List<MaskArea>();

            this.map = map;
            size = pointlist.Count;
            FindEdges();
        }

        public void SetAccessibleFromMain()
        {
            if (!isAccessibleFromMain)
            {
                isAccessibleFromMain = true;
                foreach (MaskArea connectedArea in connectedAreas)
                {
                    connectedArea.isAccessibleFromMain = true;
                }
            }
        }

        public void FindEdges()
        {
            edgePoints = new List<Point>();
            foreach (Point p in pointlist)
            {
                for (int x = p.x - 1; x <= p.x + 1; x++)
                    for (int y = p.y - 1; y <= p.y + 1; y++)
                        for (int z = p.z - 1; z <= p.z + 1; z++)
                        {
                            // if (x != p.x || y != p.y)
                            // {
                            var tempX = x;
                            var tempY = y;
                            var tempZ = z;

                            if (x < 0) tempX = 0;
                            if (x >= map.GetLength(0)) tempX = map.GetLength(0) - 1;

                            if (y < 0) tempY = 0;
                            if (y >= map.GetLength(1)) tempY = map.GetLength(1) - 1;

                            if (z < 0) tempZ = 0;
                            if (z >= map.GetLength(2)) tempZ = map.GetLength(2) - 1;

                            // if (x == 0 || x == map.GetLength(0) - 1 || y == 0 || y == map.GetLength(1) - 1)
                            // {
                            //     if (map[tempX, tempY] == 1)
                            //         if (!edgePoints.Contains(p))
                            //             edgePoints.Add(p);
                            // }
                            // else
                            // {
                            if (map[tempX, tempY, tempZ] == 0)
                                if (!edgePoints.Contains(p))
                                    edgePoints.Add(p);
                            // }
                        }
            }
        }

        public static void ConnectAreas(MaskArea areaA, MaskArea areaB)
        {
            if (areaA.isAccessibleFromMain)
            {
                areaB.SetAccessibleFromMain();
            }
            else if (areaB.isAccessibleFromMain)
            {
                areaA.SetAccessibleFromMain();
            }
            areaA.connectedAreas.Add(areaB);
            areaB.connectedAreas.Add(areaA);
        }

        public bool IsConnected(MaskArea area)
        {
            return connectedAreas.Contains(area);
        }

        public void AddPoint(Point point) => pointlist.Add(point);
        public bool PointExists(int x, int y, int z)
        {
            foreach (Point p in pointlist)
            {
                if (p.x == x && p.y == y && p.z == z)
                {
                    return true;
                }
            }
            return false;
        }

        public int CompareTo(MaskArea other)
        {
            return other.size.CompareTo(size);
        }
    }
}