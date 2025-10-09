# Unity 2D Platformer

A 2D platformer game built with Unity featuring smooth movement, jumping mechanics, and dash abilities.

## Repository Setup

This repository is optimized for Unity development with:
- ✅ **Git LFS** for binary assets (textures, audio, models, Unity files)
- ✅ **Optimized .gitignore** for Unity projects
- ✅ **Fast pushes** - only essential files are version controlled

### Binary Files Handled by Git LFS
- Unity scenes (.unity)
- Assets (.asset, .prefab, .mat, etc.)
- Images (png, jpg, psd, etc.)
- Audio files (mp3, wav, ogg, etc.)
- 3D models (fbx, blend, obj, etc.)
- Project settings

### Prerequisites
- Unity 2022.3 LTS or later
- Git with Git LFS support
- .NET SDK 9.0.200+ (for .slnx support)

### Clone Instructions
```bash
# Clone with LFS files
git clone --recurse-submodules <your-repo-url>
cd "2D Platformer"

# Pull LFS files (if not automatically downloaded)
git lfs pull
```

### Development Setup
1. Clone the repository
2. Open the project in Unity Hub
3. Unity will automatically import and setup the project

## Features
- Player movement with acceleration/deceleration
- Variable height jumping
- Double jump capability
- Dash mechanics with cooldown
- Coyote time and jump buffering
- Input System integration

## Project Structure
```
Assets/
├── Scenes/          # Game scenes
├── Scripts/         # C# scripts
│   └── PlayerMovement.cs
└── Settings/        # Unity settings and templates

ProjectSettings/     # Unity project configuration
```

## Controls
- **Move**: WASD / Arrow Keys
- **Jump**: Space
- **Dash**: Left Shift

## Technical Notes
- Uses Unity's new Input System
- Rigidbody2D-based physics
- Modular script architecture
- Cinemachine ready for camera management

---

> **Repository Optimization**: This repo uses Git LFS for binary files, ensuring fast clones and pushes. Large Unity assets are automatically managed.