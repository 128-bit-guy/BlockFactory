// See https://aka.ms/new-console-template for more information

using BlockFactory.Loader;
using BlockFactory.Server;
using BlockFactory.Side_;

Console.WriteLine("Hello, World!");
new BlockFactoryLoader(Side.Server).LoadUnstriped(typeof(BlockFactoryServer).Assembly, args);