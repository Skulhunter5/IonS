using System.Collections.Generic;

namespace IonS {

    enum OperationType {
        Push_uint64,
        Put_char,
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

    abstract class Operation : IAssemblyGenerator {
        public Operation(OperationType type) {
            Type = type;
        }
        public abstract string nasm_linux_x86_64();
        public OperationType Type { get; }
    }

    // Push operations

    sealed class Push_uint64_Operation : Operation { // -- n
        public Push_uint64_Operation(ulong value) : base(OperationType.Push_uint64) {
            Value = value;
        }
        public override string nasm_linux_x86_64() {
            //    push {uint64}
            return "    push " + Value + "\n";
        }
        public ulong Value { get; }
    }

    // Put operations

    sealed class Put_char_Operation : Operation { // a --
        public Put_char_Operation() : base(OperationType.Put_char) {}
        public override string nasm_linux_x86_64() {
            //    mov rax, 1
            //    mov rdi, 1
            //    pop rsi
            //    mov rdx, 1
            //    syscall
            return "    mov rax, 1\n    mov rdi, 1\n    pop rsi\n    mov rdx, 1\n    syscall\n";
        }
    }

    // Stack manipulation operations

    sealed class DropOperation : Operation { // a --
        public DropOperation() : base(OperationType.Drop) {}
        public override string nasm_linux_x86_64() {
            return "    add rsp, 8\n";
        }
    }

    sealed class Drop2Operation : Operation { // a b --
        public Drop2Operation() : base(OperationType.Drop2) {}
        public override string nasm_linux_x86_64() {
            return "    add rsp, 16\n";
        }
    }

    sealed class DupOperation : Operation { // a a -- a a
        public DupOperation() : base(OperationType.Dup) {}
        public override string nasm_linux_x86_64() {
            //    mov rax, [rsp]
            //    push rax
            return "    mov rax, [rsp]\n    push rax\n";
        }
    }

    sealed class Dup2Operation : Operation { // a b -- a b a b
        public Dup2Operation() : base(OperationType.Dup2) {}
        public override string nasm_linux_x86_64() {
            //    mov rbx, [rsp]
            //    mov rax, [rsp+8]
            //    push rax
            //    push rbx
            return "    mov rbx, [rsp]\n    mov rax, [rsp+8]\n    push rax\n    push rbx\n";
        }
    }

    sealed class OverOperation : Operation { // a b -- a b a
        public OverOperation() : base(OperationType.Over) {}
        public override string nasm_linux_x86_64() {
            //    mov rax, [rsp+8]
            //    push rax
            return "    mov rax, [rsp+8]\n    push rax\n";
        }
    }

    sealed class Over2Operation : Operation { // a b c d -- a b c d a b
        public Over2Operation() : base(OperationType.Over2) {}
        public override string nasm_linux_x86_64() {
            //    mov rbx, [rsp+16]
            //    mov rax, [rsp+24]
            //    push rax
            //    push rbx
            return "    mov rbx, [rsp+16]\n    mov rax, [rsp+24]\n    push rax\n    push rbx\n";
        }
    }

    sealed class SwapOperation : Operation { // a b -- b a
        public SwapOperation() : base(OperationType.Swap) {}
        public override string nasm_linux_x86_64() {
            //    mov rbx, [rsp]
            //    mov rax, [rsp+8]
            //    mov [rsp], rax
            //    mov [rsp+8], rbx
            return "    mov rbx, [rsp]\n    mov rax, [rsp+8]\n    mov [rsp], rax\n    mov [rsp+8], rbx\n";
        }
    }

    sealed class Swap2Operation : Operation { // a b c d -- c d a b
        public Swap2Operation() : base(OperationType.Swap2) {}
        public override string nasm_linux_x86_64() {
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
    }

    sealed class RotateOperation : Operation { // a b c -- b c a
        public RotateOperation() : base(OperationType.Rotate) {}
        public override string nasm_linux_x86_64() {
            //    mov rcx, [rsp]
            //    mov rbx, [rsp+8]
            //    mov rax, [rsp+16]
            //    mov [rsp], rax
            //    mov [rsp+8], rcx
            //    mov [rsp+16], rbx
            return "    mov rcx, [rsp]\n    mov rbx, [rsp+8]\n    mov rax, [rsp+16]\n    mov [rsp], rax\n    mov [rsp+8], rcx\n    mov [rsp+16], rbx\n";
        }
    }

