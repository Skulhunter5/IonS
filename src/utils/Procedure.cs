using System.Collections.Generic;

namespace IonS {

    class Procedure : IAssemblyGenerator {
        private static int nextProcedureId = 0;
        private static int ProcedureId() { return nextProcedureId++; }

        public Procedure(Word name, int argc, int rvc, CodeBlock body, bool isInlined) {
            Id = ProcedureId();
            Name = name;
            Argc = argc;
            Rvc = rvc;
            Body = body;
            Variables = new List<Variable>();
            IsUsed = false;
            UsedProcedures = new List<Procedure>();
            IsInlined = isInlined;
        }

        public int Id { get; }
        public Word Name { get; }
        public int Argc { get; }
        public int Rvc { get; }
        public CodeBlock Body { get; set; }
        public List<Variable> Variables { get; }
        public bool IsUsed { get; set; }
        public List<Procedure> UsedProcedures { get; }
        public bool IsInlined { get; set; }

        string IAssemblyGenerator.nasm_linux_x86_64() { // args[] -- ret[]
            string asm = "";
            if(!IsInlined) {
                asm += "proc_" + Id + ":\n";
                for(int i = Argc-1; i >= 0; i--) asm += "    push " + Utils.FreeUseRegisters[i] + "\n";
            }
            asm += Body.nasm_linux_x86_64();
            if(!IsInlined) {
                asm += "proc_" + Id + "_end:\n";
                for(int i = 0; i < Rvc; i++) asm += "    pop " + Utils.FreeUseRegisters[i] + "\n";
                asm += "    ret\n";
            }
            return asm;
        }
    }

}