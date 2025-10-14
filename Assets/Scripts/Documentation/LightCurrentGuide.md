# Light Current System Guide

## 🌊 Overview
Light Currents are environmental elements that propel the orb in specific directions. They're perfect for creating guided navigation, atmospheric level design, and helping players navigate through ethereal areas.

## 🎮 How They Work
- **Trigger-based**: Player enters a trigger zone to be affected by the current
- **Directional Force**: Applies continuous force in a specified direction
- **Orb Mode Specific**: Can be configured to only affect players in orb mode
- **Visual & Audio Feedback**: Includes particle effects and sound cues

## 🔧 Setup Instructions

### 1. Basic Light Current Setup
1. Create an empty GameObject
2. Name it "LightCurrent_[Direction]" (e.g., "LightCurrent_Upward")
3. Add the `LightCurrent` component
4. Add a Collider2D (BoxCollider2D or CircleCollider2D)
5. **Important**: Set the collider as "Is Trigger" ✅

### 2. Configure Current Properties
```csharp
// Current Settings
Current Direction: Vector2.up    // Direction of force (will be normalized)
Current Force: 5f                // Strength of the force
Normalize Direction: true        // Automatically normalize direction vector
Force Mode: Force               // How force is applied (Force/Impulse)
Max Velocity: 15f               // Prevent excessive speeds
Only Affect Orbs: true          // Only affect orb mode players
```

### 3. Add Visual Effects (Optional)
1. Add the `LightCurrentVisualEffect` component to the same GameObject
2. Configure particle settings:
   - The component will auto-create a particle system if needed
   - Customize colors, flow speed, and intensity
   - Particles will flow in the direction of the current

### 4. Audio Setup (Optional)
- **Enter Sound**: Sound played when player enters the current
- **Loop Sound**: Continuous sound while player is in the current
- **Volume**: Adjust audio volume (0-1)

## 🎨 Visual Customization

### Gizmo Settings
```csharp
Show Direction Gizmo: true       // Show direction arrow in scene view
Gizmo Color: Cyan               // Color of the direction indicator
Gizmo Length: 2f                // Length of the direction arrow
```

### Particle Effect Settings
```csharp
Flow Speed: 2f                  // Speed of particle animation
Idle Intensity: 0.3f            // Particle intensity when inactive
Active Intensity: 1f            // Particle intensity when player is inside
Pulse Color: true               // Animate the color
Pulse Duration: 2f              // Time for one color pulse cycle
```

## 🏗️ Level Design Patterns

### 1. Guidance Streams
- **Purpose**: Guide orb through specific paths
- **Setup**: Chain multiple currents with overlapping areas
- **Direction**: Follow the desired path
- **Force**: Medium (3-5f)

### 2. Updraft Zones  
- **Purpose**: Help orb reach higher areas
- **Setup**: Large circular or rectangular trigger areas
- **Direction**: Vector2.up
- **Force**: Strong (6-10f)

### 3. Momentum Preservers
- **Purpose**: Maintain orb speed through areas
- **Setup**: Long rectangular triggers along paths
- **Direction**: Forward movement direction
- **Force**: Light (2-4f)

### 4. Current Intersections
- **Purpose**: Create choices in navigation
- **Setup**: Multiple currents with different directions intersecting
- **Visual**: Use different colors for each direction
- **Force**: Medium, equal strength

## ⚙️ Technical Details

### Force Application
The system applies force every `FixedUpdate()` while the player is in the trigger zone:
```csharp
Vector2 forceToApply = NormalizedDirection * currentForce;
playerRb.AddForce(forceToApply, forceMode);
```

### Velocity Limiting
Prevents excessive speeds by clamping velocity in the current's direction:
- If `maxVelocity > 0`, the system will reduce force when approaching the limit
- Helps maintain controllable orb movement

### Orb Mode Detection
```csharp
// Only affects players in orb mode if enabled
if (onlyAffectOrbs && !player.IsInOrbMode) return;
```

## 🧪 Testing Tips

### Debug Information
- Light Currents log entry/exit messages to console
- Gizmos show direction and trigger area in Scene view
- Selected currents display force values and properties

### Recommended Settings by Use Case

#### **Gentle Guidance** (Subtle direction hints)
- Force: 2-3f
- Max Velocity: 8f
- Visual: Subtle particles, soft colors

#### **Strong Currents** (Clear directional push)
- Force: 5-8f  
- Max Velocity: 12f
- Visual: Bright particles, obvious flow

#### **Wind Tunnels** (Fast transport)
- Force: 8-15f
- Max Velocity: 20f
- Visual: Fast-moving particles, intense colors

## 🔗 Integration with Other Systems

### With Orb Movement
- Currents work seamlessly with orb light pulse mechanics
- Player can still pulse while in currents for combined movement
- A/D gentle movement works alongside current forces

### With Ability Unlocks
- Use `onlyAffectOrbs = true` to disable currents when player gains traditional movement
- Can create "orb-only" areas that become inaccessible after progression

### With Level Progression
- Combine with AbilityUnlockTrigger to create guided tutorials
- Use currents to lead players to ability unlock locations

## 💡 Creative Ideas

### Environmental Storytelling
- **Ascending Spirits**: Upward currents with ethereal particles
- **Ancient Winds**: Horizontal currents in ruins or caves  
- **Energy Flows**: Multi-colored currents showing power distribution
- **Gravitational Fields**: Radial currents around celestial objects

### Gameplay Mechanics
- **Current Puzzles**: Player must navigate intersecting currents to reach goals
- **Momentum Challenges**: Use currents to build up speed for long jumps
- **Wind Mazes**: Complex networks of currents creating navigation puzzles
- **Current Switches**: Activate/deactivate currents with triggers or buttons

## 🚀 Quick Start Checklist

✅ GameObject with LightCurrent component  
✅ Collider2D set as Trigger  
✅ Configure direction and force  
✅ Add LightCurrentVisualEffect (optional)  
✅ Set up audio clips (optional)  
✅ Test with orb mode player  
✅ Adjust force and max velocity as needed  

---

**Ready to flow! 🌊** Your light currents will now guide players through your ethereal game world with smooth, atmospheric environmental movement.