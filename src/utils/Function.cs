using System;
using System.Collections.Generic;

namespace IonS {

    class Function {
        private static int nextFunctionId = 0;
        private static int FunctionId() { return nextFunctionId++; }

        private int nextOccurrenceId = 0;
        private int Occurrence() { return nextOccurrenceId++; }

        public Function(Word name, DataType[] args, DataType[] rets, CodeBlock body, bool isInlined) {
            Id = FunctionId();

            Name = name;

            ArgSig = new Signature(args);
            RetSig = new Signature(rets);

            Body = body;

            Variables = new List<Variable>();

            IsUsed = false;
            UsedFunctions = new List<Function>();

            IsInlined = isInlined;
        }

        public int Id { get; }

        public Word Name { get; }

        public Signature ArgSig { get; }
        public Signature RetSig { get; }

        public CodeBlock Body { get; set; }

        public List<Variable> Variables { get; }

        public bool IsUsed { get; set; }
        public List<Function> UsedFunctions { get; }

        public bool IsInlined { get; }

        public override string ToString() {
            return Name + (ArgSig != null ? "(" + ArgSig.GetTypeString() + " -- " + RetSig.GetTypeString() + ")" : "");
        }

        public void Use() {
            IsUsed = true;
            foreach(Function function in UsedFunctions) function.Use();
        }

        public string GenerateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                int occurrence = Occurrence();
                string asm = "";
                if(!IsInlined) {
                    //function_{Id}:
                    //    mov [ret_stack_rsp], rsp
                    //    mov rsp, rax
                    asm += "function_" + Id + ":\n    mov [ret_stack_rsp], rsp\n    mov rsp, rax\n";
                } else {
                    //function_{Id}_{occurrence}:
                    asm += "function_" + Id + "_" + occurrence + ":\n";
                }
                asm += Body.GenerateAssembly(assembler);
                if(!IsInlined) {
                    //function_{Id}_end:
                    //    mov rax, rsp
                    //    mov rsp, [ret_stack_rsp]
                    //    ret
                    asm += "function_" + Id + "_end:\n    mov rax, rsp\n    mov rsp, [ret_stack_rsp]\n    ret\n";
                } else {
                    //function_{Id}_end_{occurrence}:
                    asm += "function_" + Id + "_end_" + occurrence + ":\n";
                }
                return asm;
            }
            throw new NotImplementedException();
        }

        public Error TypeCheck(TypeCheckContext context) {
            TypeCheckContract contract = new TypeCheckContract();
            foreach(DataType dataType in ArgSig.Types) contract.Push(dataType);

            Error error = Body.TypeCheck(context, contract);
            if(error != null) return error;

            if(contract.HasReturned) return null;

            if(contract.GetElementsLeft() != RetSig.Types.Length) return new InvalidReturnDataError(contract.Stack.ToArray(), this);

            for(int i = 0; i < RetSig.Size; i++) if(!DataType.IsImplicitlyCastable(contract.Peek(RetSig.Types.Length-1-i), RetSig.Types[i])) { // TODO: check what I want to do here, this looks odd
                Console.WriteLine(contract.Peek(RetSig.Size-1-i) + " " + RetSig.Types[i]);
                if(contract.Peek(RetSig.Size-1-i) != RetSig.Types[i]) ErrorSystem.AddWarning(new ImplicitCastWhenReturningWarning(contract.Peek(RetSig.Types.Length-1-i), RetSig.Types[i], this));
                return new InvalidReturnDataError(contract.Stack.ToArray(), this);
            }

            return null;
        }

    }

}