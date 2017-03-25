using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;

namespace GPlay
{
    public class ResourceConfigUtility
    {
        private static string PROJECT_PATH = Path.GetDirectoryName(Application.dataPath) + "/";
        private static float sProgress = 0;

        internal static ResConfig Generate(List<string> scenePaths)
        {
            sProgress = 0;
            ResConfig resConfig = new ResConfig();

            #region 提取所有Resources下的文件
            SetProgress("收集Resources信息...", sProgress);
            var resItemQuery = from assetPath in Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
                               where (!assetPath.EndsWith(".meta")) && IsResourceFile(assetPath)
                               select new AssetItem(EAssetType.Resources,
                                                    AssetDatabase.AssetPathToGUID(assetPath.Replace('\\', '/').Replace(PROJECT_PATH, "")),
                                                    assetPath.Replace('\\', '/').Replace(PROJECT_PATH, ""));
            foreach (AssetItem resItem in resItemQuery)
            {
                if(!IsContainsItemGUID(resConfig.lstAssetItems, resItem.guid))
                    resConfig.lstAssetItems.Add(resItem);
            }
            SetProgress("收集Resources信息...", sProgress + 0.05f);
            #endregion

            #region 提取所有StreamingAssets下的文件
            SetProgress("收集StreamingAssets信息...", sProgress);
            var streamingAssetItemQuery = from assetPath in Directory.GetFiles(Application.streamingAssetsPath, "*.*", SearchOption.AllDirectories)
                               where (!assetPath.EndsWith(".meta"))
                               select new AssetItem(EAssetType.Streaming,
                                                    AssetDatabase.AssetPathToGUID(assetPath.Replace('\\', '/').Replace(PROJECT_PATH, "")),
                                                    assetPath.Replace('\\', '/').Replace(PROJECT_PATH, ""));
            
            foreach (AssetItem streamingAsset in streamingAssetItemQuery)
            {
                if (!IsContainsItemGUID(resConfig.lstAssetItems, streamingAsset.guid))
                    resConfig.lstAssetItems.Add(streamingAsset);
            }
            SetProgress("收集StreamingAssets信息...", sProgress + 0.05f);
            #endregion

            #region 提取场景文件中的guid
            SetProgress("收集场景引用信息...", 0.1f);
            for (int sceneID = 0; sceneID < scenePaths.Count; ++sceneID)
            {
                SceneResCfg sceneResCfg = new SceneResCfg()
                {
                    sceneId = sceneID,
                    sceneName = scenePaths[sceneID]
                };

                foreach(AssetItem assetItem in ExtractAssetItemFromFile(PROJECT_PATH + scenePaths[sceneID]))
                {
                    if(!IsContainsItemGUID(resConfig.lstAssetItems, assetItem.guid))
                    {
                        resConfig.lstAssetItems.Add(assetItem);
                    }
                    if (!sceneResCfg.lstReferencedResGUID.Contains(assetItem.guid))
                        sceneResCfg.lstReferencedResGUID.Add(assetItem.guid);
                }
                
                resConfig.lstSceneResCfg.Add(sceneResCfg);

                SetProgress("收集场景引用信息...", sProgress + 0.2f / scenePaths.Count);
            }
            #endregion

            #region 提取资源引用关系
            ExtractReferenceInfo(resConfig, resConfig.lstAssetItems);
            //ExtractReferenceInfo(resConfig, resConfig.lstResourcesAssetItems);
            //ExtractReferenceInfo(resConfig, resConfig.lstExternalAssetItems);
            #endregion

            EditorUtility.ClearProgressBar();
            return resConfig;
        }

        /// <summary>
        /// 通过guid 得到 AssetItem 实例
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private static AssetItem GetAssetItemFromGUID(EAssetType type, string guid)
        {
            AssetItem assetItem = null;
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Object assetObj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (null != assetObj && assetObj.GetType() != typeof(DefaultAsset) && assetObj.GetType() != typeof(MonoScript))
            {
                assetItem = new AssetItem(type, guid, assetPath.Replace(PROJECT_PATH, ""));
            }
            return assetItem;
        }

        /// <summary>
        /// 从文件中提取所有 guid
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static IEnumerable ExtractAssetItemFromFile(string filePath)
        {
            Regex guidRegex = new Regex("(?<=guid: )[a-z0-9]{32}");

            string fileContent = File.ReadAllText(filePath);
            MatchCollection matchCollection = guidRegex.Matches(fileContent);
            foreach (Match match in matchCollection)
            {
                string guid = match.Groups[0].Value;
                AssetItem assetItem = GetAssetItemFromGUID(EAssetType.Reference, guid);
                if (assetItem == null)
                    continue;

                yield return assetItem;
            }
        }

        /// <summary>
        /// 是否是Resources文件夹下的文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static bool IsResourceFile(string filePath)
        {
            string tmp = filePath.Remove(0, Application.dataPath.Length).Replace('\\', '/');
            if (tmp.Contains("/Resources/"))
                return true;
            return false;
        }

