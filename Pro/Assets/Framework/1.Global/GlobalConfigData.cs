
/*
	整个项目全局配置数据
*/

namespace TD3_Framework
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class GlobalConfigData
    {
		//AssetBundle相关配置数据
		public static string AssetBundleBuildTargetPath = Application.streamingAssetsPath;    //AB包打包生成目录
		public static string AssetBundleConfigPath =
			"Assets/Framework/AssetBundle/Config/AssetBundleConfig.asset"; //AB包打包方式配置文件
        public static string AssetBundleDependenceXmlPath = 
			Application.dataPath + "/Framework/AssetBundle/Config/AssetBundleDependenceConfig.xml"; //AB包依赖xml文件
        public static string AssetBundleDependenceBytePath = 
			Application.dataPath + "/Framework/AssetBundle/Config/AssetBundleDependenceConfig.bytes"; //AB包依赖二进制文件
		public static string DependenceFile4AssetBundle = 
			AssetBundleBuildTargetPath + "/assetbundleconfig"; //AB包依赖文件所在的AB包（依赖文件也被打包进了AB包, 并以assetbundleconfig为该AB包命名）
		public static string DependenceFileName = "AssetBundleDependenceConfig.bytes"; //依赖文件名字
	}
}