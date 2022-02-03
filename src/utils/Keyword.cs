using System.Text.RegularExpressions;

namespace IonS {
    
    class Keyword {

        // Remember: Whenever new keyword is added: put it here
        private static readonly string[] keywords = new string[] {
            "exit",
            "drop", "2drop",
            "dup", "2dup",
            "over", "2over",
            "swap", "2swap",
            "rot", "rrot", "rot5", "rrot5",
            "+", "-", "*", "/", "%", "/%",
            "<<", ">>", "&", "|", "^", "inv",
            "&&", "||", "not",
            "min", "max",
            "==", "!=", "<", ">", "<=", ">=",
            ".",
            "true", "false",
            "null",
            "if", "else", "while", "do", "switch", "case",
            "continue", "break",
            "macro",
            "var",
            "#include",
            "here", "chere",
            "{", "}",
            "proc", "return", "inline",
            "assert",
            "iota", "reset",
            "argc", "argv",
        };

        public static bool isValidIdenfitier(string word) {
            if(word.StartsWith("#")) return false;

            if(Utils.wildcardRegex.IsMatch(word)) return false;

            if(Utils.readBytesRegex.IsMatch(word) || Utils.writeBytesRegex.IsMatch(word)) return false;

            if(word.StartsWith("syscall")) return false;
            if(word.StartsWith("ctt")) return false;
            if(word.StartsWith("cast(")) return false;

            if(Utils.binaryRegex.IsMatch(word) || Utils.octalRegex.IsMatch(word) || Utils.decimalRegex.IsMatch(word) || Utils.hexadecimalRegex.IsMatch(word)) return false;

            if(EDataType.TryParse(word, out DataType _)) return false;

            foreach(string keyword in keywords) if(keyword == word) return false;

            return true;
        }

    }

}