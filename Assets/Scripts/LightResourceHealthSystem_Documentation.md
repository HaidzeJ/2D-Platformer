# Light Resource Health System Documentation

## Overview
The Light Resource Health System is an atmospheric health mechanic for the 2D Platformer where the player's vitality is represented through visual light intensity and brightness effects. As the player takes damage, their light dims; as they heal, their light brightens. This creates an immersive connection between gameplay mechanics and visual feedback.

## System Components

### 1. LightResourceHealth.cs
**Core health management system**

#### Key Features:
- **Light-based Health**: Health is represented as "light resource" (0-100)
- **Visual Feedback**: Light intensity, sprite brightness, and particle effects scale with health
- **Warning States**: 
  - Low Light Warning (< 30%)
  - Critical Light Warning (< 10%)
  - Death State (0%)
- **Regeneration**: Optional automatic health regeneration over time
- **Audio Integration**: Sound effects for damage, healing, warnings, and death
- **Event System**: Hooks for UI and other systems to respond to health changes

#### Public Methods:
```csharp
void TakeLightDamage(float damage, string source = "Unknown")
void RestoreLightResource(float amount, string source = "Restoration")
void RespawnWithFullLight()
```

#### Public Properties:
```csharp
float CurrentLightResource     // Current health value
float MaxLightResource        // Maximum health value
float LightResourcePercent    // Health as percentage (0-1)
bool IsLightExtinguished      // Is player dead?
bool IsInLowLight            // Is health below 30%?
bool IsInCriticalLight       // Is health below 10%?
```

#### Events:
```csharp
System.Action<float, float> OnLightResourceChanged  // (current, max)
System.Action OnLightExtinguished                   // Death event
System.Action OnLightWarning                        // Low light warning
System.Action OnLightCritical                       // Critical light warning
```

### 2. LightDamageSource.cs
**Objects that can damage the player's light resource**

#### Features:
- **Multiple Damage Types**:
  - Contact: Instant damage on touch
  - Proximity: Damage when near
  - Continuous: Ongoing damage while in contact
  - Periodic: Damage at intervals
- **Visual Effects**: Particle systems and glow effects
- **Audio Feedback**: Damage sounds and continuous sound loops
- **Configurable**: Damage amounts, intervals, and detection radius

#### Use Cases:
- Shadow zones that drain light
- Dark enemies or environmental hazards
- Poison/corruption areas
- Environmental traps

### 3. LightRestorationSource.cs
**Objects that can restore the player's light resource**

#### Features:
- **Activation Types**:
  - Automatic: Heals on proximity
  - Interaction: Requires input to activate (E key)
- **Healing Modes**:
  - Instant: One-time burst healing
  - Continuous: Ongoing healing over time
- **Resource Management**:
  - Permanent sources (always available)
  - Consumable sources (single-use with recharge time)
- **Visual Effects**: Light sources, particle effects, pulsing animations
- **Audio Feedback**: Activation, healing, and depletion sounds

#### Use Cases:
- Light crystals or healing fountains
- Campfires or light torches
- Magical restoration points
- Safe zones with ambient healing

### 4. LightResourceHealthUI.cs
**UI system for displaying health status**

#### Features:
- **Visual Elements**:
  - Health bar/slider with color coding
  - Numeric health display
  - Status text ("Radiant", "Dimming", "Flickering", etc.)
  - Warning panels with flashing effects
- **Color-Coded Health States**:
  - High Health (70%+): Cyan
  - Medium Health (40-70%): Yellow
  - Low Health (20-40%): Orange
  - Critical Health (0-20%): Red
- **Dynamic Effects**: Pulse animations at low health, warning flashes
- **Event Integration**: Responds to health system events

### 5. LightDrainZone.cs
**Individual light-draining element component**

#### Features:
- **Simple Setup**: Add to any GameObject to make it drain light on contact
- **Configurable Drain Rate**: Set how fast it drains light per second
- **Visual Effects**: Optional automatic sprite, particle, and glow effects
- **Audio Integration**: Drain sounds with loop options
- **Runtime Control**: Modify drain rate and radius during gameplay

## Integration with PlayerMovement System

### Health-Affected Movement Features:
1. **Speed Scaling**: Movement speed reduced based on health (50% to 100% effectiveness)
2. **Advanced Ability Restrictions**: Dash and double jump disabled below 20% health
3. **Fall Damage**: Excessive falls cause light damage (>15 units = 2 damage per unit)

### Implementation:
```csharp
// Speed modifier based on health
float GetHealthSpeedModifier()
{
    if (lightHealth == null) return 1f;
    float healthPercentage = lightHealth.LightResourcePercent;
    return Mathf.Lerp(0.5f, 1f, healthPercentage);
}

// Advanced ability check
bool CanPerformAdvancedAbilities()
{
    if (lightHealth == null) return true;
    return lightHealth.LightResourcePercent > 0.2f; // Need 20%+ health
}
```

## Setup Instructions

