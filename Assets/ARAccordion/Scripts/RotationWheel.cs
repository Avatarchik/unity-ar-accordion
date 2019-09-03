﻿using System.Collections.Generic;
using Model;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable] public class UnityEventFloat : UnityEvent<float> { }

public class RotationWheel : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameObject wheelContainer;
    [SerializeField] private GameObject wheelElementPrefab;
    [SerializeField] private BoxCollider2D focusCollider;

    [SerializeField] private UnityEventFloat onUpdateRotationWheel;
    [SerializeField] private float speed = 0.5f;

    private float minY;
    private float maxY;

    private int maxSteps;
    private Vector3 nextStepLocalPosition;
    private float wheelElementHeight;
    private bool moving = false;

    private float step;
    private float start = 0;

    public float Speed { get => speed; set => speed = value; }

    void OnEnable()
    {
        focusCollider.enabled = false;
    }

    internal void Init(List<Layer> layers, int start)
    {
        this.maxSteps = layers.Count;
        this.start = start;

        for (int i = 0; i < layers.Count; i++) {
            GameObject wheelElement = Instantiate(wheelElementPrefab);
            wheelElement.transform.SetParent(wheelContainer.transform, false);

            Layer layer = layers[i];

            Color color = new Color(layer.color[0], layer.color[1], layer.color[2], layer.color[3]);
            wheelElement.GetComponent<Image>().color = color;

            if (layer.icon != null) {
                GameObject icon = wheelElement.transform.Find("Icon").gameObject;
                icon.SetActive(true);

                icon.transform.GetComponent<Image>().sprite = Resources.Load<Sprite>("Icons/" + layer.icon);
            }

            wheelElement.name = "WheelElement" + i;
        }

        wheelElementHeight = wheelElementPrefab.GetComponent<RectTransform>().sizeDelta.y;
        maxY = wheelElementHeight * (maxSteps - 1);

        nextStepLocalPosition = wheelContainer.transform.localPosition;
    }

    void Update()
    {
        if (!moving) {
            wheelContainer.transform.localPosition = Vector3.MoveTowards(wheelContainer.transform.localPosition, nextStepLocalPosition, Time.deltaTime * 200f);
        }

        float step = wheelContainer.transform.localPosition.y / wheelElementHeight;

        if (step != this.step) {
            this.step = step;

            if (this.step % 1 > 0.9) {
                focusCollider.enabled = true;
            }

            onUpdateRotationWheel.Invoke(step);
        }
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        moving = true;

        float newLocalY = Mathf.Max(Mathf.Min(wheelContainer.transform.localPosition.y + eventData.delta.y * Speed, maxY), 0);

        wheelContainer.transform.localPosition = new Vector3(
            wheelContainer.transform.localPosition.x,
            newLocalY,
            wheelContainer.transform.localPosition.z
        );
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        float newLocalY = Mathf.Max(Mathf.Min(wheelContainer.transform.localPosition.y + eventData.delta.y, maxY), 0);

        int step = Mathf.RoundToInt(newLocalY / wheelElementHeight);

        nextStepLocalPosition = new Vector3(
            wheelContainer.transform.localPosition.x,
            step * wheelElementHeight,
            wheelContainer.transform.localPosition.z
        );

        moving = false;
    }

    public void Reset()
    {
        nextStepLocalPosition = new Vector3(
            wheelContainer.transform.localPosition.x,
            this.start * wheelElementHeight,
            wheelContainer.transform.localPosition.z
        );

        wheelContainer.transform.localPosition = nextStepLocalPosition;
    }
}
