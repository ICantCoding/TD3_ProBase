using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD3_Framework
{
    public class WWWMgr : MonoBehaviour
    {
        #region 单例
        private static WWWMgr _instance;
        public static WWWMgr Instance
        {
            get { return Util.GetInstance(ref _instance, "_WWWMgr"); }
        }
        #endregion

        #region 公有方法
        public void Download(string url, Action<WWW> done, float delay = 0)
        {
            if (done == null) done = (WWW www) => { };
            StartCoroutine(IEDownload(url, done, delay));
        }
        #endregion

        #region 私有方法
        private IEnumerator IEDownload(string url, Action<WWW> done, float delay)
        {
            yield return new WaitForSeconds(delay);
            using (WWW www = new WWW(url))
            {
                yield return www;
                if (www.error != null)
                {
                    Debug.LogError(string.Format("{0}：{1}", url, www.error));
                    done(null);
                    yield break;
                }
                if (www.isDone)
                {
                    done(www);
                }
            }
        }
        #endregion

    }
}











