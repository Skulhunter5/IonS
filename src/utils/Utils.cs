using System;
using System.Collections.Generic;
using System.Text;

namespace IonS {

    class Utils {
        
        public static string StringLiteralToByteString(string literal) {
            return String.Join(',', Encoding.ASCII.GetBytes(literal));
        }

    }

    class Position {
        public Position(string file, int line, int column) {
            File = file;
            Line = line;
            Column = column;
        }
        public override string ToString()
        {
            return File + ":" + Line + ":" + Column;
        }
        public string File { get; }
        public int Line { get; }
        public int Column { get; }
    }

    class Word {
        public Word(Position position, string text) {
            Position = position;
            Text = text;
            ExpandedFrom = null;
        }

        public override string ToString()
        {
            if(ExpandedFrom != null) return "'" + Text + "' (expanded from " + ExpandedFrom + ")" ;
            return "'" + Text + "' at " + Position;
        }

        public Position Position { get; }
        public string Text { get; }
        public Word ExpandedFrom { get; set; }
    }
    
    abstract class Result {
        public Result(Error error) {
            Error = error;
        }
        public Error Error { get; }
    }

    class Keyword {
        // Remember: Whenever new keyword is added: put it here
        private static readonly string[] keywords = new string[] {
            "exit",
            "putc",
            "drop", "2drop",
            "dup", "2dup",
            "over", "2over",
            "swap",
            "+", "-", "*", "/", "%",
            ".",
            "if", "while", "do", "end",
            "continue", "break",
            "macro",
            "var"};
        public static bool isReserved(string word) {
            if(word.StartsWith("@") || word.StartsWith("!")) return true;
            foreach(string keyword in keywords) if(keyword == word) return true;
            return false;
        }
    }
    
}