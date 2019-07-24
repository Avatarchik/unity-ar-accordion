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
    private const float StartDelay = 0.5f;
    private const float EndDelay = 0.5f;

    [SerializeField] private GameObject questionContainer;
    [SerializeField] private GameObject[] answerContainers;
    [SerializeField] private GameObject dropArea;

    [SerializeField] private Color rightColor = new Color(0, 200, 0);
    [SerializeField] private Color wrongColor = new Color(200, 0, 0);
    [SerializeField] private Color defaultColor = new Color(255, 255, 255);

    [SerializeField] private float scaleFactor = 1.2f;
    [SerializeField] private float defaultScaleFactor = 1.0f;
    [SerializeField] private float nextQuestionDelay = 1.5f;

    [SerializeField] private int maxQuestions = 5;

    private Accordion accordion;
    private GameObject resultContainer;

    private Model.Accordion content;

    private List<KeyValuePair<int, Layer>> pickedLayers = new List<KeyValuePair<int, Layer>>();
    private int correctAnswerCount = 0;

    Question currentQuestion;
    private int currentQuestionIndex = 0;
    bool currentQuestionAnswered;

    private GameObject activeDraggable;
    private Vector3 activeDraggableStartPosition;


    public void Awake()
    {
        accordion = this.transform.parent.GetComponent<Accordion>();
        resultContainer = this.transform.Find("ResultContainer").gameObject;
    }

    public void OnEnable()
    {
        InitQuiz();
        StartCoroutine(StartQuiz());
    }

    private void InitQuiz()
    {
        currentQuestionIndex = 0;
        correctAnswerCount = 0;
        pickedLayers = GetRandomLayers(maxQuestions);
    }

    private List<KeyValuePair<int, Layer>> GetRandomLayers(int count)
    {
        System.Random random = new System.Random();

        return this.content.layers
            .Select((layer, index) => new KeyValuePair<int, Layer>(index, layer))
            .OrderBy(entry => random.Next())
            .ToList()
            .GetRange(0, count)
            .OrderBy(entry => entry.Key)
            .ToList();
    }

    IEnumerator StartQuiz()
    {
        Show(false);

        resultContainer.SetActive(false);

        StartCoroutine(accordion.MoveToLayer(0));
        while (accordion.isMoving) {
            yield return null;
        }

        accordion.mainCanvas.SetActive(false);
        yield return new WaitForSeconds(StartDelay);

        accordion.MoveToLayer(pickedLayers[currentQuestionIndex].Key + 1);

        StartCoroutine(accordion.MoveToLayer(pickedLayers[currentQuestionIndex].Key + 1));
        while (accordion.isMoving) {
            yield return null;
        }

        SetPositions();
        UpdateQuizContent();

        Show(true);
    }

    private void Show(bool show)
    {
        foreach (Transform child in transform) {
            if (child.gameObject.name != "ResultContainer") {
                child.gameObject.SetActive(show);
            }

        }
    }

    private void UpdateQuizContent()
    {
        List<Question> questions = pickedLayers[currentQuestionIndex].Value.questions;
        int questionIndex = UnityEngine.Random.Range(0, questions.Count);
        currentQuestion = questions[questionIndex];

        questionContainer.transform.GetChild(0).GetComponent<Text>().text = currentQuestion.question;

        for (int i = 0; i < answerContainers.Length; i++) {
            Text containerText = answerContainers[i].GetComponentInChildren<Text>();
            containerText.text = currentQuestion.answers[i];
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

            if (hit && !currentQuestionAnswered) {
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
            currentQuestionAnswered = true;

            CheckAnswer();
        } else {
            activeDraggable.transform.position = activeDraggableStartPosition;
            activeDraggable.transform.localPosition = new Vector3(activeDraggable.transform.localPosition.x, activeDraggable.transform.localPosition.y, -0.001f);
            activeDraggable.transform.localScale = new Vector3(defaultScaleFactor, defaultScaleFactor, defaultScaleFactor);
            activeDraggable = null;
        }
    }

    private void CheckAnswer()
    {
        int correctAnswerId = currentQuestion.correctAnswerIndex;
        int draggableIndex = Array.IndexOf(answerContainers, activeDraggable);

        if (draggableIndex == correctAnswerId) {
            Debug.Log("Right");
            correctAnswerCount++;
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
        Show(false);

        activeDraggable.transform.position = activeDraggableStartPosition;
        activeDraggable.transform.localPosition = new Vector3(activeDraggable.transform.localPosition.x, activeDraggable.transform.localPosition.y, -0.002f);

        activeDraggable.GetComponent<Image>().color = defaultColor;
        currentQuestionAnswered = false;
        activeDraggable = null;

        currentQuestionIndex++;
        StartCoroutine(UpdateQuiz());
    }

    IEnumerator UpdateQuiz()
    {
        if (currentQuestionIndex < maxQuestions) {
            StartCoroutine(accordion.MoveToLayer(pickedLayers[currentQuestionIndex].Key + 1));

            while (accordion.isMoving) {
                yield return null;
            }

            SetPositions();
            UpdateQuizContent();
            Show(true);
        } else {
            StartCoroutine(accordion.MoveToLayer(0));

            while (accordion.isMoving) {
                yield return null;
            }

            yield return new WaitForSeconds(EndDelay);

            ShowResult();
        }
    }

    private void ShowResult()
    {

        resultContainer.SetActive(true);

        Vector3 resultContainerPosition = GameObject.Find("Accordion").transform.position;
        resultContainer.transform.position = new Vector3(resultContainerPosition.x, resultContainerPosition.y, resultContainerPosition.z - 0.13f);

        string resultText = GetResultText();
        resultContainer.transform.GetChild(0).GetComponent<Text>().text = resultText;

        dropArea.SetActive(false);
    }

    private string GetResultText()
    {
        switch (correctAnswerCount) {
            case 0:
                return this.content.quiz.resultBad;
            case 1:
                return string.Format(this.content.quiz.resultOne, correctAnswerCount, maxQuestions);
            default:
                return string.Format(this.content.quiz.resultGood, correctAnswerCount, maxQuestions);
        }
    }

    public void SetPositions()
    {
        Transform anchor = accordion.ActiveComponent.transform.Find("QuizAnchor");

        transform.position = anchor.position;
        transform.rotation = anchor.rotation;

        transform.SetParent(anchor);
    }

    public void SetContent(Model.Accordion content)
    {
        this.content = content;
    }
}
