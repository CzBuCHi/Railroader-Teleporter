using System;
using System.Linq;
using Helpers;
using JetBrains.Annotations;
using Teleporter.UI.Harmony;
using UI;
using UI.Builder;
using UI.Common;
using UnityEngine;

namespace Teleporter.UI;

[PublicAPI]
public sealed class TeleporterWindow : MonoBehaviour, IProgrammaticWindow
{
    public UIBuilderAssets BuilderAssets    { get; set; } = null!;
    public string          WindowIdentifier { get; }      = "TeleporterWindow";
    public Vector2Int      DefaultSize      { get; }      = new(400, 300);
    public Window.Position DefaultPosition  { get; }      = Window.Position.UpperRight;

    public static TeleporterWindow Shared => WindowManager.Shared!.GetWindow<TeleporterWindow>()!;

    private Window   _Window = null!;
    private UIPanel? _Panel;

    public void Awake() => _Window = GetComponent<Window>()!;

    public void Show() {
        Populate();
        _Window.ShowWindow();
    }

    public static void Toggle() {
        if (Shared._Window.IsShown) {
            Shared._Window.CloseWindow();
        } else {
            Shared.Show();
        }
    }

    public void OnDisable() {
        _Panel?.Dispose();
        _Panel = null;
    }

    private void Populate() {
        _Window.Title = "Teleporter";
        _Panel?.Dispose();
        _Panel = UIPanel.Create(_Window.contentRectTransform!, BuilderAssets, Build);
    }

    private string _NewLocationName = "";

    private void Build(UIPanelBuilder builder) {
        builder.ButtonStrip(strip => {
            strip.AddButton("Save current", Add())!
                 .Tooltip("Save current position", "Save current location under new name");
            strip.AddFieldToggle("Close after", () => TeleporterPlugin.Settings.CloseAfter, UpdateSettings);
        });

        builder.AddField("New location name", builder.AddInputField(_NewLocationName, UpdateNewLocationName, "Unique name for new location")!);
        builder.VScrollView(scroll => {
            foreach (var pair in TeleporterPlugin.Settings.Locations.OrderBy(o => o.Key)) {
                scroll.AddField(pair.Key!,
                    scroll.ButtonStrip(strip => {
                        strip.AddButton(TextSprites.Destination, TeleportTo(pair.Value!)).Tooltip("Teleport","Switch to strategy camera at given location");
                        strip.Spacer();
                        strip.AddButton("Replace", Replace(pair.Key!))!.Tooltip("Replace", "Save current location under this name");
                        strip.AddButton("Remove", Remove(pair.Key!))!.Tooltip("Remove location", "Remove location from saved locations");
                    })!
                );
            }

            
        });

        return;

        void UpdateSettings(bool value) {
            TeleporterPlugin.Settings.CloseAfter = value;
            TeleporterPlugin.SaveSettings();
        }

        Action Add() {
            return () => {
                if (TeleporterPlugin.Settings.Locations.ContainsKey(_NewLocationName)) {
                    Toast.Present("Location with name " + _NewLocationName + " already exists.");
                    return;
                }

                var cameraContainer = CameraSelector.shared.strategyCamera!.CameraContainer;
                var location = new TeleportLocation(
                    WorldTransformer.WorldToGame(cameraContainer.localPosition).ToVector(),
                    cameraContainer.localRotation.ToVector()
                );

                TeleporterPlugin.Settings.Locations.Add(_NewLocationName, location);
                TeleporterPlugin.SaveSettings();
                builder.Rebuild();
            };
        }

        Action Replace(string key) {
            return () => {
                var cameraContainer = CameraSelector.shared.strategyCamera!.CameraContainer;
                var location = new TeleportLocation(
                    WorldTransformer.WorldToGame(cameraContainer.position).ToVector(),
                    cameraContainer.localEulerAngles.ToVector()
                );

                TeleporterPlugin.Settings.Locations[key] = location;
                TeleporterPlugin.SaveSettings();
                builder.Rebuild();
            };
        }

        void UpdateNewLocationName(string value) {
            _NewLocationName = value;
            builder.Rebuild();
        }

        Action TeleportTo(TeleportLocation location) {
            return () => {
                var point    = location.Position.ToVector3();
                var rotation = location.Rotation.ToQuaternion();
                CameraSelector.shared.SelectCamera(CameraSelector.CameraIdentifier.Strategy);
                CameraSelector.shared.strategyCamera.JumpTo(point, rotation);
                if (TeleporterPlugin.Settings.CloseAfter) {
                    Toggle();
                }
            };
        }

        Action Remove(string key) {
            return () => {
                TeleporterPlugin.Settings.Locations.Remove(key);
                TeleporterPlugin.SaveSettings();
                builder.Rebuild();
            };
        }
    }
}