    sealed class Rotate2Operation : Operation { // a b c -- c a b
        public Rotate2Operation() : base(OperationType.Rotate2) {}
        public override string nasm_linux_x86_64() {
            //    mov rcx, [rsp]
            //    mov rbx, [rsp+8]
            //    mov rax, [rsp+16]
            //    mov [rsp], rbx
            //    mov [rsp+8], rax
            //    mov [rsp+16], rcx
            return "    mov rcx, [rsp]\n    mov rbx, [rsp+8]\n    mov rax, [rsp+16]\n    mov [rsp], rbx\n    mov [rsp+8], rax\n    mov [rsp+16], rcx\n";
        }
    }

    sealed class Rotate5Operation : Operation { // a b c d e -- b c d e a
        public Rotate5Operation() : base(OperationType.Rotate5) {}
        public override string nasm_linux_x86_64() {
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
    }

    sealed class Rotate52Operation : Operation { // a b c d e -- e a b c d
        public Rotate52Operation() : base(OperationType.Rotate52) {}
        public override string nasm_linux_x86_64() {
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
    }

    sealed class CTTOperation : Operation {
        public CTTOperation(uint index) : base(OperationType.CTT) {
            Index = index;
        }
        public uint Index { get; }
        public override string nasm_linux_x86_64()
        {
            return "    mov rax, [rsp+" + (Index*8-8) + "]\n    push rax\n";
        }
    }

    // Calculation operations

    sealed class IncrementOperation : Operation { // a b -- (a+b)
        public IncrementOperation() : base(OperationType.Increment) {}
        public override string nasm_linux_x86_64() {
            //    pop rbx
            //    add [rsp], rbx
            return "    inc QWORD [rsp]\n";
        }
    }

    sealed class DecrementOperation : Operation { // a b -- (a+b)
        public DecrementOperation() : base(OperationType.Decrement) {}
        public override string nasm_linux_x86_64() {
            //    pop rbx
            //    add [rsp], rbx
            return "    dec QWORD [rsp]\n";
        }
    }

    sealed class AddOperation : Operation { // a b -- (a+b)
        public AddOperation() : base(OperationType.Add) {}
        public override string nasm_linux_x86_64() {
            //    pop rbx
            //    add [rsp], rbx
            return "    pop rbx\n    add [rsp], rbx\n";
        }
    }

    sealed class SubtractOperation : Operation { // a b -- (a-b)
        public SubtractOperation() : base(OperationType.Subtract) {}
        public override string nasm_linux_x86_64() {
            //    pop rbx
            //    sub [rsp], rbx
            return "    pop rbx\n    sub [rsp], rbx\n";
        }
    }

    sealed class MultiplyOperation : Operation { // a b -- (a*b)
        public MultiplyOperation() : base(OperationType.Multiply) {}
        public override string nasm_linux_x86_64() {
            //    pop rbx
            //    mov rax, [rsp]
            //    mul rbx
            //    mov [rsp], rax
            return "    pop rbx\n    mov rax, [rsp]\n    mul rbx\n    mov [rsp], rax\n";
        }
    }

    sealed class DivideOperation : Operation { // a b -- (a/b)
        public DivideOperation() : base(OperationType.Divide) {}
        public override string nasm_linux_x86_64() {
            //    pop rbx
            //    mov rax, [rsp]
            //    xor rdx, rdx
            //    div rbx
            //    mov [rsp], rax
            return "    pop rbx\n    mov rax, [rsp]\n    xor rdx, rdx\n    div rbx\n    mov [rsp], rax\n";
        }
    }

    sealed class ModuloOperation : Operation { // a b -- (a%b)
        public ModuloOperation() : base(OperationType.Modulo) {}
        public override string nasm_linux_x86_64() {
            //    pop rbx
            //    mov rax, [rsp]
            //    xor rdx, rdx
            //    div rbx
            //    mov [rsp], rdx
            return "    pop rbx\n    mov rax, [rsp]\n    xor rdx, rdx\n    div rbx\n    mov [rsp], rdx\n";
        }
    }

    sealed class DivModOperation : Operation { // a b -- (a/b) (a%b)
        public DivModOperation() : base(OperationType.DivMod) {}
        public override string nasm_linux_x86_64() {
            //    mov rbx, [rsp]
            //    mov rax, [rsp+8]
            //    xor rdx, rdx
            //    div rbx
            //    mov [rsp+8], rax
            //    mov [rsp], rdx
            return "    mov rbx, [rsp]\n    mov rax, [rsp+8]\n    xor rdx, rdx\n    div rbx\n    mov [rsp+8], rax\n    mov [rsp], rdx\n";
        }
    }

    // Bitwise operations

    sealed class ShLOperation : Operation { // a b -- (a<<b)
        public ShLOperation() : base(OperationType.ShL) {}
        public override string nasm_linux_x86_64() {
            //    pop rcx
            //    shl QWORD [rsp], cl
            return "    pop rcx\n    shl QWORD [rsp], cl\n";
        }
    }

