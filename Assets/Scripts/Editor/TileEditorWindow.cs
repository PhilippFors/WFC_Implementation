using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MyWFC;
public class TileEditorWindow : EditorWindow
{
    public MyWFC.MyTile tile;
    float height = 20f;
    float width = 40f;

    List<float> ys;
    List<float> xs;

    public bool[,] arr;

    MyWFC.Connections paintable;
    public List<Rect> rs;

    [MenuItem("Editors/Tile Editor")]
    public static void GetTileEditorWindow()
    {
        GetWindow<TileEditorWindow>("Tile editor");
    }
    private void OnGUI()
    {
        float x = width + 20f;
        float y = 70f;

        arr = new bool[3, 3];
        var selection = Selection.activeGameObject;

        var ts = Selection.gameObjects;
        if (selection != null)
            tile = selection.GetComponent<MyWFC.MyTile>();

        // if (ts != null && ts.Length == 2)
        // {
        //     var tile1 = ts[0].GetComponent<MyWFC.MyTile>();
        //     var tile2 = ts[1].GetComponent<MyWFC.MyTile>();
        //     DrawEditorRect(tile1.cells[0], x, y, false, tile1.gameObject.name);
        //     DrawEditorRect(tile2.cells[0], x * 4, y, false, tile2.gameObject.name);
        // }
        // else 
        Rect r = new Rect(10f, 10f, 50f, 20f);
        paintable = (MyWFC.Connections)EditorGUI.EnumPopup(r, paintable);
        r = new Rect(80f, 10f, 50f, 20f);
        if (GUI.Button(r, "Save"))
        {
            if (PrefabUtility.IsPartOfPrefabInstance(tile.gameObject))
                PrefabUtility.ApplyPrefabInstance(tile.gameObject, InteractionMode.UserAction);
            else
                PrefabUtility.SavePrefabAsset(tile.gameObject);
        }

        if (tile != null)
            for (int i = 0; i < tile.cells.Count; i++)
            {
                x = width + 20f;
                y = 70f;
                if (tile.cells[i].center.x > 0)
                    x = x * 4;
                if (tile.cells[i].center.z < 0)
                    y = y * 4;
                for (int j = 0, z = 0; j < tile.cells[i].sides.Count; j++)
                {
                    if (j >= (float)tile.cells[i].sides.Count / 2)
                        z = 2;

                    DrawEditorRect(x * (z == 2 ? (j - (float)tile.cells[i].sides.Count / 2) : j) * 4 + width + 10f, (y) * (z + 1), i, j);
                }
            }
    }

    private void OnDestroy()
    {
        tile.showGizmo = false;
    }
    void DrawEditorRect(float x, float y, int index, int jindex)
    {

        Rect r = new Rect((x - width) - 5f, (y - height) - 5f, width, height);
        // tile.cells[index].sides[jindex].topLeft = (MyWFC.Connections)EditorGUI.EnumPopup(r, tile.cells[index].sides[jindex].topLeft);
        DrawButton(r, ref tile.cells[index].sides[jindex].topLeft);

        r = new Rect((x), (y - height) - 5f, width, height);
        DrawButton(r, ref tile.cells[index].sides[jindex].topMiddle);

        r = new Rect((x + width) + 5f, (y - height) - 5f, width, height);
        DrawButton(r, ref tile.cells[index].sides[jindex].topRight);

        r = new Rect((x - width) - 5f, (y), width, height);
        DrawButton(r, ref tile.cells[index].sides[jindex].left);

        r = new Rect((x), (y), width, height);
        DrawButton(r, ref tile.cells[index].sides[jindex].middle);

        r = new Rect((x + width) + 5f, (y), width, height);
        DrawButton(r, ref tile.cells[index].sides[jindex].right);

        r = new Rect((x - width) - 5f, (y + height) + 5f, width, height);
        DrawButton(r, ref tile.cells[index].sides[jindex].bottomLeft);

        r = new Rect((x), (y + height) + 5f, width, height);
        DrawButton(r, ref tile.cells[index].sides[jindex].bottomMiddle);

        r = new Rect((x + width) + 5f, (y + height) + 5f, width, height);
        DrawButton(r, ref tile.cells[index].sides[jindex].bottomRight);

        r = new Rect((x), (y + height * 2.5f) + 2f, width, height);
        if (GUI.Button(r, "all"))
            PaintAll(index, jindex);

        Rect c = new Rect(x, (y + height * 4) + 5f, 50f, 20f);
        GUI.Label(c, tile.cells[index].sides[jindex].side.ToString());
    }

    void DrawButton(Rect rect, ref MyWFC.Connections c)
    {
        if (GUI.Button(rect, c.ToString()))
        {
            c = paintable;
            // if (PrefabUtility.IsPartOfPrefabInstance(tile.gameObject))
            //     PrefabUtility.ApplyPrefabInstance(tile.gameObject, InteractionMode.UserAction);
            // else
            //     PrefabUtility.SavePrefabAsset(tile.gameObject);
        }
    }

