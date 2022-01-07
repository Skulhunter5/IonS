namespace IonS {
    
    class Variable {
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
    }

}