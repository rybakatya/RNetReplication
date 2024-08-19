using Newtonsoft.Json;
using RapidNet.Replication.Prefabs.Serialization;
using RapidNet.ToolsForUnity.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace RapidNet.Replication.Editor
{

    [System.Serializable]
    internal class RNetEntityScriptable : ScriptableObject
    {
        [ReadOnly]
        public ushort key;

        public string entityName;

        public List<RNetEntityEditorData> entityData;

        [JsonIgnore]
        [HideInInspector]
        public bool isOpened;

        [JsonIgnore]
        [HideInInspector]
        public UnityEditor.Editor editor;

    }

    

    [ExtensionWindowTab(name = "Replication", order = 1)]
    internal class EntityCreationWindowTab : ExtensionWindowTab
    {
        private Dictionary<ushort, RNetEntityScriptable> _entities = new Dictionary<ushort, RNetEntityScriptable>();
        private IDGenerator idGen = new IDGenerator(ushort.MaxValue);
        public EntityCreationWindowTab(RNetWindow window) : base(window)
        {
        }

        public override void OnEnable()
        {
            string[] guids = AssetDatabase.FindAssets("t:RNetEntityScriptable");
            
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                RNetEntityScriptable a = AssetDatabase.LoadAssetAtPath<RNetEntityScriptable>(path);
                idGen.Take(a.key);
                a.editor = UnityEditor.Editor.CreateEditor(a);
                _entities.Add(a.key, a);
            }
        }

        string entityName = string.Empty;
        public override void OnGUI()
        {
            GUILayout.BeginVertical();

            foreach (var entity in _entities)
            {
                entity.Value.isOpened = EditorGUILayout.Foldout(entity.Value.isOpened, entity.Value.entityName);
                if (entity.Value.isOpened)
                {
                    entity.Value.editor.OnInspectorGUI();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if(GUILayout.Button("Save"))
                    {
                        SaveEntity(entity.Value);
                        GUILayout.EndHorizontal();
                        break;
                    }

                    if(GUILayout.Button("Delete"))
                    {
                        var asset = AssetDatabase.GetAssetPath(entity.Value);
                        AssetDatabase.DeleteAsset(asset);
                        AssetDatabase.Refresh();
                        _entities.Remove(entity.Key);
                        GUILayout.EndHorizontal();
                        break;
                      
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label("Name:");
            entityName = GUILayout.TextField(entityName, GUILayout.MinWidth(100));
            if(GUILayout.Button("Create Entity"))
            {
                CreateEntity();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void SaveEntity(RNetEntityScriptable entity)
        {
            foreach (var data in entity.entityData)
            {
                if (data.relationship == RNetRelationship.Creator)
                {
                    
                    var rootData = new RNetPrefabRootServerData()
                    {
                        key = entity.key,
                        poolSize = data.poolSize,
                        bundlePath = "server/entities/" + entity.entityName + "/" + entity.entityName + "_" + data.relationship.ToString().ToLower()
                    };
                    var assetPath = AssetDatabase.GetAssetPath(data.prefab);
                    AssetDatabase.RenameAsset(assetPath, entity.entityName + "_" + data.relationship.ToString().ToLower());
                    var json = JsonConvert.SerializeObject(rootData, Formatting.Indented);
                    var path = Application.streamingAssetsPath + "/server/entities/" + entity.entityName + "/" + entity.entityName + ".json";

                    if (Directory.Exists(Path.GetDirectoryName(path)) == false)
                        Directory.CreateDirectory(Path.GetDirectoryName(path));

                    File.WriteAllText(path, json);
                    

                }
            }



            var e = new RNetPrefabRootClientData()
            {
                data = new List<RNetEntityData>(),
                key = entity.key,
            };
            foreach (var edata in entity.entityData)
            {
                if (edata.relationship != RNetRelationship.Creator)
                {
                    
                    e.data.Add(new RNetEntityData()
                    {
                        bundleName = "client/entities/" + entity.entityName + "/" + entity.entityName + "_" + edata.relationship.ToString().ToLower(),
                        poolSize = edata.poolSize,
                        relationship = edata.relationship
                    });
                    var assetPath = AssetDatabase.GetAssetPath(edata.prefab);
                    AssetDatabase.RenameAsset(assetPath, entity.entityName + "_" + edata.relationship.ToString().ToLower());
                }
            }



            var j = JsonConvert.SerializeObject(e, Formatting.Indented);
            var p = Application.streamingAssetsPath + "/client/entities/" + entity.entityName + "/" + entity.entityName + ".json";

            if (Directory.Exists(Path.GetDirectoryName(p)) == false)
                Directory.CreateDirectory(Path.GetDirectoryName(p));



            File.WriteAllText(p, j);
            

            List<AssetBundleBuild> build = new List<AssetBundleBuild>();
            foreach (var d in entity.entityData)
            {
                var assetPath = AssetDatabase.GetAssetPath(d.prefab);
                var importer = UnityEditor.AssetImporter.GetAtPath(assetPath);
                var pre = (d.relationship == RNetRelationship.Creator) ? "server/entities/" : "client/entities/";
                importer.assetBundleName = pre + entity.entityName+ "/" + entity.entityName + "_" + d.relationship.ToString().ToLower();


                AssetBundleBuild buildMap = new AssetBundleBuild();
                buildMap.assetBundleName = importer.assetBundleName;

                // Find all asset paths assigned to this AssetBundle
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(importer.assetBundleName);
                buildMap.assetNames = assetPaths;
                build.Add(buildMap);
            }

            BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, build.ToArray(), BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

            AssetDatabase.Refresh();
        }

        private void CreateEntity()
        {
            var nextKey = idGen.Rent();

            var entity = ScriptableObject.CreateInstance<RNetEntityScriptable>();
            entity.editor = UnityEditor.Editor.CreateEditor(entity);
            entity.entityName = entityName;
            entity.key = nextKey;
            entity.name = entityName;

            var path = "Assets/Editor/Resources/Entities/" + entityName + ".asset";

            if (Directory.Exists(Path.GetDirectoryName(path)) == false)
                Directory.CreateDirectory(Path.GetDirectoryName(path));

            string name = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CreateAsset(entity, name);
            AssetDatabase.SaveAssets();
            entityName = string.Empty;
            _entities.Add(nextKey, entity);
        }

        public override void OnDestroy()
        {
            idGen.Reset();
            _entities.Clear();
        }
    }
}