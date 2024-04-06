using System.Diagnostics.CodeAnalysis;
using BlockFactory.Registry_;
using Silk.NET.Maths;

namespace BlockFactory.Gui.Control;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

[SuppressMessage("Usage", "CA2211")]
public class SynchronizedControls
{
    public static Registry<SynchronizedControlType> Registry;
    public static SynchronizedControlType Label;
    public static SynchronizedControlType Button;
    public static SynchronizedControlType TextInput;
    public static SynchronizedControlType SlottedWindow;

    public static void Init()
    {
        Registry = SynchronizedRegistries.NewSynchronizedRegistry<SynchronizedControlType>("ControlType");
        Label = Registry.Register("Label", new SynchronizedControlType(() => new LabelControl("")));
        Button = Registry.Register("Button", new SynchronizedControlType(() => new ButtonControl("")));
        TextInput = Registry.Register("TextInput", SynchronizedControlType.Create<TextInputControl>());
        SlottedWindow = Registry.Register("SlottedWindow", new SynchronizedControlType(
            () => new SlottedWindowControl(
                new Vector2D<int>(0, 0),
                Array.Empty<int>(),
                Array.Empty<int>()
                )
            ));
    }

    public static void Lock()
    {
        Registry.Lock();
    }
    
}