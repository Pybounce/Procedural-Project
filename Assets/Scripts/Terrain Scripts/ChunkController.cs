using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkController : MonoBehaviour
{
    private List<Vector2> loadedTerrainChunks = new List<Vector2>();    //Contains the index for the chunk that's loaded
   // private List<MeshFilter> terrainChunkMeshFilters = new List<MeshFilter>();   //Mesh filters of pooled terrain game objects
    //private List<MeshFilter> waterChunkMeshFilters = new List<MeshFilter>();    //Mesh filters of pooled water game objects
    private GameObject terrainChunkObject;  //Holds all the terrain chunks in the inspector
    private GameObject waterChunkObject;  //Holds all the terrain chunks in the inspector

    private int renderDist = 12;    //Distance (in chunks) away from player, that they are loaded
    private int spacing = 10;       //Distance between points in a chunk
    private const int chunkSize = 241;  //Amount of points making up width/height of a chunk

    [SerializeField] private PlayerController playerController;
    
    [SerializeField] private ComputeShader terrainCS;
    [SerializeField] private Material terrainMaterial;
    [SerializeField] private Material waterMaterial;

    private TerrainSettings terrainSettings;
    private IEnumerator chunkLoaderCoroutine;   //Used to make it 'thread safe' so-to-speak

    private void GetTerrainSettings()
    {
        GameObject settingsObject = GameObject.FindGameObjectWithTag("Settings");
        if (settingsObject != null)
        {
            terrainSettings = settingsObject.GetComponent<TerrainSettings>();
            terrainSettings.OnUpdated += UpdateTerrainSettings;
            UpdateTerrainSettings();
        }
    }
    private void OnDisable()
    {
        terrainSettings.OnUpdated -= UpdateTerrainSettings;
    }
    private void UpdateTerrainSettings()
    {
        terrainSettings.GetSpacing(out spacing);
        terrainSettings.GetRenderDistance(out int tempRenderDistance);
        terrainSettings.GetTerrainColours(out Vector4 tc1, out Vector4 tc2);
        terrainMaterial.SetColor("_ColourOne", tc1);
        terrainMaterial.SetColor("_ColourTwo", tc2);
        terrainSettings.GetWaterColours(out Vector4 wc1, out Vector4 wc2);
        waterMaterial.SetColor("_ShallowColour", wc1);
        waterMaterial.SetColor("_DeepColour", wc2);
        float fogEnd = chunkSize * spacing * (tempRenderDistance - 1.5f);
        terrainMaterial.SetFloat("_FogEnd", fogEnd);
        waterMaterial.SetFloat("_FogEnd", fogEnd);
        terrainSettings.GetFogColours(out Vector4 fc1, out Vector4 fc2);
        terrainMaterial.SetColor("_TopFog", fc1);
        terrainMaterial.SetColor("_MidFog", fc2);
        waterMaterial.SetColor("_TopFog", fc1);
        waterMaterial.SetColor("_MidFog", fc2);
        if (tempRenderDistance != renderDist)
        {
            renderDist = tempRenderDistance;
            ResetLoadedChunks();
        }
        else { renderDist = tempRenderDistance; }
    }

    private void Start()
    {
        GetTerrainSettings();
        ResetLoadedChunks();
        playerController.SetSpawn(new Vector3(0, 10000, 0));
       // LODTriangles = GetLODTriangles(24, terrainChunkMeshFilters[0].mesh.triangles);  //Used for mesh colliders
    }
    private void Update()
    {
        Vector2Int currentPlayerChunk = GetChunkAtPoint(new Vector2(playerController.GetPosition().x, playerController.GetPosition().z));
        if (currentPlayerChunk != playerController.GetLastChunk())
        {
            //Makes sure the player is in a different chunk
            playerController.SetLastChunk(currentPlayerChunk);
            SortChunks();
           // StartCoroutine(UpdateMeshColliders());
        }
    }

    private void ResetLoadedChunks()
    {
        if (terrainChunkObject != null) { Destroy(terrainChunkObject); }
        terrainChunkObject = new GameObject();
        if (waterChunkObject != null) { Destroy(waterChunkObject); }
        waterChunkObject = new GameObject();

        //Resets the chunks to be built from the ground up again - useful for in-game changes to render distance
        loadedTerrainChunks.Clear();

        Resources.UnloadUnusedAssets(); //Expensive but only called when changing render distance


        int sideSize = (renderDist * 2) + 1;
        for (int i = 0; i < sideSize * sideSize; i++)
        {
            //Adds MeshFilter components of objects so they can be pooled
            CreateTerrainObject("water", waterMaterial, waterChunkObject);
            CreateTerrainObject("mountain", terrainMaterial, terrainChunkObject);
            loadedTerrainChunks.Add(new Vector2(-9999999999, -9999999999));
        }
        void CreateTerrainObject(string name, Material mat, GameObject holder)
        {
            GameObject obj = new GameObject();
            obj.name = name;
            obj.AddComponent<MeshFilter>();
            obj.AddComponent<MeshRenderer>();
            obj.AddComponent<MeshCollider>();
            obj.GetComponent<MeshCollider>().convex = false;
            obj.transform.SetParent(holder.transform);
            obj.GetComponent<Renderer>().material = mat;
        }
        playerController.SetLastChunk(GetChunkAtPoint(new Vector2(playerController.GetPosition().x, playerController.GetPosition().z)));
        SortChunks();
    }

    private Vector2Int GetChunkAtPoint(Vector2 point)
    {
        int chunkX = (int)(point.x / (chunkSize * spacing));
        int chunkY = (int)(point.y / (chunkSize * spacing));  //heh chunky
        return new Vector2Int(chunkX, chunkY);
    }

    private void SortChunks()
    {
        //Sorts the chunks between ones that are loaded and ones that need to be loaded
        Vector2Int playerChunk = playerController.GetLastChunk();
        List<int> freeSpaceToLoad = new List<int>();
        List<Vector2> chunksToLoad = new List<Vector2>();
        for (int x = playerChunk.x - renderDist; x <= playerChunk.x + renderDist; x++)
        {
            for (int row = playerChunk.y - renderDist; row <= playerChunk.y + renderDist; row++)
            {   //Loops through all the chunks that are in render distance
                chunksToLoad.Add(new Vector2(x, row));
            }
        }

        for (int loadedIndex = 0; loadedIndex < loadedTerrainChunks.Count; loadedIndex++)
        {   //Loops through all the loaded chunks
            bool chunkFound = false;    //If true, then the chunk to be loaded, is already loaded
            for (int toLoadIndex = 0; toLoadIndex < chunksToLoad.Count; toLoadIndex++)
            {   //Loops through all the chunks to be loaded
                if (loadedTerrainChunks[loadedIndex] == chunksToLoad[toLoadIndex])
                {
                    chunkFound = true;
                    chunksToLoad.RemoveAt(toLoadIndex);
                    toLoadIndex -= 1;
                }
            }
            if (chunkFound == false) { freeSpaceToLoad.Add(loadedIndex); }
        }

        //Makes sure that LoadChunks is 'thread-safe' so that 2 coroutines aren't running at the same time
        if (chunkLoaderCoroutine != null) { StopCoroutine(chunkLoaderCoroutine); }
        chunkLoaderCoroutine = LoadChunks(chunksToLoad, freeSpaceToLoad);
        StartCoroutine(chunkLoaderCoroutine);
    }

    private IEnumerator LoadChunks(List<Vector2> chunksToLoad, List<int> freeSpaceToLoad)
    {
        TerrainGenerator terrainGenerator = new TerrainGenerator();
        terrainGenerator.GetTerrainSettings();
        
        for (int i = 0; i < chunksToLoad.Count; i++)
        {   //Loops through the filtered chunks to load 
            loadedTerrainChunks[freeSpaceToLoad[i]] = chunksToLoad[i];
            Vector2 startPos = chunksToLoad[i] * (chunkSize - 1);
            terrainGenerator.GenerateTerrainChunk(terrainCS, startPos, out Vector3[] _positions, out Vector2[] _uvs, out int[] _tris, out Vector3[] _normals);//, out Mesh tMesh, out Mesh wMesh);

            MeshFilter tMeshF = terrainChunkObject.transform.GetChild(freeSpaceToLoad[i]).GetComponent<MeshFilter>();
            if (tMeshF.sharedMesh == null) { tMeshF.sharedMesh = new Mesh(); }
            else { tMeshF.sharedMesh.Clear(); }
            //Mesh tMesh = terrainChunkMeshFilters[freeSpaceToLoad[i]].sharedMesh;
            Mesh tMesh = tMeshF.sharedMesh;
            tMesh.vertices = _positions;
            tMesh.normals = _normals;
            tMesh.SetUVs(0, _uvs);
            tMesh.triangles = _tris;

            for (int z = 0; z < _positions.Length; z++)
            {
                _positions[z].y = 4000f;
                _normals[z] = new Vector3(0, 1, 0);
            }


            MeshFilter wMeshF = waterChunkObject.transform.GetChild(freeSpaceToLoad[i]).GetComponent<MeshFilter>();
            if (wMeshF.sharedMesh == null) { wMeshF.sharedMesh = new Mesh(); }
            else { wMeshF.sharedMesh.Clear(); }

            Mesh wMesh = wMeshF.sharedMesh;
            wMesh.vertices = _positions;
            wMesh.normals = _normals;
            wMesh.SetUVs(0, _uvs);
            wMesh.triangles = _tris;



            //terrainChunkMeshFilters[freeSpaceToLoad[i]].sharedMesh = tMesh;
            //waterChunkMeshFilters[freeSpaceToLoad[i]].sharedMesh = wMesh;
            yield return null;
        }
    }
    
    private int[] LODTriangles;

    /*
    private IEnumerator UpdateMeshColliders()
    {
        Vector2Int playerChunk = GetChunkAtPoint(new Vector2(playerController.GetPosition().x, playerController.GetPosition().z));
        for (int i = 0; i < loadedTerrainChunks.Count; i++)
        {
            if (terrainChunkMeshFilters[i].gameObject.GetComponent<MeshCollider>().sharedMesh == null)
            {
                terrainChunkMeshFilters[i].gameObject.GetComponent<MeshCollider>().sharedMesh = new Mesh();
            }
            Vector2 chunkIndex = loadedTerrainChunks[i];
            int xDelta = Mathf.Abs(playerChunk.x - (int)chunkIndex.x);
            int yDelta = Mathf.Abs(playerChunk.y - (int)chunkIndex.y);
            if (Mathf.Max(xDelta, yDelta) <= 1) //Only give mesh colliders to chunks that directly surround the player
            {
                
                if (terrainChunkMeshFilters[i].gameObject.GetComponent<MeshCollider>().sharedMesh != null)
                {
                    terrainChunkMeshFilters[i].gameObject.GetComponent<MeshCollider>().sharedMesh.Clear();
                }
                Mesh LODMesh = terrainChunkMeshFilters[i].gameObject.GetComponent<MeshCollider>().sharedMesh;
                LODMesh.vertices = terrainChunkMeshFilters[i].mesh.vertices;
                LODMesh.normals = terrainChunkMeshFilters[i].mesh.normals;
                LODMesh.triangles = LODTriangles;
            }
            else if (terrainChunkMeshFilters[i].gameObject.GetComponent<MeshCollider>().sharedMesh != null)
            {
                terrainChunkMeshFilters[i].gameObject.GetComponent<MeshCollider>().sharedMesh.Clear();
            }
            yield return null;
        }

    }

    int[] GetLODTriangles(int LOD, int[] tris)
    {   //Get's the index array for triangles at a given LOD level

        int triPointSize = (chunkSize - 1) * 6;   //Amount of points making up the triangles along one row/collum
        int hLim = ((chunkSize - 1) / LOD) * triPointSize;
        int wLim = ((chunkSize - 1) / LOD) * 6;

        List<int> triangles = new List<int>();
        for (int h = 0; h < hLim; h += triPointSize)
        {
            for (int w = 0; w < wLim; w += 6)
            {
                int triIndex = w + h;
                triangles.Add(tris[triIndex] * LOD);
                triangles.Add(tris[triIndex + 1] * LOD);
                triangles.Add(tris[triIndex + 2] * LOD);
                triangles.Add(tris[triIndex + 3] * LOD);
                triangles.Add(tris[triIndex + 4] * LOD);
                triangles.Add(tris[triIndex + 5] * LOD);
            }
        }
        return triangles.ToArray();
    } 
    */
}
