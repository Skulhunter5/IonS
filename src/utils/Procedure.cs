using System;
using System.Collections.Generic;

namespace IonS {

    class Procedure {
        private static int nextProcedureId = 0;
        private static int ProcedureId() { return nextProcedureId++; }

        private int nextOccurrenceId = 0;
        private int Occurrence() { return nextOccurrenceId++; }

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
        public bool IsInlined { get; }

        public override string ToString() {
            return Name + (Args != null ? "(" + String.Join(" ", Args) + " -- " + String.Join(" ", Rets) + ")" : "");
        }

        public string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                int occurrence = Occurrence();
                string asm = "";
                if(!IsInlined) {
                    //proc_{Id}:
                    //    mov [ret_stack_rsp], rsp
                    //    mov rsp, rax
                    asm += "proc_" + Id + ":\n    mov [ret_stack_rsp], rsp\n    mov rsp, rax\n";
                } else {
                    //proc_{Id}_{occurrence}:
                    asm += "proc_" + Id + "_" + occurrence + ":\n";
                }
                asm += Body.GenerateAssembly(assembler);
                if(!IsInlined) {
                    //proc_{Id}_end:
                    //    mov rax, rsp
                    //    mov rsp, [ret_stack_rsp]
                    //    ret
                    asm += "proc_" + Id + "_end:\n    mov rax, rsp\n    mov rsp, [ret_stack_rsp]\n    ret\n";
                } else {
                    //proc_{Id}_end_{occurrence}:
                    asm += "proc_" + Id + "_end_" + occurrence + ":\n";
                }
                return asm;
            }
            throw new NotImplementedException();
        }

        public Error TypeCheck() {
            TypeCheckContract contract = new TypeCheckContract();
            foreach(DataType dataType in Args) contract.Push(dataType);

            Error error = Body.TypeCheck(contract);
            if(error != null) return error;

            if(contract.GetElementsLeft() != Rets.Length) return new InvalidReturnDataError(contract.Stack.ToArray(), this);

            for(int i = 0; i < Rets.Length; i++) if(!EDataType.IsImplicitlyCastable(contract.Peek(Rets.Length-1-i), Rets[i])) {
                if(contract.Peek(Rets.Length-1-i) != Rets[i]) Console.WriteLine("[TypeChecker] Warning: Implicit cast from " + EDataType.StringOf(contract.Peek(Rets.Length-1-i)) + " to " + Rets[i] + " while returning from " + this); // Error-Warning-System
                return new InvalidReturnDataError(contract.Stack.ToArray(), this);
            }

            return null;
        }

        public string GetArgsSignature() {
            string sig = "";
            for(int i = 0; i < Args.Length; i++) sig += EDataType.StringOf(Args[i]) + (i < Args.Length-1 ? "::" : "");
            return sig;
        }

        public string GetSignature() {
            string sig = "";
            for(int i = 0; i < Args.Length; i++) sig += EDataType.StringOf(Args[i]) + (i < Args.Length-1 ? "::" : "");
            sig += "--";
            for(int i = 0; i < Rets.Length; i++) sig += EDataType.StringOf(Rets[i]) + (i < Rets.Length-1 ? "::" : "");
            return sig;
        }

    }

}