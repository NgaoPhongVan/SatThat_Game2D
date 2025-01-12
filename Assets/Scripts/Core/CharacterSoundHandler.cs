using UnityEngine;

public class CharacterSoundHandler : MonoBehaviour
{
    // Các hàm này sẽ được gọi từ Animation Events

    // Player và Enemy melee attacks
    public void PlaySwordSwingSound()
    {
        AudioManager.Instance.PlaySound("SwordSwing");
    }

    // Archer
    public void PlayBowDrawSound()
    {
        AudioManager.Instance.PlaySound("BowDraw");
    }

    public void PlayArrowShootSound()
    {
        AudioManager.Instance.PlaySound("ArrowShoot");
    }

    // Hit reactions
    public void PlayHitSound()
    {
        AudioManager.Instance.PlaySound("HitImpact");
    }

    public void PlayArrowHitSound()
    {
        AudioManager.Instance.PlaySound("ArrowHit");
    }

    // Death sounds
    public void PlayDeathSound()
    {
        AudioManager.Instance.PlaySound("DeathSound");
    }

    // Movement sounds
    public void PlayFootstepSound()
    {
        AudioManager.Instance.PlaySound("Footstep");
    }

    public void PlayLandingSound()
    {
        AudioManager.Instance.PlaySound("Landing");
    }

    // Special moves
    public void PlayDodgeRollSound()
    {
        AudioManager.Instance.PlaySound("DodgeRoll");
    }

    public void PlayBlockSound()
    {
        AudioManager.Instance.PlaySound("BlockImpact");
    }
}