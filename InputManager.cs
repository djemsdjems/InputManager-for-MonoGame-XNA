using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Infirejer.Scripts {
    public class InputManager {
        public KeyboardState PKS;
        public KeyboardState CKS;
        public MouseState PMS;
        public MouseState CMS;
        public int HoldTime;
        private Dictionary<Keys, int> keyHoldDurations = new Dictionary<Keys, int>();

        private class TapInfo {
            public long LastTapFrame;    // When was the last tap registered
            public int TapCount;        // How many taps have occurred in sequence
            public bool WasReleased;    // Has the key been released since last tap
            public bool JustTapped;     // Has tap been registered this frame (prevents double counting)
            public int CustomInterval;  // Store custom interval for this key
        }

        private Dictionary<Keys, TapInfo> keyTapHistory = new Dictionary<Keys, TapInfo>();
        private long currentFrame = 0;

        // Maximum number of frames between taps to count as consecutive
        private const int DEFAULT_MAX_INTERVAL = 20;

        private readonly Dictionary<Keys, char> KeyChars = new(){
            // Letters
            {Keys.A, 'a'}, {Keys.B, 'b'}, {Keys.C, 'c'}, {Keys.D, 'd'}, {Keys.E, 'e'},
            {Keys.F, 'f'}, {Keys.G, 'g'}, {Keys.H, 'h'}, {Keys.I, 'i'}, {Keys.J, 'j'},
            {Keys.K, 'k'}, {Keys.L, 'l'}, {Keys.M, 'm'}, {Keys.N, 'n'}, {Keys.O, 'o'},
            {Keys.P, 'p'}, {Keys.Q, 'q'}, {Keys.R, 'r'}, {Keys.S, 's'}, {Keys.T, 't'},
            {Keys.U, 'u'}, {Keys.V, 'v'}, {Keys.W, 'w'}, {Keys.X, 'x'}, {Keys.Y, 'y'},
            {Keys.Z, 'z'},
            
            // Numbees
            {Keys.D0, '0'}, {Keys.D1, '1'}, {Keys.D2, '2'}, {Keys.D3, '3'}, {Keys.D4, '4'},
            {Keys.D5, '5'}, {Keys.D6, '6'}, {Keys.D7, '7'}, {Keys.D8, '8'}, {Keys.D9, '9'},
            {Keys.NumPad0, '0'}, {Keys.NumPad1, '1'}, {Keys.NumPad2, '2'}, {Keys.NumPad3, '3'},
            {Keys.NumPad4, '4'}, {Keys.NumPad5, '5'}, {Keys.NumPad6, '6'}, {Keys.NumPad7, '7'},
            {Keys.NumPad8, '8'}, {Keys.NumPad9, '9'},
            
            // Special characters
            {Keys.Space, ' '}, {Keys.OemPeriod, '.'}, {Keys.OemComma, ','},
            {Keys.OemMinus, '-'}, {Keys.OemPlus, '+'}, {Keys.OemQuestion, '?'},
            {Keys.OemQuotes, '\''}, {Keys.OemSemicolon, ';' }, {Keys.OemPipe, '\\'},
            {Keys.OemBackslash, '\\'},
        };

        // Add shifted characters dictioanary
        private readonly Dictionary<Keys, char> ShiftedKeyChars = new()
        {
            // Shifted numbers
            {Keys.D1, '!'}, {Keys.D2, '@'}, {Keys.D3, '#'}, {Keys.D4, '$'}, {Keys.D5, '%'},
            {Keys.D6, '^'}, {Keys.D7, '&'}, {Keys.D8, '*'}, {Keys.D9, '('}, {Keys.D0, ')'},
            
            // Shifted special characters
            {Keys.OemPeriod, '>'}, {Keys.OemComma, '<'}, {Keys.OemMinus, '_'},
            {Keys.OemPlus, '+'}, {Keys.OemQuestion, '?'}, {Keys.OemQuotes, '"'},
            {Keys.OemSemicolon, ':'},{Keys.OemPipe, '|'},{Keys.OemBackslash, '|'},
        };


        public InputManager() {
            CKS = Keyboard.GetState();
            PKS = CKS;
            CMS = Mouse.GetState();
            PMS = CMS;
        }
        public void Update() {
            PKS = CKS;
            CKS = Keyboard.GetState();

            PMS = CMS;
            CMS = Mouse.GetState();

            currentFrame++;

            if (CKS.GetPressedKeyCount() > 0 && !OnlyAModIsPressed()) HoldTime++;
            else HoldTime = 0;

            UpdateKeyHoldDurations();
            UpdateTapTracking();
        }
        private void UpdateKeyHoldDurations() {
            // Get all currently pressed keys
            Keys[] pressedKeys = CKS.GetPressedKeys();

            // Increment counters for currently pressed keys
            foreach (Keys key in pressedKeys) {
                if (keyHoldDurations.ContainsKey(key)) {
                    keyHoldDurations[key]++;
                }
                else {
                    keyHoldDurations[key] = 1;
                }
            }

            // Reset counters for released keys
            List<Keys> keysToReset = new List<Keys>();
            foreach (Keys key in keyHoldDurations.Keys) {
                if (!CKS.IsKeyDown(key)) {
                    keysToReset.Add(key);
                }
            }

            foreach (Keys key in keysToReset) {
                keyHoldDurations.Remove(key);
            }
        }
        private void UpdateTapTracking() {
            // For every tap info, reset the JustTapped flag
            foreach (var key in keyTapHistory.Keys) {
                keyTapHistory[key].JustTapped = false;
            }

            // Process all keys
            foreach (Keys key in CKS.GetPressedKeys()) {
                // Initialize tap info if not existing
                if (!keyTapHistory.ContainsKey(key)) {
                    keyTapHistory[key] = new TapInfo {
                        LastTapFrame = -DEFAULT_MAX_INTERVAL,
                        TapCount = 0,
                        WasReleased = true,
                        JustTapped = false,
                        CustomInterval = DEFAULT_MAX_INTERVAL
                    };
                }

                TapInfo info = keyTapHistory[key];

                // If key was just pressed and was previously released 
                if (!PKS.IsKeyDown(key) && CKS.IsKeyDown(key) && info.WasReleased) {
                    // Check if this tap is within the interval of the previous tap
                    if (currentFrame - info.LastTapFrame <= info.CustomInterval) {
                        info.TapCount++; // Increment tap count for consecutive taps
                    }
                    else {
                        info.TapCount = 1; // Reset tap count for new sequence
                    }

                    info.LastTapFrame = currentFrame;
                    info.WasReleased = false;
                    info.JustTapped = true;
                }
            }

            // Track key releases
            foreach (Keys key in Enum.GetValues(typeof(Keys))) {
                if (keyTapHistory.ContainsKey(key)) {
                    if (PKS.IsKeyDown(key) && !CKS.IsKeyDown(key)) {
                        keyTapHistory[key].WasReleased = true;
                    }

                    // Reset tap count if too much time passes since last tap
                    TapInfo info = keyTapHistory[key];
                    if (info.TapCount > 0 &&
                        currentFrame - info.LastTapFrame > info.CustomInterval &&
                        info.WasReleased) {
                        info.TapCount = 0;
                    }
                }
            }
        }
        private bool ModifiersPressed(bool requireShift, bool requireCtrl, bool requireAlt) {
            bool shift = CKS.IsKeyDown(Keys.LeftShift) || CKS.IsKeyDown(Keys.RightShift);
            bool ctrl = CKS.IsKeyDown(Keys.LeftControl) || CKS.IsKeyDown(Keys.RightControl);
            bool alt = CKS.IsKeyDown(Keys.LeftAlt) || CKS.IsKeyDown(Keys.RightAlt);

            return (shift == requireShift) &&
                   (ctrl == requireCtrl) &&
                   (alt == requireAlt);
        }
        public bool KeyTapped(Keys key, bool Mod = false, bool Ctrl = false, bool Alt = false) {
            return !PKS.IsKeyDown(key) && CKS.IsKeyDown(key) && ModifiersPressed(Mod, Ctrl, Alt);
        }
        public bool KeyReleased(Keys key, bool Mod = false, bool Ctrl = false, bool Alt = false) {
            return PKS.IsKeyDown(key) && !CKS.IsKeyDown(key) && ModifiersPressed(Mod, Ctrl, Alt);
        }
        public bool KeyHeld(Keys key, bool Mod = false, bool Ctrl = false, bool Alt = false) {
            return CKS.IsKeyDown(key) && ModifiersPressed(Mod, Ctrl, Alt);
        }
        public bool MBTapped(MouseButton button, bool Mod = false, bool Ctrl = false, bool Alt = false) {
            return (button switch {
                MouseButton.Left => !PMS.LeftButton.Equals(ButtonState.Pressed) && CMS.LeftButton.Equals(ButtonState.Pressed),
                MouseButton.Right => !PMS.RightButton.Equals(ButtonState.Pressed) && CMS.RightButton.Equals(ButtonState.Pressed),
                MouseButton.Middle => !PMS.MiddleButton.Equals(ButtonState.Pressed) && CMS.MiddleButton.Equals(ButtonState.Pressed),
                _ => false,
            }) && ModifiersPressed(Mod, Ctrl, Alt);
        }
        public bool MBReleased(MouseButton button, bool Mod = false, bool Ctrl = false, bool Alt = false) {
            return (button switch {
                MouseButton.Left => PMS.LeftButton.Equals(ButtonState.Pressed) && !CMS.LeftButton.Equals(ButtonState.Pressed),
                MouseButton.Right => PMS.RightButton.Equals(ButtonState.Pressed) && !CMS.RightButton.Equals(ButtonState.Pressed),
                MouseButton.Middle => PMS.MiddleButton.Equals(ButtonState.Pressed) && !CMS.MiddleButton.Equals(ButtonState.Pressed),
                _ => false,
            }) && ModifiersPressed(Mod, Ctrl, Alt);
        }
        public bool MBHeld(MouseButton button, bool Mod = false, bool Ctrl = false, bool Alt = false) {
            return (button switch {
                MouseButton.Left => CMS.LeftButton.Equals(ButtonState.Pressed),
                MouseButton.Right => CMS.RightButton.Equals(ButtonState.Pressed),
                MouseButton.Middle => CMS.MiddleButton.Equals(ButtonState.Pressed),
                _ => false,
            }) && ModifiersPressed(Mod, Ctrl, Alt);
        }
        public bool SomeKeyIsTapped() {
            return PKS.GetPressedKeyCount() < CKS.GetPressedKeyCount();
        }
        public bool SomeKeyIsHeld() {
            return CKS.GetPressedKeyCount() > 0;
        }
        public bool SomeKeyIsReleased() {
            return PKS.GetPressedKeyCount() > CKS.GetPressedKeyCount();
        }

        public bool OneKeyIsTapped(Keys key) {
            return (PKS.GetPressedKeyCount() == 0) && (CKS.GetPressedKeyCount() == 1) && KeyTapped(key);
        }
        public bool OneKeyIsHeld(Keys key) {
            return (CKS.GetPressedKeyCount() == 1) && KeyHeld(key);
        }
        public bool OneKeyIsReleased(Keys key) {
            return (PKS.GetPressedKeyCount() == 1) && (CKS.GetPressedKeyCount() == 0) && KeyReleased(key);
        }
        public bool OnlyAModIsPressed() {
            int C = 0;
            if ( CKS.IsKeyDown(Keys.LeftShift)  ||  CKS.IsKeyDown(Keys.RightShift)  ) C++;
            if (CKS.IsKeyDown(Keys.LeftControl) || CKS.IsKeyDown(Keys.RightControl) ) C++;
            if (  CKS.IsKeyDown(Keys.LeftAlt)   ||   CKS.IsKeyDown(Keys.RightAlt)   ) C++;
            return C > 0 && CKS.GetPressedKeyCount() == C;
        }
        public bool AModIsPressed() {
            int C = 0;
            if ( CKS.IsKeyDown(Keys.LeftShift)  ||  CKS.IsKeyDown(Keys.RightShift)  ) C++;
            if (CKS.IsKeyDown(Keys.LeftControl) || CKS.IsKeyDown(Keys.RightControl) ) C++;
            if (  CKS.IsKeyDown(Keys.LeftAlt)   ||   CKS.IsKeyDown(Keys.RightAlt)   ) C++;
            return C > 0;
        }

        public Point CMousePos() {
            return CMS.Position;
        }
        public Point PMousePos() {
            return PMS.Position;
        }
        public bool MWithinRect(Rectangle rect) {
            return rect.Contains(CMS.X, CMS.Y);
        }
        public bool MouseMoves() {
            return PMS.Position != CMS.Position;
        }
        public Point MouseMotionDir() {
            return new(Math.Sign(CMS.X - PMS.X), Math.Sign(CMS.Y - PMS.Y));
        }
        public Point MouseMotionSpeed() {
            return CMS.Position - PMS.Position;
        }
        public float MWheelValue() {
            return Math.Sign(CMS.ScrollWheelValue - PMS.ScrollWheelValue);
        }
        public char? GetPressedChar() {
            foreach (Keys key in CKS.GetPressedKeys()) {
                bool shift = CKS.IsKeyDown(Keys.LeftShift) || CKS.IsKeyDown(Keys.RightShift);

                if (KeyChars.ContainsKey(key) && key >= Keys.A && key <= Keys.Z) {
                    return shift ? char.ToUpper(KeyChars[key]) : KeyChars[key];
                }
                if (shift && ShiftedKeyChars.ContainsKey(key)) {
                    return ShiftedKeyChars[key];
                }
                if (!shift && KeyChars.ContainsKey(key)) {
                    return KeyChars[key];
                }
            }
            return null;
        }
        public bool KeyHeldForDuration(Keys key, int duration, bool Mod = false, bool Ctrl = false, bool Alt = false) {
            if (!KeyHeld(key, Mod, Ctrl, Alt)) {
                return false;
            }

            return keyHoldDurations.ContainsKey(key) && keyHoldDurations[key] >= duration;
        }
        public bool KeyTappedAndHeldForDuration(Keys key, int duration, bool Mod = false, bool Ctrl = false, bool Alt = false) {
            if (!KeyHeld(key, Mod, Ctrl, Alt)) {
                return false;
            }

            return (keyHoldDurations.ContainsKey(key) && keyHoldDurations[key] >= duration) || KeyTapped(key, Mod, Ctrl, Alt);
        }

        public bool KeyHeldForExactDuration(Keys key, int duration, bool Mod = false, bool Ctrl = false, bool Alt = false) {
            if (!KeyHeld(key, Mod, Ctrl, Alt)) {
                return false;
            }

            return keyHoldDurations.ContainsKey(key) && keyHoldDurations[key] == duration;
        }

        public int GetKeyHoldDuration(Keys key) {
            return keyHoldDurations.ContainsKey(key) ? keyHoldDurations[key] : 0;
        }
        // Check for N consecutive taps using the maxInterval parameter
        public bool KeyMultiTapped(Keys key, int tapCount, int maxInterval = DEFAULT_MAX_INTERVAL, bool Mod = false, bool Ctrl = false, bool Alt = false) {
            if (!ModifiersPressed(Mod, Ctrl, Alt))
                return false;

            // Update the custom interval for this key
            if (keyTapHistory.ContainsKey(key)) {
                keyTapHistory[key].CustomInterval = maxInterval;
            }
            else {
                keyTapHistory[key] = new TapInfo {
                    LastTapFrame = -maxInterval,
                    TapCount = 0,
                    WasReleased = true,
                    JustTapped = false,
                    CustomInterval = maxInterval
                };
            }

            TapInfo info = keyTapHistory[key];

            // Check if we have the right number of taps, just tapped, and within the custom interval
            if (info.TapCount == tapCount && info.JustTapped) {
                // Reset tap count so we can detect another sequence
                info.TapCount = 0;
                return true;
            }

            return false;
        }


        // Check if a key has been tapped exactly N times regardless of when
        public bool HasKeyTapCount(Keys key, int tapCount) {
            if (keyTapHistory.TryGetValue(key, out TapInfo info)) {
                return info.TapCount == tapCount;
            }
            return false;
        }
    }

    public enum MouseButton {
        Left,
        Right,
        Middle
    }
}