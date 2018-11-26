using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TD3_Framework
{
    public class Md5FileHelp
    {
        public static Dictionary<string, string> LocalFileForDict(string module, string fileName)
        {
            string path = string.Format("{0}/{1}/{2}", Util.DeviceResPath, module, fileName);
            if (!File.Exists(path)) return new Dictionary<string, string>();
            try
            {
                string text = File.ReadAllText(path);
                return ForDict(text);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                return new Dictionary<string, string>();
            }
        }

        public static Dictionary<string, string> ForDict(string text)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(text)) return dict;
            string[] lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrEmpty(line)) continue;
                string[] keyValue = line.Split('|');
                dict.Add(keyValue[0], keyValue[1]);
            }
            return dict;
        }

        public static void ForFile(Dictionary<string, string> dict, string outPath)
        {
            if (dict == null) return;
            Dictionary<string, string>.Enumerator e = dict.GetEnumerator();
            if (File.Exists(outPath)) File.Delete(outPath);
            try
            {
                FileStream fs = new FileStream(outPath, FileMode.CreateNew);
                StreamWriter sw = new StreamWriter(fs);
                while (e.MoveNext())
                {
                    sw.WriteLine(string.Format("{0}|{1}", e.Current.Key, e.Current.Value));
                }
                e.Dispose();
                fs.Flush();
                sw.Close();
                sw.Dispose();
                fs.Close();
                fs.Dispose();
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

    }
}
