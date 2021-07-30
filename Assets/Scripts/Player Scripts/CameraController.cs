using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform cameraFollowObject;
    [SerializeField] private float speed = 200;
    private float currentRotation = 0f;
    private enum CameraMovementType { Follow, Menu };
    [SerializeField] private CameraMovementType movementType;

    private TerrainSettings terrainSettings;

    private void Start()
    {
        GetTerrainSettings();
        if (movementType == CameraMovementType.Menu)
        {
            int randSeed = Random.Range(-10000000, 10000000);
            terrainSettings.SetSeed(randSeed);
        }
    }
    private void LateUpdate()
    {
        if (movementType == CameraMovementType.Follow && player.gameObject.activeInHierarchy == true) { FollowPlayer(); }
        else if (movementType == CameraMovementType.Menu)   //Used for main menu
        {
            //Sets the correct rotation over the y-axis, then rotates to look down
            this.transform.rotation = Quaternion.AngleAxis(currentRotation, Vector3.up);
            this.transform.Rotate(25 * Vector3.right);
            currentRotation += Time.deltaTime * 5f;
        }
    }

    private void FollowPlayer()
    {
        //Camera should speed up when further away from the target
        float maxDistance = 40f;
        float d = Vector3.Distance(this.transform.position, cameraFollowObject.position);
        float s = speed + (d - maxDistance);
        Vector3 targetDelta = cameraFollowObject.position - this.transform.position;
        targetDelta.Normalize();
        this.transform.position += targetDelta * s * Time.deltaTime;
        this.transform.LookAt(player);
    }

    public void GetTerrainSettings()
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
        terrainSettings.GetFogColours(out Vector4 fc1, out Vector4 fc2);
        RenderSettings.skybox.SetColor("_TopColour", fc1);
        RenderSettings.skybox.SetColor("_MidColour", fc2);
    }

}
