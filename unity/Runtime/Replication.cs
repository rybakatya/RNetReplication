using RapidNet.Connections;
using RapidNet.Replication.Culling.Observer;
using UnityEngine;

namespace RapidNet.Replication
{
    public static class Replication
    {
        private static WorkerCollection _workers;
        private static RNetObserverManager _entityManager;

        internal static void Init(WorkerCollection workerCollection, RNetObserverManager entityManager) 
        {
            _workers = workerCollection;
            _entityManager = entityManager;
        }

        public static void CreateObserver(Connection owner, Vector2 position)
        {
            _workers.gameWorker.Enqueue((ushort)ReplicationThreadEventIDS.CreateObserver, new CreateObserverThreadEvent()
            {
                owner = (ushort)owner.ID,
                position = position
            });
        }
    }
}
