namespace IonS {
    
    class Variable : IAssemblyGenerator {
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

        string IAssemblyGenerator.nasm_linux_x86_64() {
            //    var_{Id}: resb {Bytesize}
            return "    var_" + Id + ": resb " + Bytesize + "\n";
        }
    }

}