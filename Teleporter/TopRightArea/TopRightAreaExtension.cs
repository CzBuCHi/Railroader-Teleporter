using System;
using System.IO;
using System.Reflection;
using Serilog;
using UI.Tooltips;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Teleporter.TopRightArea;

public static class TopRightAreaExtension
{
    public static void AddButton(Action onClick) {
        var topRightArea = Object.FindObjectOfType<global::UI.TopRightArea>();
        if (topRightArea == null) {
            return;
        }

        var componentInChildren = topRightArea.transform.Find("Strip").gameObject.GetComponentInChildren<Button>();
        var gameObject          = Object.Instantiate(componentInChildren.gameObject, componentInChildren.transform.parent);
        gameObject.transform.SetSiblingIndex(9);

        gameObject.GetComponent<UITooltipProvider>().TooltipInfo = new TooltipInfo("Teleporter", string.Empty);

        var button = gameObject.GetComponent<Button>();
        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(() => onClick());

        var image = gameObject.GetComponent<Image>();
        image.sprite = Sprite.Create(_Icon, new Rect(0.0f, 0.0f, 24, 24), new Vector2(0.0f, 0.0f));
    }

    private static readonly Texture2D _Icon = LoadTexture2D("icon.png", 24, 24);

    private static byte[] GetBytes(string path) {
        var       assembly = Assembly.GetExecutingAssembly();
        using var stream   = assembly.GetManifestResourceStream(path)!;
        using var ms       = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    private static Texture2D LoadTexture2D(string path, int width, int height) {
        try {
            var bytes   = GetBytes($"{typeof(TopRightAreaExtension).Namespace}.{path}");
            var texture = new Texture2D(width, height);
            texture.LoadImage(bytes, true);
            return texture;
        } catch (Exception e) {
            Log.Error(e, "Failed to load texture {0}", path);
        }

        return null!;
    }
}
