using System;
using System.Reflection;
using ClientPlugin.Input;
using ClientPlugin.Logic;
using ClientPlugin.Placement;
using ClientPlugin.Rendering;
using ClientPlugin.Settings;
using ClientPlugin.Settings.Layouts;
using HarmonyLib;
using Sandbox.Graphics.GUI;
using VRage.Plugins;

// Define assembly version when compiled by Pulsar
#if !DEV_BUILD
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
#endif

namespace ClientPlugin;

// ReSharper disable once UnusedType.Global
public class Plugin : IPlugin
{
    public const string Name = "ConveyorHelper";
    public static Plugin Instance { get; private set; }
    private SettingsGenerator settingsGenerator;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public void Init(object gameInstance)
    {
        Instance = this;
        Instance.settingsGenerator = new SettingsGenerator();

        var harmony = new Harmony(Name);
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    public void Dispose()
    {
        // IMPORTANT: Do NOT call harmony.UnpatchAll() here! It may break other plugins.
        Instance = null;
    }

    public void Update()
    {
        try
        {
            ToggleController.Update();
            DrawHelpers();
        }
        catch (Exception)
        {
            // Never let helper visuals break the simulation frame.
        }
    }

    private static void DrawHelpers()
    {
        if (!Config.Current.Enabled)
            return;

        if (!PlacementWatcher.TryGetGhost(out var ghost))
            return;

        var ports = ConveyorPortResolver.Resolve(ghost.Definition);
        if (ports.Count == 0)
            return;

        ArrowDrawer.Draw(in ghost, ports);
    }

    // ReSharper disable once UnusedMember.Global
    public void OpenConfigDialog()
    {
        Instance.settingsGenerator.SetLayout<Simple>();
        MyGuiSandbox.AddScreen(Instance.settingsGenerator.Dialog);
    }

    //TODO: Uncomment and use this method to load asset files
    /*public void LoadAssets(string folder)
    {

    }*/
}
