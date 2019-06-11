using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Accordion : MonoBehaviour
{
    [Header ("Canvas")] [SerializeField] private InfoPopup infoPopUp;

    [Header("Layer")] [SerializeField] GameObject[] tiles;

    [SerializeField] float speed = 1.0f;

    [SerializeField] Material defaultSpriteMaterial;
    [SerializeField] Material dofSpriteMaterial;

    private int step = 0;

    private bool savedOrigins = false;

    private Vector3 initialCameraPosition;
    private Vector3 activeTilePosition;
    private Vector3[] tilesOrigins;

    private bool moveTowardsCamera = false;

    private Dictionary<string, Dictionary<string, string>> content;
    

    void Start()
    {    
        infoPopUp.GetComponent<Canvas>().worldCamera = Camera.main;
        infoPopUp.SetFadeDuration(0.5f);

        Camera.main.GetComponentInChildren<PostFX>().UpdateAperature(20.0f);
        Camera.main.GetComponentInChildren<PostFX>().UpdateFocusDistance(55.0f);

        Highlight();
    }

    void LateUpdate()
    {
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
            Camera.main.GetComponentInChildren<PostProcessLayer>().enabled = false;

            infoPopUp.Hide();
        }

        Highlight();
    }

    private void UpdatePositions() {
        var target = Camera.main.transform.Find("Target").transform;

        for (int i = 0; i < tiles.Length; i++)
        {
            GameObject tile = tiles[i];

            Vector3 newPosition;
            if (step == 0) {
                newPosition = tilesOrigins[i];
            } else if (moveTowardsCamera) {
                newPosition = tilesOrigins[i] + ((target.transform.position - tilesOrigins[i]) * GetDistance(step, i));
                
                Vector3 newDir = Vector3.RotateTowards(tile.transform.forward, Camera.main.transform.forward, 0.3f * Time.deltaTime, 0.0f);
                tile.transform.rotation = Quaternion.LookRotation(newDir, Camera.main.transform.up);

                tile.transform.position = Vector3.MoveTowards(tile.transform.position, newPosition, 0.5f * Time.deltaTime);
            } else {
                float distanceToCamera = Vector3.Distance(tile.transform.InverseTransformPoint(this.initialCameraPosition), tile.transform.InverseTransformPoint(tilesOrigins[0]));

                Debug.Log("Distance to Camera: " + distanceToCamera);

                var newLocalPosition = tile.transform.InverseTransformPoint(tilesOrigins[i]) + (-Vector3.forward * GetDistance(step, i) * distanceToCamera * 0.9f);
                tile.transform.position = Vector3.MoveTowards(tile.transform.position, tile.transform.TransformPoint(newLocalPosition), 0.5f * Time.deltaTime);
            }
        }

        if (step > 0) {
            float distance = Vector3.Distance(Camera.main.transform.position, tiles[tiles.Length - step].transform.position);
            Camera.main.GetComponentInChildren<PostFX>().UpdateFocusDistance(distance);
        }
    }

    private float GetDistance(int step, int index) {
        return Mathf.Pow(step + index, 4) / Mathf.Pow(tiles.Length, 4); 
    }

    private void Highlight() {
        if (step > 0) {
            for (int i = 0; i < tiles.Length; i++)
            {
                GameObject tile = tiles[i];
                Color color = tile.GetComponent<Renderer>().material.GetColor("_Color");
                tile.GetComponent<Renderer>().material = defaultSpriteMaterial;
                tile.GetComponent<Renderer>().material.SetColor("_Color", new Color(color.r, color.g, color.b, 0.5f));
            }

            GameObject activeTile = tiles[tiles.Length - step];
            Color activeTileColor = activeTile.GetComponent<Renderer>().material.GetColor("_Color");
            activeTile.GetComponent<Renderer>().material = dofSpriteMaterial;
            activeTile.GetComponent<Renderer>().material.SetColor("_Color", new Color(activeTileColor.r, activeTileColor.g, activeTileColor.b, 1.0f));
            
            infoPopUp.SetFadeDuration(0.5f);
            infoPopUp.SetAnchor(activeTile.transform.Find("TagAnchor"));
        } else {
            for (int i = 0; i < tiles.Length; i++)
            {
                GameObject tile = tiles[i];
                Color color = tile.GetComponent<Renderer>().material.GetColor("_Color");
                tile.GetComponent<Renderer>().material = defaultSpriteMaterial;
                tile.GetComponent<Renderer>().material.SetColor("_Color", new Color(color.r, color.g, color.b, 0.0f));
            }
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