using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseXSensitivity = 100f;

    public Transform playerBody;

    float xRotation = 0f;

    public bool IsPause;

    // Start is called before the first frame update
    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        Observable.EveryUpdate().Subscribe(_ =>
        {
            if (IsPause) return;
            float mouseX = Input.GetAxis("Mouse X") * mouseXSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseXSensitivity * Time.deltaTime;

            if (mouseY < -150) return;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }).AddTo(this);
    }
}