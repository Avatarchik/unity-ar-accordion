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

    private Model.Accordion content;

    private List<KeyValuePair<int, Layer>> pickedLayers = new List<KeyValuePair<int, Layer>>();
    private int correctAnswerCount = 0;

    int currentLayerId;
    private int currentQuestionIndex = 0;
    bool currentQuestionAnswered;

    private GameObject activeDraggable;
    private Vector3 activeDraggableStartPosition;

    bool isZPositionCorrected;


    public void Awake()
    {
        accordion = this.transform.parent.GetComponent<Accordion>();
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
        foreach (Transform child in transform) {
            child.gameObject.SetActive(false);
        }

        StartCoroutine(accordion.MoveToLayer(0));
        while (accordion.isMoving) {
            yield return null;
        }

        yield return new WaitForSeconds(StartDelay);

        StartCoroutine(accordion.MoveToLayer(pickedLayers[currentQuestionIndex].Key + 1));
        while (accordion.isMoving) {
            yield return null;
        }

        foreach (Transform child in transform) {
            child.gameObject.SetActive(true);
        }

        UpdateQuizContent();
    }

    private void UpdateQuizContent()
    {
        foreach (Transform child in transform) {
            child.gameObject.SetActive(true);
        }

        SetQuizTilePositions();

        List<Question> questions = pickedLayers[currentQuestionIndex].Value.questions;

        currentLayerId = UnityEngine.Random.Range(0, questions.Count);
        Question question = questions[currentLayerId];
        questionContainer.transform.GetChild(1).GetComponent<Text>().text = question.question;

        for (int i = 0; i < answerContainers.Length; i++) {

            Text containerText = answerContainers[i].GetComponentInChildren<Text>();
            containerText.text = question.answers[i];
        }
    }

    public void SetContent(Model.Accordion content)
    {
        this.content = content;
    }

    IEnumerator UpdateQuiz()
    {
        if (currentQuestionIndex < maxQuestions) {
            StartCoroutine(accordion.MoveToLayer(pickedLayers[currentQuestionIndex].Key + 1));

            while (accordion.isMoving) {
                yield return null;
            }

            UpdateQuizContent();
        } else {
            ShowResult();
        }
    }

    private void ShowResult()
    {
        this.gameObject.transform.GetChild(0).gameObject.SetActive(true);
        string resultText = string.Format(this.content.quiz.resultText, correctAnswerCount, maxQuestions);
        questionContainer.transform.GetChild(1).GetComponent<Text>().text = resultText;

        dropArea.SetActive(false);
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
                if (!isZPositionCorrected) {
                    activeDraggable.transform.localPosition = new Vector3(activeDraggable.transform.localPosition.x, activeDraggable.transform.localPosition.y, activeDraggable.transform.localPosition.z - 0.1f);
                    isZPositionCorrected = true;
                }
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
            activeDraggable.transform.localPosition = new Vector3(activeDraggable.transform.localPosition.x, activeDraggable.transform.localPosition.y, dropArea.transform.localPosition.z - 0.01f);
            activeDraggable.transform.localScale = new Vector3(defaultScaleFactor, defaultScaleFactor, defaultScaleFactor);
            currentQuestionAnswered = true;

            CheckAnswer();
        } else {
            activeDraggable.transform.position = activeDraggableStartPosition;
            activeDraggable.transform.localScale = new Vector3(defaultScaleFactor, defaultScaleFactor, defaultScaleFactor);
            isZPositionCorrected = false;
            activeDraggable = null;
        }
    }

    private void CheckAnswer()
    {
        int correctAnswerId = pickedLayers[currentQuestionIndex].Value.questions[currentLayerId].correctAnswerIndex;
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
        foreach (Transform child in transform) {
            child.gameObject.SetActive(false);
        }

        activeDraggable.transform.position = activeDraggableStartPosition;
        activeDraggable.transform.localPosition = new Vector3(activeDraggable.transform.localPosition.x, activeDraggable.transform.localPosition.y, -2f);

        activeDraggable.GetComponent<Image>().color = defaultColor;
        currentQuestionAnswered = false;
        activeDraggable = null;
        isZPositionCorrected = false;

        currentQuestionIndex++;
        StartCoroutine(UpdateQuiz());
    }

    public void SetQuizTilePositions()
    {
        GameObject quizAnchor = accordion.activeTile.transform.GetChild(1).gameObject;

        questionContainer.transform.position = quizAnchor.transform.Find("QuestionAnchor").transform.position;

        answerContainers[0].transform.position = quizAnchor.transform.Find("AnswerAnchor1").transform.position;
        answerContainers[1].transform.position = quizAnchor.transform.Find("AnswerAnchor2").transform.position;
        answerContainers[2].transform.position = quizAnchor.transform.Find("AnswerAnchor3").transform.position;
        answerContainers[3].transform.position = quizAnchor.transform.Find("AnswerAnchor4").transform.position;

        dropArea.transform.position = quizAnchor.transform.Find("DropAnchor").transform.position;
    }

}
