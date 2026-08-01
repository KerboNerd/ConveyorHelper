using ClientPlugin.Settings;
using VRage.Input;

namespace ClientPlugin.Input;

public static class ToggleController
{
    public static void Update()
    {
        var input = MyInput.Static;
        if (input == null)
            return;

        if (!Config.Current.ToggleHotkey.HasPressed(input))
            return;

        Config.Current.Enabled = !Config.Current.Enabled;
        ConfigStorage.Save(Config.Current);
    }
}
