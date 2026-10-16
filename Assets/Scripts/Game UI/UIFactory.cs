using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Small helpers that build uGUI elements from code, so the menu and HUD
/// don't need hand-made scene setup. Colors and sizes are easy to restyle here.
/// </summary>
public static class UIFactory
{
    static Font font;

    // Built-in font, so no TextMeshPro import is needed
    public static Font DefaultFont
    {
        get
        {
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font;
        }
    }

    public static Canvas CreateCanvas(string name, int sortOrder = 0)
    {
        EnsureEventSystem();

        var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;

        // Scale with screen size so the UI looks the same on any resolution
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    // Buttons need an EventSystem; the project uses the new Input System
    public static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;

        var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }

    /// <summary>Full-screen colored panel.</summary>
    public static RectTransform CreatePanel(Transform parent, string name, Color color, bool blocksClicks = true)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = blocksClicks;
        return rt;
    }

    /// <summary>Colored rectangle placed relative to an anchor (pivot = anchor).</summary>
    public static Image CreateImage(Transform parent, string name, Color color, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        Place(rt, anchor, position, size);

        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    public static Text CreateText(Transform parent, string content, int fontSize, Color color,
                                  TextAnchor alignment, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        Place(rt, anchor, position, size);

        var text = go.GetComponent<Text>();
        text.font = DefaultFont;
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow; // never hide big titles
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    /// <summary>Centered button; position is an offset from the screen center.</summary>
    public static Button CreateButton(Transform parent, string label, Vector2 position, Vector2 size, UnityAction onClick)
    {
        var go = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        Place(rt, new Vector2(0.5f, 0.5f), position, size);

        var image = go.GetComponent<Image>();
        image.color = Color.white; // the button's color tints this

        var button = go.GetComponent<Button>();
        button.targetGraphic = image;

        var colors = button.colors;
        colors.normalColor = new Color(0.16f, 0.20f, 0.30f, 1f);
        colors.highlightedColor = new Color(0.28f, 0.40f, 0.58f, 1f);
        colors.pressedColor = new Color(0.10f, 0.14f, 0.20f, 1f);
        colors.selectedColor = colors.normalColor;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        button.onClick.AddListener(onClick);

        CreateText(go.transform, label, 42, Color.white, TextAnchor.MiddleCenter,
                   new Vector2(0.5f, 0.5f), Vector2.zero, size);
        return button;
    }

    static void Place(RectTransform rt, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }
}
