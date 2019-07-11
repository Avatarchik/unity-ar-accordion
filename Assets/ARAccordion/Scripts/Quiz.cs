﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Model;
using System;
using System.Linq;

[RequireComponent(typeof(Image))]
public class Quiz : MonoBehaviour, IDragHandler, IDropHandler
{
    [SerializeField] private GameObject[] answerContainers;
    [SerializeField] private GameObject dropArea;

    [SerializeField] private Text questionText;

    [SerializeField] private Color rightColor = new Color(0, 200, 0);
    [SerializeField] private Color wrongColor = new Color(200, 0, 0);
    [SerializeField] private Color defaultColor = new Color(255, 255, 255);

    [SerializeField] private float scaleFactor = 1.2f;
    [SerializeField] private float defaultScaleFactor = 1.0f;
    [SerializeField] private float nextQuestionDelay = 1.5f;

    private int correctAnswers = 0;
    private int maxQuestions = 5;

    private Model.Accordion content;

    private List<KeyValuePair<int, Layer>> pickedLayers = new List<KeyValuePair<int, Layer>>();

    private int currentQuestionIndex = 0;

    private GameObject activeDraggable;
    private Vector3 activeDraggableStartPosition;
    bool questionAnswered;

    int pickedQuestion;


    void Start()
    {
        for (int i = 0; i < pickedLayers.Count; i++) {
            Debug.Log(pickedLayers[i].Key + " ** " + pickedLayers[i].Value.id + "**" + pickedLayers[i].Value.questions[pickedQuestion].questionText);
        }
    }

    public void SetContent(Model.Accordion content)
    {
        this.content = content;
        InitQuiz();
    }

    private void InitQuiz()
    {
        pickedLayers = GetRandomLayers(maxQuestions);
        UpdateQuiz();
    }

    private List<KeyValuePair<int, Layer>> GetRandomLayers(int count)
    {
        var random = new System.Random();
        List<KeyValuePair<int, Layer>> randomLayers = this.content.layers.Select((index, layer) => new KeyValuePair<int, Layer>(layer, index))
                                                        .OrderBy(layer => random.Next())
                                                        .ToList();

        List<KeyValuePair<int, Layer>> pickedLayers = randomLayers.GetRange(0, count);

        List<KeyValuePair<int, Layer>> sortedLayers = pickedLayers.Select((index, layer) => new KeyValuePair<int, Layer>(layer, index.Value))
                                                        .OrderBy(index => index.Value.id)
                                                        .ToList();
        return sortedLayers;
    }

    private void UpdateQuiz()
    {
        if (currentQuestionIndex < maxQuestions) {
            UpdateQuizContent();
        } else {
            ShowResult();
        }
    }

    private void UpdateQuizContent()
    {
        pickedQuestion = UnityEngine.Random.Range(0, pickedLayers[currentQuestionIndex].Value.questions.Count);
        Question question = pickedLayers[currentQuestionIndex].Value.questions[pickedQuestion];
        questionText.text = question.questionText;

        for (int i = 0; i < answerContainers.Length; i++) {
            answerContainers[i].GetComponentInChildren<Text>().text = question.answers[i];
        }
    }

    private void ShowResult()
    {
        string resultText = string.Format(this.content.resultText, correctAnswers, maxQuestions);
        questionText.text = resultText;

        dropArea.SetActive(false);
        foreach (GameObject answerContainer in answerContainers) {
            answerContainer.SetActive(false);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (activeDraggable) {
            Vector3 worldPoint;

            bool hit = RectTransformUtility.ScreenPointToWorldPointInRectangle(
                activeDraggable.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out worldPoint
            );

            if (hit && !questionAnswered) {
                activeDraggable.transform.position = worldPoint;
                activeDraggable.transform.localPosition = new Vector3(activeDraggable.transform.localPosition.x, activeDraggable.transform.localPosition.y, -0.004f);
            }

            if (eventData.pointerEnter && eventData.pointerEnter.gameObject == dropArea) {
                dropArea.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            } else {
                dropArea.transform.localScale = new Vector3(defaultScaleFactor, defaultScaleFactor, defaultScaleFactor);
            }
        } else {
            if (eventData.pointerEnter && eventData.pointerEnter.tag == "AnswerContainer") {
                activeDraggable = eventData.pointerEnter;
                activeDraggableStartPosition = activeDraggable.transform.position;

                activeDraggable.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (activeDraggable == null) {
            return;
        }

        if (eventData.pointerEnter.gameObject == dropArea) {
            dropArea.transform.localScale = new Vector3(defaultScaleFactor, defaultScaleFactor, defaultScaleFactor);
            activeDraggable.transform.position = eventData.pointerEnter.transform.position;
            activeDraggable.transform.localPosition = new Vector3(activeDraggable.transform.localPosition.x, activeDraggable.transform.localPosition.y, -0.002f);
            activeDraggable.transform.localScale = new Vector3(defaultScaleFactor, defaultScaleFactor, defaultScaleFactor);
            questionAnswered = true;

            CheckAnswer();
        } else {
            activeDraggable.transform.position = activeDraggableStartPosition;
            activeDraggable.transform.localPosition = new Vector3(activeDraggable.transform.localPosition.x, activeDraggable.transform.localPosition.y, -0.002f);
            activeDraggable.transform.localScale = new Vector3(defaultScaleFactor, defaultScaleFactor, defaultScaleFactor);
            activeDraggable = null;
        }
    }

    private void CheckAnswer()
    {
        int correctAnswerId = pickedLayers[currentQuestionIndex].Value.questions[pickedQuestion].correctAnswerId;
        int draggableIndex = Array.IndexOf(answerContainers, activeDraggable);

        if (draggableIndex == correctAnswerId) {
            Debug.Log("Right");
            correctAnswers++;
            activeDraggable.GetComponent<Image>().color = rightColor;
            Invoke("Reset", nextQuestionDelay);
        } else {
            Debug.Log("Wrong");
            activeDraggable.GetComponent<Image>().color = wrongColor;
            Invoke("Reset", nextQuestionDelay);
        }
    }

    private void Reset()
    {
        activeDraggable.transform.position = activeDraggableStartPosition;
        activeDraggable.transform.localPosition = new Vector3(activeDraggable.transform.localPosition.x, activeDraggable.transform.localPosition.y, -0.002f);

        activeDraggable.GetComponent<Image>().color = defaultColor;
        questionAnswered = false;
        activeDraggable = null;

        currentQuestionIndex++;
        UpdateQuiz();
    }
}
