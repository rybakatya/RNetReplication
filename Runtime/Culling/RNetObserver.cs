using System.Collections.Generic;
using UnityEngine;

namespace RapidNet.Replication.Culling
{
    internal class RNetObserver
    {
        public ushort ownerID;
        public Vector2 position;
        public List<RNetEntityInstance> loadedEntities;
        public float lastUpdateCulling;
        public float lastSendUpdate;
    }
}
