using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TD3_Framework
{
    public class VersionHelp
    {
        private static Version _version;
        public static Version version
        {
            get
            {
                if (_version == null)
                {
                    _version = GetLocalVersionForApp();
                }
                return _version;
            }
        }
        public static Version GetLocalVersionForModule(string module)
        {
            string version_path = string.Format("{0}/{1}/{2}", Util.DeviceResPath, module, "version.txt");
            if (!File.Exists(version_path)) return null;
            try
            {
                string text = File.ReadAllText(version_path);
                return StringForVersion(text);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }
        public static Version JsonForVersion(string json)
        {
            try
            {
                Version version = JsonUtility.FromJson<Version>(json);
                return version;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }
        public static Version StringForVersion(string text)
        {
            Version version = new Version();
            version.version = text.Trim();
            version.res_download_url = BuildConfig.res_download_url;
            return version;
        }
        public static Version GetLocalVersionForApp()
        {
            string version_path = string.Format("{0}/{1}", Util.DeviceResPath, "version.txt");
            if (!File.Exists(version_path)) return null;
            string text = File.ReadAllText(version_path);
            if (string.IsNullOrEmpty(text)) return null;
            return JsonForVersion(text);
        }
        public static void WriteLocalVersionFile(Version version)
        {
            string version_path = string.Format("{0}/{1}", Util.DeviceResPath, "version.txt");
            //创建根目录
            if (!Directory.Exists(Path.GetDirectoryName(version_path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(version_path));
            }
            //删除旧文件
            if (File.Exists(version_path))
            {
                File.Delete(version_path);
            }
            string json = JsonUtility.ToJson(version);
            try
            {
                File.WriteAllText(version_path, json);
                _version = version;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                throw;
            }
        }

    }
}


