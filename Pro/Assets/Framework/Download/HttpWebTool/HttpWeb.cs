using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;

namespace TD3_Framework
{

    public struct SWebDownloadParams
    {
        public string download_url;
        public string localPath_url;
        public int timeout;
        public Action<SDownloadFileResult> OnProgress;
        public Action<int> OnComplete;
        public Action<bool, int, int> OnSizeComplete;
    }
    public struct SWebDownloadEvent
    {
        public DownloadEventType EventType;
        public object[] objs;

        public SWebDownloadEvent(DownloadEventType type, params object[] objs)
        {
            EventType = type;
            this.objs = objs;
        }
    }

    public class HttpWeb : MonoBehaviour, IWebDownload
    {
        #region 字段和属性
        private bool isStop;
        private HttpWebRequest _request;
        private HttpWebResponse _response;
        private SDownloadFileResult _fileResult;
        private HttpDownloadFileHandler _handler;
        private SWebDownloadParams _currParams;
        private Queue<SWebDownloadEvent> _EventQueue = new Queue<SWebDownloadEvent>();
        #endregion

        #region Unity生命周期
        void Update()
        {
            if (default(SDownloadFileResult).Equals(_fileResult) == false)
            {
                if (_currParams.OnProgress != null)
                {
                    _currParams.OnProgress(_fileResult);
                }
            }

            if (_EventQueue.Count > 0)
            {
                SWebDownloadEvent e = _EventQueue.Dequeue();
                switch (e.EventType)
                {
                    case DownloadEventType.OneComplete:
                        _currParams.OnComplete((int)e.objs[0]);
                        break;
                    case DownloadEventType.SizeComplete:
                        _currParams.OnSizeComplete((bool)e.objs[0], (int)e.objs[1], (int)e.objs[2]);
                        break;
                }
            }
        }
        void OnDestroy()
        {
            isStop = true;
            if (_response != null)
                _response.Close();
            if (_handler != null)
                _handler.Dispose();
            if (_request != null)
                _request.Abort();

        }
        #endregion

        #region 方法
        private void DownloadFileSizeCallback(object obj)
        {
            _request = WebRequest.Create(_currParams.download_url) as HttpWebRequest;
            _request.Timeout = _currParams.timeout;
            _request.Proxy = WebRequest.DefaultWebProxy;
            try
            {
                _response = _request.GetResponse() as HttpWebResponse;
                _EventQueue.Enqueue(new SWebDownloadEvent(DownloadEventType.SizeComplete, true, _response.StatusCode, (int)_response.ContentLength));
            }
            catch (WebException e)
            {
                Debug.LogError(e.Message);
                int code = 0;
                _response = e.Response as HttpWebResponse;
                if (_response != null)
                {
                    code = (int)_response.StatusCode;
                }
                _EventQueue.Enqueue(new SWebDownloadEvent(DownloadEventType.SizeComplete, false, code, 0));
            }
            finally
            {
                if (_response != null)
                {
                    _response.Close();
                }
                _request.Abort();
            }
        }
        private void DownloadFileCallback(object obj)
        {
            _handler = new HttpDownloadFileHandler(_currParams.download_url, _currParams.localPath_url);
            _handler.Progress = result => _fileResult = result;
            _request = WebRequest.Create(_currParams.download_url) as HttpWebRequest;
            _request.Timeout = _currParams.timeout;
            _request.AddRange(_handler.DownloadedLength);
            _request.Proxy = WebRequest.DefaultWebProxy;
            try
            {
                _response = _request.GetResponse() as HttpWebResponse;
                _handler.ReceiveContentLength((int)_response.ContentLength);

                //byte[] buffer = new byte[1024*200];
                byte[] buffer = new byte[1024];
                using (Stream stream = _response.GetResponseStream())
                {
                    int dataLength = stream.Read(buffer, 0, buffer.Length);
                    while (dataLength > 0)
                    {
                        if (isStop || _handler.ReceiveData(buffer, dataLength) == false) return;
                        dataLength = stream.Read(buffer, 0, buffer.Length);
                    }
                }
                _handler.Dispose();
                _EventQueue.Enqueue(new SWebDownloadEvent(DownloadEventType.OneComplete, (int)_response.StatusCode));
            }
            catch (WebException e)
            {
                //Debug.LogError(e.Message);
                int code = 0;
                _response = e.Response as HttpWebResponse;
                if (_response != null)
                {
                    code = (int)_response.StatusCode;
                }
                _handler.Dispose();
                _EventQueue.Enqueue(new SWebDownloadEvent(DownloadEventType.OneComplete, code));
            }
            finally
            {
                if (_response != null)
                {
                    _response.Close();
                }
                _request.Abort();
                _fileResult = new SDownloadFileResult();
            }
        }
        #endregion

        #region 接口实现
        public void Close()
        {
            isStop = true;
        }
        public void DownloadFile(string download_url, string localPath_url, int timeout, Action<SDownloadFileResult> progress, Action<int> complete)
        {
            _EventQueue.Clear();
            _currParams = new SWebDownloadParams();
            _currParams.download_url = download_url;
            _currParams.localPath_url = localPath_url;
            _currParams.timeout = timeout;
            _currParams.OnProgress = progress;
            _currParams.OnComplete = complete;
            _fileResult = new SDownloadFileResult();
            ThreadPool.SetMaxThreads(5, 5);
            ThreadPool.QueueUserWorkItem(new WaitCallback(DownloadFileCallback), null);
        }
        public void DownloadFileSize(string url, int timeout, Action<bool, int, int> complete)
        {
            _EventQueue.Clear();
            _currParams = new SWebDownloadParams();
            _currParams.download_url = url;
            _currParams.timeout = timeout;
            _currParams.OnSizeComplete = complete;
            ThreadPool.SetMaxThreads(1, 1);
            ThreadPool.QueueUserWorkItem(new WaitCallback(DownloadFileSizeCallback), null);
        }
        #endregion

    }
}

