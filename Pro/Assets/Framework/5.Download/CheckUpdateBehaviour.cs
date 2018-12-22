using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD3_Framework
{
    public class CheckUpdateBehaviour : MonoBehaviour
    {
        private const int TIMEOUT = 10000;//10秒
        private long _size;
        private bool _isGetSize;
        private IWebDownload _webDownload;
        private Action<Md5File, long> _complete;
        private Queue<SDownloadModuleConfig> _tempQueue;

        public void CheckUpdate(string module, bool isGetSize, Action<Md5File, long> complete)
        {
            _isGetSize = isGetSize;
            _complete = complete;

            new DownloadMd5File(module, DownloadMd5FileComplete);

            GameObject obj = new GameObject("_WebDownload");
            obj.transform.SetParent(transform);
            _webDownload = obj.AddComponent<HttpWeb>();
        }

        private void DownloadMd5FileComplete(Md5File md5File)
        {
            _tempQueue = new Queue<SDownloadModuleConfig>(md5File.DownloadQueue);
            if (_tempQueue == null || !_isGetSize)
            {
                if (_complete != null) _complete(md5File, 0);
                return;
            }
            GetNext(md5File);
        }

        private void GetNext(Md5File md5File)
        {
            if (_tempQueue.Count == 0)
            {
                if (_complete != null)
                {
                    _complete(md5File, _size);
                }
                return;
            }
            SDownloadModuleConfig download = _tempQueue.Dequeue();
            _webDownload.DownloadFileSize(download.download_url, TIMEOUT, (bool ok, int code, int size) =>
            {
                if (ok)
                {
                    _size += size;
                }
                //Debug.Log(download.download_url + "  ok:" + ok + " code:" + code + " size:" + size+"         "+ _downloadQueue.Count);
                GetNext(md5File);
            });
        }
    }
}
