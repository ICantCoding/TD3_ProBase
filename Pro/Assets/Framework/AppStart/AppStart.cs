using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD3_Framework
{
    public class AppStart : MonoBehaviour
    {

        #region 字段与属性
        private string moduleName; //启动模块, 一般是LuaFramework模块
        #endregion
        private void Awake()
        {
            DownloadVersion(); //启动应用就检查客户端版本是否需要更新
        }


        #region 私有方法
        private void DownloadVersion()
        {
            string version_url = string.Format("{0}/{1}/{2}",
                GameConfig.version_download_url, GameConfig.module_name, GameConfig.version_name);
            new DownloadVersionFile(version_url, 
                GameConfig.download_Fail_Count, 
                GameConfig.download_Fail_Retry_Delay, 
                DownloadVersionCompleted);
        }
        private void DownloadVersionCompleted(VersionResType type, Version version)
        {
            switch (type)
            {
                case VersionResType.DownloadFail:
                    Debug.Log("Version 下载失败");
                    break;
                case VersionResType.DownloadSuccess:
                    moduleName = version.root_module; //LuaFramework模块
                    //DownloadRootModule();
                    break;
                case VersionResType.Different:
                    Debug.Log("Version 版本不同");
                    break;
                case VersionResType.Unusual:
                    Debug.Log("Version 解析异常");
                    break;
            }
        }

        private void DownloadRootModule()
        {
            Module module = ModuleMgr.Instance.GetModule(moduleName);
            DownloadTable table = new DownloadTable()
            {
                Befor = DownloadBefor,
                Progress = DownloadProgress,
                OneComplete = DownloadOneComplete,
                AllComplete = DownloadAllComplete,
                Error = DownloadError
            };

        }
        #endregion

        #region 下载RootModule数据的回调实现
        private void DownloadBefor(string moduleName, int count)
        {

        }
        private void DownloadProgress(string moduleName, SDownloadFileResult result)
        {

        }
        private void DownloadOneComplete(string moduleName, int downloadedCount, int downloadTotal)
        {

        }
        private void DownloadAllComplete(string moduleName)
        {

        }
        private void DownloadError(string moduleName)
        {
            Debug.LogError(moduleName + " 下载失败 ");
        }
        #endregion
    }
}
