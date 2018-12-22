using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD3_Framework
{
    public delegate void DVoid();
    public delegate void DInt(int i);
    public delegate void DString(string s);
    public delegate void DString_Int(string s, int i);
    public delegate void DString_Float(string s, float f);
    public delegate void DString_Int_Int(string s, int i1, int i2);
    public delegate void DString_Int_String(string s1, int i, string s2);
    public delegate void DString_Float_Float(string s, float f1, float f2);
    public delegate void DString_SDownloadFileResult(string s, SDownloadFileResult f1);

    
    //检查更新回调
    public class CheckUpdateTable
    {
        public DString Error;
        public DString_Int_String Complete;
    }
    //下载回调
    public class DownloadTable
    {
        public DString_Int Befor;
        public DString_SDownloadFileResult Progress;
        public DString_Int_Int OneComplete;
        public DString AllComplete;
        public DString Error;
    }
    //跳转场景回调
    public class SceneTable
    {
        public DString Error;
        public DString Complete;
        public DString_Float Progress;
    }

}
