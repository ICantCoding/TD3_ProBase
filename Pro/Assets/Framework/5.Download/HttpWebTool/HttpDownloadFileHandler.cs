using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD3_Framework
{
    public class HttpDownloadFileHandler
    {
        #region 字段和属性
        public Action<SDownloadFileResult> Progress;
        public int ContentLength { get; private set; }
        public int DownloadedLength { get; private set; }

        private FileStream _fileStream;
        private string _download_url;
        private string _localPath_url;
        private SDownloadFileResult _downloadFileResult;
        #endregion

        #region 构造方法
        public HttpDownloadFileHandler(string download_url, string localPath_url)
        {
            _download_url = download_url;
            _localPath_url = localPath_url;
            _downloadFileResult = new SDownloadFileResult();
            string dir = Path.GetDirectoryName(localPath_url);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            _fileStream = new FileStream(localPath_url + ".tmp", FileMode.Append, FileAccess.Write);
            DownloadedLength = (int)_fileStream.Length;
            ContentLength = 0;
            _downloadFileResult.downloadedLength = DownloadedLength;
            _downloadFileResult.downloadedLengthStr = Util.HumanReadableFilesize(DownloadedLength);
        }
        #endregion

        #region 方法
        public void ReceiveContentLength(int contentLength)
        {
            ContentLength = contentLength + DownloadedLength;
            _downloadFileResult.contentLength = ContentLength;
            _downloadFileResult.contentLengthStr = Util.HumanReadableFilesize(ContentLength);
        }
        public bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || dataLength == 0 || _fileStream == null)
            {
                return false;
            }
            _fileStream.Write(data, 0, dataLength);
            DownloadedLength += dataLength;
            _downloadFileResult.downloadedLength = DownloadedLength;
            _downloadFileResult.downloadedLengthStr = Util.HumanReadableFilesize(DownloadedLength);
            Progress(_downloadFileResult);
            return true;
        }
        public void Dispose()
        {
            if (_fileStream != null)
            {
                _fileStream.Close();
                _fileStream.Dispose();
                _fileStream = null;
            }

        }
        #endregion
    }
}


