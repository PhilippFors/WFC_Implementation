using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeBroglie;
using DeBroglie.Topo;
namespace MyWFC
{
    public class MaskGenerator : OverlapWFC
    {
        public List<MyTile> borderTiles = new List<MyTile>();
        public List<MaskSample> maskSamples;
        public int minAreaSize = 5;
        public int passageRadius = 1;
        public int minNeighbours = 3;
        public int smoothIterations = 1;
        ITopoArray<int> maskInput;
        public List<MaskArea> allAreas;
        public List<Point> areaEdges;
        public List<Point> passages;
        public List<Point> passageEdges;
        public bool[,] mask;
        bool[,] checker;
        GameObject full;
        GameObject empty;

        private void Start()
        {
            full = (GameObject)Resources.Load("Full");
            empty = (GameObject)Resources.Load("EmptyTile");
        }
        public override void Generate(bool multithread = false)
        {
            // inputSampler.Train();
            if (multithread)
                StartCoroutine(StartGenerate());
            else
                StartGenerateSingle();
        }

        protected override void PrepareModel()
        {
            MaskSample r = maskSamples[Random.Range(0, maskSamples.Count)];
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
            ApplyMask();
        }
        public override void Draw()
        {
            allAreas = new List<MaskArea>();
            areaEdges = new List<Point>();
            passages = new List<Point>();
            passageEdges = new List<Point>();
            checker = new bool[size.x, size.z];
            mask = new bool[size.x, size.z];
            for (int i = 0; i < size.x; i++)
                for (int j = 0; j < size.z; j++)
                    mask[i, j] = false;

            while (!AllTilesChecked())
            {
                for (int i = 0; i < size.x; i++)
                    for (int j = 0; j < size.z; j++)
                    {
                        if (modelOutput[i, j] == 1 && !PointExists(i, j))
                        {
                            List<Point> points = new List<Point>();
                            FloodFill(points, i, j);
                            MaskArea area = new MaskArea(points, allAreas.Count, modelOutput);
                            allAreas.Add(area);
                            checker[i, j] = true;
                        }
                        else
                        {
                            checker[i, j] = true;
                        }
                    }
            }
            UpdateModelOutput();
            for (int s = 0; s < smoothIterations; s++)
                Smooth();
            ProcessAreas();

            UpdateEdges();
            UpdateModelOutput();
            // base.Draw();
        }

