# System Patterns

This document outlines the architectural and design patterns used in the Brick AIAR Unity project to ensure scalability, maintainability, and performance.

## Architecture
- Component-Based Architecture: Leveraging Unity's MonoBehaviour and ScriptableObject components for modularity.
- Event-Driven Architecture: Using C# events and UnityEvent for decoupled communication between systems.
- Singleton Pattern: For managing global managers such as GameManager, AudioManager, and AIManager.

## Key Technical Decisions
- Physics Engine: Unity's built-in PhysX for collision detection and physics simulation.
- Rendering System: Universal Render Pipeline (URP) for optimized graphics across platforms.
- Asset Management: Use of Addressables for efficient loading and memory management.

## Design Patterns
- Object Pooling: For frequently instantiated objects like projectiles or UI elements to reduce GC overhead.
- State Machine: For managing game states and AI behavior states.
- Command Pattern: For input handling and undo/redo functionality in building interactions.

## Unity Component Interaction
- Prefabs: Reusable LEGO components and UI elements.
- ScriptableObjects: Data containers for configuration and shared resources.
- Layers and Tags: For collision filtering and object categorization.
