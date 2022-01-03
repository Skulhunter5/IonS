namespace IonS {
    
    class Variable {
        public Variable(Word identifier, int bytesize) {
            Identifier = identifier;
            Bytesize = bytesize;
        }
        public Word Identifier { get; }
        public int Bytesize { get; }
    }

}