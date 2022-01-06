namespace IonS {

    abstract class Error {}

    abstract class LexerError : Error {
        public override string ToString()
        {
            return "[Lexer]: ";
        }
    }

    abstract class CommentPreprocessorError : Error {
        public override string ToString()
        {
            return "[CommentPreprocessorError]: ";
        }
    }

    abstract class IncludePreprocessorError : Error {
        public override string ToString()
        {
            return "[IncludePreprocessor]: ";
        }
    }

    abstract class MacroPreprocessorError : Error {
        public override string ToString()
        {
            return "[MacroPreprocessor]: ";
        }
    }

    abstract class ParserError : Error {
        public override string ToString()
        {
            return "[Parser]: ";
        }
    }

    abstract class AssemblyTranscriberError : Error {
        public override string ToString()
        {
            return "[AssemblyTranscriber]: ";
        }
    }

    // Lexer errors
    // - Invalid string literal errors

    sealed class InvalidStringTypeError : LexerError {
        public InvalidStringTypeError(string type, Position position) {
            Type = type;
            Position = position;
        }
        public override string ToString()
        {
            return base.ToString() + "Invalid string type: '" + Type + "' at " + Position;
        }
        public string Type { get; }
        public Position Position { get; }
    }

    sealed class EOFInStringLiteralError : LexerError {
        public EOFInStringLiteralError(Position start) {
            Start = start;
        }
        public override string ToString()
        {
            return base.ToString() + "EOF inside string literal: starts at " + Start;
        }
        public Position Start { get; }
    }

    // - Invalid escape character error

    sealed class InvalidEscapeCharacterError : LexerError {
        public InvalidEscapeCharacterError(string text, Position position) {
            Text = text;
            Position = position;
        }
        public override string ToString()
        {
            return base.ToString() + "Invalid escape character: '" + Text + "' at " + Position;
        }
        public string Text { get; }
        public Position Position { get; }
    }

    // - Invalid char error

    sealed class InvalidCharError : LexerError {
        public InvalidCharError(Word word) {
            Word = word;
        }
        public override string ToString()
        {
            return base.ToString() + "Invalid char: " + Word;
        }
        public Word Word { get; }
    }

    // CommentPreprocessor errors
    // - EOF in block comment error

    sealed class EOFInBlockCommentError : CommentPreprocessorError {
        public EOFInBlockCommentError(Position start) {
            Start = start;
        }
        public override string ToString()
        {
            return base.ToString() + "EOF in block comment: starts at " + Start;
        }
        public Position Start { get; }
    }

    // IncludePreprocessor errors
    // - File not found error

    sealed class FileNotFoundError : IncludePreprocessorError {
        public FileNotFoundError(string path, string directory, Position position) {
            Path = path;
            Directory = directory;
            Position = position;
        }
        public override string ToString()
        {
            return base.ToString() + "File not found: '" + Path + "' " + (Directory != null ? "(cwd: " + Directory + ") " : "") + "at " + Position;
        }
        public string Path { get; }
        public string Directory { get; }
        public Position Position { get; }
    }

    sealed class IncompleteIncludeError : IncludePreprocessorError {
        public IncompleteIncludeError(Position position) {
            Position = position;
        }
        public override string ToString()
        {
            return base.ToString() + "Incomplete include: at " + Position;
        }
        public Position Position { get; }
    }

    sealed class FilePathNotAStringLiteralError : IncludePreprocessorError {
        public FilePathNotAStringLiteralError(Word word) {
            Word = word;
        }
        public override string ToString()
        {
            return base.ToString() + "File path not a string: " + Word;
        }
        public Word Word { get; }
    }

    // MacroPreprocessor errors
    // - Macro errors

    sealed class IncompleteMacroDefinitionError : MacroPreprocessorError {
        public IncompleteMacroDefinitionError(Word macroWord, Word key) {
            MacroWord = macroWord;
            Key = key;
        }
        public override string ToString()
        {
            return base.ToString() + "Incomplete macro: " + (Key != null ? Key : "at " + MacroWord.Position);
        }
        public Word MacroWord { get; }
        public Word Key { get; }
    }

    sealed class InvalidMacroKeyError : MacroPreprocessorError {
        public InvalidMacroKeyError(Word key) {
            Key = key;
        }
        public override string ToString()
        {
            return base.ToString() + "Invalid key for macro: " + Key;
        }
        public Word Key { get; }
    }

    sealed class MacroRedefinitionError : MacroPreprocessorError {
        public MacroRedefinitionError(Word o, Word n) {
            Old = o;
            New = n;
        }
        public override string ToString()
        {
            return base.ToString() + "Trying to redifine macro: '" + Old.Text + "' (" + Old.Position + ") at " + New.Position;
        }
        public Word Old { get; }
        public Word New { get; }
    }

    // Parser errors
    // - Unexpected word error

    sealed class UnexpectedWordError : ParserError {
        public UnexpectedWordError(Word word) {
            Word = word;
        }
        public override string ToString()
        {
            return base.ToString() + "Unexpected word: " + Word;
        }
        public Word Word { get; }
    }

    // - Block errors

    sealed class MissingDoError : ParserError {
        public MissingDoError(Position position) {
            Position = position;
        }
        public override string ToString()
        {
            return base.ToString() + "Missing do marker: at " + Position;
        }
        public Position Position { get; }
    }

    sealed class MissingWhileError : ParserError {
        public MissingWhileError(Position position) {
            Position = position;
        }
        public override string ToString()
        {
            return base.ToString() + "Missing while marker: at " + Position;
        }
        public Position Position { get; }
    }

    sealed class EOFInCodeBlockError : ParserError {
        public EOFInCodeBlockError() {}
        public override string ToString()
        {
            return base.ToString() + "EOF inside of a CodeBlock";
        }
    }

    // - Variable errors

    sealed class IncompleteVariableDeclarationError : ParserError {
        public IncompleteVariableDeclarationError(Word varWord, Word identifier) {
            VarWord = varWord;
            Identifier = identifier;
        }
        public override string ToString()
        {
            return base.ToString() + "Incomplete variable: " + (Identifier != null ? Identifier : "at " + VarWord.Position);
        }
        public Word VarWord { get; }
        public Word Identifier { get; }
    }

    sealed class VariableRedeclarationError : ParserError {
        public VariableRedeclarationError(Word o, Word n) {
            Old = o;
            New = n;
        }
        public override string ToString()
        {
            return base.ToString() + "Trying to redeclare variable: '" + Old.Text + "' (" + Old.Position + ") at " + New.Position;
        }
        public Word Old { get; }
        public Word New { get; }
    }

    sealed class InvalidVariableIdentifierError : ParserError {
        public InvalidVariableIdentifierError(Word identifier) {
            Identifier = identifier;
        }
        public override string ToString()
        {
            return base.ToString() + "Invalid identifier for variable: " + Identifier;
        }
        public Word Identifier { get; }
    }

    sealed class InvalidVariableBytesizeError : ParserError {
        public InvalidVariableBytesizeError(Word bytesize) {
            Bytesize = bytesize;
        }
        public override string ToString()
        {
            return base.ToString() + "Invalid bytesize for variable: " + Bytesize;
        }
        public Word Bytesize { get; }
    }

    // - Mem read/Write errors

    sealed class InvalidMemReadWriteAmountError : ParserError {
        public InvalidMemReadWriteAmountError(string amount, Position position) {
            Amount = amount;
            Position = position;
        }
        public override string ToString()
        {
            return base.ToString() + "Invalid amount for memory read/write: '" + Amount + "' at " + Position;
        }
        public string Amount { get; }
        public Position Position { get; }
    }

    // - Invalid syscall argc error

    sealed class InvalidSyscallArgcError : ParserError {
        public InvalidSyscallArgcError(string argc, Position position) {
            Argc = argc;
            Position = position;
        }
        public override string ToString()
        {
            return base.ToString() + "Invalid argument count for syscall: '" + Argc + "' at " + Position;
        }
        public string Argc { get; }
        public Position Position { get; }
    }

    // - Invalid break/continue error

    sealed class InvalidContinueError : ParserError {
        public InvalidContinueError(Position position) {
            Position = position;
        }
        public override string ToString()
        {
            return base.ToString() + "'continue' outside a loop or switch: at " + Position;
        }
        public Position Position { get; }
    }

    sealed class InvalidBreakError : ParserError {
        public InvalidBreakError(Position position) {
            Position = position;
        }
        public override string ToString()
        {
            return base.ToString() + "'break' outside a loop or switch: at " + Position;
        }
        public Position Position { get; }
    }

}