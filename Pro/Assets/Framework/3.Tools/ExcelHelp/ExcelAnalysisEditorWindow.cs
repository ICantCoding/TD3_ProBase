using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using OfficeOpenXml;
using System.Data;
using System.IO;
using Excel;


public class ExcelAnalysisEditorWindow : EditorWindow
{

    static string excelFileName = "test.xls";
    static string sheetName = "devices";

    string countStr = "";
    private GameObject Cube;

    // GameObject[] gos = new GameObject[2];
    // string[] names = new string[2];
    // string[] ids = new string[2];
    // string[] typeids = new string[2];
    static List<GameObject> gos = new List<GameObject>();
    static List<string> names = new List<string>();
    static List<string> ids = new List<string>();
    static List<string> typeids = new List<string>();

    private Vector2 scrollPos;


    [MenuItem("编辑器小工具/Xml/生成设备Xml文件", false, 1)]
    private static void ShowExcelAnalysisEditorWindow()
    {
        gos.Clear();
        names.Clear();
        ids.Clear();
        typeids.Clear();
        EditorWindow.GetWindow(typeof(ExcelAnalysisEditorWindow));
    }

    private void OnGUI()
    {

        GUILayout.Label("OperableObjectsExcel", EditorStyles.boldLabel);
        countStr = EditorGUILayout.TextField("Count", countStr);

        if (string.IsNullOrEmpty(countStr) == true)
        {
            return;
        }

        int count = 0;
        if (int.TryParse(countStr, out count) == false || count <= 0)
        {
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, true); //滚动视图布局
        for (int i = 0; i < count; i++)
        {
            if(i + 1 > gos.Count){
                GameObject go = null;
                gos.Add(go);
                string name = string.Empty;
                names.Add(name);
                string id = string.Empty;
                ids.Add(id);
                string typeId = string.Empty;
                typeids.Add(typeId);
            }
            if(count < gos.Count){
                gos.RemoveRange(count, gos.Count - count);
                names.RemoveRange(count, gos.Count - count);
                ids.RemoveRange(count, gos.Count - count);
                typeids.RemoveRange(count, gos.Count - count);
            }
            gos[i] = EditorGUILayout.ObjectField(gos[i], typeof(GameObject), true) as GameObject;
            EditorGUILayout.BeginHorizontal();
            names[i] = EditorGUILayout.TextField("Name", names[i]);
            typeids[i] = EditorGUILayout.TextField("StartTypeId", typeids[i]);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        

        if (GUILayout.Button("构建设备Excel文件"))
        {
            int rowIndex = 2;
            string path = Application.dataPath + "/" + excelFileName;
            FileInfo newFile = new FileInfo(path);
            if (newFile.Exists)
            {
                newFile.Delete();
                newFile = new FileInfo(path);
            }
            ExcelPackage package = new ExcelPackage(newFile);
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sheetName);
            worksheet.Cells[1, 1].Value = "Id";
            worksheet.Cells[1, 2].Value = "TypeId";
            worksheet.Cells[1, 3].Value = "Name";
            worksheet.Cells[1, 4].Value = "PosX";
            worksheet.Cells[1, 5].Value = "PosY";
            worksheet.Cells[1, 6].Value = "PosZ";
            worksheet.Cells[1, 7].Value = "RotationX";
            worksheet.Cells[1, 8].Value = "RotationY";
            worksheet.Cells[1, 9].Value = "RotationZ";

            Debug.Log("开始Build Excel...");
            int idIndex = 1;
            for (int i = 0; i < count; i++)
            {
                if (gos[i] == null) continue;
                Transform goTrans = gos[i].transform;
                if (goTrans.childCount == 0) continue;
                for (int j = 0; j < goTrans.childCount; j++)
                {
                    Transform childGoTrans = goTrans.GetChild(j);
                    EditorUtility.DisplayProgressBar("构建Xml", childGoTrans.gameObject.name, (float)i / (float)count);
                    if (childGoTrans != null)
                    {
                        float posX = childGoTrans.position.x;
                        float posY = childGoTrans.position.y;
                        float posZ = childGoTrans.position.z;
                        float rotationX = childGoTrans.eulerAngles.x;
                        float rotationY = childGoTrans.eulerAngles.y;
                        float rotationZ = childGoTrans.eulerAngles.z;

                        worksheet.Cells[rowIndex, 1].Value = idIndex ++;
                        worksheet.Cells[rowIndex, 2].Value = int.Parse(typeids[i]) + j;
                        worksheet.Cells[rowIndex, 3].Value = names[i];
                        worksheet.Cells[rowIndex, 4].Value = posX;
                        worksheet.Cells[rowIndex, 5].Value = posY;
                        worksheet.Cells[rowIndex, 6].Value = posZ;
                        worksheet.Cells[rowIndex, 7].Value = rotationX;
                        worksheet.Cells[rowIndex, 8].Value = rotationY;
                        worksheet.Cells[rowIndex, 9].Value = rotationZ;

                        rowIndex++;
                    }
                }
            }
            package.Save();
            worksheet.Dispose();
            package.Dispose();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
            Debug.Log("完成Build Excel...");
        }

        if (GUILayout.Button("读取设备Excel文件, 生成Asset资源"))
        {
            Debug.Log("开始生成Asset!");
            ReadExcel();
            CreateOperablePrefabInfoAsset();
            Debug.Log("成功生成Asset!");
        }
    }

    private static DataRowCollection ReadExcel()
    {
        string path = Application.dataPath + "/" + excelFileName;
        FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        DataSet result = excelReader.AsDataSet();
        //int columns = result.Tables[0].Columns.Count;
        //int rows = result.Tables[0].Rows.Count;
        //tables可以按照sheet名获取，也可以按照sheet索引获取
        //return result.Tables[0].Rows;
        return result.Tables[sheetName].Rows;
    }

    public static void CreateOperablePrefabInfoAsset()
    {
        DataRowCollection collect = ReadExcel();
        OperablePrefabInfoAsset asset = ScriptableObject.CreateInstance<OperablePrefabInfoAsset>();
        for (int i = 1; i < collect.Count; i++)
        {
            EditorUtility.DisplayProgressBar("生成Asset", collect[i][2].ToString(), (float)i / (float)collect.Count);
            OperablePrefabInfo operablePrefabInfo = new OperablePrefabInfo()
            {
                objectId = int.Parse(collect[i][0].ToString()),
                objectType = int.Parse(collect[i][1].ToString()),
                objectDescription = collect[i][2].ToString(),
                objectPosition = new Vector3(float.Parse(collect[i][3].ToString()), float.Parse(collect[i][4].ToString()), float.Parse(collect[i][5].ToString())),
                objectEulerAngle = new Vector3(float.Parse(collect[i][6].ToString()), float.Parse(collect[i][7].ToString()), float.Parse(collect[i][8].ToString()))
            };
            Debug.Log(operablePrefabInfo);
            asset.operablePrefabInfoList.Add(operablePrefabInfo);
        }
        AssetDatabase.CreateAsset(asset, "Assets/Editor/OperablePrefabInfoAsset.asset");
        AssetDatabase.SaveAssets(); //及时保存资源
        AssetDatabase.Refresh(); //及时刷新资源
        EditorUtility.ClearProgressBar();
    }
}