    sealed class ShROperation : Operation { // a b -- (a>>b)
        public ShROperation() : base(OperationType.ShR) {}
        public override string nasm_linux_x86_64() {
            //    pop rcx
            //    shr QWORD [rsp], cl
            return "    pop rcx\n    shr QWORD [rsp], cl\n";
        }
    }

    sealed class BitAndOperation : Operation { // a b -- (a&b)
        public BitAndOperation() : base(OperationType.BitAnd) {}
        public override string nasm_linux_x86_64() {
            //    pop rbx
            //    and [rsp], rbx
            return "    pop rbx\n    and [rsp], rbx\n";
        }
    }

    sealed class BitOrOperation : Operation { // a b -- (a|b)
        public BitOrOperation() : base(OperationType.BitOr) {}
        public override string nasm_linux_x86_64() {
            //    pop rbx
            //    or [rsp], rbx
            return "    pop rbx\n    or [rsp], rbx\n";
        }
    }

    sealed class BitXorOperation : Operation { // a b -- (a^b)
        public BitXorOperation() : base(OperationType.BitXor) {}
        public override string nasm_linux_x86_64() {
            //    pop rbx
            //    xor [rsp], rbx
            return "    pop rbx\n    xor [rsp], rbx\n";
        }
    }

    sealed class BitInvOperation : Operation { // a -- (bitwise inverted a)
        public BitInvOperation() : base(OperationType.BitInv) {}
        public override string nasm_linux_x86_64() {
            //    xor [rsp], 1111111111111111111111111111111111111111111111111111111111111111b
            return "    xor [rsp], 1111111111111111111111111111111111111111111111111111111111111111b\n";
        }
    }

    // Logical operations

    sealed class AndOperation : Operation { // a b -- (a&&b)
        public AndOperation() : base(OperationType.And) {}
        public override string nasm_linux_x86_64() {
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
    }

    sealed class OrOperation : Operation { // a b -- (a||b)
        public OrOperation() : base(OperationType.Or) {}
        public override string nasm_linux_x86_64() {
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
    }

    sealed class NotOperation : Operation { // a -- !a
        public NotOperation() : base(OperationType.Or) {}
        public override string nasm_linux_x86_64() {
            //    xor rax, rax
            //    mov rbx, [rsp]
            //    cmp rbx, 0
            //    sete al
            //    mov [rsp], rax
            return "    xor rax, rax\n    mov rbx, [rsp]\n    cmp rbx, 0\n    sete al\n    mov [rsp], rax\n";
        }
    }

    // Min/Max operations

    sealed class MinOperation : Operation { // a b -- min(a,b)
        public MinOperation() : base(OperationType.Min) {}
        public override string nasm_linux_x86_64() {
            //    pop rbx
            //    mov rax, [rsp]
            //    cmp rbx, rax
            //    cmovb rax, rbx
            //    mov [rsp], rax
            return "    pop rbx\n    mov rax, [rsp]\n    cmp rbx, rax\n    cmovb rax, rbx\n    mov [rsp], rax\n";
        }
    }

