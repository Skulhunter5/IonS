namespace IonS {

    enum WordType {
        Word,
        String,
        Char,
    }

    class Word {

        public Word(Position position, string text) {
            Type = WordType.Word;

            Position = position;
            Text = text;
            ExpandedFrom = null;
            IncludedFrom = null;
        }
        public Word(WordType type, Position position, string text) {
            Type = type;

            Position = position;
            Text = text;
            ExpandedFrom = null;
            IncludedFrom = null;
        }

        public WordType Type { get; }
        public Position Position { get; }
        public string Text { get; }
        public Word ExpandedFrom { get; set; }
        public Position[] IncludedFrom { get; set; }

        public override string ToString()
        {
            if(ExpandedFrom != null) return "'" + Text + "' (expanded from " + ExpandedFrom + ")";
            if(IncludedFrom != null) return "'" + Text + "' (Included from " + IncludedFrom[0] + " at " + IncludedFrom[1] + ")";
            return "'" + Text + "' at " + Position;
        }

    }

    class StringWord : Word {
        public StringWord(Position position, string text, string stringType) : base(WordType.String, position, text) {
            StringType = stringType;
        }

        public string StringType { get; }

        public override string ToString()
        {
            if(ExpandedFrom != null) return "'\"" + Text + "\"' (expanded from " + ExpandedFrom + ")";
            if(IncludedFrom != null) return "'\"" + Text + "\"' (Included from " + IncludedFrom[0] + " at " + IncludedFrom[1] + ")";
            return "'\"" + Text + "\"' at " + Position;
        }

    }

    class CharWord : Word {
        public CharWord(Position position, string text) : base(WordType.Char, position, text) {}

        public override string ToString()
        {
            if(ExpandedFrom != null) return "char:'" + Text + "' (expanded from " + ExpandedFrom + ")";
            if(IncludedFrom != null) return "char:'" + Text + "' (Included from " + IncludedFrom[0] + " at " + IncludedFrom[1] + ")";
            return "char:'" + Text + "' at " + Position;
        }
    }

}