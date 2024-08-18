using RapidNet.Connections;
using RapidNet.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RapidNet.Replication
{
    public class RNetReplicationExtension : RNetExtension
    {
        public RNetReplicationExtension(WorkerCollection workers) : base(workers)
        {
            Debug.Log("RNet Replication System Loaded.");
        }

        public override void OnSocketConnect(ThreadType threadType, Connection connection)
        {
           
        }

        public override void OnSocketDisconnect(ThreadType threadType, Connection connection)
        {
            
        }

        public override bool OnSocketReceive(ThreadType threadType, Connection sender, ushort messageID, IntPtr messageData)
        {
            return false;
        }

        public override void OnSocketTimeout(ThreadType threadType, Connection connection)
        {
            
        }

        public override void OnThreadEventReceived(ThreadType threadType, ushort id, IntPtr eventData)
        {
            
        }
    }
}
