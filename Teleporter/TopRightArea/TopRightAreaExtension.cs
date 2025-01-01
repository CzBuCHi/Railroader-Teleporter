﻿using System;
using System.IO;
using System.Reflection;
using Serilog;
using UI.Tooltips;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Teleporter.TopRightArea;

public static class TopRightAreaExtension
{
    public static void AddButton(string iconName, string tooltip, int index, UnityAction onClick) {
        var topRightArea = Object.FindObjectOfType<global::UI.TopRightArea>();
        if (topRightArea == null) {
            return;
        }

        var componentInChildren = topRightArea.transform.Find("Strip")!.gameObject.GetComponentInChildren<Button>()!;
        var gameObject          = Object.Instantiate(componentInChildren.gameObject, componentInChildren.transform.parent)!;
        gameObject.transform.SetSiblingIndex(index);

        gameObject.GetComponent<UITooltipProvider>()!.TooltipInfo = new TooltipInfo(tooltip, string.Empty);

        var button = gameObject.GetComponent<Button>()!;
        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(onClick);

        var image = gameObject.GetComponent<Image>()!;

        var icon = LoadTexture2D(iconName, 128, 128);
        image.sprite = Sprite.Create(icon, new Rect(0.0f, 0.0f, 128, 128), new Vector2(0.5f, 0.5f))!;
    }

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
            var texture = new Texture2D(width, height, TextureFormat.DXT5, false);
            texture.LoadImage(bytes);
            return texture;
        } catch (Exception e) {
            Log.Error(e, "Failed to load texture {0}", path);
        }

        return null!;
    }
}
