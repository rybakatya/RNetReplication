using Newtonsoft.Json;
using RapidNet.Replication.Prefabs.Serialization;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RapidNet.Replication
{

#if SERVER
    internal partial class RNetEntityPool
    {
        public Dictionary<ushort, List<RNetEntityInstance>> entityPool;
        
        internal RNetEntityPool()
        {
            parent = new GameObject("EntityPool");
            parent.transform.position = spawnPoint;
            var files = Directory.GetFiles(Application.streamingAssetsPath + "/server/entities/", "*.json", SearchOption.AllDirectories);
            entityPool = new Dictionary<ushort, List<RNetEntityInstance>>();
            foreach (var file in files)
            {
                var json = File.ReadAllText(file);

                RNetPrefabRootServerData rnetPrefab = JsonConvert.DeserializeObject<RNetPrefabRootServerData>(json);

                entityPool.Add(rnetPrefab.key, new List<RNetEntityInstance>());

                var go = LoadAssetBundle(rnetPrefab.bundlePath);

                for (int i = 0; i < rnetPrefab.poolSize; i++)
                {
                    var inst = GameObject.Instantiate(go, parent.transform, false);

                    var component = inst.GetComponent<RNetEntityInstance>();
                    if (component == null)
                    {
                        component = inst.AddComponent<RNetEntityInstance>();
                    }
                    entityPool[rnetPrefab.key].Add(component);
                }
            }
        }

        internal RNetEntityInstance Rent(ushort entityKey)
        {
            var inst = entityPool[entityKey][entityPool[entityKey].Count - 1];
            entityPool[entityKey].RemoveAt(entityPool[entityKey].Count - 1);
            return inst;
        }

        internal void Return(ushort entityKey, RNetEntityInstance entity)
        {
            entity.transform.position = spawnPoint;
            entityPool[entityKey].Add(entity);
        }

    }
#elif CLIENT
    internal partial class RNetEntityPool 
    {
        private Dictionary<ushort, Dictionary<RNetRelationship, List<RNetEntityInstance>>> entityPool;

        public RNetEntityPool()
        {
            parent = new GameObject("EntityPool");
            parent.transform.position = spawnPoint;
            var files = Directory.GetFiles(Application.streamingAssetsPath + "/client/entities/", "*.json", SearchOption.AllDirectories);
            entityPool = new Dictionary<ushort, Dictionary<RNetRelationship, List<RNetEntityInstance>>>();
            foreach (var file in files)
            {
                var json = File.ReadAllText(file);

                RNetPrefabRootClientData rnetPrefab = JsonConvert.DeserializeObject<RNetPrefabRootClientData>(json);
                entityPool.Add(rnetPrefab.key, new Dictionary<RNetRelationship, List<RNetEntityInstance>>());

                foreach(var data in rnetPrefab.data)
                {
                    
                    if(entityPool[rnetPrefab.key].ContainsKey(data.relationship) == false)
                        entityPool[rnetPrefab.key].Add(data.relationship, new List<RNetEntityInstance>());

                    var go = LoadAssetBundle(data.bundleName);
                    for (int i = 0; i < data.poolSize; i++)
                    {
                        var inst = GameObject.Instantiate(go, parent.transform, false);

                        var component = inst.GetComponent<RNetEntityInstance>();
                        if (component == null)
                        {
                            component = inst.AddComponent<RNetEntityInstance>();
                        }
                        entityPool[rnetPrefab.key][data.relationship].Add(component);
                    }

                    
                }
            }
        }

        public  RNetEntityInstance Rent(ushort key,  RNetRelationship relationship)
        {
            var inst = entityPool[key][relationship][entityPool[key][relationship].Count - 1];
            entityPool[key][relationship].RemoveAt(entityPool[key][relationship].Count - 1);
            return inst;
        }

        public void Return(ushort key, RNetRelationship relationship, RNetEntityInstance entity)
        {
            entityPool[key][relationship].Add(entity);
        }
    }
#endif


    internal partial class RNetEntityPool
    {
        private Vector3 spawnPoint = new Vector3(-30000, -30000, -30000);
        private GameObject parent;

        private GameObject LoadAssetBundle(string bundleName)
        {
            var p = Application.streamingAssetsPath + "/" + bundleName;

            AssetBundle bundle = AssetBundle.LoadFromFile(p);
            if (bundle == null)
            {
                Debug.LogError("failed to load asset bundle " + bundleName);
                return null;
            }


            var go = bundle.LoadAsset<GameObject>(bundleName.Split('/')[3]);

            return go;
        }
        
    }
}
