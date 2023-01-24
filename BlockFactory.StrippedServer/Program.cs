// See https://aka.ms/new-console-template for more information

using System.Reflection;
using BlockFactory.Loader;
using BlockFactory.Side_;

Console.WriteLine("Hello, World!");
var appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
var projectPath = Path.GetFullPath(Path.Combine(appPath, "..", "..", ".."));
var configRelativePath = Path.GetRelativePath(projectPath, appPath);
var targetProjectPath = Path.GetFullPath(Path.Combine(projectPath, "..", "BlockFactory.Server"));
var targetPath = Path.GetFullPath(Path.Combine(targetProjectPath, configRelativePath));
new BlockFactoryLoader(Side.Server).Load(targetPath, "BlockFactory.dll", args);