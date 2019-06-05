﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Quiz : MonoBehaviour, IDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{	
	private GameObject detailPopup;
	private Color heighlightCorrect = new Color (0,200,0);
	private Color heightlightWrong = new Color (200,0,0);

	private string correctAnswer = "Weisheit";
	private GameObject activeTile;
	private Vector3 tileStartPosition;

	bool fadeRunning;

	bool answerGiven;


	private void Start()
	{
		detailPopup = GameObject.Find("InformationContainer");
		detailPopup.GetComponent<CanvasGroup>().alpha = 0;

	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (activeTile == null)
		{
			activeTile = eventData.pointerEnter;
			tileStartPosition = activeTile.transform.position;

			if (activeTile.tag == "AnswerContainer")
			{
				activeTile.transform.localScale = new Vector3(1.5f,1.5f,1.5f);
			}
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		Vector3 worldPoint;
        
		if (activeTile != null)
		{
			if (RectTransformUtility.ScreenPointToWorldPointInRectangle(activeTile.GetComponent<RectTransform>(),
																	eventData.position,
																	eventData.pressEventCamera,
																	out worldPoint))
			{
				if (activeTile.tag == "AnswerContainer")
				{
					activeTile.transform.localScale = new Vector3(1,1,1);
					activeTile.transform.position = worldPoint;
				}
			}
		}
	}

	public void OnDrop(PointerEventData eventData)
	{
		if (eventData.pointerEnter.tag == "DropArea")
		{
			answerGiven = true;
			activeTile.transform.position = eventData.pointerEnter.transform.position;
			checkAnswer();		

		}
		else 
		{
			activeTile.transform.position = tileStartPosition;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (activeTile != null)
		{
			if (activeTile.tag == "AnswerContainer" && !answerGiven)
			{
				Debug.Log("snapped away");
				activeTile.transform.position = tileStartPosition;
				activeTile.GetComponent<Image>().color = new Color (100,100,100);
				activeTile.transform.localScale = new Vector3(1,1,1);
			}
			activeTile = null;
		}
	}

	private void checkAnswer() 
	{
		string currentAnswer = activeTile.GetComponentInChildren<Text>().text;

		if (currentAnswer == correctAnswer)
		{
			Debug.Log("Right");
			activeTile.GetComponent<Image>().color = heighlightCorrect;
			StartCoroutine(Fade(0.0f,1.0f,0.7f));

		}
		else
		{
			Debug.Log("Wrong");
			activeTile.GetComponent<Image>().color = heightlightWrong;
		}
	}

	private IEnumerator Fade(float fadeFrom, float fadeTo, float duration)
    {
        fadeRunning = true;

        float startTime = Time.time;
        float currentDuration = 0.0f;
        float progress = 0.0f;

        while (true)
        {
            currentDuration = Time.time - startTime;
            progress = currentDuration / duration;

            if (progress <= 1.0f) {
                detailPopup.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(fadeFrom, fadeTo, progress);
                yield return new WaitForEndOfFrame();
            } else {
                detailPopup.GetComponent<CanvasGroup>().alpha = fadeTo;
                fadeRunning = false;
                yield break;
            }
        }
    }
}
