using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace IonS {

    enum Assembler {
        nasm_linux_x86_64,
        fasm_linux_x86_64,
        iasm_linux_x86_64,
    }

    abstract class Result {
        public Result(Error error) {
            Error = error;
        }
        public Error Error { get; }
    }

    class Utils {
        
        public static readonly HashSet<string> keywords = new HashSet<string> {
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
            "function", "return", "inline",
            "assert",
            "iota", "reset",
            "argc", "argv",
            "let", "peek",
            "struct",
            "@", "!",
            "@[]", "![]",
        };


        public static readonly string[] SyscallRegisters = new string[] {"rdi", "rsi", "rdx", "r10", "r8", "r9"};


        public static readonly Regex symbolRegex = new Regex("^[a-zA-Z_][0-9a-zA-Z_]*$", RegexOptions.Compiled);

        public static readonly Regex charRegex = new Regex("^'([^\\\\]|\\\\.)'$", RegexOptions.Compiled);

        public static readonly Regex wildcardRegex = new Regex("^_+$", RegexOptions.Compiled);
        
        public static readonly Regex readBytesRegex = new Regex("^@[0-9]+$", RegexOptions.Compiled);
        public static readonly Regex writeBytesRegex = new Regex("^![0-9]+$", RegexOptions.Compiled);
        public static readonly Regex cttRegex = new Regex("^ctt[0-9]+$", RegexOptions.Compiled);
        public static readonly Regex syscallRegex = new Regex("^syscall[0-9]+$", RegexOptions.Compiled);

        public static readonly Regex binaryRegex = new Regex("^0b[01_]+$", RegexOptions.Compiled);
        public static readonly Regex octalRegex = new Regex("^0[0-7_]+$", RegexOptions.Compiled);
        public static readonly Regex decimalRegex = new Regex("^[0-9_]+$", RegexOptions.Compiled);
        public static readonly Regex hexadecimalRegex = new Regex("^0x[0-9a-fA-F_]+$", RegexOptions.Compiled);


        public static bool IsValidIdentifier(Word word) {
            if(word.Type != WordType.Word) return false;
            return IsValidIdentifier(word.Text);
        }

        public static bool IsValidIdentifier(string word) {
            if(word == ";") return false;

            if(word.StartsWith("#")) return false;

            if(Utils.wildcardRegex.IsMatch(word)) return false;

            if(Utils.readBytesRegex.IsMatch(word) || Utils.writeBytesRegex.IsMatch(word)) return false;
            if(Utils.cttRegex.IsMatch(word)) return false;
            if(Utils.syscallRegex.IsMatch(word)) return false;

            if(word.StartsWith("cast(") && word.EndsWith(")")) return false;

            if(Utils.binaryRegex.IsMatch(word) || Utils.octalRegex.IsMatch(word) || Utils.decimalRegex.IsMatch(word) || Utils.hexadecimalRegex.IsMatch(word)) return false;

            if(DataType.TryParse(word, out DataType _)) return false;

            foreach(string keyword in Utils.keywords) if(keyword == word) return false;

            return true;
        }

        public static string StringLiteralToByteString(string literal) {
            return String.Join(',', Encoding.ASCII.GetBytes(literal));
        }

        public static Position GetNewPosition(string text, Position position, int n) {
            int line = position.Line;
            int column = position.Column;
            for(int i = 0; i < n; i++) {
                if(text[i] == '\n') {
                    line++;
                    column = 0;
                }
            }
            return new Position(position.File, line, column);
        }

        public static string GetStringType(string text) {
            int index = 0;
            for(int i = 1; i < text.Length; i++) if(text[i] == '"' && text[i-1] != '\\') {
                index = i;
                break;
            }
            return text.Substring(index+1, text.Length-index-1);
        }

        public static string ConvertEscapeCharacters(string _text, Position position) {
            string text = "";
            for(int i = 0; i < _text.Length; i++) {
                char c = _text[i];
                if(c != '\\') text += c;
                else {
                    c = _text[++i];
                    if(c == 'n') text += '\n';
                    else if(c == 't') text += '\t';
                    else if(c == 'r') text += '\r';
                    else if(c == '\\') text += '\\';
                    else if(c == '"') text += '"';
                    else if(c == '0') text += '\0';
                    else if(c == '\n') {
                        ErrorSystem.AddError_s(new InvalidEscapeCharacterError("\\n", GetNewPosition(_text, position, i)));
                        return _text;
                    } else if(c == '\t') {
                        ErrorSystem.AddError_s(new InvalidEscapeCharacterError("\\t", GetNewPosition(_text, position, i)));
                        return _text;
                    } else if(c == '\r') {
                        ErrorSystem.AddError_s(new InvalidEscapeCharacterError("\\r", GetNewPosition(_text, position, i)));
                        return _text;
                    } else {
                        ErrorSystem.AddError_s(new InvalidEscapeCharacterError(""+c, GetNewPosition(_text, position, i)));
                        return _text;
                    }
                }
            }
            return text;
        }

        public static T[] Reverse<T>(T[] array) {
            int size = array.Length;
            T[] result = new T[size];
            for(int i = 0; i < array.Length; i++) result[i] = array[size-1 - i];
            return result;
        }

    }

    class Position {
        public Position(string file, int line, int column) {
            File = file;
            Line = line;
            Column = column;
        }

        public string File { get; }
        public int Line { get; }
        public int Column { get; }

        public Position Derive(int dLine, int dColumn) {
            return new Position(File, Line + dLine, Column + dColumn);
        }

        public override string ToString() {
            return File + ":" + Line + ":" + Column;
        }
    }
    
}