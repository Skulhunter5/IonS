using System;
using System.Text;
using System.Text.RegularExpressions;

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
    
    class ConvertEscapeCharactersResult : Result {
        public ConvertEscapeCharactersResult(string text, Error error) : base(error) {
            Text = text;
        }
        public string Text { get; }
    }

    class Utils {
        
        public static readonly string[] SyscallRegisters = new string[] {"rdi", "rsi", "rdx", "r10", "r8", "r9"};

        public static readonly Regex symbolRegex = new Regex("^[a-zA-Z_][0-9a-zA-Z_]*$", RegexOptions.Compiled);

        public static readonly Regex wildcardRegex = new Regex("^_+$", RegexOptions.Compiled);

        public static readonly Regex readBytesRegex = new Regex("^@[0-9]+$", RegexOptions.Compiled);
        public static readonly Regex writeBytesRegex = new Regex("^![0-9]+$", RegexOptions.Compiled);

        public static readonly Regex binaryRegex = new Regex("^0b[01]+$", RegexOptions.Compiled);
        public static readonly Regex octalRegex = new Regex("^0[0-7]+$", RegexOptions.Compiled);
        public static readonly Regex decimalRegex = new Regex("^[0-9]+$", RegexOptions.Compiled);
        public static readonly Regex hexadecimalRegex = new Regex("^0x[0-9a-fA-F]+$", RegexOptions.Compiled);


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

        public static ConvertEscapeCharactersResult ConvertEscapeCharacters(string _text, Position position) {
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
                    else if(c == '\n') return new ConvertEscapeCharactersResult(null, new InvalidEscapeCharacterError("\\n", GetNewPosition(_text, position, i)));
                    else if(c == '\t') return new ConvertEscapeCharactersResult(null, new InvalidEscapeCharacterError("\\t", GetNewPosition(_text, position, i)));
                    else if(c == '\r') return new ConvertEscapeCharactersResult(null, new InvalidEscapeCharacterError("\\r", GetNewPosition(_text, position, i)));
                    else return new ConvertEscapeCharactersResult(null, new InvalidEscapeCharacterError(""+c, GetNewPosition(_text, position, i)));
                }
            }
            return new ConvertEscapeCharactersResult(text, null);
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
        public override string ToString()
        {
            return File + ":" + Line + ":" + Column;
        }
        public string File { get; }
        public int Line { get; }
        public int Column { get; }
    }
    
}