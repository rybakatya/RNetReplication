using RapidNet.Serialization;
using UnityEngine;

namespace RapidNet.Replication.Culling.Observer
{
    internal struct CreateObserverNetworkMessage : IMessageObject
    {
        public Vector2 position;
    }
}
