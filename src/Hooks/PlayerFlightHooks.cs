using BeeWorld.Extensions;

namespace BeeWorld.Hooks;

public static class PlayerFlightHooks
{
    public static void Apply()
    {
        On.Player.UpdateMSC += Player_UpdateMSC;
    }

    private static void Player_UpdateMSC(On.Player.orig_UpdateMSC orig, Player self)
    {
        orig(self);

        if (!self.IsBee(out var bee))
        {
            return;
        }

        const float normalGravity = 0.9f;
        const float normalAirFriction = 0.999f;
        const float flightGravity = 0.12f;
        const float flightAirFriction = 0.7f;
        const float flightKickinDuration = 6f;
        
        if (bee.CanFly)
        {
            if (self.animation == Player.AnimationIndex.HangFromBeam)
            {
                bee.preventFlight = 15;
            }
            else if (bee.preventFlight > 0)
            {
                bee.preventFlight--;
            }

            if (bee.isFlying)
            {
                bee.flyingBuzzSound.Volume = Mathf.Lerp(0f, 1f, bee.currentFlightDuration / flightKickinDuration);

                bee.currentFlightDuration++;

                self.AerobicIncrease(0.08f);

                self.gravity = Mathf.Lerp(normalGravity, flightGravity, bee.currentFlightDuration / flightKickinDuration);
                self.airFriction = Mathf.Lerp(normalAirFriction, flightAirFriction, bee.currentFlightDuration / flightKickinDuration);


                if (self.input[0].x > 0)
                {
                    self.bodyChunks[0].vel.x += bee.WingSpeed;
                    self.bodyChunks[1].vel.x -= 1f;
                }
                else if (self.input[0].x < 0)
                {
                    self.bodyChunks[0].vel.x -= bee.WingSpeed;
                    self.bodyChunks[1].vel.x += 1f;
                }

                if (self.room.gravity <= 0.5)
                {
                    if (self.input[0].y > 0)
                    {
                        self.bodyChunks[0].vel.y += bee.WingSpeed;
                        self.bodyChunks[1].vel.y -= 1f;
                    }
                    else if (self.input[0].y < 0)
                    {
                        self.bodyChunks[0].vel.y -= bee.WingSpeed;
                        self.bodyChunks[1].vel.y += 1f;
                    }
                }
                else if (bee.UnlockedVerticalFlight)
                {
                    if (self.input[0].y > 0)
                    {
                        self.bodyChunks[0].vel.y += bee.WingSpeedFly + (bee.Adrenaline * 0.5f) * 0.75f;
                        self.bodyChunks[1].vel.y -= 0.3f;
                    }
                    else if (self.input[0].y < 0)
                    {
                        self.bodyChunks[0].vel.y -= bee.WingSpeedFly + (bee.Adrenaline * 0.2f);
                        self.bodyChunks[1].vel.y += 0.3f;
                    }
                }

                bee.wingStaminaRecoveryCooldown = 40;
                bee.wingStamina--;

                if (self.isNPC)
                {
                    if (!bee.CanSustainFlight())
                    {
                        bee.StopFlight();
                    }
                }
                else
                {
                    if (!self.input[0].jmp || !bee.CanSustainFlight())
                    {
                        bee.StopFlight();
                    }
                }



            }
            else
            {
                bee.flyingBuzzSound.Volume = Mathf.Lerp(1f, 0f, bee.timeSinceLastFlight / flightKickinDuration);

                bee.timeSinceLastFlight++;

                bee.flyingBuzzSound.Volume = 0f;

                if (bee.wingStaminaRecoveryCooldown > 0)
                {
                    bee.wingStaminaRecoveryCooldown--;
                }
                else
                {
                    bee.wingStamina = Mathf.Min(bee.wingStamina + bee.WingStaminaRecovery, bee.WingStaminaMax);
                }

                if (self.wantToJump > 0 && bee.wingStamina > bee.MinimumFlightStamina && bee.CanSustainFlight())
                {
                    bee.InitiateFlight();
                }

                self.airFriction = normalAirFriction;
                self.gravity = normalGravity;
            }
        }
    }
}