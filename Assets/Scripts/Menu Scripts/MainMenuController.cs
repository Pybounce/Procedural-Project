using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private DetailedSlider seedSlider;
    [SerializeField] private DetailedSlider spacingSlider;

    private void Update()
    {
        seedSlider.output.text = seedSlider.GetOutput().ToString();
        spacingSlider.output.text = spacingSlider.GetOutput().ToString();
    }
    public void LoadScene(string sceneName)
    {
        //Called on a button click event
        GameObject settingsObject = GameObject.FindGameObjectWithTag("Settings");
        if (settingsObject != null)
        {
            //Sets the terrain settings before loading the next scene
            TerrainSettings terrainSettings = settingsObject.GetComponent<TerrainSettings>();
            terrainSettings.SetSpacing(spacingSlider.GetOutput());
            terrainSettings.SetSeed(seedSlider.GetOutput() * 10000);    //Means each value on the slider is worth 10,000 meters
            SceneManager.LoadScene(sceneName);
        }

       
    }
}
