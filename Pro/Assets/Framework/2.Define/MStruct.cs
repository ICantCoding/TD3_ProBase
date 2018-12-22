using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD3_Framework
{


    //下载进度 回调数据
    public struct SDownloadFileResult
    {
        /// 文件总大小
        public int contentLength;
        /// 已下载的大小
        public int downloadedLength;
        /// 文件总大小字符串 （人类可读的）
        public string contentLengthStr;
        /// 已下载的大小字符串 （人类可读的）
        public string downloadedLengthStr;
    }

    //需要下载的模块数据信息
    public struct SDownloadModuleConfig
    {
        public string key;
        public string download_url;
        public string localPath_url;
        //下载失败时重试次数
        //如果返回404 则不会重试
        public int download_fail_retry;
    }

    /// <summary>
    /// 下载模块事件返回结果
    /// </summary>
    public struct SDownloadEventResult
    {
        public SDownloadModuleConfig Info;
        public string Error;
        public SDownloadFileResult FileResult;
        public DownloadEventType EventType;

        public SDownloadEventResult(DownloadEventType eventType)
            : this(eventType, new SDownloadFileResult(), new SDownloadModuleConfig(), string.Empty)
        { }

        public SDownloadEventResult(DownloadEventType eventType, SDownloadFileResult fileResult)
            : this(eventType, fileResult, new SDownloadModuleConfig(), string.Empty)
        { }

        public SDownloadEventResult(DownloadEventType eventType, string error)
            : this(eventType, new SDownloadFileResult(), new SDownloadModuleConfig(), error)
        { }

        public SDownloadEventResult(DownloadEventType eventType, SDownloadModuleConfig info)
            : this(eventType, new SDownloadFileResult(), info, string.Empty)
        { }
        public SDownloadEventResult(DownloadEventType eventType, SDownloadFileResult fileResult, SDownloadModuleConfig info, string error)
        {
            Error = error;
            Info = info;
            FileResult = fileResult;
            EventType = eventType;
        }
    }
}