### Basic Setup:
1. **Add to Player**: Attach `LightResourceHealth.cs` to player GameObject
2. **Required Components**: Ensure player has Light, SpriteRenderer, ParticleSystem, AudioSource
3. **Create Damage Sources**: Add `LightDamageSource.cs` to hazardous objects
4. **Create Healing Sources**: Add `LightRestorationSource.cs` to beneficial objects
5. **UI Integration**: Create UI elements and attach `LightResourceHealthUI.cs`

### Quick Setup:
1. **Add to Player**: Attach `LightResourceHealth.cs` to player GameObject
2. **Create Damage Sources**: Add `LightDrainZone.cs` to hazardous objects
3. **Create Healing Sources**: Add `LightRestorationSource.cs` to beneficial objects
4. **Setup Scene Lighting**: Use `LightingSceneSetup.cs` to configure proper lighting

## Configuration Guidelines

### Health System Tuning:
- **Max Light Resource**: 100 (standard), adjust based on game difficulty
- **Low/Critical Thresholds**: 30%/10% respectively
- **Regeneration Rate**: 2-5 per second for gradual healing
- **Warning Durations**: 3-5 seconds for visual feedback

### Damage Sources:
- **Contact Damage**: 10-25 for environmental hazards
- **Continuous Damage**: 5-15 per second for area effects
- **Fall Damage**: 2 per unit beyond 15-unit threshold

### Restoration Sources:
- **Instant Healing**: 20-50 for checkpoint-style healing
- **Continuous Healing**: 10-20 per second for safe zones
- **Recharge Times**: 30-60 seconds for consumed sources

## Visual Design Recommendations

### Light Effects:
- **Player Light**: Point light with 6-unit range, white color
- **Health Scaling**: Intensity 0.2x to 2.0x based on health percentage
- **Warning Effects**: Flickering at low health, rapid flicker at critical

### Particle Systems:
- **Player Aura**: Subtle particles around player, intensity matches health
- **Damage Sources**: Red/dark particles with ominous movement
- **Restoration Sources**: Bright, upward-flowing particles

### Color Palette:
- **Healthy**: Cyan/Blue tones (bright, cool)
- **Warning**: Yellow/Orange tones (caution)
- **Critical**: Red tones (danger)
- **Healing**: White/Green tones (restoration)

## Events and Integration

### Health Events:
```csharp
// Subscribe to health changes
lightHealth.OnLightResourceChanged += (current, max) => {
    // Update UI, trigger effects, etc.
};

// Handle critical states
lightHealth.OnLightCritical += () => {
    // Screen effects, audio cues, etc.
};

// Handle death
lightHealth.OnLightExtinguished += () => {
    // Game over logic, respawn, etc.
};
```

### External System Integration:
- **UI Systems**: Use events for real-time health display updates
- **Audio Systems**: Dynamic music intensity based on health state
- **Visual Effects**: Screen overlays, post-processing effects
- **Save Systems**: Persist current health and maximum health values
- **Difficulty Systems**: Adjust damage/healing rates based on settings

## Performance Considerations

### Optimization Tips:
- **Event Subscriptions**: Always unsubscribe in OnDestroy to prevent memory leaks
- **Visual Updates**: Use events rather than per-frame polling for UI updates
- **Particle Systems**: Use object pooling for frequently spawned effects
- **Audio**: Use AudioSource components efficiently, avoid overlapping sounds

### Scalability:
- **Multiple Players**: System supports multiple instances without conflicts
- **Large Scenes**: Damage/restoration sources use efficient trigger detection
- **Mobile Performance**: Visual effects can be scaled down via quality settings

## Troubleshooting

### Common Issues:
1. **Light not updating**: Ensure Light component is assigned in inspector
2. **No damage taken**: Check collider layers and trigger settings
3. **UI not updating**: Verify event subscriptions in UI script
4. **Audio not playing**: Ensure AudioSource components exist and are enabled
5. **Fall damage not working**: Check if PlayerMovement integration is complete

### Debug Features:
- **Console Logging**: Essential health changes logged to console
- **Gizmos**: Visual representation of damage/healing ranges in scene view
- **Scene Setup Tools**: `LightingSceneSetup.cs` for proper lighting configuration
- **Individual Components**: Easy to test with `LightDrainZone.cs` and `LightRestorationSource.cs`

## Future Expansion Ideas

### Advanced Features:
- **Light Types**: Different colored lights for various abilities/power-ups
- **Environmental Interaction**: Light level affects visibility, enemy behavior
- **Multiplayer**: Shared light resources or light transfer between players
- **Progression**: Maximum light capacity increases with game progression
- **Equipment**: Items that modify light efficiency, regeneration, or resistance

### Gameplay Mechanics:
- **Light Puzzles**: Areas requiring specific light levels to access
- **Light Currency**: Spend light to activate special abilities
- **Light Sharing**: Transfer light between players or NPCs
- **Dynamic Difficulty**: Game difficulty scales with current light level
- **Environmental Storytelling**: Light level affects dialogue and story elements