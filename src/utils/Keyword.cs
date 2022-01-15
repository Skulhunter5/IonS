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
            "null",
            "if", "else", "while", "do",
            "continue", "break",
            "macro",
            "var",
            "include",
            "here", "chere",
            "{", "}",
            "proc", "return", "inline",
            "assert",
        };
        public static bool isValidIdenfitier(string word) {
            if(word.StartsWith("@") || word.StartsWith("!")) return false;
            if(word.StartsWith("syscall")) return false;
            if(word.StartsWith("ctt")) return false;
            if(word.StartsWith("cast(")) return false;

            if(Utils.binaryRegex.IsMatch(word)) return false;
            if(Utils.octalRegex.IsMatch(word)) return false;
            if(Utils.decimalRegex.IsMatch(word)) return false;
            if(Utils.hexadecimalRegex.IsMatch(word)) return false;

            foreach(string keyword in keywords) if(keyword == word) return false;
            return true;
        }
    }

}