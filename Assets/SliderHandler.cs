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
    private UIBehaviour ui;

    private void Start()
    {
        ui = GetComponentInParent<UIBehaviour>();
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
        slider.value = newValue;
        UpdateValueText(newValue.ToString());
    }

    private void UpdateValueText(string text)
    {
        valueText.text = text;
    }
    
    public void NewValueFromSlider()
    {
        UpdateValueText(slider.value.ToString());
        
    }
}