        public void Smooth()
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.z; y++)
                {
                    int neighbours = GetNeighbours(x, y);
                    if (neighbours >= minNeighbours)
                        IsNearArea(x, y);
                }
            }
        }

        public void IsNearArea(int x, int y)
        {
            foreach (MaskArea a in allAreas)
            {
                foreach (Point p in a.pointlist)
                {
                    for (int neighbourX = p.x - 1; neighbourX <= p.x + 1; neighbourX++)
                    {
                        for (int neighbourY = p.y - 1; neighbourY <= p.y + 1; neighbourY++)
                        {
                            if (IsInMap(neighbourX, neighbourY))
                                if (neighbourX == x && y == neighbourY)
                                {
                                    if (!PointExists(neighbourX, neighbourX))
                                    {
                                        a.AddPoint(new Point(neighbourX, neighbourY));
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
                    {
                        if (IsInMap(neighbourX, neighbourY))
                            if (neighbourX == x || y == neighbourY)
                            {
                                if (!PointExists(neighbourX, neighbourX))
                                {
                                    passages.Add(new Point(neighbourX, neighbourY));
                                    return;
                                }
                            }
                    }
                }
            }
        }

        int GetNeighbours(int x, int y)
        {
            int count = 0;
            for (int neighbourX = x - 1; neighbourX <= x + 1; neighbourX++)
            {
                for (int neighbourY = y - 1; neighbourY <= y + 1; neighbourY++)
                {
                    if (IsInMap(neighbourX, neighbourY))
                        if (neighbourX != x || neighbourY != y)
                            if (modelOutput[neighbourX, neighbourY] == 1)
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
                {
                    for (int x = p.x - 1; x <= p.x + 1; x++)
                    {
                        for (int y = p.y - 1; y <= p.y + 1; y++)
                        {

                            var tempX = x;
                            var tempY = y;

                            if (x < 0) tempX = 0;
                            if (x >= size.x) tempX = size.x - 1;

                            if (y < 0) tempY = 0;
                            if (y >= size.z) tempY = size.z - 1;

                            if (x == 0 || x == size.x - 1 || y == 0 || y == size.z - 1)
                            {
                                if (modelOutput[tempX, tempY] == 1)
                                    passageEdges.Add(p);
                            }

                            if (modelOutput[tempX, tempY] == 0)
                                passageEdges.Add(p);
                        }

                    }
                }

        }

        void UpdateModelOutput()
        {
            foreach (MaskArea a in allAreas)
                foreach (Point p in a.pointlist)
                {
                    modelOutput[p.x, p.y] = 1;
                    mask[p.x, p.y] = true;
                }

            if (passages.Count > 0)
                foreach (Point p in passages)
                {
                    modelOutput[p.x, p.y] = 1;
                    mask[p.x, p.y] = true;
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
                        modelOutput[p.x, p.y] = 0;
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
                            int distanceBetweenRooms = (int)(Mathf.Pow(pointA.x - pointB.x, 2) + Mathf.Pow(pointA.y - pointB.y, 2));

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

            Debug.DrawLine(transform.localPosition + new Vector3(pointA.x, 2, pointA.y), transform.localPosition + new Vector3(pointB.x, 2, pointB.y), Color.red, 10f);
        }

        void DrawCircle(MaskArea area, Point p, int r)
        {
            for (int x = -r; x <= r; x++)
            {
                for (int y = -r; y <= r; y++)
                {
                    if (x * x + y * y <= r * r)
                    {
                        int realX = p.x + x;
                        int realY = p.y + y;
                        if (IsInMap(realX, realY))
                        {
                            if (!PointExists(realX, realY))
                            {
                                passages.Add(new Point(realX, realY));
                                // area.AddPoint(new Point(realX, realY));
                            }
                        }
                    }
                }
            }
        }

        public bool IsInMap(int x, int y)
        {
            return !(x < 0) && x < size.x && !(y < 0) && y < size.x;
        }
        List<Point> GetLine(Point from, Point to)
        {
            List<Point> list = new List<Point>();
            var x = from.x;
            var y = from.y;

            var dx = to.x - from.x;
            var dy = to.y - from.y;

            bool inverted = false;
            int step = System.Math.Sign(dx);
            int gradientStep = System.Math.Sign(dy);

            int longest = Mathf.Abs(dx);
            int shortest = Mathf.Abs(dy);

            if (longest < shortest)
            {
                inverted = true;
                longest = Mathf.Abs(dy);
                shortest = Mathf.Abs(dx);
                step = System.Math.Sign(dy);
                gradientStep = System.Math.Sign(dx);
            }

            int gradientAccumulation = longest / 2;

            for (int i = 0; i < longest; i++)
            {
                list.Add(new Point(x, y));

                if (inverted)
                    y += step;
                else
                    x += step;

                gradientAccumulation += shortest;
                if (gradientAccumulation >= longest)
                {
                    if (inverted)
                        x += gradientStep;
                    else
                        y += gradientStep;
                    gradientAccumulation -= longest;
                }
            }

            return list;
        }

        bool AllTilesChecked()
        {
            for (int i = 0; i < size.x; i++)
                for (int j = 0; j < size.z; j++)
                    if (checker[i, j])
                        continue;
                    else
                        return checker[i, j];

            return true;
        }

        bool PointExists(int x, int y, int z = 0)
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

        void FloodFillUtil(List<Point> pointList, int x, int y)
        {
            if (x < 0 || x >= size.x || y < 0 || y >= size.z)
                return;
            if (pointList.Exists(p => (p.x == x) && (p.y == y)) || PointExists(x, y) || modelOutput[x, y] == 0)
                return;

            pointList.Add(new Point(x, y));

            FloodFillUtil(pointList, x + 1, y);
            FloodFillUtil(pointList, x - 1, y);
            FloodFillUtil(pointList, x, y + 1);
            FloodFillUtil(pointList, x, y - 1);
        }

        void FloodFill(List<Point> l, int x, int y)
        {
            FloodFillUtil(l, x, y);
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
        int[,] map;
        public int AreaID;
        public List<Point> pointlist;
        public List<Point> edgePoints;
        public List<MaskArea> connectedAreas;
        public bool isAccessibleFromMain;
        public bool isMainRoom;
        public MaskArea()
        {

        }
        public MaskArea(List<Point> l, int areaID, int[,] map)
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
                {
                    for (int y = p.y - 1; y <= p.y + 1; y++)
                    {
                        // if (x != p.x || y != p.y)
                        // {
                        var tempX = x;
                        var tempY = y;

                        if (x < 0) tempX = 0;
                        if (x >= map.GetLength(0)) tempX = map.GetLength(0) - 1;

                        if (y < 0) tempY = 0;
                        if (y >= map.GetLength(1)) tempY = map.GetLength(1) - 1;


                        if (x == 0 || x == map.GetLength(0) - 1 || y == 0 || y == map.GetLength(1) - 1)
                        {
                            if (map[tempX, tempY] == 1)
                                if (!edgePoints.Contains(p))
                                    edgePoints.Add(p);
                        }
                        else
                        {
                            if (map[tempX, tempY] == 0)
                                if (!edgePoints.Contains(p))
                                    edgePoints.Add(p);
                        }
                    }
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
        public bool PointExists(int x, int y, int z = 0)
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