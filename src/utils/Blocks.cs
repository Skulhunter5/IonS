namespace IonS {

    abstract class Block {
        public Block(Position position, int id) {
            Position = position;
            Id = id;
        }
        public Position Position { get; }
        public int Id { get; }
    }

    sealed class IfBlock : Block {
        public IfBlock(Position position, int id) : base(position, id) {}
    }

    sealed class WhileBlock : Block {
        public WhileBlock(Position position, int id) : base(position, id) {}
        public bool HasDo { get; set; }
    }

    sealed class DoWhileBlock : Block {
        public DoWhileBlock(Position position, int id) : base(position, id) {}
        public bool HasWhile { get; set; }
    }

}