    sealed class MaxOperation : Operation { // a b -- max(a,b)
        public MaxOperation() : base(OperationType.Max) {}
        public override string nasm_linux_x86_64() {
            //    pop rbx
            //    mov rax, [rsp]
            //    cmp rbx, rax
            //    cmova rax, rbx
            //    mov [rsp], rax
            return "    pop rbx\n    mov rax, [rsp]\n    cmp rbx, rax\n    cmova rax, rbx\n    mov [rsp], rax\n";
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
        public ComparisonOperation(ComparisonType comparisonType) : base(OperationType.Comparison) {
            ComparisonType = comparisonType;
        }
        public ComparisonType ComparisonType { get; }
        public override string nasm_linux_x86_64() {
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
    }

    // Dump operation

    sealed class DumpOperation : Operation { // a --
        public DumpOperation() : base(OperationType.Dump) {}
        public override string nasm_linux_x86_64() {
            //    pop rdi
            //    call dump
            return "    pop rdi\n    call dump\n";
        }
    }

    // Exit operation

    sealed class ExitOperation : Operation { // a --
        public ExitOperation() : base(OperationType.Exit) {}
        public override string nasm_linux_x86_64() {
            //    mov rax, 60
            //    pop rdi
            //    syscall
            return "    mov rax, 60\n    pop rdi\n    syscall\n";
        }
    }

    // Variable operations
    
    sealed class VariableAccessOperation : Operation { // -- ptr
        public VariableAccessOperation(int id) : base(OperationType.VariableAccess) {
            Id = id;
        }
        public int Id { get; }
        public override string nasm_linux_x86_64() {
            return "    push var_" + Id + "\n";
        }
    }

    // Mem read/write operations

    sealed class MemReadOperation : Operation { // ptr -- *ptr
        public MemReadOperation(byte amount) : base(OperationType.MemRead) {
            Amount = amount;
        }
        public byte Amount { get; }
        public override string nasm_linux_x86_64() {
            //    mov rax, [rsp]
            string asm = "    mov rax, [rsp]\n";
            if(Amount < 64) asm += "    xor rbx, rbx\n";
            if(Amount == 8) asm += "    mov bl, [rax]\n";
            else if(Amount == 16) asm += "    mov bx, [rax]\n";
            else if(Amount == 32) asm += "    mov ebx, [rax]\n";
            else if(Amount == 64) asm += "    mov rbx, [rax]\n";
            return asm + "    mov [rsp], rbx\n";
        }
    }

    sealed class MemWriteOperation : Operation { // x ptr --
        public MemWriteOperation(byte amount) : base(OperationType.MemWrite) {
            Amount = amount;
        }
        public byte Amount { get; }
        public override string nasm_linux_x86_64() {
            //    pop rax
            //    pop rbx
            string asm = "    pop rax\n    pop rbx\n";
            if(Amount == 8) asm += "    mov [rax], bl\n";
            else if(Amount == 16) asm += "    mov [rax], bx\n";
            else if(Amount == 32) asm += "    mov [rax], ebx\n";
            else if(Amount == 64) asm += "    mov [rax], rbx\n";
            return asm;
        }
    }

    // String literal operation

    sealed class StringOperation : Operation { // -- len ptr
        public StringOperation(int id, int length) : base(OperationType.String) {
            Id = id;
            Length = length;
        }
        public int Id { get; }
        public int Length { get; }
        public override string nasm_linux_x86_64() {
            //    push {len}
            //    push str_{Id}
            return "    push " + Length + "\n    push str_" + Id + "\n";
        }
    }

    sealed class CStyleStringOperation : Operation { // -- ptr
        public CStyleStringOperation(int id) : base(OperationType.CStyleString) {
            Id = id;
        }
        public int Id { get; }
        public override string nasm_linux_x86_64() {
            //    push str_{Id}
            return "    push str_" + Id + "\n";
        }
    }

    // Syscall operation

    sealed class SyscallOperation : Operation { // args[] syscall --
        public SyscallOperation(int argc) : base(OperationType.Syscall) {
            Argc = argc;
        }
        public int Argc { get; }
        public override string nasm_linux_x86_64() {
            string asm = "    pop rax\n";
            for(int i = 0; i < Argc; i++) asm += "    pop " + Utils.SyscallRegisters[i] + "\n";
            return asm + "    syscall\n";
        }
    }

    // Procedure call operation

    sealed class ProcedureCallOperation : Operation { // args[] --
        public ProcedureCallOperation(Procedure proc) : base(OperationType.ProcedureCall) {
            Proc = proc;
        }
        public Procedure Proc { get; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            for(int i = 0; i < Proc.Argc; i++) asm += "    pop " + Utils.FreeUseRegisters[i] + "\n";
            asm += "    call proc_" + Proc.Id + "\n";
            for(int i = Proc.Rvc-1; i >= 0; i--) asm += "    push " + Utils.FreeUseRegisters[i] + "\n";
            return asm;
        }
    }

    // Procedure call operation

    sealed class ReturnOperation : Operation { // --
        public ReturnOperation(int id) : base(OperationType.Return) {
            Id = id;
        }
        public int Id { get; }
        public override string nasm_linux_x86_64() {
            //    jmp proc_{Id}_end
            return "    jmp proc_" + Id + "_end\n";
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

        private static int nextScopeId = 0;
        private static int ScopeId() { return nextScopeId++; }

        public Scope(Scope parent, Procedure procedure) {
            Id = ScopeId();
            Parent = parent;
            Procedure = procedure;
            Variables = new Dictionary<string, Variable>();
        }
        
        public int Id { get; }
        public Scope Parent { get; }
        public Procedure Procedure { get; }
        public Dictionary<string, Variable> Variables { get; }

        public Error RegisterVariable(Variable var) {
            Variable ownVar = GetOwnVariable(var.Identifier.Text);
            if(ownVar != null) return new VariableRedeclarationError(ownVar.Identifier, var.Identifier);
            Variables.Add(Id + "_" + var.Identifier.Text, var);
            return null;
        }
        private Variable GetOwnVariable(string identifier) {
            Variables.TryGetValue(Id + "_" + identifier, out Variable variable);
            return variable;
        }
        public Variable GetVariable(string identifier) {
            Variable ownVar = GetOwnVariable(identifier);
            if(ownVar != null) return ownVar;
            if(Parent != null) return Parent.GetVariable(identifier);
            return null;
        }

    }

    abstract class Block : Operation {
        private static int nextBlockId = 0;
        private static int BlockId() { return nextBlockId++; }

        public Block(BlockType blockType) : base(OperationType.Block) {
            Id = BlockId();
            BlockType = blockType;
        }
        public int Id { get; }
        public BlockType BlockType { get; }
    }

    sealed class CodeBlock : Block {
        public CodeBlock(Scope parentScope, Procedure procedure) : base(BlockType.Code) {
            Operations = new List<Operation>();
            Scope = new Scope(parentScope, procedure);
        }
        public List<Operation> Operations { get; }
        public Scope Scope { get; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            foreach(Operation operation in Operations) asm += operation.nasm_linux_x86_64();
            return asm;
        }
    }

    sealed class IfBlock : Block { // TODO: rework
        public IfBlock(CodeBlock blockIf, CodeBlock blockElse) : base(BlockType.If) {
            BlockIf = blockIf;
            BlockElse = blockElse;
            Conditions = new List<CodeBlock>();
            Conditionals = new List<CodeBlock>();
        }
        public CodeBlock BlockIf { get; set; }
        public List<CodeBlock> Conditions { get; }
        public List<CodeBlock> Conditionals { get; }
        public CodeBlock BlockElse { get; set; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "if_" + Id + ":\n";
            asm += "    pop rax\n";
            asm += "    cmp rax, 0\n";
            asm += (Conditionals.Count > 0) ? "    je if_" + Id + "_elseif_" + 0 + "\n" : "    je if_" + Id + "_else\n";
            asm += BlockIf.nasm_linux_x86_64();
            asm += "    jmp if_" + Id + "_end\n";
            for(int i = 0; i < Conditionals.Count; i++) {
                asm += "if_" + Id + "_elseif_" + i + ":\n";
                asm += Conditions[i].nasm_linux_x86_64();
                asm += "    pop rax\n";
                asm += "    cmp rax, 0\n";
                asm += (i < Conditionals.Count - 1) ? "    je if_" + Id + "_elseif_" + (i+1) + "\n" : "    je if_" + Id + "_else\n";
                asm += Conditionals[i].nasm_linux_x86_64();
                asm += "jmp if_" + Id + "_end\n";
            }
            asm += "if_" + Id + "_else:\n";
            asm += BlockElse != null ? BlockElse.nasm_linux_x86_64() : "";
            asm += "if_" + Id + "_end:\n";
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
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "while_" + Id + ":\n";
            asm += Condition.nasm_linux_x86_64();
            asm += "    pop rax\n";
            asm += "    cmp rax, 0\n";
            asm += "    je while_" + Id + "_end\n";
            asm += Block.nasm_linux_x86_64();
            asm += "    jmp while_" + Id + "\n";
            asm += "while_" + Id + "_end:\n";
            return asm;
        }
        public override string continue___nasm_linux_x86_64() {
            //    jmp while_{Id}
            return "    jmp while_" + Id + "\n";
        }
        public override string break___nasm_linux_x86_64() {
            //    jmp while_{Id}_end
            return "    jmp while_" + Id + "_end\n";
        }
    }

    sealed class DoWhileBlock : BreakableBlock {
        public DoWhileBlock(CodeBlock block, CodeBlock condition) : base(BlockType.DoWhile) {
            Block = block;
            Condition = condition;
        }
        public CodeBlock Block { get; set; }
        public CodeBlock Condition { get; set; }
        public override string nasm_linux_x86_64() {
            string asm = "";
            asm += "dowhile_" + Id + ":\n";
            asm += Block.nasm_linux_x86_64();
            asm += "dowhile_" + Id + "_do:\n";
            asm += Condition.nasm_linux_x86_64();
            asm += "    pop rax\n";
            asm += "    cmp rax, 0\n";
            asm += "    jne dowhile_" + Id + "\n";
            asm += "dowhile_" + Id + "_end:\n";
            return asm;
        }
        public override string continue___nasm_linux_x86_64() {
            //    jmp dowhile_{Id}_do
            return "    jmp dowhile_" + Id + "_do\n";
        }
        public override string break___nasm_linux_x86_64() {
            //    jmp dowhile_{Id}_end
            return "    jmp dowhile_" + Id + "_end\n";
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