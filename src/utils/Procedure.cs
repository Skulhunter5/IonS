using System;
using System.Collections.Generic;

namespace IonS {

    class Procedure {
        private static int nextProcedureId = 0;
        private static int ProcedureId() { return nextProcedureId++; }

        public Procedure(Word name, DataType[] args, DataType[] rets, CodeBlock body, bool isInlined) {
            Id = ProcedureId();
            Name = name;
            Args = args;
            Rets = rets;
            Body = body;
            Variables = new List<Variable>();
            IsUsed = false;
            UsedProcedures = new List<Procedure>();
            IsInlined = isInlined;
        }

        public int Id { get; }
        public Word Name { get; }
        public DataType[] Args { get; }
        public DataType[] Rets { get; }
        public CodeBlock Body { get; set; }
        public List<Variable> Variables { get; }
        public bool IsUsed { get; set; }
        public List<Procedure> UsedProcedures { get; }
        public bool IsInlined { get; set; }

        public override string ToString() {
            return Name + (Args != null ? "(" + String.Join(" ", Args) + " -- " + String.Join(" ", Rets) + ")" : "");
        }

        public string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                string asm = "";
                if(!IsInlined) {
                    asm += "proc_" + Id + ":\n";
                    for(int i = Args.Length-1; i >= 0; i--) asm += "    push " + Utils.FreeUseRegisters[i] + "\n";
                }
                asm += Body.GenerateAssembly(assembler);
                if(!IsInlined) {
                    asm += "proc_" + Id + "_end:\n";
                    for(int i = 0; i < Rets.Length; i++) asm += "    pop " + Utils.FreeUseRegisters[i] + "\n";
                    asm += "    ret\n";
                }
                return asm;
            }
            throw new NotImplementedException();
        }
    }

}