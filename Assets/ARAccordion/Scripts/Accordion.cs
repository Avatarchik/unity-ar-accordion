using System.Collections;
using UnityEngine;
using Model;
using UnityEngine.Events;
using System;

public class Accordion : MonoBehaviour
{
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject original;
    [SerializeField] private GameObject layers;

    [SerializeField] private float speed = 5.0f;
    [SerializeField] private float exponent = 1;

    [SerializeField] private Material defaultSpriteMaterial;
    [SerializeField] private Material dofSpriteMaterial;

    [SerializeField] private UnityEvent onMovementFinish;

    private InfoFactory infoFactory;

    private float distanceFactor = 0.5f;

    private bool towardsCamera = true;

    public float step = 0f;
    private int currentLayerIndex = 0;

    private GameObject currentLayerAnchor = null;
    private Transform currentInfoPointAnchors = null;

    private bool savedOrigins = false;

    private Vector3 activeTilePosition;

    private Content content;
    private int startOffset = 0;

    public bool isMoving;
    private bool infoPointsEnabled;
    private Layer currentLayerData;

    private float focusDistance;
    private float startFocusDistance;

    public float Exponent { get => exponent; set => exponent = value; }
    public float DistanceFactor { get => distanceFactor; set => distanceFactor = value; }

    public GameObject ActiveImageAnchor { get => currentLayerAnchor; }
    public bool InfoPoinsEnabled { get => infoPointsEnabled; set => infoPointsEnabled = value; }

    void OnEnable()
    {
        infoFactory = GetComponent<InfoFactory>();
    }

    void Start()
    {
        this.startFocusDistance = Vector3.Distance(Camera.main.transform.position, transform.position);

        Vector3 distanceVector = Camera.main.transform.position - transform.position;
        Vector3 focusPosition = transform.position + distanceVector * distanceFactor;
        this.focusDistance = Vector3.Distance(Camera.main.transform.position, focusPosition);
    }

    private void SetStep(float step)
    {
        this.step = step;
        Debug.Log("Step: " + this.step);

        if (this.step < startOffset) {
            this.currentLayerIndex = Mathf.FloorToInt(this.step);
        } else {
            this.currentLayerIndex = Mathf.CeilToInt(this.step);
        }

        Debug.Log("Current layer index: " + this.currentLayerIndex);

        this.currentLayerData = this.content.accordion.layers[this.currentLayerIndex];
        Debug.Log("Id: " + this.currentLayerData.id);

        this.currentLayerAnchor = this.layers.transform.GetChild(currentLayerIndex).Find("ImageAnchor").gameObject;
    }

    void Update()
    {
        if (currentLayerData == null) {
            return;
        }

        String currentType = this.currentLayerData.type;

        if (currentType.Equals("start")) {
            original.SetActive(true);
            background.SetActive(false);
            layers.SetActive(false);

            ResetToOriginPositions();

            Camera.main.GetComponentInChildren<PostFX>().UpdateFocusDistance(this.startFocusDistance);
        } else {
            layers.SetActive(true);

            Camera.main.GetComponentInChildren<PostFX>().UpdateFocusDistance(this.focusDistance);
        }

        if (currentType.Equals("behind")) {
            original.SetActive(false);
            background.SetActive(true);
        }

        if (currentType.Equals("before")) {
            original.SetActive(true);
            background.SetActive(false);
        }

        if (currentType.Equals("section")) {
            original.SetActive(false);
            background.SetActive(true);
        }

        UpdateLayers();

        // UpdateInfoPoints();
    }

    private void ResetToOriginPositions()
    {
        for (int i = 0; i < this.layers.transform.childCount; i++) {
            GameObject imageAnchor = this.layers.transform.GetChild(i).Find("ImageAnchor").gameObject;

            imageAnchor.transform.localRotation = Quaternion.Euler(0, 0, 0);
            imageAnchor.transform.localPosition = Vector3.zero;
        }
    }

    private void UpdateLayers()
    {
        for (int i = 0; i < this.layers.transform.childCount; i++) {
            Layer layerData = this.content.accordion.layers[i];

            // if (layerData.type.Equals("section")) {
            //     // HighlightImageSection(i);
            // } else if (layerData.type.Equals("before")) {
            //     UpdateBeforePosition(i);
            // } else if (layerData.type.Equals("behind")) {
            //     UpdateSectionPosition(i);
            //     UpdateMaterial(i);
            // }

            UpdatePosition(i);
            UpdateMaterial(i);
        }
    }

    private void UpdatePosition(int layerIndex)
    {
        GameObject imageAnchor = this.layers.transform.GetChild(layerIndex).Find("ImageAnchor").gameObject;

        float relativeDistance = GetRelativeDistance(step, layerIndex);

        if (towardsCamera) {
            moveTowardsCamera(imageAnchor, relativeDistance);
        } else {
            moveFromOrigin(imageAnchor, relativeDistance);
        }
    }

    private float GetRelativeDistance(float step, int index)
    {
        return Mathf.Pow(step + (this.layers.transform.childCount - index), exponent) / Mathf.Pow(this.layers.transform.childCount, exponent);
    }

    private void moveFromOrigin(GameObject go, float relativeDistance)
    {
        Vector3 origin = go.transform.parent.transform.position;

        float distanceToCamera = Mathf.Abs(Vector3.Distance(Camera.main.transform.position, origin));

        Vector3 newLocalPosition = new Vector3(0, 0, -1) * relativeDistance * distanceToCamera * distanceFactor;

        go.transform.localPosition = newLocalPosition;
    }

