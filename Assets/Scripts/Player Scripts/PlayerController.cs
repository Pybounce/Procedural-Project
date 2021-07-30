using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //This class has been made for future expansion, so other classes such as ChunkController don't become cluttered with player data and methods

    private Vector2Int lastPlayerChunk = new Vector2Int(0, 0);
    private Vector3 spawn = new Vector3();
    public void SetLastChunk(Vector2Int chunk)
    {
        lastPlayerChunk = chunk;
    }
    public Vector2Int GetLastChunk()
    {
        return lastPlayerChunk;
    }
   
    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }
    public Vector3 GetPosition()
    {
        return transform.position;
    }
    public void SetSpawn(Vector3 p)
    {
        spawn = p;
        SetPosition(spawn);
    }
    public void Kill()
    {
        transform.position = spawn;
        transform.rotation = Quaternion.identity;
    }

}
