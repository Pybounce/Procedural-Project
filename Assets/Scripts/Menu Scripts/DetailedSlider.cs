using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct DetailedSlider
{
    public Slider slider;
    public int maxValue;
    public int minValue;
    public Text output;


    public int GetOutput()
    {
        //Uses the slider value to calculate the output based on max and min values
        return Mathf.RoundToInt((slider.value * (float)(maxValue - minValue)) + minValue);
    }
    public float GetSliderValue(int output)
    {

        //Takes in the desired output, and returns the slider value at that output
        return ((float)(output - minValue) / (float)(maxValue - minValue));
    }

}