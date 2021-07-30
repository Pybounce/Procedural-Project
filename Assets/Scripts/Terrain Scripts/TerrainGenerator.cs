using System.Collections;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class TerrainGenerator
{

    private const int chunkSize = 241;    //Amount of points along a chunk (width or height)
    private int spacing = 10;    //The default amount of space between points in a chunk
    private int seed = 23523;
    private TerrainSettings terrainSettings;

    public void GetTerrainSettings()
    {
        GameObject settingsObject = GameObject.FindGameObjectWithTag("Settings");
        if (settingsObject != null)
        {
            terrainSettings = settingsObject.GetComponent<TerrainSettings>();
            UpdateTerrainSettings();
        }
    }
    

    public void UpdateTerrainSettings()
    {
        terrainSettings.GetSpacing(out spacing);
        terrainSettings.GetSeed(out seed);
    }

    //only reason terrainType stays is for biomes later
    public void GenerateTerrainChunk(ComputeShader terrainShader, Vector2 startPos, out Vector3[] _positions, out Vector2[] _uvs, out int[] _tris, out Vector3[] _normals)// out Mesh terrainMesh, out Mesh waterMesh)
    {

        Vector3[] positions = new Vector3[chunkSize * chunkSize];
        Vector2[] uvs = new Vector2[chunkSize * chunkSize];
        int[] tris = new int[(chunkSize - 1) * (chunkSize - 1) * 6];

        ComputeBuffer o_Positions = new ComputeBuffer(positions.Length, sizeof(float) * 3);
        ComputeBuffer o_uvs = new ComputeBuffer(uvs.Length, sizeof(float) * 2);
        ComputeBuffer o_Triangles = new ComputeBuffer(tris.Length, sizeof(int));


        terrainShader.SetBuffer(0, "o_Positions", o_Positions);
        terrainShader.SetBuffer(0, "o_uvs", o_uvs);
        terrainShader.SetBuffer(0, "o_Triangles", o_Triangles);
        terrainShader.SetInt("size", chunkSize);
        terrainShader.SetFloats("startPos", startPos.x, startPos.y);
        terrainShader.SetInt("spacing", spacing);
        terrainShader.SetFloat("seed", seed);
        terrainShader.Dispatch(0, chunkSize * chunkSize, 1, 1); //Amount of groups

        //KERNEL 1 ------ CSNormals
        Vector3[] normals = new Vector3[chunkSize * chunkSize];
        ComputeBuffer o_Normals = new ComputeBuffer(normals.Length, sizeof(float) * 3);

        int normalsKernel = terrainShader.FindKernel("CSNormals");
        terrainShader.SetBuffer(normalsKernel, "o_Positions", o_Positions);
        terrainShader.SetBuffer(normalsKernel, "o_Normals", o_Normals);
        terrainShader.Dispatch(normalsKernel, chunkSize * chunkSize, 1, 1); //Amount of groups

        o_Positions.GetData(positions);
        o_Normals.GetData(normals);
        o_Triangles.GetData(tris);
        o_uvs.GetData(uvs);

        _positions = positions;
        _normals = normals;
        _uvs = uvs;
        _tris = tris;
        


        //Disposes of the compute buffers
        o_Positions.Dispose();
        o_Normals.Dispose();
        o_Triangles.Dispose();
        o_uvs.Dispose();

    }



}
