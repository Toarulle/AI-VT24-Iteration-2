using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SliderHandler : MonoBehaviour
{
    private Slider slider;
    private TextMeshProUGUI valueText;

    private void Start()
    {
        slider = GetComponentInChildren<Slider>();
        var textList = GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var item in textList)
        {
            if (item.name == "SliderText")
            {
                valueText = item;
            }
        }
    }

    public void UpdateValue(float newValue)
    {
        if (slider == null)
        {
            Start();
        }
        slider.value = newValue;
    }

    private void UpdateValueText(string text)
    {
        valueText.text = text;
    }
    
    public void UpdateValueText(float value)
    {
        valueText.text = value.ToString();
    }
}