using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD3_Framework
{
    public class DownloadMd5File : MonoBehaviour
    {
        private int _failRetryCount;
        private float _failRetryDelay;
        private string _module;
        private Action<Md5File> _onCompleted;
        public DownloadMd5File(string module, Action<Md5File> callback)
        {
            _module = module;
            _onCompleted = callback;

            _failRetryCount = GameConfig.download_Fail_Count;
            _failRetryDelay = GameConfig.download_Fail_Retry_Delay;
            StartDownload(0);
        }

        private void StartDownload(float delay)
        {
            string fullModule = _module;
            if (_module != GameConfig.module_name)
            {
                fullModule = string.Format("{0}/{1}", GameConfig.module_name, _module);
            }

            string url = string.Format("{0}/{1}/{2}", VersionHelp.version.res_download_url, fullModule, GameConfig.md5_name);

            WWWMgr.Instance.Download(url, DownloadCompleted, delay);
        }
        private void DownloadCompleted(WWW www)
        {
            if (www == null)
            {
                if (_failRetryCount <= 0)
                {
                    _onCompleted(null);
                    return;
                }
                _failRetryCount--;
                StartDownload(_failRetryDelay);
                return;
            }
            Md5File md5File = new Md5File(_module, www.text);
            if (_onCompleted != null)
            {
                _onCompleted(md5File);
            }
        }
    }
}
