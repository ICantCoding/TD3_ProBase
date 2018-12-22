using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TD3_Framework
{
    public class Package : Editor
    {

        private static string ModuleRoot = string.Format("{0}/{1}", Application.dataPath, GameConfig.module_name);
        private static string TargetRoot = string.Format("{0}/{1}", Application.streamingAssetsPath, GameConfig.module_name);

        static string AB_Lua = "AB_Lua";
        static string AB_Prefab = "AB_Prefab";
        static string AB_Scene = "AB_Scene";
        static string AB_Texture = "AB_Texture";
        static string AB_Material = "AB_Material";
        static string AB_Audio = "AB_Audio";

        [MenuItem("编辑器小工具/AssetBundle/打包方式1(废弃)", false, 100000)]
        private static void BuildAssetBundle()
        {
            Debug.Log("--------- 打包开始 ---------");
            CleanAssetBundle();
            Util.CreateDirectory(TargetRoot);
            List<string> Modules = Util.GetChildDirectoriesByNoRecursive(ModuleRoot);
            foreach (var module in Modules)
            {
                BuildModule(module, AB_Lua);
                BuildModule(module, AB_Prefab);
                BuildModule(module, AB_Scene);
                BuildModule(module, AB_Texture);
                BuildModule(module, AB_Material);
                BuildModule(module, AB_Audio);
            }
            EditorUtility.DisplayProgressBar("Building AssetBundle", "waiting...", 0f);
            Close();
            BuildHandler();

            //为Modules文件做MD5
            try
            {
                string outPath = string.Format("{0}/{1}", TargetRoot, "md5file.txt");
                FileStream fs = new FileStream(outPath, FileMode.CreateNew);
                StreamWriter sw = new StreamWriter(fs);
                string file = string.Format("{0}/{1}", TargetRoot, GameConfig.module_name);
                string md5 = Util.Md5File(file);
                string value = file.Replace("\\", "/");
                value = value.Replace(TargetRoot + "/", string.Empty);
                sw.WriteLine(value + "|" + md5);
                fs.Flush();
                sw.Close();
                fs.Close();
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
                Close();
                return;
            }

            //为每个AssetBundle做MD5
            foreach (var module in Modules)
            {
                CreateMd5File(module);
                CreateVersionFile(module); //拷贝模块的版本管理文件
            }
            CreateAppVersionFile(); //创建应用程序的版本文件

            AssetDatabase.Refresh();
            Close();
            Debug.Log("--------- 打包完毕 ---------");
        }

        [MenuItem("编辑器小工具//AssetBundle/清理(废弃)", false, 100001)]
        private static void CleanAssetBundle()
        {
            string[] assetBundles = AssetDatabase.GetAllAssetBundleNames();
            int count = assetBundles.Length;
            string[] oldAssetBundles = new string[count];
            for (int i = 0; i < count; i++)
            {
                oldAssetBundles[i] = AssetDatabase.GetAllAssetBundleNames()[i];
                EditorUtility.DisplayProgressBar("清理AssetBundle", oldAssetBundles[i], (float)i / (float)count);
            }
            for (int i = 0; i < oldAssetBundles.Length; i++)
            {
                AssetDatabase.RemoveAssetBundleName(oldAssetBundles[i], true);
                EditorUtility.DisplayProgressBar("清理AssetBundle", oldAssetBundles[i], (float)i / (float)count);
            }
            EditorUtility.ClearProgressBar();
        }

        #region 私有方法
        private static void BuildModule(string module, string child)
        {
            List<string> fileList = null;
            string path = string.Format("{0}/{1}", module, child);
            fileList = Util.GetChildFilesByRecursive(path);
            if (child == AB_Lua)
            {
                BuildLua(fileList);
            }
            else
            {
                SetAssetBundleName(fileList, module, path);
            }
        }
        static void BuildLua(List<string> fileList)
        {
            if (fileList == null) return;
            for (int i = 0; i < fileList.Count; i++)
            {
                string file = fileList[i];
                file = file.Replace("\\", "/");
                string outPath = file.Replace(ModuleRoot, TargetRoot);
                string dir = Path.GetDirectoryName(outPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                outPath = outPath.Replace(".lua", ".txt");
                outPath = outPath.Replace(".Lua", ".txt");
                File.Copy(file, outPath, true);
                EditorUtility.DisplayProgressBar("复制Lua文件", file, (float)i / (float)(fileList.Count - 1));
            }
        }
        static void SetAssetBundleName(List<string> list, string module, string rootPath)
        {
            if (list == null) return;
            for (int i = 0; i < list.Count; i++)
            {
                string file = list[i];
                file = file.Replace("\\", "/");
                rootPath = rootPath.Replace("\\", "/");
                string name = GetParentDirectoryName(file.Replace(rootPath, string.Empty));
                DirectoryInfo dirInfo = new DirectoryInfo(file);
                string filePath = dirInfo.FullName.Replace("\\", "/");
                filePath = filePath.Replace(Application.dataPath, "Assets");
                AssetImporter ai = AssetImporter.GetAtPath(filePath);
                if (ai != null)
                {
                    if (name != null)
                    {
                        ai.assetBundleName = string.Format("{0}/{1}", Path.GetFileName(module), name);
                    }
                    else
                    {
                        ai.assetBundleName = string.Format("{0}/{1}", Path.GetFileName(module), Path.GetFileNameWithoutExtension(file));
                    }
                }
                EditorUtility.DisplayProgressBar("标记AssetBundle名字", file, (float)i / (float)(list.Count - 1));
            }
        }
        static string GetParentDirectoryName(string path)
        {
            string dir = Path.GetFileName(Path.GetDirectoryName(path));
            if (string.IsNullOrEmpty(dir)) return null;
            if (dir.IndexOf('_') != 0)
            {
                path = path.Replace("/" + Path.GetFileName(path), string.Empty);
                if (Path.GetDirectoryName(path) == "/") return null;
                return GetParentDirectoryName(path);
            }
            return dir;
        }
        static void BuildHandler()
        {
            if (!Directory.Exists(TargetRoot))
                Directory.CreateDirectory(TargetRoot);
            BuildPipeline.BuildAssetBundles(TargetRoot,
                BuildAssetBundleOptions.ChunkBasedCompression,
                buildTarget());
        }
        static BuildTarget buildTarget()
        {

#if UNITY_IOS
       return BuildTarget.iOS;
#endif

#if UNITY_ANDROID
        return BuildTarget.Android;
#endif

#if UNITY_EDITOR
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                return BuildTarget.StandaloneWindows64;
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                return BuildTarget.StandaloneOSXIntel64;
            }
#endif
            return BuildTarget.StandaloneWindows64;
        }
        static void CreateMd5File(string module)
        {
            try
            {
                module = module.Replace("\\", "/");
                module = module.Replace(ModuleRoot, TargetRoot);
                List<string> list = Util.GetChildFilesByRecursive(module);
                if (list == null) return;

                string outPath = string.Format("{0}/{1}", module, "md5file.txt");
                FileStream fs = new FileStream(outPath, FileMode.CreateNew);
                StreamWriter sw = new StreamWriter(fs);
                for (int i = 0; i < list.Count; i++)
                {
                    string file = list[i];
                    if (file.EndsWith(".manifest")) continue;
                    string md5 = Util.Md5File(file);
                    string value = file.Replace("\\", "/");
                    value = value.Replace(TargetRoot + "/", string.Empty);
                    sw.WriteLine(value + "|" + md5);
                    EditorUtility.DisplayProgressBar("Building AssetBundle", value + "|" + md5, (float)i / (float)(list.Count - 1));
                }
                fs.Flush();
                sw.Close();
                fs.Close();
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
                Close();
            }

        }
        static void CreateVersionFile(string module)
        {
            try
            {
                module = module.Replace("\\", "/");
                string version = string.Format("{0}/{1}", module, "version.txt");
                if (!File.Exists(version)) return;
                string target = version.Replace(ModuleRoot, TargetRoot);
                File.Copy(version, target, true);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
                Close();
            }
        }
        static void CreateAppVersionFile()
        {
            try
            {
                Version version = new Version();
                version.version = BuildConfig.version;
                version.root_module = BuildConfig.root_module;
                version.app_download_url = BuildConfig.app_download_url;
                version.res_download_url = BuildConfig.res_download_url;
                version.download_fail_retry = BuildConfig.download_fail_retry;
                version.preTamperLua = BuildConfig.preTamperLua;
                string json = JsonUtility.ToJson(version);
                string target = string.Format("{0}/{1}", TargetRoot, "version.txt");
                if (File.Exists(target)) File.Delete(target);
                FileStream fs = new FileStream(target, FileMode.CreateNew);
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(json);
                fs.Flush();
                sw.Close();
                fs.Close();
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
                Close();
            }

        }
        static void Close()
        {
            EditorUtility.ClearProgressBar();
        }
        #endregion

    }
}