    private void moveTowardsCamera(GameObject go, float relativeDistance)
    {
        Vector3 originPosition = go.transform.parent.position;

        Vector3 distanceVector = Camera.main.transform.position - originPosition;

        Vector3 newPosition = originPosition + distanceVector * distanceFactor * relativeDistance;

        go.transform.position = newPosition;

        Vector3 newDirection;
        if (Vector3.Distance(newPosition, originPosition) > 0.2f) {
            newDirection = Vector3.RotateTowards(go.transform.forward, Camera.main.transform.forward, speed * 0.002f * Time.deltaTime, 0.0f);
        } else {
            newDirection = Vector3.RotateTowards(go.transform.forward, go.transform.parent.transform.forward, speed * 0.02f * Time.deltaTime, 0.0f);
        }
        go.transform.rotation = Quaternion.LookRotation(newDirection, new Vector3(0, Camera.main.transform.up.y, 0));
    }

    // private void UpdateMaterial(int layerIndex)
    // {
    //     Layer layerData = this.content.accordion.layers[layerIndex];
    //     GameObject imageAnchor = this.layers.transform.GetChild(layerIndex).Find("ImageAnchor").gameObject;

    //     if (layerData.type == "behind") {
    //         if (this.currentLayerIndex == layerIndex) {
    //             if (this.step >= 9) {
    //                 imageAnchor.GetComponentInChildren<Renderer>().material = dofSpriteMaterial;
    //                 Material material = imageAnchor.GetComponentInChildren<Renderer>().material;
    //                 Color color = material.GetColor("_Color");

    //                 material.SetColor("_Color", new Color(color.r, color.g, color.b, 1));
    //             } else if (this.step >= 8) {
    //                 imageAnchor.GetComponentInChildren<Renderer>().material = defaultSpriteMaterial;
    //                 Material material = imageAnchor.GetComponentInChildren<Renderer>().material;
    //                 Color color = material.GetColor("_Color");

    //                 material.SetColor("_Color", new Color(color.r, color.g, color.b, step % 1));
    //             }
    //         } else {
    //             imageAnchor.GetComponentInChildren<Renderer>().material = defaultSpriteMaterial;
    //             Material material = imageAnchor.GetComponentInChildren<Renderer>().material;
    //             Color color = material.GetColor("_Color");

    //             material.SetColor("_Color", new Color(color.r, color.g, color.b, 0));
    //         }
    //     }
    // }

    private void UpdateMaterial(int layerIndex)
    {
        Renderer renderer = this.layers.transform.GetChild(layerIndex).GetComponentInChildren<Renderer>();

        if (renderer == null) {
            return;
        }

        if (GetRelativeDistance(this.step, layerIndex) <= 1) {
            renderer.material = dofSpriteMaterial;

            Material material = renderer.material;
            Color color = material.GetColor("_Color");
            material.SetColor("_Color", new Color(color.r, color.g, color.b, 1));
        } else {
            renderer.material = defaultSpriteMaterial;

            Material material = renderer.material;
            Color color = material.GetColor("_Color");
            material.SetColor("_Color", new Color(color.r, color.g, color.b, 1 - (step % 1)));
        }
    }

    private void UpdateInfoPoints()
    {
        if (step % 1 == 0) {
            if (infoPointsEnabled) {
                ShowInfoPoints();
            }
        } else {
            HideInfoPoints();
        }
    }

    private void ShowInfoPoints()
    {
        Transform infoPointAnchors = currentLayerAnchor.transform.Find("Anchors");

        if (infoPointAnchors) {
            this.currentInfoPointAnchors = infoPointAnchors;
            infoFactory.CreateInfoPoints(content.accordion.layers[this.currentLayerIndex].infos, this.currentInfoPointAnchors);
        }
    }

    private void HideInfoPoints()
    {
        if (this.currentInfoPointAnchors) {
            infoFactory.ClearInfoPoints(this.currentInfoPointAnchors);
        }
    }

    internal void SetMoveTowardsCamera(bool towardsCamera)
    {
        this.towardsCamera = towardsCamera;
    }

    internal void SetContent(Content content)
    {
        this.content = content;
    }

    public void MoveTo(float to, float duration)
    {
        StartCoroutine(DoMoveTo(to, duration));
    }

    public IEnumerator DoMoveTo(float to, float duration)
    {
        isMoving = true;

        float from = step;

        float startTime = Time.time;
        float currentDuration = 0.0f;
        float progress = 0.0f;

        while (true) {
            currentDuration = Time.time - startTime;

            if (duration > 0) {
                progress = currentDuration / duration;
            } else {
                progress = 1.1f;
            }

            if (progress <= 1.0f) {
                SetStep(Mathf.Lerp(from, to, progress));
                yield return new WaitForEndOfFrame();
            } else {
                if (to > 0) {
                    SetStep(to);
                }
                isMoving = false;
                onMovementFinish.Invoke();
                yield break;
            }
        }
    }

    public void OnUpdateStep(float step)
    {
        SetStep(step);
    }

    internal void SetStartOffset(int offset)
    {
        this.startOffset = offset;
    }

    public void Reset()
    {
        StopAllCoroutines();
        this.step = 0;
    }
}
