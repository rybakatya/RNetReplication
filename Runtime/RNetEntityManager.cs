using RapidNet.Connections;
using RapidNet.Memory;
using RapidNet.Replication.Culling;
using RapidNet.Serialization;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace RapidNet.Replication
{
    public class EntitySerializerAttribute :  System.Attribute
    {
        public ushort messageID;
        public EntitySerializerAttribute(ushort id)
        {
            messageID = id;
        }
    }


    public abstract class EntitySerializer
    {
        public ushort entityKey;
        public abstract void Serialize(ushort entityID, ushort entityKey, BitBuffer buffer, IntPtr message);
        public abstract IntPtr Deserialize(ushort entityID, ushort entityKey, BitBuffer buffer, IntPtr message);
    }


    


    internal enum ReplicationThreadEventIDS
    {
        CreateObserver,
        CreateEntity
    }

    internal class NetworkMessageIDs
    {
        public const ushort CreateObserver = ushort.MaxValue;
        public const ushort WriteEntityUpdate = ushort.MaxValue - 1;
    }


    internal struct EntityUpdateMessage : IMessageObject
    {
        public float time;
        public ushort count;
        public NativeList<EntityUpdateData> entities;
    }

    internal struct EntityUpdateData
    {
        public ushort key;
        public ushort id;
        public bool isMine;
        public Vector3 position;
        public IntPtr userData;
    }


    

    internal struct CreateObserverNetworkMessage : IMessageObject
    {
        public Vector2 position;
    }

    [Serializer(NetworkMessageIDs.CreateObserver)]
    internal class CreateObserverSerializer : Serializer
    {
        public override IntPtr Deserialize(BitBuffer buffer)
        {


            var msg = new CreateObserverNetworkMessage()
            {
                position = new Vector2()
                {
                    x = HalfPrecision.Dequantize(buffer.ReadUShort()),
                    y = HalfPrecision.Dequantize((ushort)buffer.ReadUShort())
                }
            };
           
            return MemoryHelper.Write(msg);

        }

        public override void Serialize(BitBuffer buffer, IntPtr data)
        {

            var msg = MemoryHelper.Read<CreateObserverNetworkMessage>(data);
            buffer.AddUShort(HalfPrecision.Quantize(msg.position.x));
            buffer.AddUShort(HalfPrecision.Quantize(msg.position.y));

        }
    }

    internal struct CreateObserverThreadEvent
    {
        public ushort owner;
        public Vector2 position;
    }

    public static class Replication
    {
        private static WorkerCollection _workers;
        internal static void Init(WorkerCollection workerCollection)
        {
            _workers = workerCollection;
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

#if SERVER
    public partial class RNetEntityManager
    {
        private RNetObserverPool _observerPool;
        private IDGenerator _generator;
        private RNetSpatialHash _spatialHash;
        private Dictionary<ushort, RNetObserver> _observers;
        private List<RNetEntityInstance> _queriedEntities;
        private void  Init()
        {
            ///SETTINGS
            _observerPool = new RNetObserverPool(1024);

            ///SETTINGS!
            _generator = new IDGenerator(1024);

            ///SETTINGS!
            _spatialHash = new RNetSpatialHash(-5000, -5000, 10000, 10000, 2500, 1024);

            ///SETTINGS!
            _observers = new Dictionary<ushort, RNetObserver>(1024);

            ///SETTINGS!
            _queriedEntities = new List<RNetEntityInstance>(512);
        }

        internal  ushort CreateEntity(ushort key, Vector3 position)
        {
            var id = _generator.Rent();
            var entity = _pool.Rent(key);
            entity.id = id;
            entity.transform.position = position;

            _spatialHash.Add(position, entity);
            return id;

        }

        
        internal void CreateObserver(ushort owner,Vector2 position)
        {
            var observer = _observerPool.Rent();
            observer.ownerID = owner;
            observer.position = position;

            _spatialHash.Query(new Vector3(position.x, 0, position.y), 512, ref _queriedEntities);
            var msg = new CreateObserverNetworkMessage()
            {
                position = position,
                
            };
            _observers.Add(owner, observer);
            RNet.SendMessage(owner, NetworkMessageIDs.CreateObserver, (byte)(owner % 255), ENet.PacketFlags.Reliable, msg);
        }

        internal void UpdateObserver(ushort owner, Vector2 position)
        {
            Debug.Log(position.ToString());
            _observers[owner].position = position;

            var time = Time.time;
            if (time - _observers[owner].lastUpdateCulling >= 50)
            {
                UpdateObserverInterests(owner, position, time);
            }
        }


        private void UpdateObserverInterests(ushort owner, Vector2 position, float time)
        {
            _observers[owner].lastUpdateCulling = time;
            _spatialHash.Query(new Vector3(position.x, 0, position.y), 512, ref _queriedEntities);
            var msg = new EntityUpdateMessage();
            msg.time = time;
            msg.entities = new NativeList<EntityUpdateData>(_queriedEntities.Count, Allocator.TempJob);
            msg.count = (ushort)_queriedEntities.Count;
            
            
            foreach (var entity in _queriedEntities)
            {
                msg.entities.Add(new EntityUpdateData()
                {
                    id = entity.id,
                    isMine = entity.owner == owner,
                    key = entity.key,
                    position = entity.transform.position, 
                    
                    
                });
            }
            RNet.SendMessage(owner, NetworkMessageIDs.WriteEntityUpdate, (byte)(owner % 255), ENet.PacketFlags.Unsequenced, msg);
            _queriedEntities.Clear();
            
        }



        
    }
#elif CLIENT
    public partial class RNetEntityManager : MonoBehaviour
    {
        internal void Init()
        {

        }
    }
#endif
    public partial class RNetEntityManager : MonoBehaviour
    {
        private RNetEntityPool _pool;
        
        private void Awake()
        {
            _pool = new RNetEntityPool();

            Init(); 

        }
    }
}
