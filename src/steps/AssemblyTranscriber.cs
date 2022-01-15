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

        private readonly bool _unsafeFlag = false;

        private readonly string _text, _source;

        public AssemblyTranscriber(string text, string source, bool unsafeFlag) {
            _text = text;
            _source = source;
            _unsafeFlag = unsafeFlag;
        }

        public AssemblyTranscriptionResult run(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                string asm = "";

                if(assembler == Assembler.nasm_linux_x86_64) asm += "BITS 64\n";
                else asm += "format ELF64 executable 3\n";

                var parser = new Parser(_text, _source, _unsafeFlag);
                var result = parser.Parse();
                if(result.Error != null) return new AssemblyTranscriptionResult(null, result.Error);
                var root = result.Root;

                // Begin data segment
                if(assembler == Assembler.nasm_linux_x86_64 && result.Variables.Count > 0) asm += "segment .bss\n";
                else if(assembler == Assembler.fasm_linux_x86_64 && (result.Variables.Count > 0 || result.Strings.Count > 0)) asm += "segment readable writeable\n";

                foreach(Variable var in result.Variables) asm += var.GenerateAssembly(assembler);
                foreach(Procedure proc in result.Procedures.Values) if(proc.IsUsed) foreach(Variable var in proc.Variables) asm += var.GenerateAssembly(assembler);

                // Begin data segment
                if(assembler == Assembler.nasm_linux_x86_64 && result.Strings.Count > 0) asm += "segment .data\n";

                for(int i = 0; i < result.Strings.Count; i++) {
                    if(result.Strings[i].Length > 0) asm += "    str_" + i + ": db " + Utils.StringLiteralToByteString(result.Strings[i]) + "\n";
                    else asm += "    str_" + i + ":\n";
                }

                // Begin data segment
                if(assembler == Assembler.nasm_linux_x86_64) asm += "segment .text\n";
                else asm += "segment readable executable\n";

                // Dump function
                asm += File.ReadAllText("src/asm snippets/dump.asm");

                // Procedures (only used ones for now)
                foreach(Procedure proc in result.Procedures.Values) if(proc.IsUsed) asm += proc.GenerateAssembly(Assembler.fasm_linux_x86_64);

                if(assembler == Assembler.nasm_linux_x86_64) asm += "global _start\n_start:\n";
                else asm += "entry _start\n_start:\n";

                // Actual code
                asm += root.GenerateAssembly(Assembler.fasm_linux_x86_64);

                // Exit code
                asm += "exit:\n";
                asm += "    mov rax, 60\n";
                asm += "    mov rdi, 0\n";
                asm += "    syscall\n";
                return new AssemblyTranscriptionResult(asm, null);
            }
            throw new NotImplementedException();
        }

    }

}