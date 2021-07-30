using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
   
    [Serializable]
    private struct AttitudePart
    {
        public float maxSpeed;
        public float acceleration;
        [HideInInspector] public float currentSpeed;
        public float maxFlapAngle;
        public float flapAcceleration;
        [HideInInspector] public float currentFlapAngle;

        public AttitudePart(float _maxSpeed, float _acceleration, float _maxFlapAngle, float _flapAcceleration)
        {
            this.maxSpeed = _maxSpeed;
            this.acceleration = _acceleration;
            this.maxFlapAngle = _maxFlapAngle;
            this.flapAcceleration = _flapAcceleration;
            this.currentSpeed = 0f;
            this.currentFlapAngle = 0f;
        }
    }

    [SerializeField] float speed = 200f;

    //Pitch
    [SerializeField] AttitudePart planePitch = new AttitudePart(35f, 35f, 45f, 120f);
    [SerializeField] private Transform BLFlap;
    [SerializeField] private Transform BRFlap;
    private Vector3 pitchFlapRotationAxis;
    //Roll
    [SerializeField] AttitudePart planeRoll = new AttitudePart(100f, 100f, 45f, 120f);
    [SerializeField] private Transform FLFlap;
    [SerializeField] private Transform FRFlap;
    private Vector3 FLFlapRotationAxis;
    private Vector3 FRFlapRotationAxis;
    //Yaw
    [SerializeField] AttitudePart planeYaw = new AttitudePart(35f, 35f, 25f, 60f);
    [SerializeField] private Transform Rudder;
    private Vector3 yawFlapRotationAxis;
    
    void ShowFPSInConsole()
    {
        float fps = 1 / Time.unscaledDeltaTime;
        print(fps);
        
    }

    private void Start()
    {
        //Set the flap rotation axis
        //Roll
        FLFlapRotationAxis = new Vector3(1.7f, 0f, -15.6f);
        FLFlapRotationAxis.Normalize();
        FRFlapRotationAxis = new Vector3(1.7f, 0f, 15.6f);
        FRFlapRotationAxis.Normalize();
        //Yaw
        yawFlapRotationAxis = new Vector3(-7.2511f, 11.831f, 0f);
        yawFlapRotationAxis.Normalize();
        //Pitch
        pitchFlapRotationAxis = Vector3.forward;
    }

    private float AccelerateToSpeed(float currentSpeed, float targetSpeed, float acceleration)
    {
        float speedDelta = Mathf.Abs(targetSpeed - currentSpeed);
        float realAcceleration = acceleration * Time.deltaTime;
        
        if (realAcceleration < speedDelta) { currentSpeed += acceleration * Time.deltaTime * (currentSpeed < targetSpeed ? 1 : -1); }
        else { currentSpeed += speedDelta * (currentSpeed < targetSpeed ? 1 : -1); }

        return currentSpeed;
    }

    void Update()
    {
        planePitch = CalculateAttitudePart(planePitch, KeyCode.W, KeyCode.S);
        planeRoll = CalculateAttitudePart(planeRoll, KeyCode.A, KeyCode.D);
        planeYaw = CalculateAttitudePart(planeYaw, KeyCode.Q, KeyCode.E);

        Vector3 turnDelta = new Vector3();
        turnDelta.z = -planePitch.currentSpeed * Time.deltaTime;
        turnDelta.x = planeRoll.currentSpeed * Time.deltaTime;
        turnDelta.y = -planeYaw.currentSpeed * Time.deltaTime;

        transform.Rotate(turnDelta);
        SetFlapRotations();

        transform.position += (transform.right * speed * Time.deltaTime);

       // ShowFPSInConsole();
    }

    private AttitudePart CalculateAttitudePart(AttitudePart attitudePart, KeyCode positiveKey, KeyCode negativeKey)
    {
        if (Input.GetKey(positiveKey))
        {
            attitudePart.currentSpeed = AccelerateToSpeed(attitudePart.currentSpeed, attitudePart.maxSpeed, attitudePart.acceleration);
            attitudePart.currentFlapAngle = AccelerateToSpeed(attitudePart.currentFlapAngle, attitudePart.maxFlapAngle, attitudePart.flapAcceleration);
        }
        else if (Input.GetKey(negativeKey))
        {
            attitudePart.currentSpeed = AccelerateToSpeed(attitudePart.currentSpeed, -attitudePart.maxSpeed, attitudePart.acceleration);
            attitudePart.currentFlapAngle = AccelerateToSpeed(attitudePart.currentFlapAngle, -attitudePart.maxFlapAngle, attitudePart.flapAcceleration);
        }
        else
        {
            attitudePart.currentSpeed = AccelerateToSpeed(attitudePart.currentSpeed, 0f, attitudePart.acceleration * 3f);
            attitudePart.currentFlapAngle = AccelerateToSpeed(attitudePart.currentFlapAngle, 0f, attitudePart.flapAcceleration);
        }
        return attitudePart;
    }

    private void SetFlapRotations()
    {
        FLFlap.localRotation = Quaternion.AngleAxis(planeRoll.currentFlapAngle, FLFlapRotationAxis);
        FRFlap.localRotation = Quaternion.AngleAxis(planeRoll.currentFlapAngle, FRFlapRotationAxis);
        BLFlap.localRotation = Quaternion.AngleAxis(planePitch.currentFlapAngle, pitchFlapRotationAxis);
        BRFlap.localRotation = Quaternion.AngleAxis(planePitch.currentFlapAngle, pitchFlapRotationAxis);
        Rudder.localRotation = Quaternion.AngleAxis(planeYaw.currentFlapAngle, yawFlapRotationAxis);
    }

    void OnCollisionEnter(Collision collision)
    {
        GetComponent<PlayerController>().Kill();

    }

}

