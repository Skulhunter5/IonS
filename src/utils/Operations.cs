using System.Collections.Generic;

namespace IonS {

    enum OperationType {
        Push_uint64,
        Put_char,
        Add, Subtract, Multiply, Divide, Modulo, DivMod,
        Min, Max,
        ShL, ShR, BitAnd, BitOr,
        Comparison,
        Dump,
        Drop, Drop2,
        Dup, Dup2,
        Over, Over2,
        Swap, Swap2, Rotate, Rotate2,
        Label, Jump, JumpIfZero, JumpIfNotZero,
        Exit,
        VariableAccess,
        MemRead, MemWrite,
        String, CStyleString,
        Syscall,

        Block,
    }

    enum ComparisonType {
        EQ, NEQ,
        B, A,
        BEQ, AEQ,
        LT, GT,
        LTEQ, GTEQ
    }

    abstract class Operation {
        public Operation(OperationType type) {
            Type = type;
        }
        public abstract string nasm_linux_x86_64();
        public OperationType Type { get; }
    }

    // Push operations

    sealed class Push_uint64_Operation : Operation {
        public Push_uint64_Operation(ulong value) : base(OperationType.Push_uint64) {
            Value = value;
        }
        public override string nasm_linux_x86_64() {
            return "    push " + ((Push_uint64_Operation) operation).Value + "\n";
        }
        public ulong Value { get; }
    }

    // Put operations

