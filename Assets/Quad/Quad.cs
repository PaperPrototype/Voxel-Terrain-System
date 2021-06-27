using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class Quad : MonoBehaviour
{
    void Start()
    {
        Mesh mesh = new Mesh
        {
            vertices = new Vector3[]
            {
                DataDefs.Vertices[DataDefs.BuildOrder[0, 0]],
                DataDefs.Vertices[DataDefs.BuildOrder[0, 1]],
                DataDefs.Vertices[DataDefs.BuildOrder[0, 2]],
                DataDefs.Vertices[DataDefs.BuildOrder[0, 3]]
            },
            uv = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 0),
                new Vector2(1, 1)
            },
            triangles = new int[]
            {
                0, 1, 2, 2, 1, 3
            }
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        gameObject.GetComponent<MeshFilter>().mesh = mesh;
    }
}
