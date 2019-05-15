﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.iOS;

public class Accordion : MonoBehaviour
{

    public GameObject infoPopup;

    [Header("Layer")]
    [SerializeField] GameObject[] tiles;

    [SerializeField] float factor = 1.0f;

    private int step = 0;

    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetAxis("Mouse ScrollWheel") < 0) {
            if (step > 0) {
                step--;
                infoPopup.GetComponent<infoPopup>().UpdateInformation(step);
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetAxis("Mouse ScrollWheel") > 0) {
            if (step < tiles.Length) { 
               step++;
               infoPopup.GetComponent<infoPopup>().UpdateInformation(step);
            }
        }

        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);
            Debug.Log(touch.position);

            if (touch.phase == TouchPhase.Ended) {
                if (touch.position.x < 1000) {
                    if (step > 0) { 
                        step--;
                        infoPopup.GetComponent<infoPopup>().UpdateInformation(step);

                    }
                } else {
                    if (step < tiles.Length) { 
                        step++;
                        infoPopup.GetComponent<infoPopup>().UpdateInformation(step);

                    }
                }
            }   
        }
                
        Highlight();
        UpdatePositions();
    }

    private void UpdatePositions() {
        for (int i = 0; i < tiles.Length; i++)
        {
            GameObject tile = tiles[i];

            tile.transform.localPosition = Vector3.MoveTowards(
                tile.transform.localPosition, 
                new Vector3(
                    tile.transform.localPosition.x, 
                    tile.transform.localPosition.y, 
                    -GetPositionZ(step, i)), 
                20.0f * Time.deltaTime
            );    
        }
    }

    private float GetPositionZ(int step, int index) {
        if (step == 0) {
            return 0.0f;
        }
        return Mathf.Pow((step + index) * factor, 3);
    }

    private void Highlight() {
        for (int i = 0; i < tiles.Length; i++)
        {
            GameObject tile = tiles[i];
            Color color = tile.GetComponent<Renderer>().material.GetColor("_Color");

            tile.GetComponent<Renderer>().material.SetColor("_Color", new Color(color.r, color.g, color.b, 0.5f));
        }

        if (step > 0) {
            GameObject activeTile = tiles[tiles.Length - step];
            Color activeTileColor = activeTile.GetComponent<Renderer>().material.GetColor("_Color");
            activeTile.GetComponent<Renderer>().material.SetColor("_Color", new Color(activeTileColor.r, activeTileColor.g, activeTileColor.b, 1.0f));
        }
    }
}
