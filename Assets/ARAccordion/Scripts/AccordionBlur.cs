using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class AccordionBlur : MonoBehaviour
{
    public GameObject infoPopup;

    [Header("Layer")]
    [SerializeField] GameObject[] tiles;

    [SerializeField] float scaleFactorZ = 1.0f;
    [SerializeField] float speedFactor = 1.0f;

    Vector3 targetPosition;

    private int step = 0;

    void Start()
    {    
        for (int i = 0; i < tiles.Length; i++)
        {
            GameObject tile = tiles[i];
            tile.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    void Update()
    {
        Highlight();
        UpdatePositions();
    }

    public void UpdateStep(int step) {
        this.step = step;
    }

    private void UpdatePositions() {
        if (step == 0) {
            return;
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            GameObject tile = tiles[i];

            Debug.Log(targetPosition);

            tile.transform.localPosition = Vector3.MoveTowards(
                tile.transform.localPosition, 
                targetPosition * GetDistance(step, i),
                speedFactor * Time.deltaTime
            );    
        }
    }

    private float GetDistance(int step, int index) {
        if (step == 0) {
            return 0.0001f * index + 0.0001f;
        }
        Debug.Log(Mathf.Pow((step + index) * scaleFactorZ, 3));
        return Mathf.Pow((step + index) * scaleFactorZ, 3);
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
        }
    }

    public void SetScaleFactorZ(float factor) {
        scaleFactorZ = factor;
    }

    public void SetTargetPosition(Vector3 targetPosition) {
        this.targetPosition = targetPosition;
    }
}
