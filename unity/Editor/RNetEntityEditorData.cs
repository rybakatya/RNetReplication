using RapidNet.Replication.Prefabs.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RapidNet.Replication.Editor
{
    [Serializable]
    internal class RNetEntityEditorData
    {
        public RNetRelationship relationship;
        public GameObject prefab;
        public int poolSize;
    }
}
