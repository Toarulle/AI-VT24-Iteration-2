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
    [SerializeField] private TextMeshProUGUI carTypeText;
    [SerializeField] private TextMeshProUGUI carColorText;
    [SerializeField] private List<Sprite> carSprites;
    [SerializeField] private List<CarStats> carStats;
    [SerializeField] private Slider acceleration;
    [SerializeField] private float minAcceleration, maxAcceleration;
    [SerializeField] private Slider maxSpeed;
    [SerializeField] private float minMaxSpeed, maxMaxSpeed;
    [SerializeField] private Slider braking;
    [SerializeField] private float minBraking, maxBraking;
    [SerializeField] private Slider boost;
    [SerializeField] private float minBoost, maxBoost;
    [SerializeField] private Slider handling;
    [SerializeField] private float minHandling, maxHandling;
    [SerializeField] private Slider drift;
    [SerializeField] private float minDrift, maxDrift;
    
    private int currentCarType = 0;
    private int currentCarColor = 0;
    
    private void Start()
    {
        SetupSliders();
        UpdateCarSprite();
        UpdateCarStats();
    }

    private void SetupSliders()
    {
        acceleration.minValue = minAcceleration;
        acceleration.maxValue = maxAcceleration;
        
        maxSpeed.minValue = minMaxSpeed;
        maxSpeed.maxValue = maxMaxSpeed;

        braking.minValue = minBraking;
        braking.maxValue = maxBraking;

        boost.minValue = minBoost;
        boost.maxValue = maxBoost;

        handling.minValue = minHandling;
        handling.maxValue = maxHandling;

        drift.minValue = minDrift;
        drift.maxValue = maxDrift;

        UpdateSliders();
    }

    private void UpdateSliders()
    {
        CarStats stats = GetCurrentCarStats();
        acceleration.value = stats.acceleration;
        maxSpeed.value = stats.maxSpeed;
        braking.value = stats.brakeFactor;
        boost.value = stats.boostMultiplier;
        handling.value = stats.turnSpeed;
        drift.value = stats.drift;
        carTypeText.text = ((CarType)currentCarType + 1).ToString();
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
        carColorText.text = ((CarColor)currentCarColor).ToString();
    }
    
    private void UpdateCarStats()
    {
        CarStats newStats = GetCurrentCarStats();
        
        carOnGroundBehaviour.SetStatValues((CarType)currentCarType+1, (CarColor)currentCarColor, newStats);
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
