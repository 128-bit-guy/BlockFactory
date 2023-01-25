// See https://aka.ms/new-console-template for more information

using BlockFactory.Client;
using BlockFactory.Loader;
using BlockFactory.Side_;

Console.WriteLine("Hello, World!");
new BlockFactoryLoader(Side.Client).LoadUnstriped(typeof(BlockFactoryClient).Assembly, args);