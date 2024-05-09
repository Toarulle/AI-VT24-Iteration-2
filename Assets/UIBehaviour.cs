using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBehaviour : MonoBehaviour
{
    [SerializeField] private TrackMaker trackMaker;
    [SerializeField] private TrackMesh trackMesh;

    [Header("Basesettings")]
    [SerializeField] private TextMeshProUGUI seedText;
    [SerializeField] private SliderHandler groundWidthSlider; 
    [SerializeField] private SliderHandler groundHeightSlider;
    [SerializeField] private Slider groundWidthOffsetSlider; 
    [SerializeField] private Slider groundHeightOffsetSlider;
    [Header("Dotspreading")] 
    [SerializeField] private Slider dotAmountMiddleSlider;
    [SerializeField] private Slider dotAmountSpreadSlider;
    [Header("Displacement")]
    [SerializeField] private Slider difficultySlider;
    [SerializeField] private Slider maxDisplacement;
    [Header("Push apart")]
    [SerializeField] private Slider pushIterations;
    [SerializeField] private Slider pushWhenDistance;
    [Header("Fix angles")]
    [SerializeField] private Slider angleFixIterations;
    [SerializeField] private Slider largestAngle;
    [Header("CatmullRom")]
    [SerializeField] private Slider smoothingSteps;
    [SerializeField] private Slider divideByIfClose;

    private void Start()
    {
        seedText.text = trackMaker.seed.ToString();
        groundWidthSlider.UpdateValue(trackMaker.groundWidth);
        groundHeightSlider.UpdateValue(trackMaker.groundHeight);
    }
}