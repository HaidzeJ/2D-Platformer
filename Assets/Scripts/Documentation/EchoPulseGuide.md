# Echo Pulse System Guide

## Overview
The Echo Pulse system adds atmospheric exploration mechanics to the orb mode. When the player presses Space (Light Pulse), an expanding detection ring reveals hidden echo objects for 2-3 seconds, creating engaging discovery and puzzle-solving gameplay.

## 🔊 How It Works
1. **Player triggers Light Pulse** (Space key) while in orb mode
2. **Echo Pulse expands** from player position at configurable speed
3. **Hidden objects are revealed** when the pulse reaches them
4. **Objects stay visible** for ~2.5 seconds then fade back to hidden
5. **Visual ring effect** shows the expanding pulse radius

## 🛠️ Setup Instructions

### For the Player
The Echo Pulse system is automatically integrated into PlayerMovement.cs. Configure these settings in the inspector:

**Echo Pulse Settings:**
- `echoPulseRadius`: Maximum detection range (default: 10f)
- `echoPulseExpandSpeed`: How fast the pulse expands (default: 25f)
- `echoPulseRevealDuration`: How long objects stay revealed (default: 2.5f)
- `echoObjectLayers`: Which layers contain echo objects (default: all layers)

### Creating Echo Objects

#### Echo Modes
All echo objects support three different behaviors:

**EchoMode.Reveal** (Default):
- Objects start hidden/transparent
- Echo pulse reveals them temporarily
- Auto-hide after duration
- Perfect for: Secret platforms, hidden passages

**EchoMode.Hide** (NEW):
- Objects start visible/solid
- Echo pulse hides them temporarily  
- Auto-reappear after duration
- Perfect for: Disappearing platforms, temporary gaps

**EchoMode.Toggle**:
- Objects start visible or hidden
- Echo pulse toggles their state
- Optional duration for temporary changes
- Perfect for: Switches, doors, interactive elements

#### 1. Echo Bridges (Invisible Platforms)
```csharp
// Add EchoBridge component to a GameObject with:
// - SpriteRenderer (for visual feedback)
// - Collider2D (for player collision)
// - Set layer to match echoObjectLayers in PlayerMovement
```

**Setup:**
1. Create GameObject with sprite and BoxCollider2D
2. Add `EchoBridge` component
3. Configure settings:
   - `startVisible`: false (bridge starts hidden)
   - `isPermanentOnceRevealed`: true for progression bridges
   - `fadeWarningDuration`: warning time before disappearing

#### 2. Echo Doors (Hidden Passages)
```csharp
// Add EchoDoor component for sliding doors that open/close
```

**Setup:**
1. Create door GameObject with sprite and collider
2. Add `EchoDoor` component
3. Set positions:
   - Position door at closed location
   - Use context menu "Set Closed Position"
   - Move to open location, use "Set Open Position"
4. Configure behavior:
   - `echoMode`: Toggle for doors that open/close on each pulse
   - `staysOpenPermanently`: true for progression doors

#### 3. Echo Current Switchers (Environmental Puzzles)
```csharp
// Add EchoCurrentSwitcher to control Light Currents
```

**Setup:**
1. Create switch GameObject with sprite
2. Add `EchoCurrentSwitcher` component
3. Drag Light Current objects into `targetCurrents` array
4. Configure behavior:
   - `togglesCurrentsOnReveal`: auto-toggle when revealed
   - `requiresPlayerActivation`: needs player to touch switch

### Layer Setup
1. Create a layer called "EchoObjects" (or use existing layer)
2. Set all echo objects to this layer
3. Configure `echoObjectLayers` in PlayerMovement to include this layer

## 🎨 Visual Effects

### Automatic Visual Effects
- **Expanding Ring**: LineRenderer shows pulse expansion
- **Object Glow**: Revealed objects get cyan glow effect
- **Fade Animation**: Objects smoothly fade in/out

### Adding Custom Effects
```csharp
// Add EchoPulseVisualEffect component to player for ring effect
// Add ParticleSystem components to echo objects for custom reveals
```

## 🔊 Audio Integration

### Built-in Audio Support
Each echo object type supports audio clips:
- `echoRevealSound`: Played when object is revealed
- `doorOpenSound`/`doorCloseSound`: For door movements
- `switchActivateSound`: For current switchers

### Setup Audio
1. Add AudioSource component to echo objects (auto-created if needed)
2. Assign audio clips in inspector
3. Adjust volume settings per object type

## 🎮 Gameplay Design Tips

### Level Design Principles
1. **Discovery Rhythm**: Place echo objects just outside normal sight range
2. **Progressive Reveals**: Use bridges to access new areas with more secrets
3. **Puzzle Chains**: Connect switchers to currents that lead to new echo objects
4. **Risk/Reward**: Some permanent bridges after first reveal, others temporary

### Balancing Considerations
- **Pulse Cooldown**: 0.5s prevents spam but maintains responsiveness
- **Reveal Duration**: 2.5s gives time to navigate without being too easy
- **Detection Range**: 10f radius encourages exploration but isn't overpowered

## 📝 Example Level Layouts

### Basic Discovery Area
```
Player Start → [Hidden Bridge] → Platform with [Echo Switch] → [Light Current] → New Area
```

### Puzzle Chain
```
[Echo Door] blocks path → [Hidden Switch] reveals → [Current flows] → [Bridge appears] → Progress
```

### Vertical Exploration
```
Player at bottom → [Echo Pulse] reveals → [Bridge stairs] → [Switch] → [Current upward] → [Final bridge] → Goal
```

## 🐛 Debugging

### Console Messages
- `💫 Light Pulse`: Shows pulse triggered with direction/force
- `🔊 Echo Pulse triggered`: Confirms echo detection started
- `🔍 Echo revealed`: Shows each object revealed with distance
- `🌉 Bridge revealed`: Bridge-specific feedback
- `🚪 Door opened`: Door state changes
- `🔌 Switch toggled`: Current switcher activations

### Common Issues
1. **Objects not revealing**: Check layer settings and echo object components
2. **Pulse not triggering**: Verify orb mode is active and cooldown has elapsed
3. **Visual effects missing**: Ensure EchoPulseVisualEffect component is on player
4. **Audio not playing**: Check AudioSource components and clip assignments

### Testing Tools
- Use context menus on echo objects: "Test Toggle", "Make Permanent"
- Debug logs show detailed state information
- Gizmos in scene view show door positions and current directions

## 🔧 Customization

### Creating Custom Echo Objects
Inherit from `EchoObject` base class:
```csharp
public class CustomEchoObject : EchoObject
{
    protected override void OnEchoRevealed(float duration)
    {
        base.OnEchoRevealed(duration);
        // Add custom reveal behavior
    }
}
```

### Advanced Configurations
- Modify detection algorithm for non-circular pulses
- Add multiple pulse types with different properties
- Create object-specific reveal durations
- Implement chain reactions between echo objects

The Echo Pulse system creates rich exploration gameplay where players actively search for hidden paths and mechanisms, perfectly complementing the atmospheric orb mode movement!