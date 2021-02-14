using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TileEditorWindow : EditorWindow
{
    MyWFC.MyTile tile;
    float height = 20f;
    float width = 30f;

    List<float> ys;
    List<float> xs;

    public bool[,] arr;

    public List<Rect> rs;

    [MenuItem("Editors/Tile Editor")]
    public static void GetTileEditorWindow()
    {
        GetWindow<TileEditorWindow>("Tile editor");
    }
    private void OnGUI()
    {
        float x = 85f;
        float y = 80f;
        rs = new List<Rect>();
        ys = new List<float>();
        xs = new List<float>();
        arr = new bool[3, 3];
        var selection = Selection.activeGameObject;

        var ts = Selection.gameObjects;
        if (selection != null)
            tile = selection.GetComponent<MyWFC.MyTile>();

        if (ts != null && ts.Length == 2)
        {
            var tile1 = ts[0].GetComponent<MyWFC.MyTile>();
            var tile2 = ts[1].GetComponent<MyWFC.MyTile>();
            DrawEditorRect(tile1.cells[0], x, y, false, tile1.gameObject.name);
            DrawEditorRect(tile2.cells[0], x * 4, y, false, tile2.gameObject.name);
        }
        else if (tile != null)
            for (int i = 0; i < tile.cells.Count; i++)
            {
                x = 85f;
                y = 80f;
                if (tile.cells[i].center.x > 0)
                    x = x * 4;
                if (tile.cells[i].center.z < 0)
                    y = y * 4;

                DrawEditorRect(tile.cells[i], x, y, false, tile.gameObject.name);
            }
    }

    void DrawEditorRect(MyWFC.Cell cell, float x, float y, bool showAdds = false, string name = "")
    {
        for (int j = 0; j < cell.sides.Count; j++)
        {
            if (cell.sides[j].side == MyWFC.Sides.Left)
            {
                var templeft = cell.sides[j].left;
                var tempright = cell.sides[j].right;
                var tempmiddl = cell.sides[j].middle;
                Rect r = new Rect((x - width - width / 2), (y - height), 30f, 20f);
                cell.sides[j].left = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].left);

                r = new Rect((x - width - width / 2), (y), 30f, 20f);
                cell.sides[j].middle = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].middle);

                r = new Rect((x - width - width / 2), (y + height), 30f, 20f);
                cell.sides[j].right = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].right);

                if (showAdds)
                {
                    r = new Rect((x - width * 3), (y), 30f, 20f);
                    if (!tile.cells.Exists(f => f.center.Equals(new Vector3(cell.center.x - (float)tile.size.x, cell.center.y, cell.center.z))))
                        if (GUI.Button(r, "+"))
                        {
                            cell.sides[j].left = MyWFC.Connections.BIDYES;
                            cell.sides[j].middle = MyWFC.Connections.BID;
                            cell.sides[j].right = MyWFC.Connections.BID;

                            MyWFC.Cell cell1 = new MyWFC.Cell();
                            cell1.center = new Vector3(cell.center.x - (float)tile.size.x, cell.center.y, cell.center.z);
                            foreach (MyWFC.TileSide side in cell1.sides)
                            {
                                if (side.side == MyWFC.Sides.Right)
                                {
                                    side.left = MyWFC.Connections.BID;
                                    side.middle = MyWFC.Connections.BID;
                                    side.right = MyWFC.Connections.BIDYES;
                                }
                            }
                            tile.cells.Add(cell1);
                        }
                    r = new Rect(x, y, 20f, 20f);
                    if (GUI.Button(r, "-"))
                    {
                        tile.cells.Remove(cell);
                        break;
                    }
                }
                if (templeft != cell.sides[j].left || tempright != cell.sides[j].right || tempmiddl != cell.sides[j].middle)
                    if (PrefabUtility.IsPartOfPrefabInstance(tile.gameObject))
                        PrefabUtility.ApplyPrefabInstance(tile.gameObject, InteractionMode.UserAction);
                    else
                        PrefabUtility.SavePrefabAsset(tile.gameObject);
            }
            else if (cell.sides[j].side == MyWFC.Sides.Right)
            {
                var templeft = cell.sides[j].left;
                var tempright = cell.sides[j].right;
                var tempmiddl = cell.sides[j].middle;
                Rect r = new Rect((x + width + width / 2), (y - height), 30f, 20f);
                cell.sides[j].right = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].right);

                r = new Rect((x + width + width / 2), (y), 30f, 20f);
                cell.sides[j].middle = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].middle);

                r = new Rect((x + width + width / 2), (y + height), 30f, 20f);
                cell.sides[j].left = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].left);

                r = new Rect((x + width * 3), (y), 30f, 20f);

                if (showAdds)
                {
                    if (!tile.cells.Exists(f => f.center.Equals(new Vector3(cell.center.x + (float)tile.size.x, cell.center.y, cell.center.z))))
                        if (GUI.Button(r, "+"))
                        {
                            MyWFC.Cell cell1 = new MyWFC.Cell();
                            cell1.center = new Vector3(cell.center.x + (float)tile.size.x, cell.center.y, cell.center.z);
                            tile.cells.Add(cell1);
                        }
                    r = new Rect(x, y, 20f, 20f);
                    if (GUI.Button(r, "-"))
                    {
                        tile.cells.Remove(cell);
                        break;
                    }
                }
                if (templeft != cell.sides[j].left || tempright != cell.sides[j].right || tempmiddl != cell.sides[j].middle)
                    if (PrefabUtility.IsPartOfPrefabInstance(tile.gameObject))
                        PrefabUtility.ApplyPrefabInstance(tile.gameObject, InteractionMode.UserAction);
                    else
                        PrefabUtility.SavePrefabAsset(tile.gameObject);
            }
            else if (cell.sides[j].side == MyWFC.Sides.Front)
            {
                var templeft = cell.sides[j].left;
                var tempright = cell.sides[j].right;
                var tempmiddl = cell.sides[j].middle;
                Rect r = new Rect((x - width - width / 2) + 10f, (y - height * 2), 30f, 20f);
                cell.sides[j].right = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].right);

                r = new Rect(x, (y - height * 2), 30f, 20f);
                cell.sides[j].middle = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].middle);

                r = new Rect((x + width + width / 2) - 10f, (y - height * 2), 30f, 20f);
                cell.sides[j].left = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].left);

                r = new Rect(x, (y - height * 2) - 20f, 30f, 20f);
                if (showAdds)
                {
                    if (!tile.cells.Exists(f => f.center.Equals(new Vector3(cell.center.x, cell.center.y, cell.center.z + (float)tile.size.z))))
                        if (GUI.Button(r, "+"))
                        {
                            MyWFC.Cell cell1 = new MyWFC.Cell();
                            cell1.center = new Vector3(cell.center.x, cell.center.y, cell.center.z + (float)tile.size.z);
                            tile.cells.Add(cell1);
                        }
                    r = new Rect(x, y, 20f, 20f);
                    if (GUI.Button(r, "-"))
                    {
                        tile.cells.Remove(cell);
                        break;
                    }
                }
                if (templeft != cell.sides[j].left || tempright != cell.sides[j].right || tempmiddl != cell.sides[j].middle)
                    if (PrefabUtility.IsPartOfPrefabInstance(tile.gameObject))
                        PrefabUtility.ApplyPrefabInstance(tile.gameObject, InteractionMode.UserAction);
                    else
                        PrefabUtility.SavePrefabAsset(tile.gameObject);
                // PrefabUtility.SavePrefabAsset(tile.gameObject);
            }
            else if (cell.sides[j].side == MyWFC.Sides.Back)
            {
                var templeft = cell.sides[j].left;
                var tempright = cell.sides[j].right;
                var tempmiddl = cell.sides[j].middle;
                Rect r = new Rect((x - width - width / 2) + 10f, (y + height * 2), 30f, 20f);
                cell.sides[j].left = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].left);

                r = new Rect(x, (y + height * 2), 30f, 20f);
                cell.sides[j].middle = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].middle);

                r = new Rect((x + width + width / 2) - 10f, (y + height * 2), 30f, 20f);
                cell.sides[j].right = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].right);


                r = new Rect(x, (y + height * 2) + 20f, 30f, 20f);
                if (showAdds)
                {
                    if (!tile.cells.Exists(f => f.center.Equals(new Vector3(cell.center.x, cell.center.y, cell.center.z - (float)tile.size.z))))
                        if (GUI.Button(r, "+"))
                        {
                            MyWFC.Cell cell1 = new MyWFC.Cell();
                            cell1.center = new Vector3(cell.center.x, cell.center.y, cell.center.z - (float)tile.size.z);
                            tile.cells.Add(cell1);
                        }
                    r = new Rect(x, y, 20f, 20f);
                    if (GUI.Button(r, "-"))
                    {
                        tile.cells.Remove(cell);
                        break;
                    }
                }
                if (templeft != cell.sides[j].left || tempright != cell.sides[j].right || tempmiddl != cell.sides[j].middle)
                    // if (PrefabUtility.IsPartOfPrefabInstance(tile.gameObject))
                    if (tile.gameObject.activeInHierarchy)
                        PrefabUtility.ApplyPrefabInstance(tile.gameObject, InteractionMode.UserAction);
                    else
                        PrefabUtility.SavePrefabAsset(tile.gameObject);
            }
        }
        if (name != "")
        {
            Rect c = new Rect(x - 50f, (y + height * 4), 100f, 20f);
            GUI.Label(c, name);
        }
    }
}
