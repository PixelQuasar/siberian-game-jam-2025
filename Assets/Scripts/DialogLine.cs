using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public class DialogueLine
{
    public string characterName;   // Имя говорящего
    public Sprite portrait;        // Портрет говорящего
    [TextArea(1, 3)]
    public string text;            // Текст реплики
}