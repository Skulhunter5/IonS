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
        Exit,
        VariableAccess,
        MemRead, MemWrite,
        String, CStyleString,
        Syscall,

        Block,
        Break, Continue,
    }

    enum Direction2 {
        Left,
        Right
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
            return "    push " + Value + "\n";
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
            asm += "    pop rbx\n";
            asm += "    pop rax\n";
            asm += "    xor rdx, rdx\n";
            asm += "    div rbx\n";
            asm += "    push rdx\n";
            return asm;
        }
    }

    sealed class DivModOperation : Operation {
        public DivModOperation() : base(OperationType.DivMod) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    pop rbx\n";
            asm += "    pop rax\n";
            asm += "    xor rdx, rdx\n";
            asm += "    div rbx\n";
            asm += "    push rax\n";
            asm += "    push rdx\n";
            return asm;
        }
    }

    // Bitwise operations

    sealed class ShLOperation : Operation {
        public ShLOperation() : base(OperationType.ShL) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    pop rcx\n";
            asm += "    pop rax\n";
            asm += "    shl rax, cl\n";
            asm += "    push rax\n";
            return asm;
        }
    }

    sealed class ShROperation : Operation {
        public ShROperation() : base(OperationType.ShR) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    pop rcx\n";
            asm += "    pop rax\n";
            asm += "    shr rax, cl\n";
            asm += "    push rax\n";
            return asm;
        }
    }

    sealed class BitAndOperation : Operation {
        public BitAndOperation() : base(OperationType.BitAnd) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    pop rbx\n";
            asm += "    pop rax\n";
            asm += "    and rax, rbx\n";
            asm += "    push rax\n";
            return asm;
        }
    }

    sealed class BitOrOperation : Operation {
        public BitOrOperation() : base(OperationType.BitOr) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    pop rbx\n";
            asm += "    pop rax\n";
            asm += "    or rax, rbx\n";
            asm += "    push rax\n";
            return asm;
        }
    }

    // Min/Max operations

    sealed class MinOperation : Operation {
        public MinOperation() : base(OperationType.Min) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    pop rbx\n";
            asm += "    pop rax\n";
            asm += "    cmp rbx, rax\n";
            asm += "    cmovb rax, rbx\n";
            asm += "    push rax\n";
            return asm;
        }
    }

    sealed class MaxOperation : Operation {
        public MaxOperation() : base(OperationType.Max) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    pop rbx\n";
            asm += "    pop rax\n";
            asm += "    cmp rbx, rax\n";
            asm += "    cmova rax, rbx\n";
            asm += "    push rax\n";
            return asm;
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

    sealed class ComparisonOperation : Operation {
        public ComparisonOperation(ComparisonType comparisonType) : base(OperationType.Comparison) {
            ComparisonType = comparisonType;
        }
        public ComparisonType ComparisonType { get; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    pop rbx\n";
            asm += "    pop rax\n";
            asm += "    cmp rax, rbx\n";
            asm += "    mov rax, 0\n";
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
            asm += "    push rax\n";
            return asm;
        }
    }

    // Dump operation

    sealed class DumpOperation : Operation {
        public DumpOperation() : base(OperationType.Dump) {}
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    pop rdi\n";
            asm += "    call dump\n";
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

    // Variable operations
    
    sealed class VariableAccessOperation : Operation {
        public VariableAccessOperation(int id) : base(OperationType.VariableAccess) {
            Id = id;
        }
        public int Id { get; }
        public override string nasm_linux_x86_64() {
            return "    push var_" + Id + "\n";
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
            asm += "    pop rax\n";
            if(Amount < 64) asm += "    xor rbx, rbx\n";
            if(Amount == 8) asm += "    mov bl, [rax]\n";
            else if(Amount == 16) asm += "    mov bx, [rax]\n";
            else if(Amount == 32) asm += "    mov ebx, [rax]\n";
            else if(Amount == 64) asm += "    mov rbx, [rax]\n";
            asm += "    push rbx\n";
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
            asm += "    pop rax\n";
            asm += "    pop rbx\n";
            if(Amount == 8) asm += "    mov [rax], bl\n";
            else if(Amount == 16) asm += "    mov [rax], bx\n";
            else if(Amount == 32) asm += "    mov [rax], ebx\n";
            else if(Amount == 64) asm += "    mov [rax], rbx\n";
            return asm;
        }
    }

    // String literal operation

    sealed class StringOperation : Operation {
        public StringOperation(int id, int length) : base(OperationType.String) {
            Id = id;
            Length = length;
        }
        public int Id { get; }
        public int Length { get; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "    push " + Length + "\n";
            asm += "    push str_" + Id + "\n";
            return asm;
        }
    }

    sealed class CStyleStringOperation : Operation {
        public CStyleStringOperation(int id) : base(OperationType.CStyleString) {
            Id = id;
        }
        public int Id { get; }
        public override string nasm_linux_x86_64() {
            return "    push str_" + Id + "\n";
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
            asm += "    pop rax\n";
            for(int i = 0; i < Argc; i++) asm += "    pop " + Utils.SyscallRegisters[i] + "\n";
            asm += "    syscall\n";
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

    abstract class Block : Operation {
        private static int nextBlockId = 0;
        private static int BlockId() {
            return nextBlockId++;
        }

        public Block(BlockType blockType) : base(OperationType.Block) {
            Id = BlockId();
            BlockType = blockType;
        }
        public int Id { get; }
        public BlockType BlockType { get; }
    }

    sealed class CodeBlock : Block {
        public CodeBlock(Scope parentScope) : base(BlockType.Code) {
            Operations = new List<Operation>();
            Scope = new Scope(parentScope);
        }
        public List<Operation> Operations { get; }
        public Scope Scope { get; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            foreach(Operation operation in Operations) asm += operation.nasm_linux_x86_64();
            return asm;
        }
    }

    sealed class IfBlock : Block {
        public IfBlock(CodeBlock blockIf, CodeBlock blockElse) : base(BlockType.If) {
            BlockIf = blockIf;
            BlockElse = blockElse;
        }
        public CodeBlock BlockIf { get; }
        public CodeBlock BlockElse { get; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "if_" + Id + ":\n";
            asm += "    pop rax\n";
            asm += "    cmp rax, 0\n";
            asm += "    je if_else_" + Id + "\n";
            asm += BlockIf.nasm_linux_x86_64();
            asm += "    jmp if_end_" + Id + "\n";
            asm += "if_else_" + Id + ":\n";
            asm += BlockElse != null ? BlockElse.nasm_linux_x86_64() : "";
            asm += "if_end_" + Id + ":\n";
            return asm;
        }
    }

    sealed class WhileBlock : BreakableBlock {
        public WhileBlock(CodeBlock condition, CodeBlock block) : base(BlockType.While) {
            Condition = condition;
            Block = block;
        }
        public CodeBlock Condition { get; set; }
        public CodeBlock Block { get; set; }
        public override string nasm_linux_x86_64() { // CWD
            string asm = "";
            asm += "while_" + Id + ":\n";
            asm += Condition.nasm_linux_x86_64();
            asm += "    pop rax\n";
            asm += "    cmp rax, 0\n";
            asm += "    je while_end_" + Id + "\n";
            asm += Block.nasm_linux_x86_64();
            asm += "    jmp while_" + Id + "\n";
            asm += "while_end_" + Id + ":\n";
            return asm;
        }
        public override string continue___nasm_linux_x86_64() {
            return "    jmp while_" + Id + "\n";
        }
        public override string break___nasm_linux_x86_64() {
            return "    jmp while_end_" + Id + "\n";
        }
    }

    sealed class DoWhileBlock : BreakableBlock {
        public DoWhileBlock(CodeBlock block, CodeBlock condition) : base(BlockType.DoWhile) {
            Block = block;
            Condition = condition;
        }
        public CodeBlock Block { get; set; }
        public CodeBlock Condition { get; set; }
        public override string nasm_linux_x86_64() { // CWD
            string asm = "";
            asm += "dowhile_" + Id + ":\n";
            asm += Block.nasm_linux_x86_64();
            asm += "dowhile_do_" + Id + ":\n";
            asm += Condition.nasm_linux_x86_64();
            asm += "    pop rax\n";
            asm += "    cmp rax, 0\n";
            asm += "    jne dowhile_" + Id + "\n";
            asm += "dowhile_end_" + Id + ":\n";
            return asm;
        }
        public override string continue___nasm_linux_x86_64() {
            return "    jmp dowhile_do_" + Id + "\n";
        }
        public override string break___nasm_linux_x86_64() {
            return "    jmp dowhile_end_" + Id + "\n";
        }
    }

    abstract class BreakableBlock : Block {
        public BreakableBlock(BlockType blockType) : base(blockType) {}
        public abstract string continue___nasm_linux_x86_64();
        public abstract string break___nasm_linux_x86_64();
    }

    sealed class ContinueOperation : Operation {
        public ContinueOperation(BreakableBlock block) : base(OperationType.Continue) {
            Block = block;
        }
        public BreakableBlock Block { get; }
        public override string nasm_linux_x86_64() {
            return Block.continue___nasm_linux_x86_64();
        }
    }

    sealed class BreakOperation : Operation {
        public BreakOperation(BreakableBlock block) : base(OperationType.Break) {
            Block = block;
        }
        public BreakableBlock Block { get; }
        public override string nasm_linux_x86_64() {
            return Block.break___nasm_linux_x86_64();
        }
    }

}