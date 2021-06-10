using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static int GetIndex(int x, int y, int z)
    {
        return (x * DataDefs.chunkSize * DataDefs.chunkSize) + (y * DataDefs.chunkSize) + z;
    }

    public static byte GetPerlinVoxel(FastNoiseLite noise, Vector3 position, float x, float y, float z)
    {
        float height = (noise.GetNoise(x + position.x, z + position.z) + 1) / 2 * DataDefs.chunkSize;

        if (y >= height)
        {
            return 0; // air
        }
        else
        {
            return 1; // solid (the only "voxelType")
        }
    }
}
