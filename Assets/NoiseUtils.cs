using UnityEngine;

public class NoiseUtils
{
    static int maxHeight = 25;
    static float roughness = 0.01f;
    static int octaves = 4;
    static float persistence = 0.5f;

    public static int GenerateHeight(float x, float z, int seed)
    {
        float height = ScaleTo(0f, maxHeight, 0f, 1f, fBM(x * roughness, z * roughness, octaves, persistence, seed));
        return (int)height;
    }

    public static float ScaleTo(float newMin, float newMax, float origMin, float origMax, float value)
    {
        return Mathf.Lerp(newMin, newMax, Mathf.InverseLerp(origMin, origMax, value));
    }

    static float fBM(float x, float z, int octaves, float persistence, int seed)
    {
        float total = 0f;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;
        for (int i = 0; i < octaves; i++)
        {
            total += Mathf.PerlinNoise(x * frequency + seed, z * frequency + seed) * amplitude;

            maxValue += amplitude;

            amplitude *= persistence;
            frequency *= 2f;

            seed -= seed / 2;
        }

        return total / maxValue;
    }
}