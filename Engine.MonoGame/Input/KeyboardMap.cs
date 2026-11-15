using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.MonoGame.Input
{
    // Core concept: Represents a keyboard, represented as a hexagonal grid of keys. KeyboardMap.Default might return the maps assigned to coordinates of keys
    // A professor challenged me to make a keyboard binding system that could translate default keybinds to positions on a keyboard, rather than set keys (since he uses a non-standard keyboard and is annoyed by it)
    // Thus, I want to try something like that. I proposed to him representing a keyboard with a hexagonal grid, since non-special keys all have up to 6 neighbors
    // And the keys that don't are special keys that don't tend to change between keyboard layouts (backspace, space, enter, shift)
    public class KeyboardMap : IBindingMap
    {
    }
}
