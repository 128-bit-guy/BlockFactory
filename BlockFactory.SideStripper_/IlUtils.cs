using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

namespace BlockFactory.SideStripper_;

public static class IlUtils
{
    public static bool IsCompilerGenerated(IEnumerable<CustomAttribute> attributes)
    {
        return attributes
            .Any(attr => attr.AttributeType.FullName == typeof(CompilerGeneratedAttribute).FullName);
    }

    public static FieldReference? GetBackingField(this PropertyDefinition def)
    {
        if (def.GetMethod == null) return null;

        var get = def.GetMethod;
        if (get.IsAbstract) return null;

        if (!IsCompilerGenerated(get.CustomAttributes)) return null;

        var field = get.Body.Instructions[1].Operand;
        if (field is FieldReference f) return f;

        return null;
    }

    public static void RemoveIf<T>(this Collection<T> collection, Predicate<T> predicate)
    {
        var l = collection.Where(t => predicate(t)).ToList();
        foreach (var t in l) collection.Remove(t);
    }

    public static int GetPushCount(this Instruction instruction)
    {
        var count = instruction.OpCode.StackBehaviourPush switch
        {
            StackBehaviour.Push1_push1 => 2,
            StackBehaviour.Varpush => CalculatePushCount(instruction),
            StackBehaviour.Push0 => 0,
            _ => 1
        };
        return count;
    }

    private static int CalculatePushCount(Instruction instruction)
    {
        // First simple approach: 1 if non-void, 0 otherwise
        var method = instruction.Operand as MethodReference;
        return IsVoidMethod(method) ? 0 : 1;
    }

    public static bool IsVoidMethod(MethodReference method)
    {
        return typeof(void).FullName!.Equals(method.ReturnType.FullName);
    }

    public static bool IsConstructor(this MethodReference method)
    {
        return ".ctor" == method.Name;
    }

    public static bool IsStoreLocal(this Instruction instruction, out int index)
    {
        var ret = true;
        switch (instruction.OpCode.Code)
        {
            case Code.Stloc:
            case Code.Stloc_S:
                index = ((VariableDefinition)instruction.Operand).Index;
                break;
            case Code.Stloc_0:
                index = 0;
                break;
            case Code.Stloc_1:
                index = 1;
                break;
            case Code.Stloc_2:
                index = 2;
                break;
            case Code.Stloc_3:
                index = 3;
                break;
            default:
                index = -1;
                ret = false;
                break;
        }

        return ret;
    }

    public static bool IsLoadLocal(this Instruction instruction, out int index)
    {
        var ret = true;
        switch (instruction.OpCode.Code)
        {
            case Code.Ldloca:
            case Code.Ldloca_S:
            case Code.Ldloc:
            case Code.Ldloc_S:
                index = ((VariableDefinition)instruction.Operand).Index;
                break;
            case Code.Ldloc_0:
                index = 0;
                break;
            case Code.Ldloc_1:
                index = 1;
                break;
            case Code.Ldloc_2:
                index = 2;
                break;
            case Code.Ldloc_3:
                index = 3;
                break;
            default:
                index = -1;
                ret = false;
                break;
        }

        return ret;
    }

    public static int GetPopCount(this Instruction instruction, MethodDefinition method)
    {
        int count;
        switch (instruction.OpCode.StackBehaviourPop)
        {
            case StackBehaviour.Popref_popi_popref:
            case StackBehaviour.Popref_popi_popr8:
            case StackBehaviour.Popref_popi_popr4:
            case StackBehaviour.Popref_popi_popi8:
            case StackBehaviour.Popref_popi_popi:
            case StackBehaviour.Popi_popi_popi:
                count = 3;
                break;
            case StackBehaviour.Popref_popi:
            case StackBehaviour.Popref_pop1:
            case StackBehaviour.Popi_popr8:
            case StackBehaviour.Popi_popr4:
            case StackBehaviour.Popi_popi8:
            case StackBehaviour.Popi_popi:
            case StackBehaviour.Popi_pop1:
            case StackBehaviour.Pop1_pop1:
                count = 2;
                break;
            case StackBehaviour.Varpop:
                count = CalculatePopCount(instruction, method);
                break;
            case StackBehaviour.Pop0:
                count = 0;
                break;
            case StackBehaviour.PopAll:
                // Signal to the caller that the evaluation stack is emptied.
                count = int.MaxValue;
                break;
            default:
                count = 1;
                break;
        }

        return count;
    }

    private static int CalculatePopCount(Instruction instruction, MethodDefinition method)
    {
        int count;
        if (instruction.OpCode.FlowControl == FlowControl.Call)
        {
            // First simple approach: number of parameters, + 1 if virtual call
            var calledMethod = instruction.Operand as MethodReference;
            count = calledMethod.Parameters.Count + (AssumesObjectOnStack(instruction, calledMethod) ? 1 : 0);
        }
        else if (instruction.OpCode == OpCodes.Ret)
        {
            count = IsVoidMethod(method) ? 0 : 1;
        }
        else
        {
            throw new ArgumentException("Unhandled instruction: " + instruction.OpCode);
        }

        return count;
    }

    private static bool AssumesObjectOnStack(Instruction instruction, MethodReference method)
    {
        // Newobj creates the 'this' instance rather than consuming it.
        if (instruction.OpCode == OpCodes.Newobj)
            return false;
        return method.HasThis;
    }

    public static bool IsRef(this ParameterDefinition pdef)
    {
        return pdef.ParameterType.IsByReference;
    }

    public static TypeReference? GetPushedType(this Instruction insn)
    {
        if (
            insn.OpCode == OpCodes.Ldfld
            || insn.OpCode == OpCodes.Ldsfld
            || insn.OpCode == OpCodes.Ldflda
            || insn.OpCode == OpCodes.Ldsflda
        )
            return ((FieldReference)insn.Operand).FieldType;
        if (insn.OpCode == OpCodes.Call || insn.OpCode == OpCodes.Calli || insn.OpCode == OpCodes.Callvirt)
        {
            var m = (MethodReference)insn.Operand;
            return m.ReturnType == m.DeclaringType.Module.TypeSystem.Void ? null : m.ReturnType;
        }

        if (insn.OpCode == OpCodes.Newobj)
        {
            var m = (MethodReference)insn.Operand;
            return m.DeclaringType;
        }

        if (insn.OpCode == OpCodes.Newarr)
        {
            var t = (TypeReference)insn.Operand;
            return t.MakeArrayType();
        }

        return null;
    }
}