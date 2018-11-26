using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD3_Framework
{
    public enum GameModel
    {
        //本地测试热更新
        Local,
        //本地开发模式 不更新
        Editor,
        //打包发布模式
        Remote,
    }
    //打包资源配置
    public class BuildConfig
    {
        //安装包版本
        public const string version = "1.0.1";
        //默认启动模块
        public const string root_module = "LuaFramework";
        //安装包下载地址
        public const string app_download_url = "http://192.168.0.125";
        //资源下载地址
        public const string res_download_url = "http://192.168.0.125";
        //下载失败重试次数
        public const int download_fail_retry = 3;
        //true 重新获取文件MD5对比远程MD5   false 本地md5文件对比远程MD5 (只针对lua文件)
        public const bool preTamperLua = false;
    }
    public class GameConfig
    {
        //模块根目录
        public const string module_name = "Modules";
        //版本配置文件下载地址
        public const string version_download_url = "http://192.168.0.125";
        //版本配置文件名
        public const string version_name = "version.txt";
        //md5文件名
        public const string md5_name = "md5file.txt";
        //version || md5file 下载失败重试次数
        public const int download_Fail_Count = 3;
        //version || md5file 下载失败延迟重试时间 秒
        public const int download_Fail_Retry_Delay = 2;
        //开发时使用Editor || Local  打包发布使用Remote
        public const GameModel gameModel = GameModel.Editor;
    }
}