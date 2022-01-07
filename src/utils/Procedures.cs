namespace IonS {

    class Procedure {
        private static int nextProcedureId = 0;
        private static int ProcedureId() { return nextProcedureId++; }

        public Procedure(Word name, int argc, int rvc, CodeBlock body) {
            Id = ProcedureId();
            Name = name;
            Argc = argc;
            Rvc = rvc;
            Body = body;
            IsUsed = false;
        }
        public int Id { get; }
        public Word Name { get; }
        public int Argc { get; }
        public int Rvc { get; }
        public CodeBlock Body { get; set; }
        public bool IsUsed { get; set; }
        public string nasm_linux_x86_64() {
            string asm = "";
            asm += "proc_" + Id + ":\n";
            for(int i = Argc-1; i >= 0; i--) asm += "    push " + Utils.FreeUseRegisters[i] + "\n";
            asm += Body.nasm_linux_x86_64();
            asm += "proc_" + Id + "_end:\n";
            for(int i = 0; i < Rvc; i++) asm += "    pop " + Utils.FreeUseRegisters[i] + "\n";
            asm += "    ret\n";
            return asm;
        }
    }

}