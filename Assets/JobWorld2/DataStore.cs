using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DataStore : MonoBehaviour
{
    public Dictionary<Vector3, DataDefs.ChunkData> dataStore;

    public void Add(Vector3 key, DataDefs.ChunkData value)
    {

    }

    //public bool TryGet(Vector3 key, out DataDefs.ChunkData value)
    //{
    //    if (dataStore.ContainsKey(key))
    //    {
    //        value = dataStore.
    //        return true;
    //    }
    //    value = 
    //}
}
