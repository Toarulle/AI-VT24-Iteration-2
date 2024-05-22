using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class CarBehaviour : MonoBehaviour
{
    public UnityAction doneLap = delegate {};
    public UnityAction gotPenalty = delegate {};
    
    [SerializeField][Range(0,20)] private int steeringWhenNotMovingFactor;
    [SerializeField] private CarStats carStats;
    [SerializeField] private float engineBrakingFactor;
    [SerializeField] private float engineBrakingSpeed;
    [SerializeField] private float cameraWindowSize = 15;
    [SerializeField] private LayerMask groundLayer;
    private float horizontal;
    private float vertical;
    private bool boostKey;
    private float rot;
    private Rigidbody2D rb;
    private Camera camera;
    private float cameraZOffset;
    private float velocityForward;
    private bool lastDirForward = false;
    private Quaternion startRotation = Quaternion.identity;
    private bool startSetup = true;
    private List<Vector3> gizmostuff = new List<Vector3>();
    private CarType carType;
    private CarColor carColor;
    private AudioSource audio;
    private float audioVolume = 0f;

    public CarType CarType
    {
        get => carType;
        set => carType = value;
    }

    public CarColor CarColor
    {
        get => carColor;
        set => carColor = value;
    }

    public void SetStatValues(CarType type, CarColor color, CarStats newStats)
    {
        carType = type;
        carColor = color;
        carStats = newStats;
        audioVolume = 0f;
        audio = GetComponent<AudioSource>();
        audio.clip = carStats.engineSound;
    }

    public void SetStartRot(Quaternion rot)
    {
        startRotation = rot;
        rb = GetComponent<Rigidbody2D>();
        rb.MoveRotation(startRotation);
        startSetup = false;
        this.rot = rb.rotation;
    }
    
    
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audio = GetComponent<AudioSource>();
        camera = Camera.main;
        cameraZOffset = camera.transform.position.z;
        audioVolume = 0f;
        audio.loop = true;
        audio.Play();
    }

    private void FixedUpdate()
    {
        if (startSetup)
            return;
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        boostKey = Input.GetKey(KeyCode.LeftShift);
        audio.pitch = 1f;
        if (boostKey)
        {
            audio.pitch = 1.12f;
        }

        camera.transform.position = transform.position + Vector3.forward*cameraZOffset;
        camera.orthographicSize = cameraWindowSize;
        Acceleration();
        DriftFixer();
        Steering();
        //CheckOnTrack();
    }

    private void EngineSound(float acc)
    {
        audioVolume = Mathf.Lerp(audioVolume, Mathf.Clamp(Mathf.Abs(velocityForward/(carStats.maxSpeed/2)),0,0.75f), Time.deltaTime*acc);
        audio.volume = audioVolume;
        //audio.pitch = Mathf.Lerp(audio.pitch, maxpitch, Time.deltaTime * acc);
    }
    
    private bool CheckOnTrack()
    {
        bool allOutsideTrack = true;
        Vector3 vertical = new Vector2(0, 0.6f)*transform.forward;
        Vector3 horizontal = new Vector2(0.28f, 0)*transform.forward;
        List<Vector2> corners = new List<Vector2>();
        corners.Add(vertical - horizontal);
        corners.Add(vertical + horizontal);
        corners.Add(- vertical - horizontal);
        corners.Add(- vertical + horizontal);
        gizmostuff = new List<Vector3>();
        gizmostuff.Add(corners[0]);
        gizmostuff.Add(corners[1]);
        gizmostuff.Add(corners[2]);
        gizmostuff.Add(corners[3]);
        RaycastHit2D hit;
        foreach (var corner in corners)
        {
            Ray ray = new Ray(transform.position + (Vector3)corner+Vector3.back, Vector3.forward);
            hit = Physics2D.GetRayIntersection(ray,5f,groundLayer);
            if (!hit.transform.CompareTag("Ground"))
            {
                allOutsideTrack = false;
            }
            Debug.Log($"{hit.collider.gameObject.name}, {hit.collider.tag}");
        }
        if (allOutsideTrack)
        {
            gotPenalty.Invoke();
        }

        return allOutsideTrack;
    }
    
    private void Acceleration()
    {
        var maxSpeed = carStats.maxSpeed;
        float acc = carStats.acceleration;
        
        if (boostKey)
        {
            acc *= carStats.boostMultiplier;
            maxSpeed *= 1.25f;
        }
        velocityForward = Vector2.Dot(transform.up, rb.velocity);
        if (velocityForward > 0)
            lastDirForward = true;
        else if (velocityForward == 0 && vertical == 0 && lastDirForward)
            lastDirForward = false;
        
        EngineSound(acc);
        
        if (velocityForward > maxSpeed && vertical > 0)
            return;
        if (velocityForward < -maxSpeed * 0.5f && vertical < 0)
            return;
        if (rb.velocity.sqrMagnitude > maxSpeed*maxSpeed && vertical > 0)
            return;
        
        if (vertical == 0)
        {
            rb.drag = Mathf.Lerp(rb.drag, engineBrakingFactor, Time.fixedDeltaTime * engineBrakingSpeed);
        }
        else rb.drag = 0;
        

        if (velocityForward > 0 && vertical < 0)
            acc *= carStats.brakeFactor;
        if (velocityForward < 0 && vertical > 0)
            acc *= carStats.brakeFactor;
        
        if (lastDirForward && vertical < 0)
        {
            Vector2 accVector = transform.up * (vertical * acc);
            if (velocityForward > 0)
            {
                rb.AddForce(accVector, ForceMode2D.Force);
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }
        else
        {
            Vector2 accVector = transform.up * (vertical * acc);
            rb.AddForce(accVector, ForceMode2D.Force);
        }
    }

    private void Steering()
    {
        float turnWithoutSpeed = rb.velocity.magnitude / steeringWhenNotMovingFactor;
        turnWithoutSpeed = Mathf.Clamp01(turnWithoutSpeed);
        rot -= horizontal * carStats.turnSpeed * turnWithoutSpeed;
        rb.MoveRotation(rot);
    }
    
    private void DriftFixer()
    {
        Vector2 forwardVel = transform.up * Vector2.Dot(rb.velocity, transform.up);
        Vector2 rightVel = transform.right * Vector2.Dot(rb.velocity, transform.right);

        rb.velocity = forwardVel + rightVel * carStats.drift;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("StartGrid"))
        {
            doneLap.Invoke();
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < gizmostuff.Count; i++)
        {
            Gizmos.DrawSphere(transform.position + gizmostuff[i] + Vector3.back,0.2f);
        }
    }
}
