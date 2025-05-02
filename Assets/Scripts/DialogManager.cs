using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour {
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public Image portraitImage;
    public TextMeshProUGUI dialogueText;
    
    [Header("Dialogue Data")]
    public DialogueLine[] lines;
    public float typingSpeed = 0.05f;
    
    [Header("Player Control")]
    public PlayerInput playerInput;     // Ссылка на компонент PlayerInput
    public string dialogActionName = "Dialog";  // Имя действия для продвижения диалога

    private int currentLineIndex = 0;
    private Coroutine typingCoroutine;
    private string previousActionMap;
    private InputAction dialogAction;

    private void Start() {
        // Находим действие Dialog из PlayerControls
        if (playerInput != null) {
            dialogAction = playerInput.actions.FindAction(dialogActionName);
        }
    }

    private void OnDialog(InputAction.CallbackContext ctx) {
        if (ctx.performed) {
            Debug.Log("Dialog input received");
            
            if (typingCoroutine != null) {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
                dialogueText.text = lines[currentLineIndex].text;
            }
            else {
                currentLineIndex++;
                ShowCurrentLine();
            }
        }
    }

    public void StartDialogue() {
        currentLineIndex = 0;
        dialoguePanel.SetActive(true);
        
        DisablePlayerMovement();
        ShowCurrentLine();
    }

    private void ShowCurrentLine() {
        if (typingCoroutine != null) {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (currentLineIndex < lines.Length) {
            nameText.text = lines[currentLineIndex].characterName;
            portraitImage.sprite = lines[currentLineIndex].portrait;
            typingCoroutine = StartCoroutine(TypeText(lines[currentLineIndex].text));
        }
        else {
            EndDialogue();
        }
    }

    private void EndDialogue() {
        dialoguePanel.SetActive(false);
        EnablePlayerMovement();
    }

    private void DisablePlayerMovement() {
        if (playerInput != null) {
            // Запоминаем текущую карту действий
            previousActionMap = playerInput.currentActionMap.name;
            
            // Переключаемся на UI карту, где есть действие Dialog
            playerInput.SwitchCurrentActionMap("UI");
            
            // Привязываем обработчик к действию Dialog
            if (dialogAction != null) {
                // Отвязываем на всякий случай, чтобы избежать двойной привязки
                dialogAction.performed -= OnDialog;
                dialogAction.performed += OnDialog;
            }
            else {
                Debug.LogError("Dialog action not found! Check the action name in inspector.");
            }
        }
    }

    private void EnablePlayerMovement() {
        if (playerInput != null) {
            // Отвязываем обработчик от действия Dialog
            if (dialogAction != null) {
                dialogAction.performed -= OnDialog;
            }
            
            // Возвращаемся к предыдущей карте действий
            playerInput.SwitchCurrentActionMap(previousActionMap);
        }
    }

    IEnumerator TypeText(string fullText) {
        dialogueText.text = "";

        foreach (char c in fullText) {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        typingCoroutine = null;
    }

    private void OnDestroy() {
        // Отвязываем обработчик при уничтожении компонента
        if (dialogAction != null) {
            dialogAction.performed -= OnDialog;
        }
    }
}