    sealed class Put_char_Operation : Operation {
        public Put_char_Operation() : base(OperationType.Put_char) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    mov rax, 1\n";
            asm += "    mov rdi, 1\n";
            asm += "    pop rsi\n";
            asm += "    mov rdx, 1\n";
            asm += "    syscall\n";
            return asm;
        }
    }

    // Stack manipulation operations

    sealed class DropOperation : Operation {
        public DropOperation() : base(OperationType.Drop) {}
        public override string nasm_linux_x86_64() {
            return "    pop rax\n";
        }
    }

    sealed class Drop2Operation : Operation {
        public Drop2Operation() : base(OperationType.Drop2) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    pop rax\n";
            asm += "    pop rax\n";
            return asm;
        }
    }

    sealed class DupOperation : Operation {
        public DupOperation() : base(OperationType.Dup) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    pop rax\n";
            asm += "    push rax\n";
            asm += "    push rax\n";
            return asm;
        }
    }

    sealed class Dup2Operation : Operation {
        public Dup2Operation() : base(OperationType.Dup2) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    pop rbx\n";
            asm += "    pop rax\n";
            asm += "    push rax\n";
            asm += "    push rbx\n";
            asm += "    push rax\n";
            asm += "    push rbx\n";
            return asm;
        }
    }

    sealed class OverOperation : Operation {
        public OverOperation() : base(OperationType.Over) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    pop rbx\n";
            asm += "    pop rax\n";
            asm += "    push rax\n";
            asm += "    push rbx\n";
            asm += "    push rax\n";
            return asm;
        }
    }

    sealed class Over2Operation : Operation {
        public Over2Operation() : base(OperationType.Over2) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    pop rdx\n";
            asm += "    pop rcx\n";
            asm += "    pop rbx\n";
            asm += "    pop rax\n";
            asm += "    push rax\n";
            asm += "    push rbx\n";
            asm += "    push rcx\n";
            asm += "    push rdx\n";
            asm += "    push rax\n";
            asm += "    push rbx\n";
            return asm;
        }
    }

    sealed class SwapOperation : Operation {
        public SwapOperation() : base(OperationType.Swap) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    pop rbx\n";
            asm += "    pop rax\n";
            asm += "    push rbx\n";
            asm += "    push rax\n";
            return asm;
        }
    }

    sealed class Swap2Operation : Operation {
        public Swap2Operation() : base(OperationType.Swap2) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    pop rdx\n";
            asm += "    pop rcx\n";
            asm += "    pop rbx\n";
            asm += "    pop rax\n";
            asm += "    push rcx\n";
            asm += "    push rdx\n";
            asm += "    push rax\n";
            asm += "    push rbx\n";
            return asm;
        }
    }

    sealed class RotateOperation : Operation {
        public RotateOperation() : base(OperationType.Rotate) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    pop rcx\n";
            asm += "    pop rbx\n";
            asm += "    pop rax\n";
            asm += "    push rbx\n";
            asm += "    push rcx\n";
            asm += "    push rax\n";
            return asm;
        }
    }

    sealed class Rotate2Operation : Operation {
        public Rotate2Operation() : base(OperationType.Rotate2) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    pop rcx\n";
            asm += "    pop rbx\n";
            asm += "    pop rax\n";
            asm += "    push rcx\n";
            asm += "    push rax\n";
            asm += "    push rbx\n";
            return asm;
        }
    }

    // Calculation operations

    sealed class AddOperation : Operation {
        public AddOperation() : base(OperationType.Add) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    pop rbx\n";
            asm += "    pop rax\n";
            asm += "    add rax, rbx\n";
            asm += "    push rax\n";
            return asm;
        }
    }

    sealed class SubtractOperation : Operation {
        public SubtractOperation() : base(OperationType.Subtract) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    pop rbx\n";
            asm += "    pop rax\n";
            asm += "    sub rax, rbx\n";
            asm += "    push rax\n";
            return asm;
        }
    }

    sealed class MultiplyOperation : Operation {
        public MultiplyOperation() : base(OperationType.Multiply) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    pop rbx\n";
            asm += "    pop rax\n";
            asm += "    mul rbx\n";
            asm += "    push rax\n";
            return asm;
        }
    }

    sealed class DivideOperation : Operation {
        public DivideOperation() : base(OperationType.Divide) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    pop rbx\n";
            asm += "    pop rax\n";
            asm += "    xor rdx, rdx\n"; // TODO: check why this is necessary
            asm += "    div rbx\n";
            asm += "    push rax\n";
            return asm;
        }
    }

    sealed class ModuloOperation : Operation {
        public ModuloOperation() : base(OperationType.Modulo) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

    sealed class DivModOperation : Operation {
        public DivModOperation() : base(OperationType.DivMod) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

    // Bitwise operations

    sealed class ShLOperation : Operation {
        public ShLOperation() : base(OperationType.ShL) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

    sealed class ShROperation : Operation {
        public ShROperation() : base(OperationType.ShR) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

    sealed class BitAndOperation : Operation {
        public BitAndOperation() : base(OperationType.BitAnd) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

    sealed class BitOrOperation : Operation {
        public BitOrOperation() : base(OperationType.BitOr) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

    // Min/Max operations

    sealed class MinOperation : Operation {
        public MinOperation() : base(OperationType.Min) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

    sealed class MaxOperation : Operation {
        public MaxOperation() : base(OperationType.Max) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

    // Comparison operations

    sealed class ComparisonOperation : Operation {
        public ComparisonOperation(ComparisonType comparisonType) : base(OperationType.Comparison) {
            ComparisonType = comparisonType;
        }
        public ComparisonType ComparisonType { get; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

    // Dump operation

    sealed class DumpOperation : Operation {
        public DumpOperation() : base(OperationType.Dump) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

    // Exit operation

    sealed class ExitOperation : Operation {
        public ExitOperation() : base(OperationType.Exit) {}
        public override string nasm_linux_x86_64() {
            return "    jmp exit\n";
        }
    }

    // Label and jump operations

    sealed class LabelOperation : Operation {
        public LabelOperation(string label) : base(OperationType.Label) {
            Label = label;
        }
        public string Label { get; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

    sealed class JumpOperation : Operation {
        public JumpOperation(string label, int direction) : base(OperationType.Jump) {
            Label = label;
            Direction = direction;
        }
        public string Label { get; }
        public int Direction { get; } // Only used for optimization of the simulator
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

    sealed class JumpIfZeroOperation : Operation {
        public JumpIfZeroOperation(string label, int direction) : base(OperationType.JumpIfZero) {
            Label = label;
            Direction = direction;
        }
        public string Label { get; }
        public int Direction { get; } // Only used for optimization of the simulator
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

    sealed class JumpIfNotZeroOperation : Operation {
        public JumpIfNotZeroOperation(string label, int direction) : base(OperationType.JumpIfNotZero) {
            Label = label;
            Direction = direction;
        }
        public string Label { get; }
        public int Direction { get; } // Only used for optimization of the simulator
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

    // Variable operations
    
    sealed class VariableAccessOperation : Operation {
        public VariableAccessOperation(string identifier) : base(OperationType.VariableAccess) {
            Identifier = identifier;
        }
        public string Identifier { get; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

    // Mem read/write operations

    sealed class MemReadOperation : Operation {
        public MemReadOperation(byte amount) : base(OperationType.MemRead) {
            Amount = amount;
        }
        public byte Amount { get; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

    sealed class MemWriteOperation : Operation {
        public MemWriteOperation(byte amount) : base(OperationType.MemWrite) {
            Amount = amount;
        }
        public byte Amount { get; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

    // String literal operation

    sealed class StringOperation : Operation {
        public StringOperation(int id) : base(OperationType.String) {
            Id = id;
        }
        public int Id { get; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

    sealed class CStyleStringOperation : Operation {
        public CStyleStringOperation(int id) : base(OperationType.CStyleString) {
            Id = id;
        }
        public int Id { get; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

    // Syscall operation

    sealed class SyscallOperation : Operation {
        public SyscallOperation(int argc) : base(OperationType.Syscall) {
            Argc = argc;
        }
        public int Argc { get; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

    // Block operations

    enum BlockType {
        Code,
        If,
        While,
        DoWhile,
    }

    class Scope {
        public Scope(Scope parent) {
            Parent = parent;
            Variables = new List<Variable>();
        }
        public Error RegisterVariable(Variable var) {
            Variable ownVar = GetOwnVariable(var.Identifier.Text);
            if(ownVar != null) return new VariableRedeclarationError(ownVar.Identifier, var.Identifier);
            Variables.Add(var);
            return null;
        }
        private Variable GetOwnVariable(string identifier) {
            foreach(Variable var in Variables) if(var.Identifier.Text == identifier) return var;
            return null;
        }
        public Variable GetVariable(string identifier) {
            foreach(Variable var in Variables) if(var.Identifier.Text == identifier) return var;
            if(Parent != null) return Parent.GetVariable(identifier);
            return null;
        }
        public Scope Parent { get; }
        public List<Variable> Variables { get; }
    }

    abstract class BlockOperation : Operation {
        private static int nextBlockId = 0;
        private static int BlockId() {
            return nextBlockId++;
        }

        public BlockOperation(BlockType blockType) : base(OperationType.Block) {
            Id = BlockId();
            BlockType = blockType;
        }
        public int Id { get; }
        public BlockType BlockType { get; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

    sealed class CodeBlock : BlockOperation {
        public CodeBlock() : base(BlockType.Code) {
            Operations = new List<Operation>();
        }
        public List<Operation> Operations { get; }
        public Scope Scope { get; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

    sealed class IfBlockOperation : BlockOperation {
        public IfBlockOperation(CodeBlock blockIf, CodeBlock blockElse) : base(BlockType.Code) {
            BlockIf = blockIf;
            BlockElse = blockElse;
        }
        public CodeBlock BlockIf { get; }
        public CodeBlock BlockElse { get; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            
            return asm;
        }
    }

}