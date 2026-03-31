using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue/New Dialogue")]
public class DialogueAsset : ScriptableObject
{
    public List<DialogueLine> lines;
}