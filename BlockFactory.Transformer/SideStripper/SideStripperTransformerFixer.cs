using BlockFactory.Base;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace BlockFactory.Transformer.SideStripper;

public partial class SideStripperTransformer
{
    private Instruction BuildException(Instruction insn, ILProcessor processor)
    {
        var constructorRef = processor.Body.Method.DeclaringType.Module
            .ImportReference(typeof(InvalidSideException).GetConstructor(new[] { typeof(string) }));
        Instruction res;
        processor.InsertBefore(insn, res = Instruction.Create(
            OpCodes.Ldstr,
            $"{insn} is not available on side {_side}"
        ));
        processor.InsertBefore(insn, Instruction.Create(OpCodes.Newobj, constructorRef));
        processor.InsertBefore(insn, Instruction.Create(OpCodes.Throw));
        var debugInfo = processor.Body.Method.DebugInformation;
        if (debugInfo == null) return res;
        var point = debugInfo.GetSequencePoint(insn);
        if (point == null) return res;
        var index = debugInfo.SequencePoints.IndexOf(point);
        var newPoint = new SequencePoint(res, point.Document);
        debugInfo.SequencePoints[index] = newPoint;
        newPoint.StartLine = point.StartLine;
        newPoint.EndLine = point.EndLine;
        newPoint.StartColumn = point.StartColumn;
        newPoint.EndColumn = point.EndColumn;
        // processor.Body.Method.DebugInformation.GetSequencePoint(insn).Offset = res.Offset = 1414;
        return res;
    }

    private void FixMethod(MethodDefinition method,
        Func<string, SideStripperScanData?> scanDataGetter)
    {
        foreach (var parameter in method.Parameters)
            if (ScanDataContains(scanDataGetter, parameter.ParameterType))
                throw new InvalidOperationException(
                    $"Method {method.FullName} has parameter {parameter.Name} of excluded type {parameter.ParameterType.FullName}");

        if (ScanDataContains(scanDataGetter, method.ReturnType))
            throw new InvalidOperationException(
                $"Method {method.FullName} has return value of excluded type {method.ReturnType.FullName}");
        if (!method.HasBody) return;
        
        foreach (var variableDefinition in method.Body.Variables)
        {
            if (ScanDataContains(scanDataGetter, variableDefinition.VariableType))
                throw new InvalidOperationException(
                    $"Method {method.FullName} has local variable of excluded type {variableDefinition.VariableType.FullName}");
        }

        method.Body.SimplifyMacros();

        var replacedInstructions = new List<Instruction>();
        foreach (var insn in method.Body.Instructions)
            // Console.WriteLine($"{insn.OpCode.Name}: {insn.Operand?.GetType()}");
            if (insn.Operand is TypeReference t)
            {
                if (ScanDataContains(scanDataGetter, t)) replacedInstructions.Add(insn);
            }
            else if (insn.Operand is MemberReference member)
            {
                if (ScanDataContains(scanDataGetter, member.DeclaringType))
                    replacedInstructions.Add(insn);
                else if (member is FieldReference f && ScanDataContains(scanDataGetter, f))
                    replacedInstructions.Add(insn);
                else if (member is MethodReference m && ScanDataContains(scanDataGetter, m))
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
                if (t.FullName == typeof(int).FullName)
                    processor.InsertAfter(insn, Instruction.Create(OpCodes.Ldc_I4_0));
                else if (t.FullName == typeof(float).FullName)
                    processor.InsertAfter(insn, Instruction.Create(OpCodes.Ldc_R4, 0.0f));
                else if (t.IsValueType)
                    throw new NotImplementedException("Value types are not implemented yet");
                else
                    processor.InsertAfter(insn, Instruction.Create(OpCodes.Ldnull));
            }

            var insn2 = BuildException(insn, processor);
            processor.Remove(insn);

            foreach (var insn1 in method.Body.Instructions)
                if (insn1.Operand == insn)
                    insn1.Operand = insn2;
        }

        method.Body.OptimizeMacros();
    }

    private void FixType(TypeDefinition type,
        Func<string, SideStripperScanData?> scanDataGetter)
    {
        if (type.BaseType != null && ScanDataContains(scanDataGetter, type.BaseType))
            throw new InvalidOperationException(
                $"Base type {type.BaseType.FullName} of type {type.FullName} is excluded");

        foreach (var itf in type.Interfaces.Select(itf => itf.InterfaceType))
            if (ScanDataContains(scanDataGetter, itf))
                throw new InvalidOperationException(
                    $"Type {type.FullName} has implementation of excluded interface {itf.FullName}");

        foreach (var field in type.Fields.Where(f => !ScanDataContains(scanDataGetter, f)))
            if (ScanDataContains(scanDataGetter, field.FieldType))
                throw new InvalidOperationException(
                    $"Field {field.FullName} is of excluded type {field.FieldType.FullName}");
        foreach (var methodDefinition in type.Methods
                     .Where(m => !ScanDataContains(scanDataGetter, m)))
            FixMethod(methodDefinition, scanDataGetter);
    }

    private void FixThings(AssemblyDefinition def,
        Func<string, SideStripperScanData?> scanDataGetter)
    {
        foreach (var type in def.AllTypes())
        {
            var shouldSkip = false;
            for (var curType = type; curType != null; curType = curType.DeclaringType)
            {
                if (!ScanDataContains(scanDataGetter, curType)) continue;
                shouldSkip = true;
                break;
            }

            if (shouldSkip) continue;
            FixType(type, scanDataGetter);
        }
    }
}