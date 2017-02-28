using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    private Camera mainCam;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }

        mainCam = Camera.main;
    }

    public bool GetPointerPos(out Vector3 pos)
    {
        pos = Vector3.zero;
        RaycastHit hit;
        Ray camRay = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(camRay, out hit))
        {
            pos = hit.point;
            return true;
        }
        return false;
    }
}
