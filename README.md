# Doom-Inspired FPS in Unity

This project provides the core scripts needed to create a Doom-inspired FPS game in Unity. This README explains how to set up the project and use the various components.

## Setup Instructions

1. Create a new Unity 3D project
2. Import these scripts into your project's Assets/Scripts folder
3. Set up the following basic scene structure:

### Player Setup
- Create a Player GameObject with:
  - CharacterController component
  - Add PlayerController.cs script
  - Add WeaponController.cs script
  - Add PlayerHealth.cs script
  - Add InteractionSystem.cs script
  - Add a Camera as a child object with the CameraShake.cs script

### Weapon Setup
1. Create weapon models as prefabs
2. Configure the WeaponController with weapons:
   - Set up weapon models, fire rates, damages, sounds
   - Create muzzle flash effects
   - Set up UI elements for ammo display

### Enemy Setup
1. Create enemy GameObjects with:
   - NavMeshAgent component
   - Animator component (with appropriate animations)
   - Collider component
   - Enemy.cs script
2. Bake NavMesh for your level to allow enemies to navigate

### Level Design
1. Create your level geometry
2. Add interactable elements:
   - Doors with DoorController.cs
   - Add InteractableDoor.cs to door triggers
   - Create pickups (health, armor, ammo) with PickupItem.cs
   - Set up switches and key cards for progression

### Effects System
1. Create a VisualEffects GameObject with the VisualEffects.cs script
2. Set up effect prefabs:
   - Bullet impacts for different surfaces
   - Muzzle flashes
   - Blood effects
   - Explosion effects

### Game Management
1. Create a GameManager GameObject with the GameManager.cs script
   - Set up UI elements for health, armor, ammo
   - Configure level transitions
   - Set up pause menu

## Key Features

### Player Movement
- WASD movement
- Mouse look
- Jumping
- Running (shift key)

### Weapon System
- Multiple weapon support
- Automatic and semi-automatic firing modes
- Ammo management and reloading
- Weapon switching (number keys or scroll wheel)

### Enemy AI
- Enemies detect and chase player
- Attack when in range
- Take damage and die
- Can drop items

### Interaction System
- Interact with doors, switches, and items
- Key card system for locked doors
- Enemy kill count doors

### Visual Effects
- Muzzle flashes
- Impact effects based on surface type
- Blood effects
- Custom decal system
- Camera shake

## Scripts Overview

- **PlayerController.cs**: Handles player movement, jumping, and mouse look
- **WeaponController.cs**: Manages weapons, shooting, ammo, and switching
- **CameraShake.cs**: Adds camera shake effects for weapons and impacts
- **Enemy.cs**: Controls enemy AI, health, and attacks
- **PlayerHealth.cs**: Manages player health, armor, and damage effects
- **PickupItem.cs**: Handles pickable items like health, armor, ammo
- **GameManager.cs**: Controls game flow, level management, and UI
- **DoorController.cs**: Manages door animations and locking mechanisms
- **InteractionSystem.cs**: Handles player interactions with objects
- **VisualEffects.cs**: Creates effects like bullet impacts and muzzle flashes

## Extending the Project

- Add more weapons with different behaviors
- Create additional enemy types with unique AI
- Design more complex levels with secrets and puzzles
- Implement a save/load system
- Add boss battles
- Create a more detailed UI with a minimap
- Add weapon upgrades and collectibles
