using BlockFactory.Side_;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace BlockFactory.SideStripper_;

public class SideStripper
{
    private readonly HashSet<string> _excludedFields;
    private readonly HashSet<string> _excludedMethods;
    private readonly HashSet<string> _excludedProperties;
    private readonly HashSet<string> _excludedTypes;
    private readonly Side _side;
    public readonly AssemblyDefinition[] Definitions;

    public SideStripper(Side side, AssemblyDefinition[] definitions)
    {
        _side = side;
        Definitions = definitions;
        _excludedTypes = new HashSet<string>();
        _excludedMethods = new HashSet<string>();
        _excludedFields = new HashSet<string>();
        _excludedProperties = new HashSet<string>();
    }

    private bool IsExcluded(IEnumerable<CustomAttribute> attributes)
    {
        return attributes
            .Where(attr => attr.AttributeType.FullName == typeof(ExclusiveToAttribute).FullName)
            .Any(attr => (Side)attr.ConstructorArguments[0].Value != _side);
    }

    private void AddExcludedTypes(TypeDefinition type)
    {
        Console.WriteLine($"Excluding type {type.FullName}");
        _excludedTypes.Add(type.FullName);
        foreach (var nestedType in type.NestedTypes) AddExcludedTypes(nestedType);
    }

    private void FindExcludedThings()
    {
        foreach (
            var type in Definitions
                .SelectMany(a => a.Modules)
                .SelectMany(m => m.Types)
        )
        {
            if (IsExcluded(type.CustomAttributes))
            {
                AddExcludedTypes(type);
                continue;
            }

            foreach (var method in type.Methods.Where(method => IsExcluded(method.CustomAttributes)))
            {
                Console.WriteLine($"Excluding method {method.FullName}");
                _excludedMethods.Add(method.FullName);
            }

            foreach (var field in type.Fields.Where(field => IsExcluded(field.CustomAttributes)))
            {
                Console.WriteLine($"Excluding field {field.FullName}");
                _excludedFields.Add(field.FullName);
            }

            foreach (var property in type.Properties.Where(property => IsExcluded(property.CustomAttributes)))
            {
                Console.WriteLine($"Excluding property {property.FullName} and it's methods");
                _excludedProperties.Add(property.FullName);
                if (property.GetMethod != null) _excludedMethods.Add(property.GetMethod.FullName);

                if (property.SetMethod != null) _excludedMethods.Add(property.SetMethod.FullName);

                var field = property.GetBackingField();
                if (field != null) _excludedFields.Add(field.FullName);
            }
        }
    }

    private void RemoveExcludedThings()
    {
        foreach (var module in Definitions.SelectMany(def => def.Modules))
        {
            module.Types.RemoveIf(t => _excludedTypes.Contains(t.FullName));
            foreach (var t in module.Types)
            {
                t.Fields.RemoveIf(f => _excludedFields.Contains(f.FullName));
                t.Methods.RemoveIf(m => _excludedMethods.Contains(m.FullName));
                t.Properties.RemoveIf(p => _excludedProperties.Contains(p.FullName));
            }
        }
    }

    private void BuildException(Instruction insn, ILProcessor processor)
    {
        var constructorRef = processor.Body.Method.DeclaringType.Module
            .ImportReference(typeof(InvalidSideException).GetConstructor(new[] { typeof(string) }));
        processor.InsertBefore(insn, Instruction.Create(
            OpCodes.Ldstr,
            $"{insn} is not available on side {_side}"
        ));
        processor.InsertBefore(insn, Instruction.Create(OpCodes.Newobj, constructorRef));
        processor.InsertBefore(insn, Instruction.Create(OpCodes.Throw));
    }

    private void FixMethod(MethodDefinition method)
    {
        foreach (var parameter in method.Parameters)
            if (_excludedTypes.Contains(parameter.ParameterType.FullName))
                throw new InvalidOperationException(
                    $"Method {method.FullName} has parameter {parameter.Name} of excluded type {parameter.ParameterType.FullName}");

        if (_excludedTypes.Contains(method.ReturnType.FullName))
            throw new InvalidOperationException(
                $"Method {method.FullName} has return value of excluded type {method.ReturnType.FullName}");

        if (method.IsAbstract) return;

        var replacedInstructions = new List<Instruction>();
        foreach (var insn in method.Body.Instructions)
            // Console.WriteLine($"{insn.OpCode.Name}: {insn.Operand?.GetType()}");
            if (insn.Operand is TypeReference t)
            {
                if (_excludedTypes.Contains(t.FullName)) replacedInstructions.Add(insn);
            }
            else if (insn.Operand is MemberReference member)
            {
                if (_excludedTypes.Contains(member.DeclaringType.FullName))
                    replacedInstructions.Add(insn);
                else if (member is FieldReference f && _excludedFields.Contains(f.FullName))
                    replacedInstructions.Add(insn);
                else if (member is MethodReference m && _excludedMethods.Contains(m.FullName))
                    replacedInstructions.Add(insn);
            }

        // if (insn.Operand is TypeReference r)
        // {
        //     Console.WriteLine($"Type: {r.FullName}");
        // }
        var processor = method.Body.GetILProcessor();
        foreach (var insn in replacedInstructions)
        {
            var popcnt = insn.GetPopCount(method);
            for (var i = 0; i < popcnt; ++i) processor.InsertBefore(insn, Instruction.Create(OpCodes.Pop));

            var t = insn.GetPushedType();
            if (t != null)
            {
                if (t.IsValueType) throw new NotImplementedException("Value types are not implemented yet");
                processor.InsertAfter(insn, Instruction.Create(OpCodes.Ldnull));
            }

            BuildException(insn, processor);
            processor.Remove(insn);
        }
    }

    private void FixDependentCode()
    {
        foreach (var type in Definitions
                     .SelectMany(def => def.Modules)
                     .SelectMany(m => m.Types)
                     .Where(t => !_excludedTypes.Contains(t.FullName))
                )
        {
            if (type.BaseType != null && _excludedTypes.Contains(type.BaseType.FullName))
                throw new InvalidOperationException(
                    $"Base type {type.BaseType.FullName} of type {type.FullName} is excluded");

            foreach (var itf in type.Interfaces.Select(itf => itf.InterfaceType))
                if (_excludedTypes.Contains(itf.FullName))
                    throw new InvalidOperationException(
                        $"Type {type.FullName} has implementation of excluded interface {itf.FullName}");

            foreach (var field in type.Fields.Where(f => !_excludedFields.Contains(f.FullName)))
                if (_excludedTypes.Contains(field.FieldType.FullName))
                    throw new InvalidOperationException(
                        $"Field {field.FullName} is of excluded type {field.FieldType.FullName}");
            foreach (var methodDefinition in type.Methods
                         .Where(m => !_excludedMethods.Contains(m.FullName)))
                FixMethod(methodDefinition);
        }
    }

    public void Process()
    {
        FindExcludedThings();
        FixDependentCode();
        RemoveExcludedThings();
    }
}