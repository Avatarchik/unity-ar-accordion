using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Model;
using System.Linq;

public class Accordion : MonoBehaviour
{
    [Header("Canvas")] [SerializeField] private InfoPopup infoPopUp;

    [SerializeField] private GameObject background;
    [SerializeField] private GameObject original;
    [SerializeField] private GameObject componentAnchors;

    [SerializeField] private float speed = 5.0f;
    private float distanceFactor = 0.5f;
    [SerializeField] private float exponent = 1;

    [SerializeField] private float moveDuration = 1.5f;

    [SerializeField] private Material defaultSpriteMaterial;
    [SerializeField] private Material dofSpriteMaterial;

    private bool towardsCamera = true;

    private GameObject[] components;

    public float step = 0f;

    private bool savedOrigins = false;

    private ARSessionOrigin sessionOrigin;

    private Vector3 initialCameraPosition;
    private Vector3 activeTilePosition;

    private Content content;

    public bool isMoving;

    public float Exponent { get => exponent; set => exponent = value; }

    private GameObject activeComponent;

    public GameObject ActiveComponent { get => activeComponent; }

    void OnEnable()
    {
        components = new GameObject[componentAnchors.transform.childCount];
        for (int i = 0; i < componentAnchors.transform.childCount; i++) {
            components[i] = componentAnchors.transform.GetChild(i).Find("Image").gameObject;
        }

        if (Application.isEditor) {
            UpdateAnchors();
        }

        var dummys = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "DummyObject");
        foreach (GameObject obj in dummys) obj.transform.gameObject.SetActive(false);
    }

    void Start()
    {
        infoPopUp.gameObject.SetActive(true);
        infoPopUp.GetComponent<Canvas>().worldCamera = Camera.main;
        infoPopUp.SetFadeDuration(0.5f);

        background.SetActive(false);
    }

    public IEnumerator MoveToLayer(float moveTo)
    {
        isMoving = true;

        float moveFrom = step;

        float startTime = Time.time;
        float currentDuration = 0.0f;
        float progress = 0.0f;

        while (true) {
            currentDuration = Time.time - startTime;
            progress = currentDuration / moveDuration;

            if (progress <= 1.0f) {
                UpdateStep(Mathf.Lerp(moveFrom, moveTo, progress));
                yield return new WaitForEndOfFrame();
            } else {
                if (moveTo > 0) UpdateStep(moveTo);
                isMoving = false;
                yield break;
            }
        }
    }

    void LateUpdate()
    {
        original.SetActive(step == 0);
        background.SetActive(step > 0);
        componentAnchors.SetActive(step > 0);

        if (step == 0) {
            SetOriginPositions();
        } else {
            SetNewPositions();
        }
    }

    private void SetOriginPositions()
    {
        for (int i = 0; i < components.Length; i++) {
            GameObject tile = components[i];

            tile.transform.localRotation = Quaternion.Euler(0, 0, 0);
            tile.transform.position = Vector3.zero;
        }
    }

    private void SetNewPositions()
    {
        for (int i = 0; i < components.Length; i++) {
            GameObject component = this.components[i];

            float distance = GetDistance(step, i);

            if (towardsCamera) {
                moveTowardsCamera(component, distance);
            } else {
                moveFromOrigin(component, distance);
            }
        }

        if (step > 0) {
            float focusDistance = Vector3.Distance(Camera.main.transform.position, components[components.Length - Mathf.CeilToInt(this.step)].transform.position);
            Camera.main.GetComponentInChildren<PostFX>().UpdateFocusDistance(focusDistance);
        }
    }

    private float GetDistance(float step, int index)
    {
        return Mathf.Pow(step + index, exponent) / Mathf.Pow(components.Length, exponent);
    }

    private void moveFromOrigin(GameObject component, float stepDistance)
    {
        Vector3 origin = component.gameObject.transform.parent.transform.position;

        float distanceToCamera = Mathf.Abs(Vector3.Distance(this.initialCameraPosition, origin));

        Vector3 newLocalPosition = new Vector3(0, 0, -1) * stepDistance * distanceToCamera * distanceFactor;

        component.transform.localPosition = newLocalPosition;
    }

    private void moveTowardsCamera(GameObject component, float stepDistance)
    {
        Vector3 origin = component.gameObject.transform.parent.transform.position;

        Vector3 distanceVector = Camera.main.transform.position - origin;

        Vector3 newPosition = origin + distanceVector * distanceFactor * stepDistance;

        component.transform.position = newPosition;

        if (Vector3.Distance(component.transform.position, origin) > 0.1f) {
            Vector3 newDirection = Vector3.RotateTowards(component.transform.forward, Camera.main.transform.forward, speed * 0.001f * Time.deltaTime, 0.0f);
            component.transform.rotation = Quaternion.LookRotation(newDirection, Camera.main.transform.up);
        } else {
            Vector3 newDirection = Vector3.RotateTowards(component.transform.forward, component.gameObject.transform.parent.transform.forward, speed * 0.01f * Time.deltaTime, 0.0f);
            component.transform.rotation = Quaternion.LookRotation(newDirection, Camera.main.transform.up);
        }
    }

    public void UpdateStep(float step)
    {
        this.step = step;

        if (step > 0) {
            if (step % 1 == 0) {
                UpdateLayerUI();
            }
        } else {
            if (infoPopUp.isActiveAndEnabled) {
                infoPopUp.Hide();
            }
        }

        Highlight();
    }

    private void UpdateLayerUI()
    {
        int layer = components.Length - Mathf.CeilToInt(step);
        activeComponent = components[layer];

        if (infoPopUp.isActiveAndEnabled) {
            infoPopUp.SetAnchor(activeComponent.transform.Find("TagAnchor"));
            infoPopUp.Show(content.accordion.layers[layer].information, "Images/icon" + layer);
        }
    }

    private void Highlight()
    {
        int activeTileIndex = components.Length - Mathf.CeilToInt(step);

        float distanceOfActiveTile = GetDistance(step, activeTileIndex);

        for (int i = 0; i < components.Length; i++) {
            GameObject tile = components[i];
            Color color = tile.GetComponent<Renderer>().material.GetColor("_Color");

            if (i == activeTileIndex) {
                tile.GetComponent<Renderer>().material = dofSpriteMaterial;
                infoPopUp.SetAnchor(tile.transform.Find("TagAnchor"));
            }

            float distanceOfTile = GetDistance(step, i);
            if (distanceOfTile > distanceOfActiveTile) {
                tile.GetComponent<Renderer>().material = defaultSpriteMaterial;
                StartCoroutine(Fade(color.a, 0.5f, 1.0f, tile.GetComponent<Renderer>().material));
            } else {
                tile.GetComponent<Renderer>().material = dofSpriteMaterial;
                StartCoroutine(Fade(color.a, 1.0f, 1.0f, tile.GetComponent<Renderer>().material));
            }
        }
    }

    private IEnumerator Fade(float fadeFrom, float fadeTo, float duration, Material material)
    {
        float startTime = Time.time;
        float currentDuration = 0.0f;
        float progress = 0.0f;

        Color color = material.GetColor("_Color");

        while (true) {
            currentDuration = Time.time - startTime;
            progress = currentDuration / duration;

            if (progress <= 1.0f) {
                material.SetColor("_Color", new Color(color.r, color.g, color.b, Mathf.Lerp(fadeFrom, fadeTo, progress)));
                yield return new WaitForEndOfFrame();
            } else {
                material.SetColor("_Color", new Color(color.r, color.g, color.b, fadeTo));
                yield break;
            }
        }
    }

    public void UpdateAnchors()
    {
        if (initialCameraPosition == null) {
            this.initialCameraPosition = Camera.main.transform.position;
        }
    }

    internal void SetMoveTowardsCamera(bool towardsCamera)
    {
        this.towardsCamera = towardsCamera;
    }

    internal void ShowInfoTag(bool show)
    {
        this.infoPopUp.gameObject.SetActive(show);
    }

    internal void SetContent(Content content)
    {
        this.content = content;
    }
}
