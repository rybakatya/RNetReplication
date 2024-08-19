using RapidNet.Replication.Prefabs.Serialization;
using RapidNet.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RapidNet.Replication
{
    public abstract class RNetEntityInstance : MonoBehaviour, IEquatable<RNetEntityInstance>
    {
        public ushort id;
        public ushort key;
        public ushort owner;
        
        public byte size;

       
        

        public bool Equals(RNetEntityInstance other)
        {
            if(id == other.id)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return id;
        }
    }
}