    void PaintAll(int index, int jindex)
    {
        tile.cells[index].sides[jindex].topLeft = paintable;
        tile.cells[index].sides[jindex].topMiddle = paintable;
        tile.cells[index].sides[jindex].topRight = paintable;
        tile.cells[index].sides[jindex].left = paintable;
        tile.cells[index].sides[jindex].middle = paintable;
        tile.cells[index].sides[jindex].right = paintable;
        tile.cells[index].sides[jindex].bottomLeft = paintable;
        tile.cells[index].sides[jindex].bottomMiddle = paintable;
        tile.cells[index].sides[jindex].bottomRight = paintable;

        if (PrefabUtility.IsPartOfPrefabInstance(tile.gameObject))
            PrefabUtility.ApplyPrefabInstance(tile.gameObject, InteractionMode.UserAction);
        else
            PrefabUtility.SavePrefabAsset(tile.gameObject);
    }
    // void DrawEditorRect(MyWFC.Cell cell, float x, float y, bool showAdds = false, string name = "")
    // {
    //     for (int j = 0; j < cell.sides.Count; j++)
    //     {
    //         if (cell.sides[j].side == MyWFC.Sides.Left)
    //         {
    //             var templeft = cell.sides[j].left;
    //             var tempright = cell.sides[j].right;
    //             var tempmiddl = cell.sides[j].middle;

    //             Rect r = new Rect((x - width - width / 2), (y - height), width, 20f);
    //             cell.sides[j].left = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].left);

    //             r = new Rect((x - width - width / 2), (y), width, 20f);
    //             cell.sides[j].middle = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].middle);

    //             r = new Rect((x - width - width / 2), (y + height), width, 20f);
    //             cell.sides[j].right = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].right);

    //             if (showAdds)
    //             {
    //                 r = new Rect((x - width * 3), (y), width, 20f);
    //                 if (!tile.cells.Exists(f => f.center.Equals(new Vector3(cell.center.x - (float)tile.size.x, cell.center.y, cell.center.z))))
    //                     if (GUI.Button(r, "+"))
    //                     {
    //                         AddNewCell(cell.sides[j].side, cell, cell.sides[j]);
    //                     }
    //                 r = new Rect(x, y, 20f, 20f);
    //                 if (GUI.Button(r, "-"))
    //                 {
    //                     tile.cells.Remove(cell);
    //                     break;
    //                 }
    //             }
    //             if (templeft != cell.sides[j].left || tempright != cell.sides[j].right || tempmiddl != cell.sides[j].middle)
    //                 if (PrefabUtility.IsPartOfPrefabInstance(tile.gameObject))
    //                     PrefabUtility.ApplyPrefabInstance(tile.gameObject, InteractionMode.UserAction);
    //                 else
    //                     PrefabUtility.SavePrefabAsset(tile.gameObject);
    //         }
    //         else if (cell.sides[j].side == MyWFC.Sides.Right)
    //         {
    //             var templeft = cell.sides[j].left;
    //             var tempright = cell.sides[j].right;
    //             var tempmiddl = cell.sides[j].middle;
    //             Rect r = new Rect((x + width + width / 2), (y - height), width, 20f);
    //             cell.sides[j].right = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].right);

    //             r = new Rect((x + width + width / 2), (y), width, 20f);
    //             cell.sides[j].middle = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].middle);

    //             r = new Rect((x + width + width / 2), (y + height), width, 20f);
    //             cell.sides[j].left = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].left);

    //             r = new Rect((x + width * 3), (y), 30f, 20f);

    //             if (showAdds)
    //             {
    //                 if (!tile.cells.Exists(f => f.center.Equals(new Vector3(cell.center.x + (float)tile.size.x, cell.center.y, cell.center.z))))
    //                     if (GUI.Button(r, "+"))
    //                     {
    //                         AddNewCell(cell.sides[j].side, cell, cell.sides[j]);
    //                     }
    //                 r = new Rect(x, y, 20f, 20f);
    //                 if (GUI.Button(r, "-"))
    //                 {
    //                     tile.cells.Remove(cell);
    //                     break;
    //                 }
    //             }
    //             if (templeft != cell.sides[j].left || tempright != cell.sides[j].right || tempmiddl != cell.sides[j].middle)
    //                 if (PrefabUtility.IsPartOfPrefabInstance(tile.gameObject))
    //                     PrefabUtility.ApplyPrefabInstance(tile.gameObject, InteractionMode.UserAction);
    //                 else
    //                     PrefabUtility.SavePrefabAsset(tile.gameObject);
    //         }
    //         else if (cell.sides[j].side == MyWFC.Sides.Front)
    //         {
    //             var templeft = cell.sides[j].left;
    //             var tempright = cell.sides[j].right;
    //             var tempmiddl = cell.sides[j].middle;
    //             Rect r = new Rect((x - width - width / 2) + 10f, (y - height * 2), width, 20f);
    //             cell.sides[j].right = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].right);

