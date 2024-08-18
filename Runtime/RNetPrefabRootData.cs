using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RNetPrefabRootServerData 
{
    public ushort key;
    public int poolSize;
    public string bundlePath;
}

public class RNetPrefabRootClientData
{
    public ushort key;
    public List<RNetEntityData> data;
}


public enum RNetRelationship
{
    Creator,
    Controller,
    Peer,
    Proxy
}

public class RNetEntityData
{
    public RNetRelationship relationship;
    public string bundleName;
    public int poolSize;
}