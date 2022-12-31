// See https://aka.ms/new-console-template for more information

using BlockFactory.Serialization;

Console.WriteLine("Hello, World!");
Console.WriteLine(TagTypes.CreateTag(TagType.Byte).Type);
Console.WriteLine(TagTypes.CreateValueBasedTag(228345676543).Value);