    //             r = new Rect(x, (y - height * 2), width, 20f);
    //             cell.sides[j].middle = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].middle);

    //             r = new Rect((x + width + width / 2) - 10f, (y - height * 2), width, 20f);
    //             cell.sides[j].left = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].left);

    //             r = new Rect(x, (y - height * 2) - 20f, 30f, 20f);
    //             if (showAdds)
    //             {
    //                 if (!tile.cells.Exists(f => f.center.Equals(new Vector3(cell.center.x, cell.center.y, cell.center.z + (float)tile.size.z))))
    //                     if (GUI.Button(r, "+"))
    //                     {
    //                         AddNewCell(cell.sides[j].side, cell, cell.sides[j]);
    //                     }
    //                 r = new Rect(x, y, 20f, 20f);
    //                 if (GUI.Button(r, "-"))
    //                 {
    //                     tile.cells.Remove(cell);
    //                     break;
    //                 }
    //             }
    //             if (templeft != cell.sides[j].left || tempright != cell.sides[j].right || tempmiddl != cell.sides[j].middle)
    //                 if (PrefabUtility.IsPartOfPrefabInstance(tile.gameObject))
    //                     PrefabUtility.ApplyPrefabInstance(tile.gameObject, InteractionMode.UserAction);
    //                 else
    //                     PrefabUtility.SavePrefabAsset(tile.gameObject);
    //             // PrefabUtility.SavePrefabAsset(tile.gameObject);
    //         }
    //         else if (cell.sides[j].side == MyWFC.Sides.Back)
    //         {
    //             var templeft = cell.sides[j].left;
    //             var tempright = cell.sides[j].right;
    //             var tempmiddl = cell.sides[j].middle;
    //             Rect r = new Rect((x - width - width / 2) + 10f, (y + height * 2), width, 20f);
    //             cell.sides[j].left = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].left);

    //             r = new Rect(x, (y + height * 2), width, 20f);
    //             cell.sides[j].middle = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].middle);

    //             r = new Rect((x + width + width / 2) - 10f, (y + height * 2), width, 20f);
    //             cell.sides[j].right = (MyWFC.Connections)EditorGUI.EnumPopup(r, cell.sides[j].right);


    //             r = new Rect(x, (y + height * 2) + 20f, 30f, 20f);
    //             if (showAdds)
    //             {
    //                 if (!tile.cells.Exists(f => f.center.Equals(new Vector3(cell.center.x, cell.center.y, cell.center.z - (float)tile.size.z))))
    //                     if (GUI.Button(r, "+"))
    //                     {
    //                         AddNewCell(cell.sides[j].side, cell, cell.sides[j]);
    //                     }
    //                 r = new Rect(x, y, 20f, 20f);
    //                 if (GUI.Button(r, "-"))
    //                 {
    //                     tile.cells.Remove(cell);
    //                     break;
    //                 }
    //             }
    //             if (templeft != cell.sides[j].left || tempright != cell.sides[j].right || tempmiddl != cell.sides[j].middle)
    //                 // if (PrefabUtility.IsPartOfPrefabInstance(tile.gameObject))
    //                 if (tile.gameObject.activeInHierarchy)
    //                     PrefabUtility.ApplyPrefabInstance(tile.gameObject, InteractionMode.UserAction);
    //                 else
    //                     PrefabUtility.SavePrefabAsset(tile.gameObject);
    //         }
    //     }
    //     if (name != "")
    //     {
    //         Rect c = new Rect(x - 50f, (y + height * 4), 100f, 20f);
    //         GUI.Label(c, name);
    //     }
    // }

