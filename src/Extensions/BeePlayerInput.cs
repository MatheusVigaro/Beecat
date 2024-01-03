namespace BeeWorld.Extensions;

public readonly struct BeePlayerInput
{
    private readonly Player player;

    public BeePlayerInput(Player player)
    {
        this.player = player;
    }

    public bool StingerAttackPressed =>
            !player.IsBup() && player.playerState.playerNumber switch
            {
                0 => Input.GetKey(BeeOptions.StingerAttackPlayer1.Value) || player.input[0].controllerType == Options.ControlSetup.Preset.KeyboardSinglePlayer && Input.GetKey(BeeOptions.StingerAttackKeyboard.Value),
                1 => Input.GetKey(BeeOptions.StingerAttackPlayer2.Value) || player.input[0].controllerType == Options.ControlSetup.Preset.KeyboardSinglePlayer && Input.GetKey(BeeOptions.StingerAttackKeyboard.Value),
                2 => Input.GetKey(BeeOptions.StingerAttackPlayer3.Value) || player.input[0].controllerType == Options.ControlSetup.Preset.KeyboardSinglePlayer && Input.GetKey(BeeOptions.StingerAttackKeyboard.Value),
                3 => Input.GetKey(BeeOptions.StingerAttackPlayer4.Value) || player.input[0].controllerType == Options.ControlSetup.Preset.KeyboardSinglePlayer && Input.GetKey(BeeOptions.StingerAttackKeyboard.Value),
                
                _ => false
            };
}

public static class PlayerInputExtension
{
    public static BeePlayerInput Input(this Player player) => new BeePlayerInput(player);
}