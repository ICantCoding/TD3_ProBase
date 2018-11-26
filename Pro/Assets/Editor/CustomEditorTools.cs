using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class CustomEditorTools {

    #region 物体对象操作
    [MenuItem("编辑器小工具/物体信息/Hierarchy绝对路径", false, 1)]
    private static void GameObjectHierarchyPath()
    {
        GameObject selectedGo = Selection.activeGameObject;
        if (selectedGo == null)
        {
            Debug.Log("对不起, 您未选中Hierarchy面板中的物体!!!");
            return;
        }

        Transform trans = selectedGo.transform;
        string path = "/" + selectedGo.name;
        while (trans.parent != null)
        {
            trans = trans.parent;
            path = "/" + trans.name + path;
        }
        path = path.Substring(1, path.Length - 1);
        Debug.Log(selectedGo.name + "的绝对路径: " + path);
    }
    #endregion

    #region 组件操作
    private static Component[] copiedComponents;
    private static Dictionary<string, List<Component>> multiCopiedComponentsDic;
    [MenuItem("编辑器小工具/拷贝组件/单选物体复制", false, 1)]
    private static void Copy()
    {
        Array.Clear(copiedComponents, 0, copiedComponents.Length);
        copiedComponents = Selection.activeGameObject.GetComponents<Component>();
    }
    [MenuItem("编辑器小工具/拷贝组件/单选物体粘贴", false, 2)]
    private static void Paste()
    {
        GameObject targetGo = Selection.activeGameObject;
        if (copiedComponents == null || copiedComponents.Length == 0) return;
        foreach (var component in copiedComponents)
        {
            Type type = component.GetType();
            if (type == typeof(Transform) || type == typeof(MeshRenderer) || type == typeof(MeshFilter) || type == typeof(MeshCollider))
            {
                continue;
            }
            UnityEditorInternal.ComponentUtility.CopyComponent(component);
            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(targetGo);
        }
    }
    [MenuItem("编辑器小工具/拷贝组件/多选物体复制", false, 3)]
    private static void MultiCopy()
    {
        if (multiCopiedComponentsDic != null)
        {
            multiCopiedComponentsDic.Clear();
            multiCopiedComponentsDic = null;
        }
        multiCopiedComponentsDic = new Dictionary<string, List<Component>>();

        GameObject[] sourceGos = Selection.gameObjects;
        for (int i = 0; i < sourceGos.Length; i++)
        {
            GameObject sourceGo = sourceGos[i];
            List<Component> components = new List<Component>(sourceGo.GetComponents<Component>());
            if (components != null && components.Count > 0)
            {
                multiCopiedComponentsDic.Add(sourceGo.name, components);
            }
        }
    }
    [MenuItem("编辑器小工具/拷贝组件/多选物体粘贴", false, 4)]
    private static void MultiPaste()
    {
        GameObject[] targetGos = Selection.gameObjects;
        for (int i = 0; i < targetGos.Length; i++)
        {
            GameObject targetGo = targetGos[i];
            if (multiCopiedComponentsDic.ContainsKey(targetGo.name))
            {
                List<Component> components = multiCopiedComponentsDic[targetGo.name];
                foreach (var component in components)
                {
                    Type type = component.GetType();
                    if (type == typeof(Transform) || type == typeof(MeshRenderer) || type == typeof(MeshFilter) || type == typeof(MeshCollider))
                    {
                        continue;
                    }
                    UnityEditorInternal.ComponentUtility.CopyComponent(component);
                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(targetGo);
                }
            }
        }
    }
    [MenuItem("编辑器小工具/拷贝组件/多选物体组件清空", false, 5)]
    private static void MultiClearComponent()
    {
        GameObject[] targetGos = Selection.gameObjects;
        for (int i = 0; i < targetGos.Length; i++)
        {
            GameObject targetGo = targetGos[i];
            Component[] components = targetGo.GetComponents<Component>();
            foreach (var component in components)
            {
                Type type = component.GetType();
                if (type == typeof(Transform) || type == typeof(MeshRenderer) || type == typeof(MeshFilter) || type == typeof(MeshCollider))
                {
                    continue;
                }
                GameObject.DestroyImmediate(component);
            }
        }
    }
    #endregion

    #region 资源面板,资源配置体
    public class ScriptableConfig : ScriptableObject
    {
        public string _name = "名字";
        public string _description = "功能描述";
    }
    [Serializable]
    public class ConfigPrefab : ScriptableConfig
    {

    }
    [MenuItem("Assets/Create/设置资源配置体", false, 1)]
    private static void ResourceMark()
    {
        ConfigPrefab obj = ScriptableObject.CreateInstance<ConfigPrefab>();
        if (!obj)
        {
            return;
        }
        string[] strs = Selection.assetGUIDs;
        string path = AssetDatabase.GUIDToAssetPath(strs[0]);

        path = string.Format("{0}/{1}.asset", path, obj._name);
        AssetDatabase.CreateAsset(obj, path);
    }

    [MenuItem("Assets/Create/获取资源配置体信息", false, 2)]
    private static void GetResourceMark()
    {
        string[] paths = Selection.assetGUIDs;
        if (paths.Length <= 0) return;
        string path = AssetDatabase.GUIDToAssetPath(paths[0]);
        Debug.Log(path);
        ConfigPrefab configPrefab = AssetDatabase.LoadAssetAtPath<ConfigPrefab>(path);
    }
    #endregion

}
