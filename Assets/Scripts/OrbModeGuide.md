# Orb Mode System Guide

## 🌟 Overview
The Orb Mode is the initial movement state in your 2D Platformer, where the player controls a floating orb with unique physics and limited movement options. This creates an ethereal, mysterious introduction before the player gains traditional platformer abilities.

## 🎮 Controls

### Orb Mode (Stage 0)
- **A/D**: Apply gentle forces for drifting movement (only while airborne/floating)
- **Space**: Light Pulse (upward boost that creates lasting momentum)  
- **Space + A/D**: Directional Light Pulse (diagonal momentum)
- **Physics-based movement** - orb maintains momentum and drifts naturally
- **Airborne control only** - no left/right movement when resting on ground
- **Momentum preservation** - forces from pulses and environmental elements create lasting motion

### Traditional Platformer (Stage 1+)  
- **A/D**: Left/Right movement
- **Space**: Jump (when unlocked)
- **Shift**: Dash (when unlocked)

## 🔧 Technical Implementation

### Key Features
1. **Reduced Gravity**: `orbGravityScale = 0.3f` creates floating effect
2. **Light Pulse Mechanics**: Space bar triggers upward or directional impulses
3. **Inertia-Driven Movement**: Orb maintains momentum from forces and drifts naturally
4. **Air Resistance**: Minimal drag allows momentum preservation while preventing runaway speeds
5. **Pulse Cooldown**: Prevents spam clicking for balanced gameplay

### Settings (Configurable in Inspector)
```csharp
// Basic Orb Settings
[SerializeField] float orbGravityScale = 0.3f;      // Reduced gravity for floating
[SerializeField] float orbPulseForce = 8f;          // Upward pulse strength  
[SerializeField] float orbPulseSideForce = 6f;      // Horizontal pulse strength
[SerializeField] float orbPulseCooldown = 0.2f;     // Cooldown between pulses

// Inertia-Driven Movement
[SerializeField] float orbMoveForce = 4f;           // Force applied for A/D movement (airborne only)
[SerializeField] float orbAirResistance = 0.99f;    // Air resistance (0.99 = very little drag)  
[SerializeField] float orbMaxSpeed = 12f;           // Maximum speed limit
[SerializeField] bool enableSpeedLimiting = true;   // Whether to limit maximum speed
```

**Important**: A/D movement only works when the orb is airborne (`!isGrounded`). When resting on the ground, the orb cannot move horizontally - it must first pulse (Space) to become airborne before gaining directional control.

## 🏗️ Level Design Considerations

### Orb Mode Areas Should Have:
- **Wider spaces** - Player has less precise control
- **Gentle slopes** - Orb can slide and bounce naturally  
- **Vertical challenges** - Emphasize the pulse mechanic
- **Environmental movement aids** - Wind zones, currents, etc.

### Transition Elements:
- **Ability Unlock Triggers** - Use `AbilityUnlockTrigger` to progress stages
- **Visual feedback** - Particle effects, lighting changes
- **Audio cues** - Different sounds for orb vs. traditional movement

## 🧪 Testing & Debug

### Debug Controls
- **U Key**: Unlock next movement stage
- **R Key**: Reset to Orb Mode (Stage 0)
- **I Key**: Show current abilities
- **On-screen GUI**: Real-time ability display and stage controls

### Stage Progression
0. **Orb Mode** → Light Pulse only
1. **Movement** → Left/Right movement unlocked
2. **Jump** → Basic jumping unlocked  
3. **Double Jump** → Aerial double jump unlocked
4. **Dash** → Fast dash ability unlocked
5. **Wall Jump** → Future expansion ready

## 💡 Gameplay Ideas

### Environmental Elements (Future)
- **Wind Zones**: Push the orb in specific directions
- **Gravity Wells**: Attract or repel the orb
- **Light Collectors**: Recharge pulse energy faster
- **Momentum Preservers**: Areas where damping is reduced
- **Pulse Amplifiers**: Increase pulse force temporarily

### Visual/Audio Feedback
- **Pulse Effect**: Flash of light when space is pressed
- **Floating Particles**: Ambient particles around the orb
- **Energy Meter**: Visual representation of pulse cooldown
- **Transformation Sequence**: When transitioning to platformer mode

## 🔗 Integration with Existing Systems

The orb mode integrates seamlessly with:
- ✅ **Movement Stage System** - Uses Stage 0 
- ✅ **Ability Unlock Triggers** - Can transition to traditional movement
- ✅ **Debug Manager** - Full testing support
- ✅ **Input System** - Reuses existing jump input for pulse

## 🚀 Next Steps

1. **Test in Unity** - Play with the orb physics settings
2. **Create Orb Levels** - Design areas specifically for orb gameplay
3. **Add Visual Effects** - Particle systems for pulse feedback
4. **Environmental Elements** - Add wind zones or other orb-affecting elements
5. **Transition Sequence** - Create dramatic transformation from orb to platformer

---

**Ready to float! 🌟** Your player now starts as a mystical orb with unique movement mechanics, perfect for creating an atmospheric introduction to your game world.