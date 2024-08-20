using RapidNet.Replication.Culling;
using System.Collections.Generic;
using UnityEngine;

namespace RapidNet.Replication.Culling.Observer
{

#if SERVER
    public partial class RNetObserverManager
    {
        private RNetObserverPool _observerPool;       
        private Dictionary<ushort, RNetObserver> _observers;
        
        private void  Init()
        {
            ///SETTINGS
            _observerPool = new RNetObserverPool(1024);
            _observers = new Dictionary<ushort, RNetObserver>(1024);
            
        }
        
        internal void CreateObserver(ushort owner,Vector2 position)
        {
            var observer = _observerPool.Rent();
            observer.ownerID = owner;
            observer.position = position;
          
            var msg = new CreateObserverNetworkMessage()
            {
                position = position,
                
            };
            _observers.Add(owner, observer);
            RNet.SendMessage(owner, NetworkMessageIDs.CreateObserver, (byte)(owner % 255), ENet.PacketFlags.Reliable, msg);
        }

        internal void UpdateObserver(ushort owner, Vector2 position)
        {
            Debug.Log(position.x + " " + position.y);
            _observers[owner].position = position;
        }   
    }

#elif CLIENT
    public partial class RNetObserverManager : MonoBehaviour
    {
        internal void Init()
        {

        }
    }
#endif

    public partial class RNetObserverManager : MonoBehaviour
    {       
        private void Awake()
        {
            Init(); 
        }
    }
}
