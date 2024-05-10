using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CarSelector : MonoBehaviour
{
    [SerializeField] private Image carDisplay;
    [SerializeField] private SpriteRenderer carOnGroundSprite;
    [SerializeField] private CarBehaviour carOnGroundBehaviour;
    [SerializeField] private List<Sprite> carSprites;
    [SerializeField] private List<CarStats> carStats;
    [SerializeField] private Slider acceleration;
    [SerializeField] private float minAcceleration;
    [SerializeField] private float maxAcceleration;
    
    private int currentCarType = 0;
    private int currentCarColor = 0;
    
    private void Start()
    {
        SetupSliders();
    }

    private void SetupSliders()
    {
        acceleration.minValue = minAcceleration;
        acceleration.maxValue = maxAcceleration;
        CarStats stats = GetCurrentCarStats();
        acceleration.value = stats.acceleration;
    }

    private void UpdateSliders()
    {
        CarStats stats = GetCurrentCarStats();
        acceleration.value = stats.acceleration;
    }
    
    public void NextCarType()
    {
        currentCarType++;
        if (currentCarType >= 5)
        {
            currentCarType = 0;
        }
        UpdateCarSprite();
        UpdateCarStats();
    }
    public void PrevCarType()
    {
        currentCarType--;
        if (currentCarType < 0)
        {
            currentCarType = 4;
        }
        UpdateCarSprite();
        UpdateCarStats();
    }
    public void NextCarColor()
    {
        currentCarColor++;
        if (currentCarColor >= 5)
        {
            currentCarColor = 0;
        }
        UpdateCarSprite();
    }
    public void PrevCarColor()
    {
        currentCarColor--;
        if (currentCarColor < 0)
        {
            currentCarColor = 4;
        }
        UpdateCarSprite();
    }

    private void UpdateCarSprite()
    {
        Sprite newSprite = carDisplay.sprite;
        for (int i = 0; i < carSprites.Count; i++)
        {
            if (carSprites[i].name == $"car_{(CarColor)currentCarColor}_{currentCarType+1}")
            {
                newSprite = carSprites[i];
                break;
            }
        }
        carDisplay.sprite = newSprite;
        carOnGroundSprite.sprite = newSprite;
    }

    private void UpdateCarStats()
    {
        CarStats newStats = GetCurrentCarStats();
        
        carOnGroundBehaviour.SetStatValues(newStats);
        UpdateSliders();
    }

    private CarStats GetCurrentCarStats()
    {
        for (int i = 0; i < carStats.Count; i++)
        {
            if (carStats[i].name == $"{(CarType)currentCarType+1}Stats")
            {
                return carStats[i];
            }
        }

        return carStats[0];
    }
}
