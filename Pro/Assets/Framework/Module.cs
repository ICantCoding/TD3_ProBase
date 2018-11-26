using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace TD3_Framework
{
    public class Module : MonoBehaviour
    {
        #region 字段和属性
        private long _size;
        private string _sizeStr;
        private Md5File _md5File;
        private DownloadBehaviour db;
        private SceneTable _sceneTable;// 缓存全局Table 避免每次Get的开销 
        private DownloadTable _downloadTable;// 缓存全局Table 避免每次Get的开销 
        private CheckUpdateTable _checkUpdateTable;// 缓存全局Table 避免每次Get的开销 
        private Queue<SDownloadModuleConfig> _downloadQueue;

        //模块名
        public string Name { get; private set; }
        /// 资源大小
        public long Size { get { return _size; } }
        /// 资源大小字符串
        public string SizeStr { get { return _sizeStr; } }

        #endregion

        #region Unity生命周期
        void Awake()
        {
            Name = gameObject.name;
        }
        void OnDestroy()
        {

        }
        #endregion

        #region 检查更新并下载
        public void CheckAndDownload(DownloadTable table)
        {
            CheckUpdateTable checkUpdateTable = new CheckUpdateTable()
            {
                Complete = (string moduleName, int downloadCount, string sizeStr) =>
                {
                    //2.下载模块
                    Download(table);
                },
                Error = (string moduleName) =>
                {
                    Debug.LogError("下载失败");
                }
            };
            //1.检查更新
            CheckUpdate(checkUpdateTable, false);
        }
        #endregion

        #region 检查更新
        public void CheckUpdate(CheckUpdateTable table, bool isGetSize)
        {
            //编辑器模式模拟检查完成
            if (GameConfig.gameModel == GameModel.Editor)
            {
                if (table != null && table.Complete != null)
                {
                    table.Complete(Name, 0, string.Empty);
                    return;
                }
            }
            CheckUpdateBehaviour cub = new GameObject(Name + "_CheckUpdateBehaviour").AddComponent<CheckUpdateBehaviour>();
            cub.CheckUpdate(Name, isGetSize, (Md5File md5File, long size) =>
            {
                _size = size;
                _md5File = md5File;
                _downloadQueue = md5File.DownloadQueue;
                _sizeStr = Util.HumanReadableFilesize(Convert.ToDouble(_size));
                if (table == null) return;
                if (_downloadQueue == null)
                {
                    Debug.LogError(string.Format("{0}：检查更新失败！", Name));
                    if (table.Error != null) table.Error(Name);
                    return;
                }
                Debug.Log(Name + " 需要下载 " + _sizeStr);
                if (table.Complete != null) table.Complete(Name, _downloadQueue.Count, _sizeStr);
                Destroy(cub.gameObject);
                cub = null;
            });
        }
        #endregion

        #region 下载
        public void Download(DownloadTable table)
        {
            if (GameConfig.gameModel == GameModel.Editor)
            {
                if (table != null && table.AllComplete != null) table.AllComplete(Name);
                return;
            }

            if (_downloadQueue == null || _downloadQueue.Count == 0)
            {
                if (table != null && table.AllComplete != null) table.AllComplete(Name);
                return;
            }

            int downloadedCount = 0;
            int downloadTotal = _downloadQueue.Count;
            if (db == null)
            {
                db = new GameObject(Name + "_DownloadBehaviour").AddComponent<DownloadBehaviour>();
                db.transform.SetParent(transform);
                //下载进度
                db.Progress = (SDownloadEventResult result) =>
                {
                    //Debug.Log("----" + (float)result.FileResult.downloadedLength / (float)result.FileResult.contentLength);
                    if (table != null && table.Progress != null) table.Progress(Name, result.FileResult);
                };
                db.OneComplete = (SDownloadEventResult result) =>
                {
                    downloadedCount++;
                    //下载一个完成
                    if (table != null && table.OneComplete != null)
                    {
                        table.OneComplete(Name, downloadedCount, downloadTotal);
                    }
                };
                db.AllComplete = (SDownloadEventResult e) =>
                {
                    if (table != null && table.AllComplete != null) table.AllComplete(Name);
                    Destroy(db.gameObject);
                    db = null;
                };
                //下载失败
                db.Error = (SDownloadEventResult e) =>
                {
                    if (table != null && table.Error != null) table.Error(Name);
                    Destroy(db.gameObject);
                    db = null;
                };
                if (table != null && table.Befor != null) table.Befor(Name, _downloadQueue.Count);
                db.Download(Name, _md5File);
            }
        }
        //暂停下载
        public void StopDownload()
        {
            if (db == null) return;
            Destroy(db.gameObject);
            db = null;
        }
        #endregion
    }
}
