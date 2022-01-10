using System;
using System.Text;

namespace IonS {

    interface IAssemblyGenerator {
        string nasm_linux_x86_64();
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
        
        public static readonly string[] FreeUseRegisters = new string[] {"rax", "rbx", "rcx", "rdx", "r8", "r9", "r10", "r11", "r12", "r13", "r14", "r15"};
        public static readonly string[] SyscallRegisters = new string[] {"rdi", "rsi", "rdx", "r10", "r8", "r9"};

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