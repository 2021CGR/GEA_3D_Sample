using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera virualCam;  // 기본 TPS 카메라

    public CinemachineFreeLook freeLookCam;  // 자유 회전 TPS 카메라

    public bool usingFreeLook = false;

    void Start()
    {
        //시작은 Virtual Camera 활성화
        virualCam.Priority = 10;
        freeLookCam.Priority = 0;
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(1)) //우클릭
        {
            usingFreeLook = !usingFreeLook;
            if (usingFreeLook)
            {
                freeLookCam.Priority = 20;   // FreeLook 활성화 
                virualCam.Priority = 0;
            }
            else
            {
                virualCam.Priority = 20;     // Virtual Camera 활성화
                freeLookCam.Priority = 0;
            }

        }
    }
}
