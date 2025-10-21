using UnityEngine;

/// <summary>
/// Simple platform that can be revealed or hidden by echo pulses.
/// Supports all three echo modes: Reveal, Hide, and Toggle.
/// This is the most basic implementation of EchoObject for general platform use.
/// </summary>
public class EchoPlatform : EchoObject
{
    [Header("Platform Settings")]
    [SerializeField] private bool playEffectSounds = true;
    [SerializeField] private AudioClip revealSound;
    [SerializeField] private AudioClip hideSound;
    
    private AudioSource audioSource;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Setup audio source if sounds are provided
        if (playEffectSounds && (revealSound != null || hideSound != null))
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.volume = 0.7f;
            }
        }
    }
    
    protected override void OnEchoRevealed(float duration)
    {
        base.OnEchoRevealed(duration);
        
        // Play appropriate sound based on what happened
        if (audioSource != null && playEffectSounds)
        {
            switch (echoMode)
            {
                case EchoMode.Reveal when isRevealed:
                    // Platform was revealed
                    if (revealSound != null)
                        audioSource.PlayOneShot(revealSound);
                    break;
                    
                case EchoMode.Hide when !isRevealed:
                    // Platform was hidden
                    if (hideSound != null)
                        audioSource.PlayOneShot(hideSound);
                    break;
                    
                case EchoMode.Toggle:
                    // Platform was toggled
                    AudioClip soundToPlay = isRevealed ? revealSound : hideSound;
                    if (soundToPlay != null)
                        audioSource.PlayOneShot(soundToPlay);
                    break;
            }
        }
        
        // Debug logging
        string action = echoMode switch
        {
            EchoMode.Reveal => isRevealed ? "revealed" : "already visible",
            EchoMode.Hide => isRevealed ? "still visible" : "hidden",
            EchoMode.Toggle => isRevealed ? "revealed" : "hidden",
            _ => "affected"
        };
        
        Debug.Log($"🔊 Echo Platform '{name}' was {action} for {duration} seconds");
    }
}