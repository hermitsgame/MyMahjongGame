using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public enum Mode { Disabled, Play, Build }

    public Mode mode = Mode.Build;

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

    public bool GetPointerPos(out RaycastHit hit)
    {
        hit = new RaycastHit();
        Ray camRay = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(camRay, out hit))
        {
            return true;
        }
        return false;
    }
}
