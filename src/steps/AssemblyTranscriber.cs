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

        public AssemblyTranscriptionResult nasm_linux_x86_64() {
            string asm = "BITS 64\n";
            var parser = new Parser(_text, _source);
            var result = parser.Parse();
            if(result.Error != null) return new AssemblyTranscriptionResult(null, result.Error);
            var root = result.Root;

            if(result.Variables.Count > 0) asm += "segment .bss\n";
            foreach(Variable var in result.Variables) asm += ((IAssemblyGenerator) var).nasm_linux_x86_64();
            foreach(Procedure proc in result.Procedures.Values) if(proc.IsUsed) foreach(Variable var in proc.Variables) asm += ((IAssemblyGenerator) var).nasm_linux_x86_64();

            if(result.Strings.Count > 0) asm += "segment .data\n";
            for(int i = 0; i < result.Strings.Count; i++) {
                if(result.Strings[i].Length > 0) asm += "    str_" + i + ": db " + Utils.StringLiteralToByteString(result.Strings[i]) + "\n";
                else asm += "    str_" + i + ":\n";
            }

            asm += "segment .text\n";
            asm += File.ReadAllText("res/asm snippets/dump.asm");

            // TODO: fix: procs used in other procs that aren't used still count as used
            // --> maybe add a list that contains all used procs and only add the ones used directly
            // --> the ones used by other procs should be collected inside the other proc and then added when that proc is added
            foreach(Procedure proc in result.Procedures.Values) if(proc.IsUsed) asm += ((IAssemblyGenerator) proc).nasm_linux_x86_64();

            asm += "global _start\n_start:\n";

            asm += root.nasm_linux_x86_64();

            asm += "exit:\n";
            asm += "    mov rax, 60\n";
            asm += "    mov rdi, 0\n";
            asm += "    syscall\n";
            return new AssemblyTranscriptionResult(asm, null);
        }

    }

}