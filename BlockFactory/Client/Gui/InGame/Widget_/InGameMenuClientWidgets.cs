using System.Reflection;
using BlockFactory.Gui.Widget;
using BlockFactory.Init;
using BlockFactory.Registry_;
using BlockFactory.Util;

namespace BlockFactory.Client.Gui.InGame.Widget_;

public static class InGameMenuClientWidgets
{
    public delegate Widget ClientWidgetCreator<in T>(T inGameMenuWidget, InGameMenuScreen screen)
        where T : InGameMenuWidget;

    public delegate Widget ClientWidgetCreatorNG(InGameMenuWidget inGameMenuWidget, InGameMenuScreen screen);

    public static AttachmentRegistry<InGameMenuWidgetType, ClientWidgetCreatorNG> ClientWidgetCreators;

    public static void Init()
    {
        ClientWidgetCreators =
            new AttachmentRegistry<InGameMenuWidgetType, ClientWidgetCreatorNG>(InGameMenuWidgetTypes.Registry);
        RegisterDefault<SlotClientWidget>(InGameMenuWidgetTypes.Slot);
        RegisterDefault<LabelClientWidget>(InGameMenuWidgetTypes.Label);
    }

    public static void RegisterDefault<T>(InGameMenuWidgetType type) where T : Widget
    {
        ConstructorInfo constructor = typeof(T).GetConstructors()[0];
        Type t = constructor.GetParameters()[0].ParameterType;
        Type dT = typeof(ClientWidgetCreator<>).MakeGenericType(t);
        MethodInfo createDelegate = typeof(ReflectionUtils).GetMethod(nameof(ReflectionUtils.CreateDelegate))!;
        MethodInfo genericCreateDelegate = createDelegate.MakeGenericMethod(dT);
        object d = genericCreateDelegate.Invoke(null, new object?[] { constructor})!;
        MethodInfo registerGeneric = typeof(InGameMenuClientWidgets).GetMethod(nameof(RegisterGeneric))!;
        MethodInfo registerGenericGeneric = registerGeneric.MakeGenericMethod(t);
        registerGenericGeneric.Invoke(null, new[] { type, d });
    }

    public static void RegisterGeneric<T>(InGameMenuWidgetType type, ClientWidgetCreator<T> creator)
        where T : InGameMenuWidget
    {
        Widget ClientWidgetCreatorNg(InGameMenuWidget w, InGameMenuScreen s)
        {
            return creator((T)w, s);
        }
        ClientWidgetCreators.Register(type, (ClientWidgetCreatorNG)ClientWidgetCreatorNg);
    }
}