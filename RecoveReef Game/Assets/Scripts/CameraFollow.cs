using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Camera myCamera;
    private Func<Vector3> GetCameraFollowPositionFunc;
    private Func<float> GetCameraZoomFunc;
    
    public void Setup(Func<Vector3> GetCameraFollowPositionFunc, Func<float> GetCameraZoomFunc)
    {
        this.GetCameraFollowPositionFunc = GetCameraFollowPositionFunc;
        this.GetCameraZoomFunc = GetCameraZoomFunc;
    }

    // Start is called before the first frame update
    private void Start() {
        myCamera = transform.GetComponent<Camera>();
    }

    public void SetCameraFollowPosition(Vector3 cameraFollowPosition) {
        SetGetCameraFollowPositionFunc(() => cameraFollowPosition);
    }

    public void SetGetCameraFollowPositionFunc(Func<Vector3> GetCameraFollowPositionFunc) {
        this.GetCameraFollowPositionFunc = GetCameraFollowPositionFunc;
    }
    public void SetCameraZoom (float cameraZoom) {
        SetGetCameraZoomFunc(() => cameraZoom);
    }
    public void SetGetCameraZoomFunc(Func<float> GetCameraZoomFunc) {
        this.GetCameraZoomFunc = GetCameraZoomFunc;
    }

    // Update is called once per frame
    void Update() {
        HandleZoom();
        HandleMovement();
    }

    private void HandleMovement() {
        Vector3 cameraFollowPosition = GetCameraFollowPositionFunc();
        cameraFollowPosition.z = transform.position.z;

        Vector3 cameraMoveDir = (cameraFollowPosition - transform.position).normalized;
        float distance = Vector3.Distance(cameraFollowPosition, transform.position);
        float cameraMoveSpeed = 1.5f;

        if (distance > 0) {
            print("transform.position: " + transform.position);
            print("cameraFollowPosition: " + cameraFollowPosition);
            Vector3 newCameraPosition = transform.position + cameraFollowPosition * distance * cameraMoveSpeed * Time.deltaTime;
            float distanceAfterMoving = Vector3.Distance(newCameraPosition, cameraFollowPosition);
            if (distanceAfterMoving > distance) {
                newCameraPosition = cameraFollowPosition;
            }
            transform.position = new Vector3(
                Mathf.Clamp(newCameraPosition.x,-75.0f,75.0f),
                Mathf.Clamp(newCameraPosition.y,-75.0f,75.0f),
                newCameraPosition.z
            );
        }  
    }

    private void HandleZoom() {
        float cameraZoom = GetCameraZoomFunc();
        float cameraZoomDifference = cameraZoom - myCamera.orthographicSize;
        float cameraZoomSpeed = 1f;
        myCamera.orthographicSize += cameraZoomDifference * cameraZoomSpeed * Time.deltaTime;

        if (cameraZoomDifference > 0) {
            if (myCamera.orthographicSize > cameraZoom) {
                myCamera.orthographicSize = cameraZoom;
            }
        } else {
            if (myCamera.orthographicSize < cameraZoom) {
                myCamera.orthographicSize = cameraZoom;
            }
        }
    }
}
