using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Accordion : MonoBehaviour
{
    [Header ("Canvas")]
    [SerializeField] private InfoPopup infoPopUp;

    [Header("Layer")]
    [SerializeField] GameObject[] tiles;

    [SerializeField] float speed = 1.0f;

    private Vector3 initialCameraPosition;

    private int step = 0;
    private Vector3 activeTilePosition;

    private Vector3[] tilesOrigins;

    private bool moveTowardsCamera = false;

    private bool savedOrigins = false;

    private Dictionary<string, Dictionary<string, string>> content;
    

    void Start()
    {    
        infoPopUp.GetComponent<Canvas>().worldCamera = Camera.main;
        infoPopUp.SetFadeDuration(0.5f);
    }

    void LateUpdate()
    {
        Highlight();
        
        if (tilesOrigins != null) {
            UpdatePositions();
        }
    }

    public void UpdateStep(int step) {
        this.step = step;

        if (this.step > 0) {
            if (!savedOrigins) {
                tilesOrigins = new Vector3[tiles.Length];
                for (int i = 0; i < tiles.Length; i++)
                {
                    GameObject tile = tiles[i];
                    tilesOrigins[i] = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z);
                }

                savedOrigins = true;
            }

            if (initialCameraPosition == null) {
                this.initialCameraPosition = Camera.main.transform.position;
            }

            GameObject activeTile = tiles[tiles.Length - step];
            infoPopUp.SetAnchor(activeTile.transform.Find("TagAnchor"));
            infoPopUp.Show(tiles.Length - step);
        } else {
            infoPopUp.Hide();
        }
    }

    private void UpdatePositions() {
        for (int i = 0; i < tiles.Length; i++)
        {
            GameObject tile = tiles[i];

            Vector3 newTarget;
            if (step == 0) {
                newTarget = tilesOrigins[i];
            } else if (moveTowardsCamera) {
                newTarget = tilesOrigins[i] + ((Camera.main.transform.position - tilesOrigins[i]) * GetDistance(step, i));
            } else {
                newTarget = tilesOrigins[i] + ((initialCameraPosition - tilesOrigins[i]) * GetDistance(step, i));
            }

            tile.transform.position = Vector3.MoveTowards(
                tile.transform.position,
                newTarget,
                speed * Time.deltaTime
            );
        }
    }

    private float GetDistance(int step, int index) {
        return Mathf.Pow(step + index, 4) / Mathf.Pow(tiles.Length + 1.0f, 4); 
    }

    private void Highlight() {
        for (int i = 0; i < tiles.Length; i++)
        {
            GameObject tile = tiles[i];
            Color color = tile.GetComponent<Renderer>().material.GetColor("_Color");

            tile.GetComponent<Renderer>().material.SetColor("_Color", new Color(color.r, color.g, color.b, 0.3f));
        }

        if (step > 0) {
            GameObject activeTile = tiles[tiles.Length - step];
            Color activeTileColor = activeTile.GetComponent<Renderer>().material.GetColor("_Color");
            activeTile.GetComponent<Renderer>().material.SetColor("_Color", new Color(activeTileColor.r, activeTileColor.g, activeTileColor.b, 1.0f));
            
            infoPopUp.SetFadeDuration(0.5f);
            infoPopUp.SetAnchor(activeTile.transform.Find("TagAnchor"));
        }
    }

    internal void SetMoveTowardsCamera(bool moveTowardsCamera)
    {
        this.moveTowardsCamera = moveTowardsCamera;
    }

    internal void SetContent(Dictionary<string, Dictionary<string, string>> content)
    {
        this.content = content;
        infoPopUp.SetContent(content);
    }
}
