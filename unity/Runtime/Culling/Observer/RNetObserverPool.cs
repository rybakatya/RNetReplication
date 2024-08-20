using System.Collections.Generic;
using UnityEngine;

namespace RapidNet.Replication.Culling.Observer
{
    internal class RNetObserverPool
    {
        public Stack<RNetObserver> pool;

        public RNetObserverPool(int size)
        {
            pool = new Stack<RNetObserver>(1024);
            for(int i = 0; i < size; i ++)
            {
                pool.Push(new RNetObserver()
                {
                    ownerID = ushort.MaxValue,
                    position = new Vector2(-30000, -30000)
                });
            }
        }

        public RNetObserver Rent()
        {
            return pool.Pop();
        }

        public void Return(RNetObserver observer)
        {
            
            observer.ownerID = ushort.MaxValue;
            observer.position = new Vector2(-30000, -30000);
            pool.Push(observer);
        }
    }
}
