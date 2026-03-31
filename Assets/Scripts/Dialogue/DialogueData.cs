using System.Collections.Generic;
using UnityEngine;

public enum DialogueType
{
    NPC,
    Player,
    Choice
}

[System.Serializable]
public class DialogueLine
{
    public DialogueType type;
    public string text;

    // ÷ª”– Choice ”√
    public List<string> options;
}