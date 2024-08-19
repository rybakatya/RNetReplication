#if CLIENT
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RapidNet.Replication.Culling
{
    internal class RNetObserverInstance : MonoBehaviour
    {
        Vector3 lastPosition = Vector3.zero;
        private void FixedUpdate()
        {
            if (transform.position != lastPosition)
            {
                RNet.BroadcastUnreliable(NetworkMessageIDs.CreateObserver, 11, new CreateObserverNetworkMessage()
                {
                    position = new Vector2(transform.position.x, transform.position.z)
                });
                lastPosition = transform.position;
                Debug.Log("called");
            }
        }
    }
}
#endif