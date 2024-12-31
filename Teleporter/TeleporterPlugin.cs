using System;
using GalaSoft.MvvmLight.Messaging;
using Game.Events;
using HarmonyLib;
using JetBrains.Annotations;
using Railloader;
using Teleporter.TopRightArea;
using Teleporter.UI;

namespace Teleporter;

[PublicAPI]
public sealed class TeleporterPlugin : SingletonPluginBase<TeleporterPlugin>
{
    private const string PluginIdentifier = "CzBuCHi.Teleporter";

    private readonly IModdingContext _Context;
    private readonly Settings        _Settings;

    public TeleporterPlugin(IModdingContext context, IUIHelper uiHelper) {
        _Context = context;

        _Settings = _Context.LoadSettingsData<Settings>(PluginIdentifier) ?? new Settings();
    }

    private Messenger _Messenger = null!;

    public static Settings Settings => Shared!._Settings;

    public static void SaveSettings() {
        Shared!._Context.SaveSettingsData(PluginIdentifier, Shared._Settings);
    }

    public override void OnEnable() {
        _Messenger = Messenger.Default!;
        _Messenger.Register(this, new Action<MapDidLoadEvent>(OnMapDidLoad));
        _Messenger.Register(this, new Action<MapDidUnloadEvent>(OnMapDidUnload));

        var harmony = new Harmony(PluginIdentifier);
        harmony.PatchAll();

    }

    public override void OnDisable() {
        _Messenger.Unregister(this);

        var harmony = new Harmony(PluginIdentifier);
        harmony.UnpatchAll();
    }

    private void OnMapDidLoad(MapDidLoadEvent @event) {
        TopRightAreaExtension.AddButton(TeleporterWindow.Toggle);
    }

    private void OnMapDidUnload(MapDidUnloadEvent @event) {
        SaveSettings();
    }
}
