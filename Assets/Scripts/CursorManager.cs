using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [Tooltip("Add your cursor sprite here.")]
    public Sprite cursorSprite;

    [Tooltip("Offset of the cursor hotspot (hotspot) from the top left corner of the sprite.")]
    public Vector2 hotSpot = Vector2.zero;

    [Tooltip("Cursor display mode.")]
    public CursorMode cursorMode = CursorMode.Auto;

    private Texture2D generatedCursorTexture;

    void Start()
    {
        if (cursorSprite == null)
        {
            Debug.LogError("Cursor sprite not assigned in CursorManager!", this);
            return;
        }

        generatedCursorTexture = GenerateTextureFromSprite(cursorSprite);

        if (generatedCursorTexture == null)
        {
            Debug.LogError("Failed to generate cursor texture. Ensure the original sprite sheet has 'Read/Write Enabled' enabled in the import settings.", this);
            return;
        }

        if (hotSpot == Vector2.zero)
        {
            hotSpot = new Vector2(generatedCursorTexture.width / 2f, generatedCursorTexture.height / 2f);
        }

        SetCustomCursor();
        Cursor.visible = true;
    }

    Texture2D GenerateTextureFromSprite(Sprite sprite)
    {
        if (!sprite.texture.isReadable)
        {
            Debug.LogError($"Texture '{sprite.texture.name}' for sprite '{sprite.name}' is not readable. Enable 'Read/Write Enabled' in the import settings.", sprite.texture);
            return null;
        }

        try
        {
            Rect spriteRect = sprite.rect;
            Texture2D newTex = new Texture2D((int)spriteRect.width, (int)spriteRect.height);
            Color[] pixels = sprite.texture.GetPixels(
                (int)spriteRect.x,
                (int)spriteRect.y,
                (int)spriteRect.width,
                (int)spriteRect.height
            );
            newTex.SetPixels(pixels);
            newTex.Apply();
            return newTex;
        }
        catch (UnityException ex)
        {
            Debug.LogError($"Error getting pixels from texture '{sprite.texture.name}'. Ensure 'Read/Write Enabled' is enabled. Error: {ex.Message}", sprite.texture);
            return null;
        }
    }


    void SetCustomCursor()
    {
        if (generatedCursorTexture != null)
        {
            Cursor.SetCursor(generatedCursorTexture, hotSpot, cursorMode);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            SetCustomCursor();
        }
        else
        {
            ResetCursor();
        }
    }

    void OnDestroy()
    {
       ResetCursor();
    }

    void ResetCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
