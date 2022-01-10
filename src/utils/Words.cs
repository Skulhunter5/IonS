namespace IonS {

    class Word {
        public Word(Position position, string text) {
            Position = position;
            Text = text;
            ExpandedFrom = null;
            IncludedFrom = null;
        }

        public override string ToString()
        {
            if(ExpandedFrom != null) return "'" + Text + "' (expanded from " + ExpandedFrom + ")";
            if(IncludedFrom != null) return "'" + Text + "' (Included from " + IncludedFrom[0] + " at " + IncludedFrom[1] + ")";
            return "'" + Text + "' at " + Position;
        }

        public Position Position { get; }
        public string Text { get; }
        public Word ExpandedFrom { get; set; }
        public Position[] IncludedFrom { get; set; }
    }

    class StringWord : Word {
        public StringWord(Position position, string text, string stringType) : base(position, text) {
            StringType = stringType;
        }

        public override string ToString()
        {
            if(ExpandedFrom != null) return "'\"" + Text + "\"' (expanded from " + ExpandedFrom + ")";
            if(IncludedFrom != null) return "'\"" + Text + "\"' (Included from " + IncludedFrom[0] + " at " + IncludedFrom[1] + ")";
            return "'\"" + Text + "\"' at " + Position;
        }

        public string StringType { get; }
    }

    class CharWord : Word {
        public CharWord(Position position, string text) : base(position, text) {}

        public override string ToString()
        {
            if(ExpandedFrom != null) return "char:'" + Text + "' (expanded from " + ExpandedFrom + ")";
            if(IncludedFrom != null) return "char:'" + Text + "' (Included from " + IncludedFrom[0] + " at " + IncludedFrom[1] + ")";
            return "char:'" + Text + "' at " + Position;
        }
    }

}