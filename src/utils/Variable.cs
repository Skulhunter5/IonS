using System;

namespace IonS {
    
    class Variable : AssemblyGenerator {
        private static int nextVariableId = 0;
        private static int VariableId() { return nextVariableId++; }

        public Variable(Word identifier, int bytesize) {
            Id = VariableId();
            Identifier = identifier;
            Bytesize = bytesize;
        }

        public int Id { get; }
        public Word Identifier { get; }
        public int Bytesize { get; }

        public override string generateAssembly(Assembler assembler) {
            if(assembler == Assembler.nasm_linux_x86_64 || assembler == Assembler.fasm_linux_x86_64) {
                //    var_{Id}: resb {Bytesize}
                return "    var_" + Id + ": resb " + Bytesize + "\n";
            }
            if(assembler == Assembler.fasm_linux_x86_64) {
                //    var_{Id}: resb {Bytesize}
                return "    var_" + Id + ": rb " + Bytesize + "\n";
            }
            throw new NotImplementedException();
        }
    }

}