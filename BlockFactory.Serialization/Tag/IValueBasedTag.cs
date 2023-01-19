namespace BlockFactory.Serialization.Tag;

public interface IValueBasedTag<T> : ITag
{
    T Value { get; set; }
}