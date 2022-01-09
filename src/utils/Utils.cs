using System;
using System.Collections.Generic;
using System.Text;

namespace IonS {

    interface IAssemblyGenerator {
        string nasm_linux_x86_64();
    }

    class Scope {

        private static int nextScopeId = 0;
        private static int ScopeId() { return nextScopeId++; }

        public Scope(Scope parent, Procedure procedure) {
            Id = ScopeId();
            Parent = parent;
            Procedure = procedure;
            Variables = new Dictionary<string, Variable>();
        }
        
        public int Id { get; }
        public Scope Parent { get; }
        public Procedure Procedure { get; }
        public Dictionary<string, Variable> Variables { get; }

        public Error RegisterVariable(Variable var) {
            Variable ownVar = GetOwnVariable(var.Identifier.Text);
            if(ownVar != null) return new VariableRedeclarationError(ownVar.Identifier, var.Identifier);
            Variables.Add(Id + "_" + var.Identifier.Text, var);
            return null;
        }
        private Variable GetOwnVariable(string identifier) {
            Variables.TryGetValue(Id + "_" + identifier, out Variable variable);
            return variable;
        }
        public Variable GetVariable(string identifier) {
            Variable ownVar = GetOwnVariable(identifier);
            if(ownVar != null) return ownVar;
            if(Parent != null) return Parent.GetVariable(identifier);
            return null;
        }

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
            "swap", "2swap",
            "rot", "2rot", "rot5", "2rot5",
            "+", "-", "*", "/", "%", "/%",
            "<<", ">>", "&", "|", "^", "inv",
            "&&", "||", "not",
            "min", "max",
            "==", "!=", "<", ">", "<=", ">=",
            ".",
            "if", "else", "while", "do",
            "continue", "break",
            "macro",
            "var",
            "include",
            "here", "chere",
            "{", "}",
            "proc", "return",
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