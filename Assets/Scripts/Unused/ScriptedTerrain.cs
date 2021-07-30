using UnityEngine;
using System;
[CreateAssetMenu(fileName = "NAME_ME_st", menuName = "Scripted Objects/Scripted Terrain")]



public class ScriptedTerrain : ScriptableObject
{
    public SimpleTerrainLayer[] terrainLayers;
    public float minHeight;
}

[Serializable]
public struct SimpleTerrainLayer
{
    public Vector2 scale;
    //The following v2's represent a bezier curve
    public Vector2 a1;
    public Vector2 c1;
    public Vector2 c2;
    public Vector2 a2; 
    public int doesOverwrite;  //If the terrain height is greater then the current height, it will replace it instad of adding  [1=true, 0=false]
}


