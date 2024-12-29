using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using UI;

namespace Teleporter.UI.Harmony;

[PublicAPI]
[HarmonyPatch]
public static class ProgrammaticWindowCreatorPatches
{
    private static readonly Action<ProgrammaticWindowCreator, Action<TeleporterWindow>> _CreateWindowDelegate;

    static ProgrammaticWindowCreatorPatches() => _CreateWindowDelegate = GetCreateWindowDelegate();

    private static Action<ProgrammaticWindowCreator, Action<TeleporterWindow>> GetCreateWindowDelegate() {
        var methodInfo =
            typeof(ProgrammaticWindowCreator)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(o => o.IsGenericMethod && o.Name == "CreateWindow" && o.GetParameters().Length == 1);

        if (methodInfo == null) {
            throw new InvalidOperationException("Cannot find method UI.ProgrammaticWindowCreator:CreateWindow<TWindow>(Action<>)");
        }

        var genericMethod = methodInfo.MakeGenericMethod(typeof(TeleporterWindow));

        return (Action<ProgrammaticWindowCreator, Action<TeleporterWindow>>)
            Delegate.CreateDelegate(typeof(Action<ProgrammaticWindowCreator, Action<TeleporterWindow>>), genericMethod);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ProgrammaticWindowCreator), "Start")]
    public static void Start(ProgrammaticWindowCreator __instance) {
        _CreateWindowDelegate(__instance, _ => {
        
        
        
        });
    }
}