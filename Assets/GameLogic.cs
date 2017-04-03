using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    private BlockGameObject _firstSelected = null;

    void Update()
    {
        switch (GameController.instance.mode)
        {
            case GameController.Mode.Build:
                Build();
                break;

            case GameController.Mode.Play:
                Play();
                break;

            default:
                break;
        }
    }

    void Play()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            if (GameController.instance.GetPointerPos(out hit))
            {
                if (hit.collider.tag == "Block")
                {
                    BlockGameObject go = hit.collider.GetComponent<BlockGameObject>();

                    if (go.IsFree())
                    {
                        if (!_firstSelected)
                        {
                            _firstSelected = go;
                            _firstSelected.selected = true;
                        }
                        else if (go == _firstSelected)
                        {
                            _firstSelected.selected = false;
                            _firstSelected = null;
                        }
                        else if (_firstSelected.type == go.type)
                        {
                            _firstSelected.Remove();
                            go.Remove();
                        }
                    }
                }
            }
        }
    }

    void Build()
    {

    }
}
