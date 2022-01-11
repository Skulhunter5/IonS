namespace IonS {

    abstract class Error {}

    abstract class LexerError : Error {
        public override string ToString() {
            return "[Lexer]: ";
        }
    }

    abstract class CommentPreprocessorError : Error {
        public override string ToString() {
            return "[CommentPreprocessorError]: ";
        }
    }

    abstract class IncludePreprocessorError : Error {
        public override string ToString() {
            return "[IncludePreprocessor]: ";
        }
    }

    abstract class MacroPreprocessorError : Error {
        public override string ToString() {
            return "[MacroPreprocessor]: ";
        }
    }

    abstract class ParserError : Error {
        public override string ToString() {
            return "[Parser]: ";
        }
    }

    abstract class TypeCheckerError : Error {
        public override string ToString() {
            return "[TypeChecker]: ";
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
    // - Internal parser error

    sealed class InternalParserError : ParserError {
        public InternalParserError(string message) {
            Message = message;
        }
        public override string ToString()
        {
            return base.ToString() + "Internal error: " + Message;
        }
        public string Message { get; }
    }

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

    sealed class MissingIfError : ParserError {
        public MissingIfError(Position position) {
            Position = position;
        }
        public override string ToString()
        {
            return base.ToString() + "Missing if: at " + Position;
        }
        public Position Position { get; }
    }

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
        
        public Position Position { get; }
        
        public override string ToString() {
            return base.ToString() + "Missing while marker: at " + Position;
        }
    }

    sealed class MissingCodeBlockError : ParserError {
        public MissingCodeBlockError() {}

        public override string ToString() {
            return base.ToString() + "Missing CodeBlock at the end of the file";
        }
    }

    sealed class EOFInCodeBlockError : ParserError {
        public EOFInCodeBlockError(Position position) {
            Position = position;
        }

        public Position Position { get; }

        public override string ToString() {
            return base.ToString() + "EOF inside of a CodeBlock: starting at " + Position;
        }
    }

    // - Variable errors

    sealed class IncompleteVariableDeclarationError : ParserError {
        public IncompleteVariableDeclarationError(Word varWord, Word identifier) {
            VarWord = varWord;
            Identifier = identifier;
        }
        
        public Word VarWord { get; }
        public Word Identifier { get; }
        
        public override string ToString() {
            return base.ToString() + "Incomplete variable: " + (Identifier != null ? Identifier : "at " + VarWord.Position);
        }
    }

    sealed class VariableRedeclarationError : ParserError {
        public VariableRedeclarationError(Word o, Word n) {
            Old = o;
            New = n;
        }
        
        public Word Old { get; }
        public Word New { get; }
        
        public override string ToString() {
            return base.ToString() + "Trying to redeclare variable: '" + Old.Text + "' (" + Old.Position + ") at " + New.Position;
        }
    }

    sealed class InvalidVariableIdentifierError : ParserError {
        public InvalidVariableIdentifierError(Word identifier) {
            Identifier = identifier;
        }
        
        public Word Identifier { get; }
        
        public override string ToString() {
            return base.ToString() + "Invalid identifier for variable: " + Identifier;
        }
    }

    sealed class InvalidVariableBytesizeError : ParserError {
        public InvalidVariableBytesizeError(Word bytesize) {
            Bytesize = bytesize;
        }
        
        public Word Bytesize { get; }
        
        public override string ToString() {
            return base.ToString() + "Invalid bytesize for variable: " + Bytesize;
        }
    }

    // - Procedure errors

    sealed class ProcedureNotInGlobalScopeError : ParserError {
        public ProcedureNotInGlobalScopeError(Position position) {
            Position = position;
        }
        
        public Position Position { get; }
        
        public override string ToString() {
            return base.ToString() + "Procedure inside a local scope: at " + Position;
        }
    }

    sealed class InvalidProcedureNameError : ParserError {
        public InvalidProcedureNameError(Word name) {
            Name = name;
        }
        
        public Word Name { get; }
        
        public override string ToString() {
            return base.ToString() + "Invalid name for procedure: " + Name;
        }
    }

    sealed class ProcedureRedefinitionError : ParserError {
        public ProcedureRedefinitionError(Word o, Word n) {
            Old = o;
            New = n;
        }
        
        public Word Old { get; }
        public Word New { get; }
        
        public override string ToString() {
            return base.ToString() + "Trying to redefine procedure: '" + Old.Text + "' (" + Old.Position + ") at " + New.Position;
        }
    }

    sealed class IncompleteProcedureError : ParserError {
        public IncompleteProcedureError(Word procWord, Word name) {
            ProcWord = procWord;
            Name = name;
        }
        
        public Word ProcWord { get; }
        public Word Name { get; }
        
        public override string ToString() {
            return base.ToString() + "Incomplete Procedure: " + (Name != null ? Name : "at " + ProcWord.Position);
        }
    }

    sealed class InvalidProcedureArgsError : ParserError {
        public InvalidProcedureArgsError(Word args) {
            Args = args;
        }
        
        public Word Args { get; }
        
        public override string ToString() {
            return base.ToString() + "Invalid args for procedure: " + Args;
        }
    }

    sealed class MissingProcAfterInlineError : ParserError {
        public MissingProcAfterInlineError(Word procWord) {
            ProcWord = procWord;
        }
        
        public Word ProcWord { get; }
        
        public override string ToString() {
            return base.ToString() + "Expecting 'proc' after 'inline': got " + ProcWord;
        }
    }

    // - Return outside procedure error

    sealed class ReturnOutsideProcedureError : ParserError {
        public ReturnOutsideProcedureError(Position position) {
            Position = position;
        }
        
        public Position Position { get; }
        
        public override string ToString() {
            return base.ToString() + "Return ouside of a procedure: at " + Position;
        }
    }

    // - Mem read/Write errors

    sealed class InvalidMemReadWriteAmountError : ParserError {
        public InvalidMemReadWriteAmountError(string amount, Position position) {
            Amount = amount;
            Position = position;
        }
        
        public string Amount { get; }
        public Position Position { get; }
        
        public override string ToString() {
            return base.ToString() + "Invalid amount for memory read/write: '" + Amount + "' at " + Position;
        }
    }

    // - Invalid syscall argc error

    sealed class InvalidSyscallArgcError : ParserError {
        public InvalidSyscallArgcError(string argc, Position position) {
            Argc = argc;
            Position = position;
        }
        
        public Position Position { get; }
        public string Argc { get; }
        
        public override string ToString() {
            return base.ToString() + "Invalid argument count for syscall: '" + Argc + "' at " + Position;
        }
    }

    // - Invalid break/continue error

    sealed class InvalidContinueError : ParserError {
        public InvalidContinueError(Position position) {
            Position = position;
        }
        
        public Position Position { get; }
        
        public override string ToString() {
            return base.ToString() + "'continue' outside a loop or switch: at " + Position;
        }
    }

    sealed class InvalidBreakError : ParserError {
        public InvalidBreakError(Position position) {
            Position = position;
        }
        
        public Position Position { get; }
        
        public override string ToString() {
            return base.ToString() + "'break' outside a loop or switch: at " + Position;
        }
    }

    // - Invalid ctt index error

    sealed class InvalidCTTIndexError : ParserError {
        public InvalidCTTIndexError(Word index) {
            Index = index;
        }

        public Word Index { get; }

        public override string ToString() {
            return base.ToString() + "Invalid ctt index: " + Index;
        }
    }

    // TypeChecker errors
    // - Invalid stack after block errors

    sealed class MissingDataAfterBlockError : TypeCheckerError {
        public MissingDataAfterBlockError(DataType[] missing) {
            Missing = missing;
        }

        public DataType[] Missing { get; }

        public override string ToString() {
            return base.ToString() + "Missing elements on the stack after Block: " + Missing;
        }
    }

    sealed class ExcessDataAfterBlockError : TypeCheckerError {
        public ExcessDataAfterBlockError(DataType[] excess) {
            Excess = excess;
        }

        public DataType[] Excess { get; }

        public override string ToString() {
            return base.ToString() + "Excess elements on the stack after Block: " + Excess;
        }
    }

}