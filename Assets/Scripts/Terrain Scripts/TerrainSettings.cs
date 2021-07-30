using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class TerrainSettings : MonoBehaviour
{
    private Vector4 terrainColourOne = new Vector4(1, 1, 1, 1);
    private Vector4 terrainColourTwo = new Vector4(0.4f, 0.4f, 0.4f, 1);
    private Vector4 waterColourOne = new Vector4(0.3f, 1, 1, 1);
    private Vector4 waterColourTwo = new Vector4(0.4f, 0.7f, 0.7f, 1);
    private Vector4 fogColourOne = new Vector4(0, 1, 1, 1);
    private Vector4 fogColourTwo = new Vector4(0.5f, 1, 1, 1);

    private int renderDistance = 8;
    private int spacing = 10;
    private int mapSize = 20;
    private int seed = 0;

    public delegate void UpdateAction();
    public event UpdateAction OnUpdated;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void UpdateTerrainSettings()
    {
        OnUpdated();
    }

    //Terrain Colours--------------------
    public void GetTerrainColours(out Vector4 tc1, out Vector4 tc2)
    {
        tc1 = terrainColourOne;
        tc2 = terrainColourTwo;
    }
    public void SetTerrainColours(Vector4 tc1, Vector4 tc2)
    {
        terrainColourOne = tc1;
        terrainColourTwo = tc2;
    }
    //Water Colours--------------------
    public void GetWaterColours(out Vector4 wc1, out Vector4 wc2)
    {
        wc1 = waterColourOne;
        wc2 = waterColourTwo;
    }
    public void SetWaterColours(Vector4 wc1, Vector4 wc2)
    {
        waterColourOne = wc1;
        waterColourTwo = wc2;
    }
    //Background Colour--------------------
    public void GetFogColours(out Vector4 fc1, out Vector4 fc2)
    {
        fc1 = fogColourOne;
        fc2 = fogColourTwo;
    }
    public void SetFogColours(Vector4 fc1, Vector4 fc2)
    {
        fogColourOne = fc1;
        fogColourTwo = fc2;
    }
    //Render Distance--------------------
    public void GetRenderDistance(out int rd)
    {
        rd = renderDistance;
    }
    public void SetRenderDistance(int rd)
    {
        renderDistance = rd;
    }
    //Spacing--------------------
    public void GetSpacing(out int s)
    {
        s = spacing;
    }
    public void SetSpacing(int s)
    {
        spacing = s;
    }
    //Map Size--------------------
    public void GetMapSize(out int ms)
    {
        ms = mapSize;
    }
    public void SetMapSize(int ms)
    {
        mapSize = ms;
    }
    //Seed--------------------
    public void GetSeed(out int s)
    {
        s = seed;
    }
    public void SetSeed(int s)
    {
        seed = s;
    }


}
