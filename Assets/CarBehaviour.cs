using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CarBehaviour : MonoBehaviour
{
    [SerializeField][Range(0,20)] private int steeringWhenNotMovingFactor;
    [SerializeField] private CarStats carStats;
    [SerializeField] private float engineBrakingFactor;
    [SerializeField] private float engineBrakingSpeed;
    [SerializeField] private float cameraWindowSize = 15;
    private float horizontal;
    private float vertical;
    private bool boostKey;
    private float rot;
    private Rigidbody2D rb;
    private Camera camera;
    private float cameraZOffset;
    private float velocityForward;
    private bool lastDirForward = false;

    public void SetStatValues(CarStats newStats)
    {
        carStats = newStats;
    }
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        camera = Camera.main;
        cameraZOffset = camera.transform.position.z;
    }

    private void FixedUpdate()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        boostKey = Input.GetKey(KeyCode.LeftShift);

        camera.transform.position = transform.position + Vector3.forward*cameraZOffset;
        camera.orthographicSize = cameraWindowSize;
        Acceleration();
        DriftFixer();
        Steering();
    }

    private void Acceleration()
    {
        velocityForward = Vector2.Dot(transform.up, rb.velocity);
        if (velocityForward > 0)
            lastDirForward = true;
        else if (velocityForward == 0 && vertical == 0 && lastDirForward)
            lastDirForward = false;
        
        if (velocityForward > carStats.maxSpeed && vertical > 0)
            return;
        if (velocityForward < -carStats.maxSpeed * 0.5f && vertical < 0)
            return;
        if (rb.velocity.sqrMagnitude > carStats.maxSpeed*carStats.maxSpeed && vertical > 0)
            return;
        
        if (vertical == 0)
        {
            rb.drag = Mathf.Lerp(rb.drag, engineBrakingFactor, Time.fixedDeltaTime * engineBrakingSpeed);
        }
        else rb.drag = 0;
        
        float acc = carStats.acceleration;
        if (boostKey)
            acc *= carStats.boostMultiplier;
        if (vertical < 0)
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
}
