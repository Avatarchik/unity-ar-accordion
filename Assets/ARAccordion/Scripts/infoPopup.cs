﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InfoPopup : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.7f;
    [SerializeField] private GameObject image;
    [SerializeField] private Text text;

    Dictionary<string, Dictionary<string, string>> content;

    public void SwitchLayer(int layer)
    {
        StartCoroutine(Fade(1.0f, 0.0f, fadeDuration, layer));
    }
    
    private IEnumerator Fade(float fadeFrom, float fadeTo, float fadeDuration, int layer)
    {
        float startTime = Time.time;
        float currentTime = Time.time - startTime;
        float progress = currentTime / fadeDuration;

        while (progress < 1.0f)
        {
            currentTime = Time.time - startTime;
            progress = currentTime / fadeDuration;

            float currentAlphaValue = Mathf.Lerp(fadeFrom, fadeTo, progress);

            this.GetComponent<CanvasGroup>().alpha = currentAlphaValue;

            yield return new WaitForEndOfFrame();
        }

        if (this.GetComponent<CanvasGroup>().alpha == 0)
        {
            UpdateInformation(layer);
        }
    }

    private void UpdateInformation(int layer)
    {
        text.gameObject.GetComponent<Text>();
        text.text = content["layer" + layer]["information"];

        ChangeImage(layer);

        StartCoroutine(Fade(0.0f, 1.0f, fadeDuration, 0));
    }

    private void ChangeImage(int layer)
    {
        Sprite sprite = Resources.Load<Sprite>("Images/icon" + layer) ;
        image.GetComponent<Image>().sprite = sprite;
    }

    public void TogglePopup()
    {
        if (this.gameObject.activeInHierarchy)
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(true);
        }
    }

    public void SetContent(Dictionary<string, Dictionary<string, string>> content) {
        this.content = content;
    }
}