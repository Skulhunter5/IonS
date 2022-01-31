using System;

namespace IonS {

    enum OperationType {
        Push_bool, Push_uint64, Push_ptr,
        Increment, Decrement,
        Add, Subtract, Multiply, Divide, Modulo, DivMod,
        Min, Max,
        ShL, ShR, BitAnd, BitOr, BitXor, BitInv,
        And, Or,
        Comparison,
        Dump,
        Drop,
        Dup, Dup2,
        Over, Over2,
        Swap, Swap2, Rot, RRot, Rot5, RRot5,
        CTT,
        Exit,
        VariableAccess,
        MemRead, MemWrite,
        String, CStyleString,
        Syscall,

        Block,
        Break, Continue,

        ProcedureCall, Return,

        Assert,

        Cast,

        Argc, Argv,
    }

    enum Direction2 {
        Left,
        Right
    }

    abstract class Operation {
        /* public Operation(OperationType type) {
            Type = type;
        } */
        public Operation(OperationType type, Position position) {
            Type = type;
            Position = position;
        }
        public OperationType Type { get; }
        public Position Position { get; }

        public override string ToString() {
            return "Operation:" + Type.ToString() + " at " + Position;
        }

        public abstract string GenerateAssembly(Assembler assembler);
        public abstract Error TypeCheck(TypeCheckContract contract);
    }

    // Push operations

    sealed class Push_bool_Operation : Operation { // -- n
        public Push_bool_Operation(bool value, Position position) : base(OperationType.Push_bool, position) {
            Value = value;
        }

