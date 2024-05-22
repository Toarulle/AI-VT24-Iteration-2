using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LapTimeUIHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lapText;
    [SerializeField] private TextMeshProUGUI lapTimeText;
    [SerializeField] private Image carImage;

    private Lap lap;
    
    public void SetLapStats(Lap lap, Sprite car)
    {
        this.lap = lap;
        carImage.sprite = car;
        RefreshLapStats();
    }

    public void RefreshLapStats()
    {
        lapText.text = $"Lap {lap.lap.ToString()}";
        lapTimeText.text = $"{lap.lapTime:#0.00}";
        if (lap.penalty)
        {
            lapTimeText.color = Color.red;
        }
        lapTimeText.color = lap.fastest ? Color.green : Color.white;
    }
}