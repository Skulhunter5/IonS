using System;

namespace IonS {

    enum OperationType {
        Push_bool, Push_uint8, Push_uint64,
        Increment, Decrement,
        Add, Subtract, Multiply, Divide, Modulo, DivMod,
        Min, Max,
        ShL, ShR, BitAnd, BitOr, BitXor, BitInv,
        And, Or,
        Comparison,
        Dump,
        Drop, Drop2,
        Dup, Dup2,
        Over, Over2,
        Swap, Swap2, Rotate, Rotate2, Rotate5, Rotate52,
        CTT,
        Exit,
        VariableAccess,
        MemRead, MemWrite,
        String, CStyleString,
        Syscall,

        Block,
        Break, Continue,

        ProcedureCall, Return,
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

        public override string ToString() { // TODO: add ToString() for all Operations
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
            return contract.Provide(DataType.boolean, this);
        }
    }

    sealed class Push_uint8_Operation : Operation { // -- n
        public Push_uint8_Operation(byte value, Position position) : base(OperationType.Push_uint8, position) {
            Value = value;
        }

        public byte Value { get; }
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    push {uint8}
                return "    push " + Value + "\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.Provide(DataType.uint8, this);
        }
    }

    sealed class Push_uint64_Operation : Operation { // -- n
        public Push_uint64_Operation(ulong value, Position position) : base(OperationType.Push_uint64, position) {
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
            return contract.Provide(DataType.uint64, this);
        }
    }

    // Stack manipulation operations

    sealed class DropOperation : Operation { // a --
        public DropOperation(Position position) : base(OperationType.Drop, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
               return "    add rsp, 8\n"; 
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(contract.GetElementsLeft() < 1) return new StackUnderflowError(this);
            contract.Pop();
            return null;
        }
    }

    sealed class Drop2Operation : Operation { // a b --
        public Drop2Operation(Position position) : base(OperationType.Drop2, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
               return "    add rsp, 16\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(contract.GetElementsLeft() < 2) return new StackUnderflowError(this);
            contract.Pop();
            contract.Pop();
            return null;
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
            return contract.Provide(contract.Peek(), this);
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
            return contract.Provide(new DataType[] {contract.Peek(1), contract.Peek()}, this);
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
            return contract.Provide(contract.Peek(1), this);
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
            return contract.Provide(new DataType[] {contract.Peek(3), contract.Peek(2)}, this);
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
            return contract.Provide(provided, this);
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
            if(contract.GetElementsLeft() < 2) return new StackUnderflowError(this);
            DataType d = contract.Pop();
            DataType c = contract.Pop();
            DataType b = contract.Pop();
            DataType a = contract.Pop();
            return contract.Provide(new DataType[] {c, d, a, b}, this);
        }
    }

    sealed class RotateOperation : Operation { // a b c -- b c a
        public RotateOperation(Position position) : base(OperationType.Rotate, position) {}
        
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
            return contract.Provide(new DataType[] {b, c, a}, this);
        }
    }

    sealed class Rotate2Operation : Operation { // a b c -- c a b
        public Rotate2Operation(Position position) : base(OperationType.Rotate2, position) {}
        
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
            return contract.Provide(new DataType[] {c, a, b}, this);
        }
    }

    sealed class Rotate5Operation : Operation { // a b c d e -- b c d e a
        public Rotate5Operation(Position position) : base(OperationType.Rotate5, position) {}
        
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
            return contract.Provide(new DataType[] {b, c, d, e, a}, this);
        }
    }

    sealed class Rotate52Operation : Operation { // a b c d e -- e a b c d
        public Rotate52Operation(Position position) : base(OperationType.Rotate52, position) {}
        
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
            return contract.Provide(new DataType[] {e, a, b, c, d}, this);
        }
    }

    sealed class CTTOperation : Operation { // a [] -- a [] a
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

        public override Error TypeCheck(TypeCheckContract contract) { // DOCHECK
            if(contract.GetElementsLeft() < Index) return new StackUnderflowError(this);
            return contract.Provide(contract.Peek(Index-1), this);
        }
    }

    // Calculation operations

    sealed class IncrementOperation : Operation { // a -- (a+1)
        public IncrementOperation(Position position) : base(OperationType.Increment, position) {}
        
        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    pop rbx
                //    add [rsp], rbx
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
                //    pop rbx
                //    add [rsp], rbx
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

        private static DataType[] required = new DataType[] {DataType.uint64, DataType.uint64};
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

        private static DataType[] required = new DataType[] {DataType.uint64, DataType.uint64};
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

        private static DataType[] required = new DataType[] {DataType.uint64, DataType.uint64};
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

        private static DataType[] required = new DataType[] {DataType.uint64, DataType.uint64};
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

        private static DataType[] required = new DataType[] {DataType.uint64, DataType.uint64};
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

        private static DataType[] required = new DataType[] {DataType.uint64, DataType.uint64};
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
            if(!EDataType.Is_uint(dataType)) return new UnexpectedDataTypeError(dataType, "uint8|uint16|uint32|uint64", this);
            dataType = contract.Pop();
            if(!EDataType.Is_uint(dataType)) return new UnexpectedDataTypeError(dataType, "uint8|uint16|uint32|uint64", this);
            return contract.Provide(dataType, this);
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
            if(!EDataType.Is_uint(dataType)) return new UnexpectedDataTypeError(dataType, "uint8|uint16|uint32|uint64", this);
            dataType = contract.Pop();
            if(!EDataType.Is_uint(dataType)) return new UnexpectedDataTypeError(dataType, "uint8|uint16|uint32|uint64", this);
            return contract.Provide(dataType, this);
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
            if(!EDataType.Is_uint(dataType)) return new UnexpectedDataTypeError(dataType, "uint8|uint16|uint32|uint64", this);
            dataType = contract.Pop();
            if(!EDataType.Is_uint(dataType)) return new UnexpectedDataTypeError(dataType, "uint8|uint16|uint32|uint64", this);
            return contract.Provide(dataType, this); // TODONOW: overthink this (do I have to push the bigger or the smaller one or do I have to make them the same size)
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
            if(!EDataType.Is_uint(dataType)) return new UnexpectedDataTypeError(dataType, "uint8|uint16|uint32|uint64", this);
            dataType = contract.Pop();
            if(!EDataType.Is_uint(dataType)) return new UnexpectedDataTypeError(dataType, "uint8|uint16|uint32|uint64", this);
            return contract.Provide(dataType, this); // TODONOW: overthink this (do I have to push the bigger or the smaller one or do I have to make them the same size)
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
            if(!EDataType.Is_uint(dataType)) return new UnexpectedDataTypeError(dataType, "uint8|uint16|uint32|uint64", this);
            dataType = contract.Pop();
            if(!EDataType.Is_uint(dataType)) return new UnexpectedDataTypeError(dataType, "uint8|uint16|uint32|uint64", this);
            return contract.Provide(dataType, this); // TODONOW: overthink this (do I have to push the bigger or the smaller one or do I have to make them the same size)
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
            if(!EDataType.Is_uint(contract.Peek())) return new UnexpectedDataTypeError(contract.Peek(), "uint8|uint16|uint32|uint64", this);
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

        public override Error TypeCheck(TypeCheckContract contract) {
            Error error = contract.Require(new DataType[] {DataType.uint64, DataType.uint64}, this);
            if(error != null) return error;
            return contract.Provide(DataType.boolean, this);
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

        public override Error TypeCheck(TypeCheckContract contract) {
            Error error = contract.Require(new DataType[] {DataType.uint64, DataType.uint64}, this);
            if(error != null) return error;
            return contract.Provide(DataType.boolean, this);
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

        public override Error TypeCheck(TypeCheckContract contract) { // TODONOW: do it right and for all sizes
            return contract.CheckFor(DataType.uint64, this);
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

        private static DataType[] required = new DataType[] {DataType.uint64, DataType.uint64};
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

        private static DataType[] required = new DataType[] {DataType.uint64, DataType.uint64};
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

        public override Error TypeCheck(TypeCheckContract contract) { // TODONOW: do it right and for all sizes
            Error error = contract.Require(new DataType[] {DataType.uint64, DataType.uint64}, this);
            if(error != null) return error;
            return contract.Provide(DataType.uint64, this);
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
            if(contract.GetElementsLeft() < 1) return new StackUnderflowError(this);
            Error error = contract.Require(DataType.uint8, this);
            if(error != null) return error;
            if(!contract.IsEmpty()) Console.WriteLine("[TypeChecker] Warning: excess data on the stack after exit: [" + String.Join(", ", contract.Stack) + "]");  // Error-Warning-System
            return null;
        }
    }

    // Variable operations
    
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
            return contract.Provide(DataType.Pointer, this);
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
            Error error = contract.Require(DataType.Pointer, this);
            if(error != null) return error;
            return contract.Provide(DataType.uint64, this); // TODONOW: change to work for all amounts
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

        private static DataType[] required = new DataType[] {DataType.uint64, DataType.Pointer};
        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.Require(required, this); // TODONOW: change to work for all amounts
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

        private static DataType[] provided = new DataType[] {DataType.uint64, DataType.Pointer};
        public override Error TypeCheck(TypeCheckContract contract) {
            return contract.Provide(provided, this);
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
            return contract.Provide(DataType.Pointer, this);
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
                string asm = "    pop rax\n";
                for(int i = 0; i < Argc; i++) asm += "    pop " + Utils.SyscallRegisters[i] + "\n";
                return asm + "    syscall\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) {
            if(contract.GetElementsLeft() <= Argc) return new StackUnderflowError(this);
            for(int i = -1; i < Argc; i++) contract.Pop();
            return null;
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
                    string asm = "";
                    for(int i = 0; i < Proc.Args.Length; i++) asm += "    pop " + Utils.FreeUseRegisters[i] + "\n";
                    asm += "    call proc_" + Proc.Id + "\n";
                    for(int i = Proc.Rets.Length-1; i >= 0; i--) asm += "    push " + Utils.FreeUseRegisters[i] + "\n";
                    return asm;
                }
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) { // TODONOW: see if this works
            Error error = contract.Require(Proc.Args, this);
            if(error != null) return error;
            return contract.Provide(Proc.Rets, this);
        }
    }

    // Procedure call operation

    sealed class ReturnOperation : Operation { // --
        public ReturnOperation(Procedure procedure, Position position) : base(OperationType.Return, position) {
            Proc = procedure;
        }

        public Procedure Proc { get; }

        public override string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    jmp proc_{Id}_end_{Occurrence}
                if(Proc.IsInlined) throw new NotImplementedException();
                //    jmp proc_{Id}_end
                return "    jmp proc_" + Proc.Id + "_end\n";
            }
            throw new NotImplementedException();
        }

        public override Error TypeCheck(TypeCheckContract contract) { // TODO: make this better and somehow check if all of the subtrees have returned
            if(contract.GetElementsLeft() != Proc.Rets.Length) return new InvalidReturnDataError(contract.Stack.ToArray(), Proc, this);

            for(int i = 0; i < Proc.Rets.Length; i++) if(EDataType.IsImplicitlyCastable(contract.Peek(Proc.Rets.Length-1-i), Proc.Rets[i])) {
                if(contract.Peek(Proc.Rets.Length-1-i) != Proc.Rets[i]) Console.WriteLine("[TypeChecker] Warning: Implicit cast from " + EDataType.String(contract.Peek(Proc.Rets.Length-1-i)) + " to " + Proc.Rets[i] + " while returning from " + Proc + " at " + this); // Error-Warning-System
                return new InvalidReturnDataError(contract.Stack.ToArray(), Proc, this);
            }

            return null;
        }
    }

}