using System;
using System.Collections.Generic;
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
    
    class Utils {
        
        public static readonly string[] FreeUseRegisters = new string[] {"rax", "rbx", "rcx", "rdx", "r8", "r9", "r10", "r11", "r12", "r13", "r14", "r15"};
        public static readonly string[] SyscallRegisters = new string[] {"rdi", "rsi", "rdx", "r10", "r8", "r9"};

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
    
}