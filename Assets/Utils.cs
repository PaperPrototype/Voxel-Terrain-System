using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static int GetIndex(int x, int y, int z)
    {
        return (x * Data.chunkSize * Data.chunkSize) + (y * Data.chunkSize) + z;
    }
}
