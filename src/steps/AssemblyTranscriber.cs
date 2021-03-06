using System;
using System.IO;

namespace IonS {

    sealed class AssemblyTranscriptionResult : Result {
        public AssemblyTranscriptionResult(string asm, Error error) : base(error) {
            Asm = asm;
        }
        public string Asm { get; }
    }

    class AssemblyTranscriber {

        private static readonly int X86_64_RET_STACK_CAP = 1024 * 64; // 512KB 8-bit segments // TODO: look into customization option for this

        private readonly string _text, _source;

        public AssemblyTranscriber(string text, string source) {
            _text = text;
            _source = source;
        }

        public AssemblyTranscriptionResult run(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                string asm = "";

                if(assembler == Assembler.nasm_linux_x86_64) asm += "BITS 64\n";
                else asm += "format ELF64 executable 3\n";

                ParseResult result = new Parser(_text, _source, assembler).Parse();
                if(ErrorSystem.ShouldTerminateAfterStep()) ErrorSystem.WriteAndExit();

                // Begin data segment
                string rq = assembler == Assembler.nasm_linux_x86_64 ? "resq" : "rq";
                if(assembler == Assembler.nasm_linux_x86_64) asm += "segment .bss\n";
                else asm += "segment readable writeable\n";
                asm += "    args_ptr: " + rq + " 1\n";
                asm += "    ret_stack_rsp: " + rq + " 1\n"; // TODO: maybe do something against ret_stack overflow
                asm += "    ret_stack: " + rq + " " + X86_64_RET_STACK_CAP + "\n";
                asm += "    ret_stack_end:\n";

                foreach(Variable var in result.Variables) asm += var.GenerateAssembly(assembler);
                foreach(var functions in result.Functions.Values) foreach(Function function in functions.Values) if(function.IsUsed) foreach(Variable var in function.Variables) asm += var.GenerateAssembly(assembler);

                // Begin data segment
                if(assembler == Assembler.nasm_linux_x86_64 && result.Strings.Count > 0) asm += "segment .data\n";

                for(int i = 0; i < result.Strings.Count; i++) {
                    if(result.Strings[i].Length > 0) asm += "    str_" + i + ": db " + Utils.StringLiteralToByteString(result.Strings[i]) + "\n";
                    else asm += "    str_" + i + ":\n";
                }

                // Begin code segment
                if(assembler == Assembler.nasm_linux_x86_64) asm += "segment .text\n";
                else asm += "segment readable executable\n";

                // Dump function
                asm += File.ReadAllText("src/asm snippets/dump.asm");

                // Functions (only used ones for now)
                foreach(var overloads in result.Functions.Values) foreach(Function function in overloads.Values) if(function.IsUsed) asm += function.GenerateAssembly(assembler);

                if(assembler == Assembler.nasm_linux_x86_64) asm += "global _start\n_start:\n";
                else asm += "entry _start\n_start:\n";

                // Prepare args
                asm += "    mov [args_ptr], rsp\n";
                // Prepare ret_stack
                asm += "    mov rax, ret_stack_end\n";
                asm += "    mov [ret_stack_rsp], rax\n";

                // Actual code
                asm += result.Root.GenerateAssembly(assembler);

                // Exit code
                asm += "exit:\n";
                asm += "    mov rax, 60\n";
                asm += "    mov rdi, 0\n";
                asm += "    syscall\n";
                return new AssemblyTranscriptionResult(asm, null);
            }
            if(assembler == Assembler.iasm_linux_x86_64) {
                string asm = "";

                ParseResult result = new Parser(_text, _source, assembler).Parse();
                if(ErrorSystem.ShouldTerminateAfterStep()) ErrorSystem.WriteAndExit();

                // Actual code
                asm += result.Root.GenerateAssembly(assembler);

                // Exit code
                asm += "exit:\n";
                asm += "    mov rax 60\n";
                asm += "    mov rdi 0\n";
                asm += "    syscall\n";
                return new AssemblyTranscriptionResult(asm, null);
            }
            throw new NotImplementedException();
        }

    }

}