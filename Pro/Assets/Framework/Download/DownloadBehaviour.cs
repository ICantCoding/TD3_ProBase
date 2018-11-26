using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TD3_Framework
{
    public class DownloadBehaviour : MonoBehaviour
    {

        #region 字段和属性
        public Action<SDownloadEventResult> Error;
        public Action<SDownloadEventResult> Progress;
        public Action<SDownloadEventResult> OneComplete;
        public Action<SDownloadEventResult> AllComplete;

        private string _module;
        private Md5File _md5File;
        private const int TIMEOUT = 10000;//10秒

        private IWebDownload _webDownload;
        private SDownloadModuleConfig _currentConfig;
        private List<SDownloadModuleConfig> _failList = new List<SDownloadModuleConfig>();
        #endregion

        #region Unity生命周期
        void Awake()
        {
            GameObject obj = new GameObject("_WebDownload");
            obj.transform.SetParent(transform);
            //使用 HttpWebRequest
            _webDownload = obj.AddComponent<HttpWeb>();

            //使用 UnityWebRequest
            //_webDownload = obj.AddComponent<UnityWeb>();
        }
        void OnDestroy()
        {
            _webDownload.Close();
            if (_md5File != null)
            {
                _md5File.Destroy();
            }
            if (!default(SDownloadModuleConfig).Equals(_currentConfig))
            {
                Debug.Log(_currentConfig.key + "没下载完 ");
                List<SDownloadModuleConfig> list = new List<SDownloadModuleConfig>(_md5File.DownloadQueue.ToArray());
                list.Insert(0, _currentConfig);
                _md5File.DownloadQueue = new Queue<SDownloadModuleConfig>(list);
            }
        }
        #endregion

        #region 方法
        public void Download(string module, Md5File md5File)
        {
            _md5File = md5File;
            _module = module;
            _failList.Clear();
            DownloadNext();
        }
        private void DownloadNext()
        {
            if (_md5File.DownloadQueue.Count == 0)
            {
                if (_failList.Count > 0)
                {
                    Error(new SDownloadEventResult(DownloadEventType.Error, "下载失败"));
                    for (int i = 0; i < _failList.Count; i++)
                    {
                        Debug.LogError("失败的文件：" + _failList[i].download_url);
                    }
                }
                else
                {
                    AllComplete(new SDownloadEventResult(DownloadEventType.AllComplete));
                }
                return;
            }

            _currentConfig = _md5File.DownloadQueue.Dequeue();
            //将这个文件记录到下载中
            _md5File.PushTmpFile(_currentConfig.key);

            _webDownload.DownloadFile(_currentConfig.download_url, _currentConfig.localPath_url, TIMEOUT, OnProgress, OnComplete);

        }
        private void OnProgress(SDownloadFileResult result)
        {
            Progress(new SDownloadEventResult(DownloadEventType.Progress, result));
        }
        private void OnComplete(int code)
        {

            if (code == 206)
            {
                DownloadSuccess();
            }
            else if (code == 416)
            {
                ///在使用暂停下载的时候有几率会出现此问题
                ///因为是线程下载，在下载完成的瞬间暂停后 会把当前文件重新加入下载队列导致重复下载， 实际上暂停之后的同时或者下一帧这个文件已经下载完毕
                /// 所以临时文件 aaa.mp4.tmp 和 远程 aaa.mp4 的大小一样 只不过没有被移动 在续传的时候会返回一次416
                /// 所以这里判断如果临时文件的md5和远程md5相等 直接判定下载成功 不走下面下载失败流程 否则会删除重新下载
                /// 这里跳过了manifest文件 因为这个文件没有对应的md5字符串，也没有必要做对比，一般都是1k大小 重新下载没毛病
                if (_currentConfig.key.EndsWith(".manifest") == false)
                {
                    string fileMd5 = Util.Md5File(_currentConfig.localPath_url + ".tmp");
                    string remoteMd5 = _md5File.GetRemoteMd5(_currentConfig.key);
                    if (fileMd5.Trim() == remoteMd5.Trim())
                    {
                        Debug.LogWarning(_currentConfig.key + " 返回了416 但是文件已经下载完毕 不需要重新下载" + fileMd5 + "==" + remoteMd5);
                        DownloadSuccess();
                    }
                    else
                    {
                        Debug.LogWarning(_currentConfig.key + " 返回了416 文件不一样 重新下载" + fileMd5 + "!=" + remoteMd5);
                        DownloadFail(code);
                    }
                }
            }
            else
            {
                DownloadFail(code);
            }

            _currentConfig = new SDownloadModuleConfig();
            DownloadNext();
            //if (_currentConfig.key != "Modules" && _currentConfig.key != "Modules.manifest")
            //{
            //    Destroy(gameObject);
            //}
        }
        //下载成功时
        private void DownloadSuccess()
        {
            string tmp = _currentConfig.localPath_url + ".tmp";
            if (File.Exists(tmp))
            {
                if (File.Exists(_currentConfig.localPath_url))
                    File.Delete(_currentConfig.localPath_url);
                File.Move(tmp, _currentConfig.localPath_url);
            }
            //将这个文件从下载中移除
            _md5File.PopTmpFile(_currentConfig.key);
            //下载完成 更新md5
            _md5File.UpdateLocalMd5File(_currentConfig.key);
            OneComplete(new SDownloadEventResult(DownloadEventType.OneComplete, _currentConfig));
        }
        //下载失败时
        private void DownloadFail(int code)
        {
            string tmp = _currentConfig.localPath_url + ".tmp";
            //code=0 超时 不需要删除 重试后会续传
            if (code != 0 && File.Exists(tmp))
            {
                File.Delete(tmp);
            }
            if (code != 404 && _currentConfig.download_fail_retry > 0)
            {
                _currentConfig.download_fail_retry--;
                //重新加入下载队列中
                _md5File.DownloadQueue.Enqueue(_currentConfig);
            }
            else
            {
                //已经无能为力
                Debug.LogError(_currentConfig.download_url + " 下载失败 code:" + code);
                _failList.Add(_currentConfig);
            }
        }
        #endregion
    }
}
