using Menu.Remix.MixedUI;

namespace BeeWorld;

public class BeeOptions : OptionsTemplate
{
    public static BeeOptions Instance { get; } = new();
    public static void RegisterOI()
    {
        if (MachineConnector.GetRegisteredOI(Plugin.MOD_ID) != Instance)
            MachineConnector.SetRegisteredOI(Plugin.MOD_ID, Instance);
    }

    public static Configurable<float> MaximumStamina = Instance.config.Bind(nameof(MaximumStamina), 200f, new ConfigurableInfo("Maximum Stamina", new ConfigAcceptableRange<float>(10, 1000), "", "Maximum Stamina"));
    public static Configurable<float> StaminaRecoverySpeed = Instance.config.Bind(nameof(StaminaRecoverySpeed), 1f, new ConfigurableInfo("Stamina Recovery Speed", new ConfigAcceptableRange<float>(0.1f, 10), "", "Stamina Recovery Speed"));
    public static Configurable<float> FlightSpeed = Instance.config.Bind(nameof(FlightSpeed), 2f, new ConfigurableInfo("Flight Speed", new ConfigAcceptableRange<float>(0.1f, 10), "", "Flight Speed"));
    public static Configurable<bool> UnlimitedStingers = Instance.config.Bind(nameof(UnlimitedStingers), false, new ConfigurableInfo("Unlimited Stinger Usage", null, "", "Unlimited Stinger Usage"));
    public static Configurable<bool> VerticalFight = Instance.config.Bind(nameof(VerticalFight), false, new ConfigurableInfo("Unlock Vertical Flight", null, "", "Unlock Vertical Flight"));

    public static Configurable<float> BupsSpawnRate = Instance.config.Bind(nameof(BupsSpawnRate), 0.1f, new ConfigurableInfo("Bups Spawnrate (enable top one)", new ConfigAcceptableRange<float>(0.1f, 1), "", "Bups Spawnrate (enable top one)"));
    public static Configurable<bool> BupsSpawnAll = Instance.config.Bind(nameof(BupsSpawnAll), false, new ConfigurableInfo("Enable Bups to spawn in all campaign", null, "", "Enable Bups to spawn in all campaign"));
    public static Configurable<float> BupsFly = Instance.config.Bind(nameof(BupsFly), 5f, new ConfigurableInfo("Bups Upward fly", new ConfigAcceptableRange<float>(5f, 15), "", "Bups Upward fly"));

    public static Configurable<KeyCode> StingerAttackKeyboard = Instance.config.Bind(nameof(StingerAttackKeyboard), KeyCode.C, new ConfigurableInfo("Keybind for Keyboard.", null, "", "Keyboard"));
    public static Configurable<KeyCode> StingerAttackPlayer1 = Instance.config.Bind(nameof(StingerAttackPlayer1), KeyCode.Joystick1Button3, new ConfigurableInfo("Keybind for Player 1", null, "", "Keybind for Player 1"));
    public static Configurable<KeyCode> StingerAttackPlayer2 = Instance.config.Bind(nameof(StingerAttackPlayer2), KeyCode.Joystick2Button3, new ConfigurableInfo("Keybind for Player 2", null, "", "Keybind for Player 2"));
    public static Configurable<KeyCode> StingerAttackPlayer3 = Instance.config.Bind(nameof(StingerAttackPlayer3), KeyCode.Joystick3Button3, new ConfigurableInfo("Keybind for Player 3", null, "", "Keybind for Player 3"));
    public static Configurable<KeyCode> StingerAttackPlayer4 = Instance.config.Bind(nameof(StingerAttackPlayer4), KeyCode.Joystick4Button3, new ConfigurableInfo("Keybind for Player 4", null, "", "Keybind for Player 4"));

    private const int TAB_COUNT = 3;

    public override void Initialize()
    {
        base.Initialize();

        Tabs = new OpTab[TAB_COUNT];
        int tabIndex = -1;

        InitInputStinger(ref tabIndex);
        InitBups(ref tabIndex);
        InitCheats(ref tabIndex);
    }

    private void InitInputStinger(ref int tabIndex)
    {
        AddTab(ref tabIndex, "Stinger Keybinds");

        AddNewLine(2);

        DrawKeybinders(StingerAttackKeyboard, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(StingerAttackPlayer1, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(StingerAttackPlayer2, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(StingerAttackPlayer3, ref Tabs[tabIndex]);
        AddNewLine(1);

        DrawKeybinders(StingerAttackPlayer4, ref Tabs[tabIndex]);

        DrawBox(ref Tabs[tabIndex]);
    }

    public static readonly Color WarnRed = new(0.85f, 0.35f, 0.4f);

    private void InitCheats(ref int tabIndex)
    {
        AddTab(ref tabIndex, "Cheats");
        Tabs[tabIndex].colorButton = WarnRed;
        Tabs[tabIndex].colorCanvas = WarnRed;

        AddFloatSlider(MaximumStamina, MaximumStamina.info.description);
        DrawFloatSliders(ref Tabs[tabIndex]);

        AddFloatSlider(StaminaRecoverySpeed, StaminaRecoverySpeed.info.description);
        DrawFloatSliders(ref Tabs[tabIndex]);

        AddFloatSlider(FlightSpeed, FlightSpeed.info.description);
        DrawFloatSliders(ref Tabs[tabIndex]);

        AddCheckBox(UnlimitedStingers);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddCheckBox(VerticalFight);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddFloatSlider(BupsFly, BupsFly.info.description);
        DrawFloatSliders(ref Tabs[tabIndex]);

        DrawBox(ref Tabs[tabIndex]);
    }

    private void InitBups(ref int tabIndex)
    {
        AddTab(ref tabIndex, "Bups Modify");
        Tabs[tabIndex].colorButton = Color.yellow;
        Tabs[tabIndex].colorCanvas = Color.yellow;
        AddCheckBox(BupsSpawnAll);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddFloatSlider(BupsSpawnRate, BupsSpawnRate.info.description);
        DrawFloatSliders(ref Tabs[tabIndex]);
        DrawBox(ref Tabs[tabIndex]);
    }
}