        public bool Value { get; }
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    push {bool}
                return "    push " + (Value ? '1' : '0') + "\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.Provide(DataType.boolean);
        }
    }

    sealed class Push_uint64_Operation : Operation { // -- n
        public Push_uint64_Operation(ulong value, Position position) : base(OperationType.Push_uint64, position) {
            Value = value;
        }

        public ulong Value { get; }
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                if(Value > ((ulong) int.MaxValue)) {
                    //    mov rax, {uint64}
                    //    push rax
                    return "    mov rax, " + Value + "\n    push rax\n";
                } else {
                    //    push {uint64}
                    return "    push " + Value + "\n";
                }
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.Provide(DataType.uint64);
        }
    }

    sealed class Push_ptr_Operation : Operation { // -- n
        public Push_ptr_Operation(ulong value, Position position) : base(OperationType.Push_ptr, position) {
            Value = value;
        }

        public ulong Value { get; }
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    push {uint64}
                return "    push " + Value + "\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.Provide(DataType.pointer);
        }
    }

    // Bind access operation
    
    sealed class PushBindingOperation : Operation { // -- a
        public PushBindingOperation(Binding binding, Position position) : base(OperationType.VariableAccess, position) {
            Binding = binding;
        }

        public Binding Binding { get; }
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    mov rax, [ret_stack_rsp]
                //    add rax, {Offset * 8}
                //    push QWORD [rax]
                string asm = "";
                asm += "    mov rax, [ret_stack_rsp]\n";
                if(Binding.Offset > 0) asm += "    add rax, " + Binding.Offset + "\n";
                asm += "    push QWORD [rax]\n";
                return asm;
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.Provide(Binding.DataType);
        }
    }

    // Stack manipulation operations

    sealed class DropOperation : Operation { // a --
        public DropOperation(int n, Position position) : base(OperationType.Drop, position) {
            N = n;
        }
        
        public int N { get; }

        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
               return "    add rsp, " + (8 * N) + "\n"; 
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.RemoveElements(N, this);
        }
    }

    sealed class DupOperation : Operation { // a -- a a
        public DupOperation(Position position) : base(OperationType.Dup, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    mov rax, [rsp]
                //    push rax
                return "    mov rax, [rsp]\n    push rax\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(contract.GetElementsLeft() < 1) return new StackUnderflowError(this);

            return contract.Provide(contract.Peek());
        }
    }

    sealed class Dup2Operation : Operation { // a b -- a b a b
        public Dup2Operation(Position position) : base(OperationType.Dup2, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    mov rbx, [rsp]
                //    mov rax, [rsp+8]
                //    push rax
                //    push rbx
                return "    mov rbx, [rsp]\n    mov rax, [rsp+8]\n    push rax\n    push rbx\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(contract.GetElementsLeft() < 2) return new StackUnderflowError(this);

            return contract.Provide(new DataType[] {contract.Peek(1), contract.Peek()});
        }
    }

    sealed class OverOperation : Operation { // a b -- a b a
        public OverOperation(Position position) : base(OperationType.Over, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    mov rax, [rsp+8]
                //    push rax
                return "    mov rax, [rsp+8]\n    push rax\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(contract.GetElementsLeft() < 2) return new StackUnderflowError(this);

            return contract.Provide(contract.Peek(1));
        }
    }

    sealed class Over2Operation : Operation { // a b c d -- a b c d a b
        public Over2Operation(Position position) : base(OperationType.Over2, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    mov rbx, [rsp+16]
                //    mov rax, [rsp+24]
                //    push rax
                //    push rbx
                return "    mov rbx, [rsp+16]\n    mov rax, [rsp+24]\n    push rax\n    push rbx\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(contract.GetElementsLeft() < 4) return new StackUnderflowError(this);

            return contract.Provide(new DataType[] {contract.Peek(3), contract.Peek(2)});
        }
    }

    sealed class SwapOperation : Operation { // a b -- b a
        public SwapOperation(Position position) : base(OperationType.Swap, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    mov rbx, [rsp]
                //    mov rax, [rsp+8]
                //    mov [rsp], rax
                //    mov [rsp+8], rbx
                return "    mov rbx, [rsp]\n    mov rax, [rsp+8]\n    mov [rsp], rax\n    mov [rsp+8], rbx\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(contract.GetElementsLeft() < 2) return new StackUnderflowError(this);

            DataType[] provided = new DataType[] {contract.Pop(), contract.Pop()};
            return contract.Provide(provided);
        }
    }

    sealed class Swap2Operation : Operation { // a b c d -- c d a b
        public Swap2Operation(Position position) : base(OperationType.Swap2, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    mov rax, [rsp]
                //    mov rbx, [rsp+16]
                //    mov [rsp+16], rax
                //    mov [rsp], rbx
                //    mov rax, [rsp+8]
                //    mov rbx, [rsp+24]
                //    mov [rsp+24], rax
                //    mov [rsp+8], rbx
                return "    mov rax, [rsp]\n    mov rbx, [rsp+16]\n    mov [rsp+16], rax\n    mov [rsp], rbx\n    mov rax, [rsp+8]\n    mov rbx, [rsp+24]\n    mov [rsp+24], rax\n    mov [rsp+8], rbx\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(contract.GetElementsLeft() < 4) return new StackUnderflowError(this);

            DataType d = contract.Pop();
            DataType c = contract.Pop();
            DataType b = contract.Pop();
            DataType a = contract.Pop();

            return contract.Provide(new DataType[] {c, d, a, b});
        }
    }

    sealed class RotOperation : Operation { // a b c -- b c a
        public RotOperation(Position position) : base(OperationType.Rot, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    mov rcx, [rsp]
                //    mov rbx, [rsp+8]
                //    mov rax, [rsp+16]
                //    mov [rsp], rax
                //    mov [rsp+8], rcx
                //    mov [rsp+16], rbx
                return "    mov rcx, [rsp]\n    mov rbx, [rsp+8]\n    mov rax, [rsp+16]\n    mov [rsp], rax\n    mov [rsp+8], rcx\n    mov [rsp+16], rbx\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(contract.GetElementsLeft() < 3) return new StackUnderflowError(this);

            DataType c = contract.Pop();
            DataType b = contract.Pop();
            DataType a = contract.Pop();

            return contract.Provide(new DataType[] {b, c, a});
        }
    }

    sealed class RRotOperation : Operation { // a b c -- c a b
        public RRotOperation(Position position) : base(OperationType.RRot, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    mov rcx, [rsp]
                //    mov rbx, [rsp+8]
                //    mov rax, [rsp+16]
                //    mov [rsp], rbx
                //    mov [rsp+8], rax
                //    mov [rsp+16], rcx
                return "    mov rcx, [rsp]\n    mov rbx, [rsp+8]\n    mov rax, [rsp+16]\n    mov [rsp], rbx\n    mov [rsp+8], rax\n    mov [rsp+16], rcx\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(contract.GetElementsLeft() < 3) return new StackUnderflowError(this);

            DataType c = contract.Pop();
            DataType b = contract.Pop();
            DataType a = contract.Pop();

            return contract.Provide(new DataType[] {c, a, b});
        }
    }

    sealed class Rot5Operation : Operation { // a b c d e -- b c d e a
        public Rot5Operation(Position position) : base(OperationType.Rot5, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    mov r8, [rsp]
                //    mov rdx, [rsp+8]
                //    mov rcx, [rsp+16]
                //    mov rbx, [rsp+24]
                //    mov rax, [rsp+32]
                //    mov [rsp], rax
                //    mov [rsp+8], r8
                //    mov [rsp+16], rdx
                //    mov [rsp+24], rcx
                //    mov [rsp+32], rbx
                return "    mov r8, [rsp]\n    mov rdx, [rsp+8]\n    mov rcx, [rsp+16]\n    mov rbx, [rsp+24]\n    mov rax, [rsp+32]\n    mov [rsp], rax\n    mov [rsp+8], r8\n    mov [rsp+16], rdx\n    mov [rsp+24], rcx\n    mov [rsp+32], rbx\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(contract.GetElementsLeft() < 5) return new StackUnderflowError(this);

            DataType e = contract.Pop();
            DataType d = contract.Pop();
            DataType c = contract.Pop();
            DataType b = contract.Pop();
            DataType a = contract.Pop();

            return contract.Provide(new DataType[] {b, c, d, e, a});
        }
    }

    sealed class RRot5Operation : Operation { // a b c d e -- e a b c d
        public RRot5Operation(Position position) : base(OperationType.RRot5, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    mov r8, [rsp]
                //    mov rdx, [rsp+8]
                //    mov rcx, [rsp+16]
                //    mov rbx, [rsp+24]
                //    mov rax, [rsp+32]
                //    mov [rsp], rdx
                //    mov [rsp+8], rcx
                //    mov [rsp+16], rbx
                //    mov [rsp+24], rax
                //    mov [rsp+32], r8
                return "    mov r8, [rsp]\n    mov rdx, [rsp+8]\n    mov rcx, [rsp+16]\n    mov rbx, [rsp+24]\n    mov rax, [rsp+32]\n    mov [rsp], rdx\n    mov [rsp+8], rcx\n    mov [rsp+16], rbx\n    mov [rsp+24], rax\n    mov [rsp+32], r8\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(contract.GetElementsLeft() < 5) return new StackUnderflowError(this);

            DataType e = contract.Pop();
            DataType d = contract.Pop();
            DataType c = contract.Pop();
            DataType b = contract.Pop();
            DataType a = contract.Pop();

            return contract.Provide(new DataType[] {e, a, b, c, d});
        }
    }

    sealed class CTTOperation : Operation { // a [] -- a [] a // TODO: Deprecate
        public CTTOperation(int index, Position position) : base(OperationType.CTT, position) {
            Index = index;
        }

        public int Index { get; }
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                return "    mov rax, [rsp+" + (Index*8-8) + "]\n    push rax\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(contract.GetElementsLeft() < Index) return new StackUnderflowError(this);

            return contract.Provide(contract.Peek(Index-1));
        }
    }

    // Calculation operations

    sealed class IncrementOperation : Operation { // a -- (a+1)
        public IncrementOperation(Position position) : base(OperationType.Increment, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    inc QWORD [rsp]
                return "    inc QWORD [rsp]\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.CheckFor(DataType.uint64, this);
        }
    }

    sealed class DecrementOperation : Operation { // a -- (a-1)
        public DecrementOperation(Position position) : base(OperationType.Decrement, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    dec QWORD [rsp]
                return "    dec QWORD [rsp]\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.CheckFor(DataType.uint64, this);
        }
    }

    sealed class AddOperation : Operation { // a b -- (a+b)
        public AddOperation(Position position) : base(OperationType.Add, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    pop rbx
                //    add [rsp], rbx
                return "    pop rbx\n    add [rsp], rbx\n";
            }
            throw new NotImplementedException();
        }

        private static readonly DataType[] required = new DataType[] {DataType.uint64, DataType.uint64};
        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.RequireAndProvide(required, DataType.uint64, this);
        }
    }

    sealed class SubtractOperation : Operation { // a b -- (a-b)
        public SubtractOperation(Position position) : base(OperationType.Subtract, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    pop rbx
                //    sub [rsp], rbx
                return "    pop rbx\n    sub [rsp], rbx\n";
            }
            throw new NotImplementedException();
        }

        private static readonly DataType[] required = new DataType[] {DataType.uint64, DataType.uint64};
        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.RequireAndProvide(required, DataType.uint64, this);
        }
    }

    sealed class MultiplyOperation : Operation { // a b -- (a*b)
        public MultiplyOperation(Position position) : base(OperationType.Multiply, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    pop rbx
                //    mov rax, [rsp]
                //    mul rbx
                //    mov [rsp], rax
                return "    pop rbx\n    mov rax, [rsp]\n    mul rbx\n    mov [rsp], rax\n";
            }
            throw new NotImplementedException();
        }

        private static readonly DataType[] required = new DataType[] {DataType.uint64, DataType.uint64};
        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.RequireAndProvide(required, DataType.uint64, this);
        }
    }

    sealed class DivideOperation : Operation { // a b -- (a/b)
        public DivideOperation(Position position) : base(OperationType.Divide, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    pop rbx
                //    mov rax, [rsp]
                //    xor rdx, rdx
                //    div rbx
                //    mov [rsp], rax
                return "    pop rbx\n    mov rax, [rsp]\n    xor rdx, rdx\n    div rbx\n    mov [rsp], rax\n";
            }
            throw new NotImplementedException();
        }

        private static readonly DataType[] required = new DataType[] {DataType.uint64, DataType.uint64};
        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.RequireAndProvide(required, DataType.uint64, this);
        }
    }

    sealed class ModuloOperation : Operation { // a b -- (a%b)
        public ModuloOperation(Position position) : base(OperationType.Modulo, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    pop rbx
                //    mov rax, [rsp]
                //    xor rdx, rdx
                //    div rbx
                //    mov [rsp], rdx
                return "    pop rbx\n    mov rax, [rsp]\n    xor rdx, rdx\n    div rbx\n    mov [rsp], rdx\n";
            }
            throw new NotImplementedException();
        }

        private static readonly DataType[] required = new DataType[] {DataType.uint64, DataType.uint64};
        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.RequireAndProvide(required, DataType.uint64, this);
        }
    }

    sealed class DivModOperation : Operation { // a b -- (a/b) (a%b)
        public DivModOperation(Position position) : base(OperationType.DivMod, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    mov rbx, [rsp]
                //    mov rax, [rsp+8]
                //    xor rdx, rdx
                //    div rbx
                //    mov [rsp+8], rax
                //    mov [rsp], rdx
                return "    mov rbx, [rsp]\n    mov rax, [rsp+8]\n    xor rdx, rdx\n    div rbx\n    mov [rsp+8], rax\n    mov [rsp], rdx\n";
            }
            throw new NotImplementedException();
        }

        private static readonly DataType[] required = new DataType[] {DataType.uint64, DataType.uint64};
        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.CheckFor(required, this);
        }
    }

    // Bitwise operations

    sealed class ShLOperation : Operation { // a b -- (a<<b)
        public ShLOperation(Position position) : base(OperationType.ShL, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    pop rcx
                //    shl QWORD [rsp], cl
                return "    pop rcx\n    shl QWORD [rsp], cl\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(contract.GetElementsLeft() < 2) return new StackUnderflowError(this);

            DataType dataType = contract.Pop();
            if(!EDataType.Is_uint(dataType)) return new UnexpectedDataTypeError(dataType, DataType.uint64, this);

            dataType = contract.Pop();
            if(!EDataType.Is_uint(dataType)) return new UnexpectedDataTypeError(dataType, DataType.uint64, this);

            return contract.Provide(dataType);
        }
    }

    sealed class ShROperation : Operation { // a b -- (a>>b)
        public ShROperation(Position position) : base(OperationType.ShR, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    pop rcx
                //    shr QWORD [rsp], cl
                return "    pop rcx\n    shr QWORD [rsp], cl\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(contract.GetElementsLeft() < 2) return new StackUnderflowError(this);
            
            DataType dataType = contract.Pop();
            if(!EDataType.Is_uint(dataType)) return new UnexpectedDataTypeError(dataType, DataType.uint64, this);

            dataType = contract.Pop();
            if(!EDataType.Is_uint(dataType)) return new UnexpectedDataTypeError(dataType, DataType.uint64, this);

            return contract.Provide(dataType);
        }
    }

    sealed class BitAndOperation : Operation { // a b -- (a&b)
        public BitAndOperation(Position position) : base(OperationType.BitAnd, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    pop rbx
                //    and [rsp], rbx
                return "    pop rbx\n    and [rsp], rbx\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(contract.GetElementsLeft() < 2) return new StackUnderflowError(this);

            DataType dataType = contract.Pop();
            if(!EDataType.Is_uint(dataType)) return new UnexpectedDataTypeError(dataType, DataType.uint64, this);

            dataType = contract.Pop();
            if(!EDataType.Is_uint(dataType)) return new UnexpectedDataTypeError(dataType, DataType.uint64, this);

            return contract.Provide(dataType);
        }
    }

    sealed class BitOrOperation : Operation { // a b -- (a|b)
        public BitOrOperation(Position position) : base(OperationType.BitOr, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    pop rbx
                //    or [rsp], rbx
                return "    pop rbx\n    or [rsp], rbx\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(contract.GetElementsLeft() < 2) return new StackUnderflowError(this);
            
            DataType dataType = contract.Pop();
            if(!EDataType.Is_uint(dataType)) return new UnexpectedDataTypeError(dataType, DataType.uint64, this);
            
            dataType = contract.Pop();
            if(!EDataType.Is_uint(dataType)) return new UnexpectedDataTypeError(dataType, DataType.uint64, this);

            return contract.Provide(dataType);
        }
    }

    sealed class BitXorOperation : Operation { // a b -- (a^b)
        public BitXorOperation(Position position) : base(OperationType.BitXor, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    pop rbx
                //    xor [rsp], rbx
                return "    pop rbx\n    xor [rsp], rbx\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(contract.GetElementsLeft() < 2) return new StackUnderflowError(this);
            
            DataType dataType = contract.Pop();
            if(!EDataType.Is_uint(dataType)) return new UnexpectedDataTypeError(dataType, DataType.uint64, this);
            
            dataType = contract.Pop();
            if(!EDataType.Is_uint(dataType)) return new UnexpectedDataTypeError(dataType, DataType.uint64, this);
            
            return contract.Provide(dataType);
        }
    }

    sealed class BitInvOperation : Operation { // a -- (bitwise inverted a)
        public BitInvOperation(Position position) : base(OperationType.BitInv, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    xor [rsp], 1111111111111111111111111111111111111111111111111111111111111111b
                return "    xor [rsp], 1111111111111111111111111111111111111111111111111111111111111111b\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(contract.GetElementsLeft() < 1) return new StackUnderflowError(this);

            if(!EDataType.Is_uint(contract.Peek())) return new UnexpectedDataTypeError(contract.Peek(), DataType.uint64, this);

            return null;
        }
    }

    // Logical operations

    sealed class AndOperation : Operation { // a b -- (a&&b)
        public AndOperation(Position position) : base(OperationType.And, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    xor rax, rax
                //    xor rbx, rbx
                //    pop rcx
                //    cmp rcx, 0
                //    setne al
                //    mov rcx, [rsp]
                //    cmp rcx, 0
                //    setne bl
                //    and rax, rbx
                //    mov [rsp], rax
                return "    xor rax, rax\n    xor rbx, rbx\n    pop rcx\n    cmp rcx, 0\n    setne al\n    mov rcx, [rsp]\n    cmp rcx, 0\n    setne bl\n    and rax, rbx\n    mov [rsp], rax\n";
            }
            throw new NotImplementedException();
        }

        private static readonly DataType[] required = new DataType[] {DataType.boolean, DataType.boolean};
        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.RequireAndProvide(required, DataType.boolean, this);
        }
    }

    sealed class OrOperation : Operation { // a b -- (a||b)
        public OrOperation(Position position) : base(OperationType.Or, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    xor rax, rax
                //    xor rbx, rbx
                //    pop rcx
                //    cmp rcx, 0
                //    setne al
                //    mov rcx, [rsp]
                //    cmp rcx, 0
                //    setne bl
                //    or rax, rbx
                //    mov [rsp], rax
                return "    xor rax, rax\n    xor rbx, rbx\n    pop rcx\n    cmp rcx, 0\n    setne al\n    mov rcx, [rsp]\n    cmp rcx, 0\n    setne bl\n    or rax, rbx\n    mov [rsp], rax\n";
            }
            throw new NotImplementedException();
        }

        private static readonly DataType[] required = new DataType[] {DataType.boolean, DataType.boolean};
        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.RequireAndProvide(required, DataType.boolean, this);
        }
    }

    sealed class NotOperation : Operation { // a -- !a
        public NotOperation(Position position) : base(OperationType.Or, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    xor rax, rax
                //    mov rbx, [rsp]
                //    cmp rbx, 0
                //    sete al
                //    mov [rsp], rax
                return "    xor rax, rax\n    mov rbx, [rsp]\n    cmp rbx, 0\n    sete al\n    mov [rsp], rax\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.CheckFor(DataType.boolean, this);
        }
    }

    // Min/Max operations

    sealed class MinOperation : Operation { // a b -- min(a,b)
        public MinOperation(Position position) : base(OperationType.Min, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    pop rbx
                //    mov rax, [rsp]
                //    cmp rbx, rax
                //    cmovb rax, rbx
                //    mov [rsp], rax
                return "    pop rbx\n    mov rax, [rsp]\n    cmp rbx, rax\n    cmovb rax, rbx\n    mov [rsp], rax\n";
            }
            throw new NotImplementedException();
        }

        private static readonly DataType[] required = new DataType[] {DataType.uint64, DataType.uint64};
        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.RequireAndProvide(required, DataType.uint64, this);
        }
    }

    sealed class MaxOperation : Operation { // a b -- max(a,b)
        public MaxOperation(Position position) : base(OperationType.Max, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    pop rbx
                //    mov rax, [rsp]
                //    cmp rbx, rax
                //    cmova rax, rbx
                //    mov [rsp], rax
                return "    pop rbx\n    mov rax, [rsp]\n    cmp rbx, rax\n    cmova rax, rbx\n    mov [rsp], rax\n";
            }
            throw new NotImplementedException();
        }

        private static readonly DataType[] required = new DataType[] {DataType.uint64, DataType.uint64};
        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.RequireAndProvide(required, DataType.uint64, this);
        }
    }

    // Comparison operations

    enum ComparisonType {
        EQ, NEQ,
        B, A,
        BEQ, AEQ,
        LT, GT,
        LTEQ, GTEQ
    }

    sealed class ComparisonOperation : Operation { // a b -- (a cmp b)
        public ComparisonOperation(ComparisonType comparisonType, Position position) : base(OperationType.Comparison, position) {
            ComparisonType = comparisonType;
        }
        public ComparisonType ComparisonType { get; }
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    xor rax, rax
                //    pop rbx
                //    cmp [rsp], rbx
                string asm = "    xor rax, rax\n    pop rbx\n    cmp [rsp], rbx\n";
                if(ComparisonType == ComparisonType.EQ) asm += "    sete al\n";
                else if(ComparisonType == ComparisonType.NEQ) asm += "    setne al\n";
                else if(ComparisonType == ComparisonType.B) asm += "    setb al\n";
                else if(ComparisonType == ComparisonType.A) asm += "    seta al\n";
                else if(ComparisonType == ComparisonType.BEQ) asm += "    setbe al\n";
                else if(ComparisonType == ComparisonType.AEQ) asm += "    setae al\n";
                else if(ComparisonType == ComparisonType.LT) asm += "    setl al\n";
                else if(ComparisonType == ComparisonType.GT) asm += "    setg al\n";
                else if(ComparisonType == ComparisonType.LTEQ) asm += "    setle al\n";
                else if(ComparisonType == ComparisonType.GTEQ) asm += "    setge al\n";
                return asm + "    mov [rsp], rax\n";
            }
            throw new NotImplementedException();
        }

        private static readonly DataType[] required = new DataType[] {DataType.uint64, DataType.uint64};
        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.RequireAndProvide(required, DataType.boolean, this);
        }
    }

    // Dump operation

    sealed class DumpOperation : Operation { // a --
        public DumpOperation(Position position) : base(OperationType.Dump, position) {}

        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    pop rdi
                //    call dump
                return "    pop rdi\n    call dump\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.Require(DataType.uint64, this);
        }
    }

    // Exit operation

    sealed class ExitOperation : Operation { // a --
        public ExitOperation(Position position) : base(OperationType.Exit, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    mov rax, 60
                //    pop rdi
                //    syscall
                return "    mov rax, 60\n    pop rdi\n    syscall\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            Error error = contract.Require(DataType.uint64, this);
            if(error != null) return error;

            if(!contract.IsEmpty()) Console.WriteLine("[TypeChecker] Warning: excess data on the stack after exit: [" + String.Join(", ", contract.Stack) + "]");  // Error-Warning-System
            
            return null;
        }
    }

    // Variable access operation
    
    sealed class VariableAccessOperation : Operation { // -- ptr
        public VariableAccessOperation(int id, Position position) : base(OperationType.VariableAccess, position) {
            Id = id;
        }

        public int Id { get; }
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                return "    push var_" + Id + "\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.Provide(DataType.pointer);
        }
    }

    // Mem read/write operations

    sealed class MemReadOperation : Operation { // ptr -- *ptr
        public MemReadOperation(byte amount, Position position) : base(OperationType.MemRead, position) {
            Amount = amount;
        }

        public byte Amount { get; }
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    mov rax, [rsp]
                string asm = "    mov rax, [rsp]\n";
                if(Amount < 64) asm += "    xor rbx, rbx\n";
                if(Amount == 8) asm += "    mov bl, [rax]\n";
                else if(Amount == 16) asm += "    mov bx, [rax]\n";
                else if(Amount == 32) asm += "    mov ebx, [rax]\n";
                else if(Amount == 64) asm += "    mov rbx, [rax]\n";
                return asm + "    mov [rsp], rbx\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.RequireAndProvide(DataType.pointer, DataType.uint64, this);
        }
    }

    sealed class MemWriteOperation : Operation { // x ptr --
        public MemWriteOperation(byte amount, Position position) : base(OperationType.MemWrite, position) {
            Amount = amount;
        }

        public byte Amount { get; }
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    pop rax
                //    pop rbx
                string asm = "    pop rax\n    pop rbx\n";
                if(Amount == 8) asm += "    mov [rax], bl\n";
                else if(Amount == 16) asm += "    mov [rax], bx\n";
                else if(Amount == 32) asm += "    mov [rax], ebx\n";
                else if(Amount == 64) asm += "    mov [rax], rbx\n";
                return asm;
            }
            throw new NotImplementedException();
        }

        private static readonly DataType[] required = new DataType[] {DataType.uint64, DataType.pointer};
        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.Require(required, this);
        }
    }

    // String literal operation

    sealed class StringOperation : Operation { // -- len ptr
        public StringOperation(int id, int length, Position position) : base(OperationType.String, position) {
            Id = id;
            Length = length;
        }

        public int Id { get; }
        public int Length { get; }

        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    push {len}
                //    push str_{Id}
                return "    push " + Length + "\n    push str_" + Id + "\n";
            }
            throw new NotImplementedException();
        }

        private static readonly DataType[] provided = new DataType[] {DataType.uint64, DataType.pointer};
        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.Provide(provided);
        }
    }

    sealed class CStyleStringOperation : Operation { // -- ptr
        public CStyleStringOperation(int id, Position position) : base(OperationType.CStyleString, position) {
            Id = id;
        }

        public int Id { get; }

        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    push str_{Id}
                return "    push str_" + Id + "\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.Provide(DataType.pointer);
        }
    }

    // Syscall operation

    sealed class SyscallOperation : Operation { // args[] syscall --
        public SyscallOperation(int argc, Position position) : base(OperationType.Syscall, position) {
            Argc = argc;
        }

        public int Argc { get; }

        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    pop rax
                //    ... push other arguments ...
                //    syscall
                //    push rax
                string asm = "    pop rax\n";
                for(int i = 0; i < Argc; i++) asm += "    pop " + Utils.SyscallRegisters[i] + "\n";
                return asm + "    syscall\n    push rax\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            Error error = contract.RemoveElements(Argc+1, this);
            if(error != null) return error;

            return contract.Provide(DataType.uint64);
        }
    }

    // Procedure call operation

    sealed class ProcedureCallOperation : Operation { // args[] -- ret[]
        public ProcedureCallOperation(Procedure proc, Position position) : base(OperationType.ProcedureCall, position) {
            Proc = proc;
        }

        public Procedure Proc { get; }

        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                if(Proc.IsInlined) return Proc.GenerateAssembly(assembler);
                else {
                    //    mov rax, rsp
                    //    mov rsp, [ret_stack_rsp]
                    //    call proc_{Id}
                    //    mov [ret_stack_rsp], rsp
                    //    mov rsp, rax
                    string asm = "";
                    asm += "    mov rax, rsp\n";
                    asm += "    mov rsp, [ret_stack_rsp]\n";
                    asm += "    call proc_" + Proc.Id + "\n";
                    asm += "    mov [ret_stack_rsp], rsp\n";
                    asm += "    mov rsp, rax\n";
                    return asm;
                }
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.RequireAndProvide(Proc.Args, Proc.Rets, this);
        }
    }

    // Return operation

    sealed class ReturnOperation : Operation {
        public ReturnOperation(Procedure procedure, Scope scope, Position position) : base(OperationType.Return, position) {
            Proc = procedure;
            Scope = scope;
        }

        public Procedure Proc { get; }
        public Scope Scope { get; }

        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    jmp proc_{Id}_end_{Occurrence}
                if(Proc.IsInlined) throw new NotImplementedException();
                //    add QWORD [ret_stack_rsp], {RetStackOffset}
                //    jmp proc_{Id}_end
                return "    add QWORD [ret_stack_rsp], " + Scope.GetRetStackOffset() + "\n    jmp proc_" + Proc.Id + "_end\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(contract.GetElementsLeft() != Proc.Rets.Length) return new InvalidReturnDataError(contract.Stack.ToArray(), Proc, this);

            for(int i = 0; i < Proc.Rets.Length; i++) if(!EDataType.IsImplicitlyCastable(contract.Peek(Proc.Rets.Length-1-i), Proc.Rets[i])) {
                if(contract.Peek(Proc.Rets.Length-1-i) != Proc.Rets[i]) Console.WriteLine("[TypeChecker] Warning: Implicit cast from " + EDataType.StringOf(contract.Peek(Proc.Rets.Length-1-i)) + " to " + Proc.Rets[i] + " while returning from " + Proc + " at " + this); // Error-Warning-System
                return new InvalidReturnDataError(contract.Stack.ToArray(), Proc, this);
            }

            contract.HasReturned = true;

            return null;
        }
    }

    // Assert operation

    sealed class AssertOperation : Operation {
        private static int nextAssertId = 0;
        private static int AssertId() { return nextAssertId++; }

        public AssertOperation(CodeBlock condition, CodeBlock response, int stringLength, int stringId, Position position) : base(OperationType.Assert, position) {
            Condition = condition;
            Response = response;
            StringLength = stringLength;
            StringId = stringId;
        }

        public int Id { get; }
        public CodeBlock Condition { get; }
        public CodeBlock Response { get; }
        public int StringLength { get; }
        public int StringId { get; }

        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    ... Condition ...
                //    pop rax
                //    cmp rax, 0
                //    jne assert_{Id}
                //    mov rax, 1
                //    mov rdi, 1
                //    mov rsi, str_{Id}
                //    mov rdx, {len}
                //    syscall
                //    ... Response ...
                //    mov rax, 60
                //    pop rdi
                //    syscall
                //assert_{Id}:
                string asm = "";
                asm += Condition.GenerateAssembly(assembler);
                asm += "    pop rax\n    cmp rax, 0\n    jne assert_" + Id + "\n";
                asm += "    mov rax, 1\n    mov rdi, 1\n    mov rsi, str_" + StringId + "\n    mov rdx, " + StringLength + "\n    syscall\n";
                asm += Response.GenerateAssembly(assembler);
                asm += "    mov rax, 60\n    mov rdi, 1\n    syscall\nassert_" + Id + ":\n";
                return asm;
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            TypeCheckContract reference = contract.Copy();
            Error error = Condition.TypeCheck(contract);
            if(error != null) return error;

            error = contract.Require(DataType.boolean, this);
            if(error != null) return error;
            
            if(!contract.IsStackCompatible(reference)) return new SignatureMustBeNoneError(reference, contract, Condition);

            error = Response.TypeCheck(contract);
            if(error != null) return error;

            return null;
        }
    }

    // Cast operations

    sealed class SingleCastOperation : Operation {
        public SingleCastOperation(DataType dataType, Position position) : base(OperationType.Cast, position) {
            DataType = dataType;
        }

        public DataType DataType { get; }

        public override string GenerateAssembly(Assembler assembler) { return ""; }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(contract.GetElementsLeft() < 1) return new StackUnderflowError(this);
            contract.Pop();
            
            return contract.Provide(DataType);
        }
    }

    sealed class MultiCastOperation : Operation {
        public MultiCastOperation(DataType[] dataTypes, Position position) : base(OperationType.Cast, position) {
            DataTypes = dataTypes;
        }

        public DataType[] DataTypes { get; }

        public override string GenerateAssembly(Assembler assembler) { return ""; }

        public override Error TypeCheck(TypeCheckContract contract) {
            Error error = contract.RemoveElements(DataTypes.Length, this);
            if(error != null) return error;

            return contract.Provide(DataTypes);
        }
    }

    // Argument operations

    sealed class ArgcOperation : Operation {
        public ArgcOperation(Position position) : base(OperationType.Argc, position) {}

        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    mov rax, [args_ptr]
                //    push [rax]
                return "    mov rax, [args_ptr]\n    push QWORD [rax]\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.Provide(DataType.uint64);
        }
    }

    sealed class ArgvOperation : Operation {
        public ArgvOperation(Position position) : base(OperationType.Argv, position) {}

        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    push QWORD [args_ptr]
                //    add [rsp], 8
                return "    push QWORD [args_ptr]\n    add QWORD [rsp], 8\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.Provide(DataType.pointer);
        }
    }

}