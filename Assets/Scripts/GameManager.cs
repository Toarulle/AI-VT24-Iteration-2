using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Canvas setupCanvas;
    [SerializeField] private Canvas racingCanvas;
    [SerializeField] private CarBehaviour car;
    [SerializeField] private GameObject lapTimePanel;
    [SerializeField] private GameObject lapTimePrefab;
    [SerializeField] private TextMeshProUGUI currentLapText;
    [SerializeField] private TextMeshProUGUI currentLapTimeText;
    [SerializeField] private TextMeshProUGUI totalTimeText;
    [SerializeField] private TextMeshProUGUI currentPenaltyText;

    private LapList race;
    private Camera camera;
    private float cameraZoom;
    private TrackMaker trackMaker;
    private TrackPhotoBehaviour trackPhoto;
    private GameObject lapTimePanelPrefab;
    
    private float lapTime = 0f;
    private float surviveTime = 0f;
    private int currentLap = 0;
    private bool penalty = false;
    private GameState state = GameState.pause;
    private Vector3 cameraOGPos;
    
    private void Start()
    {
        camera = Camera.main;
        cameraOGPos = camera.transform.position;
        cameraZoom = camera.orthographicSize;
        racingCanvas.enabled = false;
        trackMaker = FindObjectOfType<TrackMaker>();
        trackPhoto = FindObjectOfType<TrackPhotoBehaviour>();
        lapTimePanelPrefab = lapTimePanel;
    }

    private void FixedUpdate()
    {
        if (state == GameState.racing && currentLap > 0)
        {
            var dt = Time.fixedDeltaTime;
            lapTime += dt;
            surviveTime += dt;
            UpdateUI();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoToMenu();
        }
    }

    private void UpdateUI()
    {
        currentLapText.text = currentLap.ToString();
        currentLapTimeText.text = $"{lapTime:#0.00}";
        totalTimeText.text = $"{surviveTime:#0.00}";
        currentPenaltyText.text = penalty ? "yes" : "no";
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    
    public void StartRacing()
    {
        setupCanvas.enabled = false;
        racingCanvas.enabled = true;
        car.enabled = true;
        state = GameState.racing;
        lapTime = 0f;
        surviveTime = 0f;
        currentLap = 0;
        UpdateUI();
        for (int i = lapTimePanel.transform.childCount - 1; i >= 0; i--)
        {
            if (lapTimePanel.transform.childCount == 0)
            {
                break;
            }
            Destroy(lapTimePanel.transform.GetChild(i).gameObject);
        }
        race = new LapList(trackMaker.SmoothHull,trackMaker.Seed, car.CarType, car.CarColor, car.GetComponent<SpriteRenderer>().sprite);
        race.saveDirectory = trackPhoto.SaveCameraView($"{race.seed}_{(CarType)car.CarType}");
        trackMaker.SetCarPos();
        var tempRot = car.transform.rotation;
        car.SetStartRot(tempRot);
    }

    private void SaveMapData(LapList data)
    {
        string fileName = race.saveDirectory + "/mapData.json";
        string saveData = JsonUtility.ToJson(data);
        
        File.WriteAllText(fileName, saveData);
    }
    
    public void GoToMenu()
    {
        camera.orthographicSize = cameraZoom;
        camera.transform.position = cameraOGPos;
        car.enabled = false;
        setupCanvas.enabled = true;
        racingCanvas.enabled = false;
        state = GameState.pause;
        SaveMapData(race);
    }

    private void AddLap()
    {
        race.AddLap(new Lap(lapTime,currentLap, penalty, Instantiate(lapTimePrefab, lapTimePanel.transform).GetComponent<LapTimeUIHandler>()));
    }
    
    public void CarDoneLap()
    {
        if (currentLap == 0)
        {
            currentLap++;
            lapTime = 0f;
            surviveTime = 0f;
        }
        else
        {
            AddLap();
            currentLap++;
            lapTime = 0f;
            currentPenaltyText.color = Color.white;
            penalty = false;
        }
    }

    public void GotPenalty()
    {
        penalty = true;
        currentPenaltyText.color = Color.red;
    }
    
    private void OnEnable()
    {
        car.doneLap += CarDoneLap;
        car.gotPenalty += GotPenalty;
    }
    private void OnDisable()
    {
        car.doneLap -= CarDoneLap;
        car.gotPenalty -= GotPenalty;
    }
    
    enum GameState
    {
        racing,
        pause
    }
}

[System.Serializable]
public class LapList
{
    public string saveDirectory;
    public List<Lap> laps;
    public List<Vector2> track;
    public int seed;
    public CarType carType;
    public CarColor carColor;
    public Sprite carSprite;
    public int fastestIndex;
    
    public LapList(List<Vector2> track, int seed, CarType carType, CarColor carColor, Sprite carSprite)
    {
        this.track = track;
        this.seed = seed;
        this.carType = carType;
        this.carColor = carColor;
        this.carSprite = carSprite;
        laps = new List<Lap>();
    }
    
    public void AddLap(Lap lap)
    {
        laps.Add(lap);
        laps.Last().SetUI(carSprite);
        if (laps.Count > 1 && laps.Last().lapTime < laps[fastestIndex].lapTime)
        {
            laps.Last().fastest = true;
            laps[fastestIndex].fastest = false;
            laps.Last().RefreshUI();
            laps[fastestIndex].RefreshUI();
            fastestIndex = laps.IndexOf(laps.Last());
        }
        else if (laps.Count == 1)
        {
            laps.Last().fastest = true;
            fastestIndex = 0;
            laps.Last().RefreshUI();
        }
    }
}
[System.Serializable]
public class Lap
{
    public Lap(float lapTime, int lap, bool penalty, LapTimeUIHandler lapUI)
    {
        this.lapTime = lapTime;
        this.lap = lap;
        this.penalty = penalty;
        this.lapUI = lapUI;
    }

    public void SetUI(Sprite sprite)
    {
        lapUI.SetLapStats(this, sprite);
    }
    public void RefreshUI()
    {
        lapUI.RefreshLapStats();
    }

    private LapTimeUIHandler lapUI;
    public float lapTime;
    public int lap;
    public bool penalty;
    public bool fastest;
}