    void AddNewCell(MyWFC.Sides sides, MyWFC.Cell oldCell, MyWFC.TileSide tileSide)
    {
        MyWFC.Cell cell1 = new MyWFC.Cell();
        switch (sides)
        {
            case MyWFC.Sides.Left:
                cell1.center = new Vector3(oldCell.center.x - (float)tile.size.x, oldCell.center.y, oldCell.center.z);
                if (cell1.center.z > 0)
                {
                    tileSide.left = MyWFC.Connections.BIDYES;
                    tileSide.middle = MyWFC.Connections.BID;
                    tileSide.right = MyWFC.Connections.BID;

                    foreach (MyWFC.TileSide s in cell1.sides)
                    {
                        if (s.side == MyWFC.Sides.Right)
                        {
                            s.left = MyWFC.Connections.BID;
                            s.middle = MyWFC.Connections.BID;
                            s.right = MyWFC.Connections.BIDYES;
                        }
                    }

                    tile.cells.Add(cell1);
                }
                else
                {
                    tileSide.left = MyWFC.Connections.BID;
                    tileSide.middle = MyWFC.Connections.BID;
                    tileSide.right = MyWFC.Connections.BIDYES;

                    foreach (MyWFC.TileSide s in cell1.sides)
                    {
                        if (s.side == MyWFC.Sides.Right)
                        {
                            s.left = MyWFC.Connections.BIDYES;
                            s.middle = MyWFC.Connections.BID;
                            s.right = MyWFC.Connections.BID;
                        }
                    }
                    tile.cells.Add(cell1);
                }
                break;
            case MyWFC.Sides.Right:
                cell1.center = new Vector3(oldCell.center.x + (float)tile.size.x, oldCell.center.y, oldCell.center.z);
                if (cell1.center.z > 0)
                {
                    tileSide.left = MyWFC.Connections.BID;
                    tileSide.middle = MyWFC.Connections.BID;
                    tileSide.right = MyWFC.Connections.BIDYES;


                    foreach (MyWFC.TileSide s in cell1.sides)
                    {
                        if (s.side == MyWFC.Sides.Left)
                        {
                            s.left = MyWFC.Connections.BIDYES;
                            s.middle = MyWFC.Connections.BID;
                            s.right = MyWFC.Connections.BID;
                        }
                    }
                    tile.cells.Add(cell1);
                }
                else
                {
                    tileSide.left = MyWFC.Connections.BIDYES;
                    tileSide.middle = MyWFC.Connections.BID;
                    tileSide.right = MyWFC.Connections.BID;


                    foreach (MyWFC.TileSide s in cell1.sides)
                    {
                        if (s.side == MyWFC.Sides.Left)
                        {
                            s.left = MyWFC.Connections.BID;
                            s.middle = MyWFC.Connections.BID;
                            s.right = MyWFC.Connections.BIDYES;
                        }
                    }
                    tile.cells.Add(cell1);
                }
                break;

            case MyWFC.Sides.Front:
                cell1.center = new Vector3(oldCell.center.x, oldCell.center.y, oldCell.center.z + (float)tile.size.z);
                if (cell1.center.x > 0)
                {
                    tileSide.left = MyWFC.Connections.BIDYES;
                    tileSide.middle = MyWFC.Connections.BID;
                    tileSide.right = MyWFC.Connections.BID;


                    foreach (MyWFC.TileSide s in cell1.sides)
                    {
                        if (s.side == MyWFC.Sides.Back)
                        {
                            s.left = MyWFC.Connections.BID;
                            s.middle = MyWFC.Connections.BID;
                            s.right = MyWFC.Connections.BIDYES;
                        }
                    }
                    tile.cells.Add(cell1);
                }
                else
                {
                    tileSide.left = MyWFC.Connections.BID;
                    tileSide.middle = MyWFC.Connections.BID;
                    tileSide.right = MyWFC.Connections.BIDYES;


                    foreach (MyWFC.TileSide s in cell1.sides)
                    {
                        if (s.side == MyWFC.Sides.Back)
                        {
                            s.left = MyWFC.Connections.BIDYES;
                            s.middle = MyWFC.Connections.BID;
                            s.right = MyWFC.Connections.BID;
                        }
                    }
                    tile.cells.Add(cell1);
                }
                break;
            case MyWFC.Sides.Back:
                cell1.center = new Vector3(oldCell.center.x, oldCell.center.y, oldCell.center.z - (float)tile.size.z);
                if (cell1.center.x > 0)
                {
                    tileSide.left = MyWFC.Connections.BID;
                    tileSide.middle = MyWFC.Connections.BID;
                    tileSide.right = MyWFC.Connections.BIDYES;


                    foreach (MyWFC.TileSide s in cell1.sides)
                    {
                        if (s.side == MyWFC.Sides.Front)
                        {
                            s.left = MyWFC.Connections.BIDYES;
                            s.middle = MyWFC.Connections.BID;
                            s.right = MyWFC.Connections.BID;
                        }
                    }
                    tile.cells.Add(cell1);
                }
                else
                {
                    tileSide.left = MyWFC.Connections.BIDYES;
                    tileSide.middle = MyWFC.Connections.BID;
                    tileSide.right = MyWFC.Connections.BIDYES;


                    foreach (MyWFC.TileSide s in cell1.sides)
                    {
                        if (s.side == MyWFC.Sides.Front)
                        {
                            s.left = MyWFC.Connections.BID;
                            s.middle = MyWFC.Connections.BID;
                            s.right = MyWFC.Connections.BIDYES;
                        }
                    }
                    tile.cells.Add(cell1);
                }
                break;
        }
    }
}