        /// <summary>
        /// AssetItem 列表中是否含有 某个 guid 的AssetItem
        /// </summary>
        /// <param name="lstAssetItem"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        private static bool IsContainsItemGUID(List<AssetItem> lstAssetItem, string guid)
        {
            for (int i = 0; i < lstAssetItem.Count; ++i)
                if (lstAssetItem[i].guid == guid)
                    return true;
            return false;
        }

        /// <summary>
        /// 提取某个Asset 的引用信息
        /// </summary>
        /// <param name="assetItem"></param>
        /// <returns></returns>
        private static AssetReferenceInfo GetAssetReferenceInfo(AssetItem assetItem)
        {
            var referenceInfo = new AssetReferenceInfo() { guid = assetItem.guid };

            string solutePath = PROJECT_PATH + assetItem.path;
            using (StreamReader reader = new StreamReader(solutePath))
            {
                string firstLine = reader.ReadLine();
                if (firstLine.Contains("YAML"))
                {
                    foreach (AssetItem referenceItem in ExtractAssetItemFromFile(solutePath))
                    {
                        if (!referenceInfo.lstReferences.Contains(referenceItem.guid))
                            referenceInfo.lstReferences.Add(referenceItem.guid);
                    }
                }

                string metaFilePath = AssetDatabase.GetTextMetaFilePathFromAssetPath(assetItem.path);
                foreach (AssetItem referenceItem in ExtractAssetItemFromFile(metaFilePath))
                {
                    if (!referenceInfo.lstReferences.Contains(referenceItem.guid))
                        referenceInfo.lstReferences.Add(referenceItem.guid);
                }
            }
            return referenceInfo;
        }

        private static void ExtractReferenceInfo(ResConfig resConfig, List<AssetItem> lstAssetItem)
        {
            SetProgress("收集文件引用信息...", 0.3f);
            for (int i = 0; i < lstAssetItem.Count; ++i)
            {
                AssetItem assetItem = lstAssetItem[i];
                AssetReferenceInfo referenceInfo = GetAssetReferenceInfo(assetItem);
                if (referenceInfo.lstReferences.Count > 0)
                {
                    resConfig.lstAssetReferenceInfos.Add(referenceInfo);
                    foreach (string referenceAssetGUID in referenceInfo.lstReferences)
                    {
                        //if (IsContainsItemGUID(resConfig.lstExternalAssetItems, referenceAssetGUID))
                        if (IsContainsItemGUID(resConfig.lstAssetItems, referenceAssetGUID))
                            continue;

                        AssetItem referenceAssetItem = GetAssetItemFromGUID(EAssetType.Reference, referenceAssetGUID);
                        if (referenceAssetItem != null)
                            resConfig.lstAssetItems.Add(referenceAssetItem);
                        //resConfig.lstExternalAssetItems.Add(referenceAssetItem);
                    }
                }
                SetProgress("收集文件引用信息...", sProgress + 0.7f / lstAssetItem.Count);
            }
        }

        private static void SetProgress(string info, float progress)
        {
            sProgress = progress;
            EditorUtility.DisplayProgressBar("收集资源信息", info, sProgress);
        }
    }

    public class ResConfig
    {
        [XmlArray("AssetItemList")]
        public List<AssetItem> lstAssetItems = new List<AssetItem>();
        
        [XmlArray("SceneResourceConfigList")]
        public List<SceneResCfg> lstSceneResCfg = new List<SceneResCfg>();

        [XmlArray("AssetReferenceInfoList")]
        public List<AssetReferenceInfo> lstAssetReferenceInfos = new List<AssetReferenceInfo>();
    }

    public class SceneResCfg
    {
        [XmlAttribute("scene_id")]
        public int sceneId;

        [XmlAttribute("scene_name")]
        public string sceneName;

        [XmlArray("ReferencedResGUIDList")]
        public List<string> lstReferencedResGUID = new List<string>();
    }

    public enum EAssetType
    {
        Resources,
        Streaming,
        Reference,
    }

    public class AssetItem
    {
        [XmlAttribute] public EAssetType type;
        [XmlAttribute] public string guid;
        [XmlAttribute] public string path;
        [XmlArray("SubAssetList")] public List<SubAsset> lstSubAssets;

        public AssetItem() { }
        public AssetItem(EAssetType type, string guid, string path)
        {
            this.type = type;
            this.guid = guid;
            this.path = path;
        }

        public override string ToString()
        {
            return string.Format("{{ type: {0}, guid: {1}, name: {2}}}", type, guid, path);
        }
    }

    public class SubAsset
    {
        [XmlAttribute] public string name;
        [XmlAttribute] public string type;
    }

    public class AssetReferenceInfo
    {
        [XmlElement] public string guid;
        [XmlArray("References")] public List<string> lstReferences = new List<string>();
    }
}
