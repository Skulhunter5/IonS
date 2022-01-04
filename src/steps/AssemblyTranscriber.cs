using System.IO;

namespace IonS {

    class AssemblyTranscriptionResult : Result {
        public AssemblyTranscriptionResult(string asm, Error error) : base(error) {
            Asm = asm;
        }
        public string Asm { get; }
    }

    class AssemblyTranscriber { // TODO: Add support for other assemblers (such as fasm)
        private readonly string _text, _source;
        public AssemblyTranscriber(string text, string source) {
            _text = text;
            _source = source;
        }

        public AssemblyTranscriptionResult generate_nasm_linux_x86_64() {
            string asm = "BITS 64\n";
            var parser = new Parser(_text, _source);
            var result = parser.Parse();
            if(result.Error != null) return new AssemblyTranscriptionResult(null, result.Error);
            var operations = result.Operations;

            if(result.Variables.Count > 0) asm += "segment .bss\n";
            foreach(Variable var in result.Variables) asm += "    var_" + var.Identifier.Text + ": resb " + var.Bytesize + "\n";

            if(result.Strings.Count > 0) asm += "segment .data\n";
            for(int i = 0; i < result.Strings.Count; i++) asm += "    str_" + i + ": db " + Utils.StringLiteralToByteString(result.Strings[i]) + "\n";

            asm += "segment .text\n";

            // SMALL assembly size optimization
            foreach(Operation operation in operations) if(operation.Type == OperationType.Dump) {
                asm += File.ReadAllText("res/asm snippets/dump.asm");
                break;
            }

            asm += "global _start\n_start:\n";
            for(int i = 0; i < operations.Count; i++) {
                Operation operation = operations[i];
                switch(operation.Type) {
                    case OperationType.Exit: {
                        if(i != operations.Count - 1) asm += "    jmp exit\n"; // TODO: move into a dedicated optimizer
                        break;
                    }
                    case OperationType.Push_uint64: {
                        asm += $"    push {((Push_uint64_Operation) operation).Value}\n";
                        break;
                    }
                    case OperationType.Put_char: {
                        asm += "    mov rax, 1\n";
                        asm += "    mov rdi, 1\n";
                        asm += "    pop rsi\n";
                        asm += "    mov rdx, 1\n";
                        asm += "    syscall\n";
                        break;
                    }
                    case OperationType.Drop: {
                        asm += "    pop rax\n";
                        break;
                    }
                    case OperationType.Drop2: {
                        asm += "    pop rax\n";
                        asm += "    pop rax\n";
                        break;
                    }
                    case OperationType.Dup: {
                        asm += "    pop rax\n";
                        asm += "    push rax\n";
                        asm += "    push rax\n";
                        break;
                    }
                    case OperationType.Dup2: {
                        asm += "    pop rbx\n";
                        asm += "    pop rax\n";
                        asm += "    push rax\n";
                        asm += "    push rbx\n";
                        asm += "    push rax\n";
                        asm += "    push rbx\n";
                        break;
                    }
                    case OperationType.Swap: {
                        asm += "    pop rbx\n";
                        asm += "    pop rax\n";
                        asm += "    push rbx\n";
                        asm += "    push rax\n";
                        break;
                    }
                    case OperationType.Over: {
                        asm += "    pop rbx\n";
                        asm += "    pop rax\n";
                        asm += "    push rax\n";
                        asm += "    push rbx\n";
                        asm += "    push rax\n";
                        break;
                    }
                    case OperationType.Over2: {
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
                        break;
                    }
                    case OperationType.Add: {
                        asm += "    pop rbx\n";
                        asm += "    pop rax\n";
                        asm += "    add rax, rbx\n";
                        asm += "    push rax\n";
                        break;
                    }
                    case OperationType.Subtract: {
                        asm += "    pop rbx\n";
                        asm += "    pop rax\n";
                        asm += "    sub rax, rbx\n";
                        asm += "    push rax\n";
                        break;
                    }
                    case OperationType.Multiply: {
                        asm += "    pop rbx\n";
                        asm += "    pop rax\n";
                        asm += "    mul rbx\n";
                        asm += "    push rax\n";
                        break;
                    }
                    case OperationType.Divide: {
                        asm += "    pop rbx\n";
                        asm += "    pop rax\n";
                        asm += "    xor rdx, rdx\n"; // TODO: check why this is necessary
                        asm += "    div rbx\n";
                        asm += "    push rax\n";
                        break;
                    }
                    case OperationType.Modulo: {
                        asm += "    pop rbx\n";
                        asm += "    pop rax\n";
                        asm += "    xor rdx, rdx\n";
                        asm += "    div rbx\n";
                        asm += "    push rdx\n";
                        break;
                    }
                    case OperationType.Dump: {
                        asm += "    pop rdi\n";
                        asm += "    call dump\n";
                        break;
                    }
                    case OperationType.Label: {
                        asm += ((LabelOperation) operation).Label + ":\n";
                        break;
                    }
                    case OperationType.Jump: {
                        asm += "    jmp " + ((JumpOperation) operation).Label + "\n";
                        break;
                    }
                    case OperationType.JumpIfZero: {
                        asm += "    pop rax\n";
                        asm += "    cmp rax, 0\n";
                        asm += "    je " + ((JumpIfZeroOperation) operation).Label + "\n";
                        break;
                    }
                    case OperationType.JumpIfNotZero: {
                        asm += "    pop rax\n";
                        asm += "    cmp rax, 0\n";
                        asm += "    jne " + ((JumpIfNotZeroOperation) operation).Label + "\n";
                        break;
                    }
                    case OperationType.VariableAccess: {
                        asm += "    push var_" + ((VariableAccessOperation) operation).Identifier + "\n";
                        break;
                    }
                    case OperationType.MemWrite: {
                        MemWriteOperation memWriteOperation = (MemWriteOperation) operation;
                        asm += "    pop rax\n";
                        asm += "    pop rbx\n";
                        if(memWriteOperation.Amount == 8) asm += "    mov [rax], bl\n";
                        else if(memWriteOperation.Amount == 16) asm += "    mov [rax], bx\n";
                        else if(memWriteOperation.Amount == 32) asm += "    mov [rax], ebx\n";
                        else if(memWriteOperation.Amount == 64) asm += "    mov [rax], rbx\n";
                        break;
                    }
                    case OperationType.MemRead: {
                        MemReadOperation memReadOperation = (MemReadOperation) operation;
                        asm += "    pop rax\n";
                        if(memReadOperation.Amount < 64) asm += "    xor rbx, rbx\n";
                        if(memReadOperation.Amount == 8) asm += "    mov bl, [rax]\n";
                        else if(memReadOperation.Amount == 16) asm += "    mov bx, [rax]\n";
                        else if(memReadOperation.Amount == 32) asm += "    mov ebx, [rax]\n";
                        else if(memReadOperation.Amount == 64) asm += "    mov rbx, [rax]\n";
                        asm += "    push rbx\n";
                        break;
                    }
                    case OperationType.StringLiteral: {
                        asm += "    push str_" + ((StringLiteralOperation) operation).Id + "\n";
                        break;
                    }
                    default: {
                        return new AssemblyTranscriptionResult(null, new UnimplementedOperationAssemblyTranscriberError(operation.Type));
                    }
                }
            }
            asm += "exit:\n";
            asm += "    mov rax, 60\n";
            asm += "    pop rdi\n";
            asm += "    syscall\n";
            return new AssemblyTranscriptionResult(asm, null);
        }
    }

}