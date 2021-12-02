using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LodWorld : MonoBehaviour
{
    public Material material;

    public void Start()
    {
        LodWorldChunk chunk = new LodWorldChunk(material, Vector3.zero);
        chunk.ScheduleDraw();
        chunk.CompleteDraw();
    }
}
