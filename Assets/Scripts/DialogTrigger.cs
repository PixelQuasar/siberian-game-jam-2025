
using UnityEngine;

public class DialogTrigger : MonoBehaviour
{
    public DialogueManager dialogueManager;  // Ссылка на ваш DialogueManager

    private bool hasTriggered = false;       // Флаг, чтобы диалог не запускался повторно

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, чтобы триггер сработал только на игроке
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;

            // Запускаем диалог
            if (dialogueManager != null)
            {
                dialogueManager.StartDialogue();
            }

            // Если хотите убрать/выключить этот объект после запуска:
            // gameObject.SetActive(false);
            // или Destroy(gameObject);
        }
    }
}
