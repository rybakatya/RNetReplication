using RapidNet.Connections;
using RapidNet.Extensions;
using RapidNet.Memory;
using RapidNet.Replication.Culling;
using RapidNet.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RapidNet.Replication
{

    public class RNetReplicationExtension : RNetExtension
    {
        private RNetEntityManager _entityManager;
        public RNetReplicationExtension(WorkerCollection workers) : base(workers)
        {
            var go = new GameObject("EntityManager");
            _entityManager = go.AddComponent<RNetEntityManager>();
            
        }

        public override void OnSocketConnect(ThreadType threadType, Connection connection)
        {
#if SERVER
            if(threadType == ThreadType.Game)
            {
                Debug.Log("Client connected!");
                _entityManager.CreateObserver(connection.ID, Vector2.zero);
            }
#endif
        }

        public override void OnSocketDisconnect(ThreadType threadType, Connection connection)
        {
            
        }

        public override bool OnSocketReceive(ThreadType threadType, Connection sender, ushort messageID, IntPtr messageData)
        {
           
            if(threadType == ThreadType.Game)
            {
                if(messageID == NetworkMessageIDs.CreateObserver)
                {
                    var msg = MemoryHelper.Read<CreateObserverNetworkMessage>(messageData);
#if CLIENT
                    
                    var go = new GameObject("NetworkObserver");
                    go.transform.position = new Vector3(msg.position.x, 0, msg.position.y);
                    go.AddComponent<RNetObserverInstance>();
                    
#elif SERVER
                    _entityManager.UpdateObserver(sender.ID, msg.position);
#endif
                    return true;
                }
            }

            return false;
        }

        public override void OnSocketTimeout(ThreadType threadType, Connection connection)
        {
            
        }

        public override void OnThreadEventReceived(ThreadType threadType, ushort id, IntPtr eventData)
        {
#if SERVER
            if (threadType == ThreadType.Game)
            {
                if (id == (ushort)ReplicationThreadEventIDS.CreateObserver)
                {
                    var data = MemoryHelper.Read<CreateObserverThreadEvent>(eventData);
                    _entityManager.CreateObserver(data.owner, data.position);

                }
            }
#endif
        }
        internal Dictionary<ushort, EntitySerializer> serializers = new Dictionary<ushort, EntitySerializer>();
        public override void OnThreadRegistered(ThreadType threadType)
        {
            if (threadType == ThreadType.Logic)
            {
                LoadEntitySerializers();
            }
        }

        private void LoadEntitySerializers()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.GetCustomAttributes(typeof(EntitySerializerAttribute), false).Length > 0)
                    {
                        if (type.IsSubclassOf(typeof(EntitySerializer)))
                        {
                            Logging.Logger.Log(Logging.LogLevel.Info, "EntitySerializer " + type.FullName + " initialized!");

                            var t = type.GetCustomAttribute<EntitySerializerAttribute>();
                            if (serializers.ContainsKey(t.messageID) == false)
                                serializers[t.messageID] = Activator.CreateInstance(type) as EntitySerializer;

                            else
                            {
                                Logging.Logger.Log(Logging.LogLevel.Error, "Cannot create serializer " + type.FullName + " id is already in use!");
                            }
                        }
                    }
                }
            }
        }

        public override bool CheckInterceptMessage(ushort messageID, BitBuffer buffer)
        {
            if(messageID == NetworkMessageIDs.WriteEntityUpdate)
            {
                var ptr = serializers[messageID].Deserialize(buffer);

                
                return true;
            }
            return false;
        }
    }
}
