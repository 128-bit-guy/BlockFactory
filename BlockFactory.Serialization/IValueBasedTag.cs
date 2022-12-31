namespace BlockFactory.Serialization;

public interface IValueBasedTag<T> : ITag
{
    T Value { get; set; }
}