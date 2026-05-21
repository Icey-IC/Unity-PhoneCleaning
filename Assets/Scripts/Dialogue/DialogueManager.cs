using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    private System.Action onDialogueComplete;

    [Header("Prefab")]
    public GameObject npcBubblePrefab;
    public GameObject playerBubblePrefab;
    public GameObject choiceBubblePrefab;

    [Header("位置")]
    public Transform npcSpawnPoint;
    public Transform playerSpawnPoint;
    public Transform choiceSpawnRoot;

    private Queue<DialogueLine> dialogueQueue = new Queue<DialogueLine>();
    private GameObject currentBubble;

    void Awake()
    {
        Instance = this;
    }

    public void StartDialogue(List<DialogueLine> lines, System.Action onComplete = null)
    {
        dialogueQueue.Clear();

        foreach (var line in lines)
            dialogueQueue.Enqueue(line);

        onDialogueComplete = onComplete;

        ShowNext();
    }

    public void ShowNext()
    {
        if (currentBubble != null)
            Destroy(currentBubble);

        if (dialogueQueue.Count == 0)
        {
            onDialogueComplete?.Invoke();
            onDialogueComplete = null;
            return;
        }

        var line = dialogueQueue.Dequeue();

        switch (line.type)
        {
            case DialogueType.NPC:
                SpawnNPC(line);
                break;

            case DialogueType.Player:
                SpawnPlayer(line);
                break;

            case DialogueType.Choice:
                SpawnChoices(line);
                break;
        }
    }

    void SpawnNPC(DialogueLine line)
    {
        currentBubble = Instantiate(npcBubblePrefab, npcSpawnPoint);
        currentBubble.transform.localPosition = Vector3.zero;

        currentBubble.GetComponent<DialogueBubble>()
            .Init(line.text, OnBubbleClicked);
    }

    void SpawnPlayer(DialogueLine line)
    {
        currentBubble = Instantiate(playerBubblePrefab, playerSpawnPoint);
        currentBubble.transform.localPosition = Vector3.zero;

        currentBubble.GetComponent<DialogueBubble>()
            .Init(line.text, OnBubbleClicked);
    }

    void SpawnChoices(DialogueLine line)
    {
        float offsetY = 0f;

        foreach (var option in line.options)
        {
            GameObject bubble = Instantiate(choiceBubblePrefab, choiceSpawnRoot);

            RectTransform rt = bubble.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, offsetY);

            bubble.GetComponent<DialogueBubble>().Init(option, () =>
            {
                ClearAllChoices();
                ShowNext();
            });

            offsetY -= 120f; // UI 用像素，不是 1.2f
        }
    }

    void OnBubbleClicked()
    {
        ShowNext();
    }

    void ClearAllChoices()
    {
        foreach (Transform child in choiceSpawnRoot)
        {
            Destroy(child.gameObject);
        }
    }
}