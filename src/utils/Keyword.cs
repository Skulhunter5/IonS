namespace IonS {
    
    class Keyword {
        // Remember: Whenever new keyword is added: put it here
        private static readonly string[] keywords = new string[] {
            "exit",
            "drop", "2drop",
            "dup", "2dup",
            "over", "2over",
            "swap", "2swap",
            "rot", "2rot", "rot5", "2rot5",
            "+", "-", "*", "/", "%", "/%",
            "<<", ">>", "&", "|", "^", "inv",
            "&&", "||", "not",
            "min", "max",
            "==", "!=", "<", ">", "<=", ">=",
            ".",
            "true", "false",
            "if", "else", "while", "do",
            "continue", "break",
            "macro",
            "var",
            "include",
            "here", "chere",
            "{", "}",
            "proc", "return", "inline",
        };
        public static bool isReserved(string word) {
            if(word.StartsWith("@") || word.StartsWith("!")) return true;
            if(word.StartsWith("syscall")) return true;
            if(word.StartsWith("ctt")) return true;
            foreach(string keyword in keywords) if(keyword == word) return true;
            return false;
        }
    }

}