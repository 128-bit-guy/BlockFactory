// See https://aka.ms/new-console-template for more information

using System.Reflection;
using BlockFactory.Side_;
using BlockFactory.SideStripper;
using Mono.Cecil;

Console.WriteLine("Hello, World!");
var appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
var projectPath = Path.GetFullPath(Path.Combine(appPath, "..", "..", ".."));
var configRelativePath = Path.GetRelativePath(projectPath, appPath);
var targetProjectPath = Path.GetFullPath(Path.Combine(projectPath, "..", "BlockFactory.Client"));
var targetPath = Path.GetFullPath(Path.Combine(targetProjectPath, configRelativePath));
var assemblyPath = Path.GetFullPath(Path.Combine(targetPath, "BlockFactory.dll"));
var def = AssemblyDefinition.ReadAssembly(assemblyPath);
var stripper = new SideStripper(Side.Client, new[]{def});
stripper.Process();