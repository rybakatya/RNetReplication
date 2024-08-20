using RapidNet.Connections;
using RapidNet.Extensions;
using RapidNet.Memory;
using RapidNet.Replication.Culling;
using RapidNet.Replication.Culling.Observer;
using RapidNet.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RapidNet.Replication
{

    public class RNetReplicationExtension : RNetExtension
    {
        private RNetObserverManager _entityManager;
        public RNetReplicationExtension(WorkerCollection workers) : base(workers)
        {
            var go = new GameObject("EntityManager");
            _entityManager = go.AddComponent<RNetObserverManager>();

            Replication.Init(workers, _entityManager);
            
        }

        public override void OnSocketConnect(ThreadType threadType, Connection connection)
        {

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
        
        public override void OnThreadRegistered(ThreadType threadType)
        {
           
        }

       

        public override bool CheckInterceptMessage(ushort messageID, IntPtr message, BitBuffer buffer)
        {
            return false;
        }
    }
}
