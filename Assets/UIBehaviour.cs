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
    [SerializeField] private TMP_InputField seedText;
    [SerializeField] private SliderHandler groundWidthSlider; 
    [SerializeField] private SliderHandler groundHeightSlider;
    [SerializeField] private SliderHandler groundWidthOffsetSlider; 
    [SerializeField] private SliderHandler groundHeightOffsetSlider;
    [Header("Dotspreading")] 
    [SerializeField] private SliderHandler dotAmountMiddleSlider;
    [SerializeField] private SliderHandler dotAmountSpreadSlider;
    [Header("Displacement")]
    [SerializeField] private SliderHandler difficultySlider;
    [SerializeField] private SliderHandler maxDisplacement;
    [Header("Push apart")]
    [SerializeField] private SliderHandler pushIterations;
    [SerializeField] private SliderHandler pushWhenDistance;
    [Header("Fix angles")]
    [SerializeField] private SliderHandler angleFixIterations;
    [SerializeField] private SliderHandler largestAngle;
    [Header("CatmullRom")]
    [SerializeField] private SliderHandler smoothingSteps;
    [SerializeField] private SliderHandler subtractIfClose;
    [Header("Track")]
    [SerializeField] private SliderHandler trackWidth;
    
    private void Start()
    {
        seedText.text = trackMaker.Seed.ToString();
        groundWidthSlider.UpdateValue(trackMaker.GroundWidth);
        groundHeightSlider.UpdateValue(trackMaker.GroundHeight);
        groundWidthOffsetSlider.UpdateValue(trackMaker.GroundWidthOffset);
        groundHeightOffsetSlider.UpdateValue(trackMaker.GroundHeightOffset);
        dotAmountMiddleSlider.UpdateValue(trackMaker.DotAmountMiddle);
        dotAmountSpreadSlider.UpdateValue(trackMaker.DotAmountSpread);
        difficultySlider.UpdateValue(trackMaker.Difficulty);
        maxDisplacement.UpdateValue(trackMaker.MaxDisplacement);
        pushIterations.UpdateValue(trackMaker.PushIterations);
        pushWhenDistance.UpdateValue(trackMaker.PushSmallestDistance);
        angleFixIterations.UpdateValue(trackMaker.FixAngleIterations);
        largestAngle.UpdateValue(trackMaker.LargestTurnDegrees);
        smoothingSteps.UpdateValue(trackMaker.SmoothingSteps);
        subtractIfClose.UpdateValue(trackMaker.SubtractIfClose);
        trackWidth.UpdateValue(trackMesh.TrackWidth);
    }

    public void UpdateSeedText()
    {
        seedText.text = trackMaker.Seed.ToString();
    }
}