using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class JobWorld2 : MonoBehaviour
{
    public Dictionary<Vector3, DataDefs.ChunkData> worldData;
    public JobWorldChunk2 chunk;
    public string worldSaveName = "JobWorld2";

    public Material material;

    private void Start()
    {
        worldData = new Dictionary<Vector3, DataDefs.ChunkData>();
        chunk = new JobWorldChunk2(worldSaveName, this, Vector3.zero, material);

        chunk.ScheduleCalc();
        chunk.CompleteCalc();

        chunk.ScheduleDraw();
        chunk.CompleteDraw();
    }
}