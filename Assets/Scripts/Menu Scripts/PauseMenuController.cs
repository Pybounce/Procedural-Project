using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{

    private Button selectedButtonColour;
    private bool isPaused = false;   //Is the game paused
    [SerializeField] private Canvas pauseMenuCanvas;

    [SerializeField] private DetailedSlider redSlider;
    [SerializeField] private DetailedSlider greenSlider;
    [SerializeField] private DetailedSlider blueSlider;
    [SerializeField] private DetailedSlider renderDistSlider;

    [SerializeField] private Button tc1Button;  //Terrain colour 1
    [SerializeField] private Button tc2Button;  //Terrain colour 2
    [SerializeField] private Button wc1Button;  //Water colour 1
    [SerializeField] private Button wc2Button;  //Water colour 2
    [SerializeField] private Button fc1Button;   //Fog top colour
    [SerializeField] private Button fc2Button;  //Fog mid colour

    [SerializeField] private Button quitButton;
    [SerializeField] private Button mainMenuButton;

    [SerializeField] private Button applyButton; //Applies settings
    private TerrainSettings terrainSettings;

    // Start is called before the first frame update
    void Start()
    {
        GetTerrainSettings();

        //Add listeners
        tc1Button.onClick.AddListener(() => SetSelectedButton(tc1Button));
        tc2Button.onClick.AddListener(() => SetSelectedButton(tc2Button));
        wc1Button.onClick.AddListener(() => SetSelectedButton(wc1Button));
        wc2Button.onClick.AddListener(() => SetSelectedButton(wc2Button));
        fc1Button.onClick.AddListener(() => SetSelectedButton(fc1Button));
        fc2Button.onClick.AddListener(() => SetSelectedButton(fc2Button));
        quitButton.onClick.AddListener(QuitGame);
        applyButton.onClick.AddListener(ApplySettings);
        if (mainMenuButton != null) { mainMenuButton.onClick.AddListener(LoadMainMenu); }
        
        SetSelectedButton(tc1Button);   //Sets tc1Button as the default selected so that there's always 1 button selected

        pauseMenuCanvas.enabled = isPaused;

    }

    // Update is called once per frame
    void Update()
    {
        if (isPaused)
        {
            //Updates slider text and button colour
            redSlider.output.text = redSlider.GetOutput().ToString();
            greenSlider.output.text = greenSlider.GetOutput().ToString();
            blueSlider.output.text = blueSlider.GetOutput().ToString();
            renderDistSlider.output.text = renderDistSlider.GetOutput().ToString();
            UpdateButtonColour(selectedButtonColour, new Vector4(redSlider.slider.value, greenSlider.slider.value, blueSlider.slider.value, 1f));
        }
        if (Input.GetKeyDown(KeyCode.Escape) && pauseMenuCanvas != null)
        {
            //Pause/Unpause game
            isPaused = !isPaused;
            pauseMenuCanvas.enabled = isPaused;
            Time.timeScale = isPaused is true ? 0f : 1f;
            if (isPaused) { UpdateTerrainSettings(); }  //Makes sure settings are up-to-date
        }

    }

    private void ApplySettings()
    {
        terrainSettings.SetTerrainColours(tc1Button.colors.normalColor, tc2Button.colors.normalColor);
        terrainSettings.SetWaterColours(wc1Button.colors.normalColor, wc2Button.colors.normalColor);
        terrainSettings.SetFogColours(fc1Button.colors.normalColor, fc2Button.colors.normalColor);
        terrainSettings.SetRenderDistance(renderDistSlider.GetOutput());
        terrainSettings.UpdateTerrainSettings();
    }

    private void UpdateButtonColour(Button btn, Color c)
    {
        ColorBlock btnCB = btn.colors;
        btnCB.normalColor = c;
        btn.colors = btnCB;
    }

    private void SetSelectedButton(Button btn)
    {   
        selectedButtonColour = btn;
        redSlider.slider.value = selectedButtonColour.colors.normalColor.r;
        greenSlider.slider.value = selectedButtonColour.colors.normalColor.g;
        blueSlider.slider.value = selectedButtonColour.colors.normalColor.b;
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

    public void UpdateTerrainSettings()
    {
        terrainSettings.GetTerrainColours(out Vector4 tc1, out Vector4 tc2);
        UpdateButtonColour(tc1Button, tc1);
        UpdateButtonColour(tc2Button, tc2);
        terrainSettings.GetWaterColours(out Vector4 wc1, out Vector4 wc2);
        UpdateButtonColour(wc1Button, wc1);
        UpdateButtonColour(wc2Button, wc2);
        terrainSettings.GetFogColours(out Vector4 fc1, out Vector4 fc2);
        UpdateButtonColour(fc1Button, fc1);
        UpdateButtonColour(fc2Button, fc2);
        terrainSettings.GetRenderDistance(out int rd);
        renderDistSlider.slider.value = renderDistSlider.GetSliderValue(rd);

    }

    private void QuitGame()
    {
        Application.Quit();
    }
    private void LoadMainMenu()
    {
        Destroy(terrainSettings.gameObject);
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }

}
