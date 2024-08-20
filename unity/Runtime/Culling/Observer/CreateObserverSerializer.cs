using RapidNet.Memory;
using RapidNet.Serialization;
using System;
using UnityEngine;

namespace RapidNet.Replication.Culling.Observer
{
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
}
