using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class JobWorld2 : MonoBehaviour
{
    public Dictionary<Vector3, DataDefs.ChunkData> dataStore;
    public JobWorldChunk2[,,] chunks; // physical chunk gameObjects
    public string worldSaveName = "JobWorld2";

    public List<DataDefs.ChunkData> savesToBeCompleted;

    public Material material;
    
    private void Start()
    {
        //dataStore = new Dictionary<Vector3, DataDefs.ChunkData>();
        //chunks = new JobWorldChunk2[DataDefs.chunkNum, DataDefs.chunkNum, DataDefs.chunkNum];

        //for (int x = 0; x < DataDefs.chunkNum; x++)
        //{
        //    for (int y = 0; y < DataDefs.chunkNum; y++)
        //    {
        //        for (int z = 0; z < DataDefs.chunkNum; z++)
        //        {

        //        }
        //    }
        //}
        JobWorldChunk2 chunk = new JobWorldChunk2(worldSaveName, this, Vector3.zero, material);

        chunk.ScheduleCalc();
        chunk.CompleteCalc();

        chunk.ScheduleDraw();
        chunk.CompleteDraw();
        
        /***** Individual Chunk Saving
        // get the chunk's data out of the worldData dictionary
        // classes are passed by a reference (like a pointer) so memory is NOT being copied (which could cause slowness)
        DataDefs.ChunkData data = worldData[chunk.gameObject.transform.position]; // gets data out of 
        data.ScheduleSave(GetSaveName(chunk.gameObject.transform.position));
        data.CompleteSave();
        *****/

        ScheduleWorldDataSave();
        CompleteWorldDataSave();
    }

    private void ScheduleWorldCalc()
    {

    }

    private void ScheduleWorldDataSave()
    {
        // go through each item in worldData
        foreach (KeyValuePair<Vector3, DataDefs.ChunkData> item in dataStore)
        {
            // get the dictionary item and its value (AKA ChunkData)
            // then run the ScheduleSave function that is in the ChunkData class
            item.Value.ScheduleSave(GetSaveName(item.Key));

            // add ChunkData instance to saveToBeCompleted list
            savesToBeCompleted.Add(item.Value);
        }
    }

    private void CompleteWorldDataSave()
    {
        // go through all the ChunkDatas in "savesToBeCompleted" and complete their save job
        foreach(DataDefs.ChunkData data in savesToBeCompleted)
        {
            data.CompleteSave();
        }
        
        savesToBeCompleted.Clear();
    }

    private string GetSaveName(Vector3 pos)
    {
        return Application.persistentDataPath + "/" + worldSaveName + "/chunks/" + pos + ".chunk";
    }
}