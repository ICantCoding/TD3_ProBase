using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD3_Framework
{
    public enum DownloadEventType
    {
        SizeComplete,
        Progress,
        OneComplete,
        AllComplete,
        Error,
    }
}