using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Full-screen meta dialogue (e.g. on a black screen). Reuses DialogueLine / DialogueAsset / DialogueBubble.
/// Bubbles spawn at the bottom-center anchor; choice options stack upward.
/// </summary>
public class MetaDialogueManager : MonoBehaviour
{
    public AudioSource BGM;
    public static MetaDialogueManager Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject npcBubblePrefab;
    public GameObject playerBubblePrefab;
    public GameObject choiceBubblePrefab;

    [Header("Layout")]
    [Tooltip("Bottom-center anchor on the meta UI canvas.")]
    public Transform bubbleSpawnRoot;

    [Tooltip("Vertical spacing between choice bubbles (pixels).")]
    public float choiceStackSpacing = 120f;

    [Header("Events")]
    public UnityEvent onDialogueComplete;

    [Header("Next scene")]
    [Tooltip("Load a scene when all dialogue lines finish.")]
    public bool loadNextSceneOnComplete = true;

    [Tooltip("Scene name as listed in Build Settings (e.g. Meta, Level1).")]
    public string nextSceneName;


    readonly Queue<DialogueLine> dialogueQueue = new Queue<DialogueLine>();
    GameObject currentBubble;
    Action onDialogueCompleteCallback;
    bool dialogueActive;

    public bool IsDialogueActive => dialogueActive;

    void Awake()
    {
        Instance = this;
    }

    public void StartDialogue(DialogueAsset asset, Action onComplete = null)
    {
        BGM.Play();
        if (asset == null)
        {
            onComplete?.Invoke();
            return;
        }

        StartDialogue(asset.lines, onComplete);
    }

    public void StartDialogue(List<DialogueLine> lines, Action onComplete = null)
    {
        BGM.Play();

        dialogueQueue.Clear();

        if (lines == null || lines.Count == 0)
        {
            FinishDialogue(onComplete);
            return;
        }

        foreach (var line in lines)
            dialogueQueue.Enqueue(line);

        onDialogueCompleteCallback = onComplete;
        dialogueActive = true;
        ShowNext();
    }

    public void ShowNext()
    {
        if (currentBubble != null)
            Destroy(currentBubble);

        if (dialogueQueue.Count == 0)
        {
            FinishDialogue(onDialogueCompleteCallback);
            onDialogueCompleteCallback = null;
            return;
        }

        var line = dialogueQueue.Dequeue();

        switch (line.type)
        {
            case DialogueType.NPC:
                SpawnBubble(npcBubblePrefab, line.text);
                break;

            case DialogueType.Player:
                SpawnBubble(playerBubblePrefab, line.text);
                break;

            case DialogueType.Choice:
                SpawnChoices(line);
                break;
        }
    }

    void SpawnBubble(GameObject prefab, string text)
    {
        if (bubbleSpawnRoot == null)
        {
            Debug.LogWarning("MetaDialogueManager: bubbleSpawnRoot is not assigned.");
            return;
        }

        currentBubble = Instantiate(prefab, bubbleSpawnRoot);
        currentBubble.transform.localPosition = Vector3.zero;
        currentBubble.transform.localRotation = Quaternion.identity;
        currentBubble.transform.localScale = Vector3.one;

        currentBubble.GetComponent<DialogueBubble>().Init(text, OnBubbleClicked);
    }

    void SpawnChoices(DialogueLine line)
    {
        if (bubbleSpawnRoot == null || line.options == null)
            return;

        float offsetY = 0f;

        foreach (var option in line.options)
        {
            GameObject bubble = Instantiate(choiceBubblePrefab, bubbleSpawnRoot);

            RectTransform rt = bubble.GetComponent<RectTransform>();
            if (rt != null)
                rt.anchoredPosition = new Vector2(0f, offsetY);

            bubble.GetComponent<DialogueBubble>().Init(option, () =>
            {
                ClearChoiceBubbles();
                ShowNext();
            });

            offsetY += choiceStackSpacing;
        }
    }

    void OnBubbleClicked()
    {
        ShowNext();
    }

    void ClearChoiceBubbles()
    {
        if (bubbleSpawnRoot == null)
            return;

        for (int i = bubbleSpawnRoot.childCount - 1; i >= 0; i--)
            Destroy(bubbleSpawnRoot.GetChild(i).gameObject);
    }

    void FinishDialogue(Action callback)
    {
        dialogueActive = false;
        callback?.Invoke();
        onDialogueComplete?.Invoke();

        if (loadNextSceneOnComplete && !string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }
}
