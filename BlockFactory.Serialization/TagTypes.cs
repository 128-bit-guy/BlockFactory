namespace BlockFactory.Serialization;

public static class TagTypes
{
    private static Func<ITag>[]? _tagConstructors;
    private static Dictionary<TagType, Func<ITag>>? _dictionary = new();

    static TagTypes()
    {
        RegisterTag<ByteTag>();
        RegisterTag<Int32Tag>();
        RegisterTag<Int64Tag>();
        RegisterTag<StringTag>();
        RegisterTag<ListTag>();
        RegisterTag<DictionaryTag>();
        RegisterTag<BooleanTag>();
        RegisterTag<SingleTag>();
        RegisterTag<Int16Tag>();
        RegisterTag<Int16ArrayTag>();
        RegisterTag<ByteArrayTag>();
        BuildTags();
    }

    private static void RegisterTag<T>() where T : class, ITag, new()
    {
        var type = new T().Type;
        _dictionary!.Add(type, () => new T());
        Console.WriteLine($"[BlockFactory.Serialization] Registered tag of type {typeof(T).Name}");
        foreach (var t in typeof(T).GetInterfaces())
            if (t.IsConstructedGenericType)
            {
                var def = t.GetGenericTypeDefinition();
                if (def == typeof(IValueBasedTag<>))
                {
                    var v = t.GetGenericArguments()[0];
                    Console.WriteLine(
                        $"[BlockFactory.Serialization] Found implementation of value based tag for type {v.Name}");
                    var vbti = typeof(ValueBasedTagInfo<>);
                    var vbtiBuilt = vbti.MakeGenericType(v);
                    var info = typeof(T).GetConstructor(new[] { v }) ??
                               throw new InvalidOperationException(
                                   "Tried to register value based tag type without constructor from value");
                    var f = typeof(Func<,>);
                    var fBuilt = f.MakeGenericType(v, t);
                    var ru = typeof(ReflectionUtils);
                    var cd = ru.GetMethod(nameof(ReflectionUtils.CreateDelegate))!;
                    var cdBuilt = cd.MakeGenericMethod(fBuilt);
                    var constructor = cdBuilt.Invoke(null, new object?[] { info });
                    var tagCreator = vbtiBuilt.GetField(nameof(ValueBasedTagInfo<object>.TagCreator))!;
                    tagCreator.SetValue(null, constructor);
                    var tagType = vbtiBuilt.GetField(nameof(ValueBasedTagInfo<object>.TagType))!;
                    tagType.SetValue(null, type);
                    var fGetterBuilt = f.MakeGenericType(t, v);
                    var p = t.GetProperty(nameof(IValueBasedTag<object>.Value))!;
                    var m = p.GetMethod!;
                    var d = m.CreateDelegate(fGetterBuilt);
                    var valueGetter = vbtiBuilt.GetField(nameof(ValueBasedTagInfo<object>.ValueGetter))!;
                    valueGetter.SetValue(null, d);
                }
            }
    }

    private static void BuildTags()
    {
        var max = _dictionary!.Keys.Max();
        _tagConstructors = new Func<ITag>[(int)max + 1];
        foreach (var (type, constructor) in _dictionary) _tagConstructors[(int)type] = constructor;

        _dictionary = null;
    }

    public static IValueBasedTag<T> CreateValueBasedTag<T>(T value)
    {
        return (ValueBasedTagInfo<T>.TagCreator ??
                throw new InvalidOperationException(
                    "Tried to create value based tag for type which is not registered"))(value);
    }

    public static ITag CreateTag(TagType type)
    {
        return _tagConstructors![(int)type]();
    }

    public static TagType GetValueBasedTagType<T>()
    {
        return ValueBasedTagInfo<T>.TagType;
    }

    public static IValueBasedTag<T> CreateDefaultValueBasedTag<T>()
    {
        return (IValueBasedTag<T>)CreateTag(GetValueBasedTagType<T>());
    }

    public static T GetValue<T>(IValueBasedTag<T> tag)
    {
        return (ValueBasedTagInfo<T>.ValueGetter ??
                throw new InvalidOperationException("Tried to get value from tag for type which is not registered"))(
            tag);
    }

    private static class ValueBasedTagInfo<T>
    {
        public static Func<T, IValueBasedTag<T>>? TagCreator = null;
        public static TagType TagType;
        public static Func<IValueBasedTag<T>, T>? ValueGetter = null;
    }

    private static class TagInfo<T>
    {
        public static TagType TagType;
    }
}