# InputManager for MonoGame/XNA

A comprehensive, feature-rich input management system for MonoGame and XNA Framework projects.

## Features

- **Complete Input State Tracking** - Keyboard and mouse state monitoring across frames
- **Advanced Key Detection**
  - Single, double, and multi-tap detection with customizable timing intervals
  - Key hold duration tracking
  - Key release detection
  - Combined tap and hold detection
- **Modifier Key Support** - Convenient methods for detecting Shift, Ctrl, and Alt combinations
- **Text Input** - Character mapping for keyboard input
- **Mouse Functionality**
  - Button state detection (press, hold, release)
  - Position and movement tracking
  - Scroll wheel detection
  - Rectangle-based hover detection
- **Input Aggregation** - Methods for detecting any key press/release events

## Getting Started

1. Add the `InputManager.cs` file to your project
2. Create an instance of `InputManager` in your game class
3. Call the `Update()` method once per frame in your game's update loop

```csharp
// In your Game class
private InputManager inputManager;

protected override void Initialize()
{
    base.Initialize();
    inputManager = new InputManager();
}

protected override void Update(GameTime gameTime)
{
    // Update the input manager first
    inputManager.Update();
    
    // Now you can use input detection in your game logic
    if (inputManager.KeyTapped(Keys.Space))
    {
        // Do something when space is tapped
    }
    
    base.Update(gameTime);
}
```

## Usage Examples

### Basic Input Detection

```csharp
// Check if a key was just pressed this frame
if (inputManager.KeyTapped(Keys.A))
{
    // Handle A key tap
}

// Check if a key is being held down
if (inputManager.KeyHeld(Keys.W))
{
    // Move player forward
}

// Check if a key was released this frame
if (inputManager.KeyReleased(Keys.Escape))
{
    // Open menu
}
```

### Multi-tap Detection

```csharp
// Detect double-tap (within 60 frames)
if (inputManager.KeyMultiTapped(Keys.E, 2, 60))
{
    // Perform double-tap action
}

// Detect triple-tap
if (inputManager.KeyMultiTapped(Keys.Q, 3))
{
    // Perform triple-tap action
}
```

### Hold Duration

```csharp
// Check if a key has been held for at least 30 frames
if (inputManager.KeyHeldForDuration(Keys.R, 30))
{
    // Reload weapon
}

// Check if a key has been held for exactly 60 frames
if (inputManager.KeyHeldForExactDuration(Keys.Shift, 60))
{
    // Execute charged attack
}

// Check if a key was just tapped OR has been held for the specified duration
if (inputManager.KeyTappedAndHeldForDuration(Keys.F, 45))
{
    // Interact with object - triggers immediately on tap and again after hold duration
    // Perfect for actions that can be quick-pressed or charged
}
```

### Modifier Keys

```csharp
// Check for Shift+S (save)
if (inputManager.KeyTapped(Keys.S, true, false, false))
{
    // Save game
}

// Check for Ctrl+Z (undo)
if (inputManager.KeyTapped(Keys.Z, false, true, false))
{
    // Undo action
    //this will NOT trigger if only Z is tapped
}
```

### Mouse Input

```csharp
// Check for left mouse button click
if (inputManager.MBTapped(MouseButton.Left))
{
    // Handle click
}

// Get mouse position
Point mousePosition = inputManager.CMousePos();

// Check if mouse is within a UI element
if (inputManager.MWithinRect(myButton.Bounds))
{
    // Mouse is hovering over button
}

// Check for mouse wheel scrolling
float scrollValue = inputManager.MWheelValue();
```

### Text Input

```csharp
// Get character input for text fields
char? pressedChar = inputManager.GetPressedChar();
if (pressedChar.HasValue)
{
    inputText += pressedChar.Value;
}
```

## License

This project is available under the MIT License. Feel free to use, modify, and distribute as needed.

## Acknowledgments

Created by djemsdjems. Inspired by the need for a robust input system for MonoGame projects.
