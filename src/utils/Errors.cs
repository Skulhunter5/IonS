namespace IonS {

    abstract class Error {}

    abstract class LexerError : Error {
        public override string ToString()
        {
            return "[Lexer]: ";
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

    abstract class SimulatorError : Error {
        public override string ToString()
        {
            return "[Simulator]: ";
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
    // - Internal error
    
    sealed class InternalParserError : ParserError {
        public override string ToString()
        {
            return base.ToString() + "Internal Error";
        }
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

    sealed class UnexpectedMarkerError : ParserError {
        public UnexpectedMarkerError(Word word) {
            Word = word;
        }
        public override string ToString()
        {
            return base.ToString() + "Unexpected marker: " + Word;
        }
        public Word Word { get; }
    }

    sealed class IncompleteBlockError : ParserError {
        public IncompleteBlockError(Block block) {
            Block = block;
        }
        public override string ToString()
        {
            if(Block.GetType() == typeof(IfBlock)) return base.ToString() + "Incomplete block: 'if' at " + Block.Position + " is missing: 'end'";
            else if(Block.GetType() == typeof(WhileBlock)) return base.ToString() + "Incomplete block: 'while' at " + Block.Position + " is missing: '" + (((WhileBlock) Block).HasDo ? "end" : "do") + "'";
            else if(Block.GetType() == typeof(DoWhileBlock)) return base.ToString() + "Incomplete block: 'do-while' at " + Block.Position + " is missing: '" + (((DoWhileBlock) Block).HasWhile ? "end" : "while") + "'";
            else return base.ToString() + "Incomplete block: '" + Block.GetType() + "' at " + Block.Position;
        }
        public Block Block { get; }
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

    // Simulator errors
    // - Stack errors

    sealed class StackUnderflowError : SimulatorError {
        public StackUnderflowError(string text) {
            Text = text;
        }
        public override string ToString()
        {
            return base.ToString() + "Stack underflow: '" + Text + "'";
        }
        public string Text { get; }
    }

    sealed class StackOverflowError : SimulatorError {
        public StackOverflowError(string text) {
            Text = text;
        }
        public override string ToString()
        {
            return base.ToString() + "Stack overflow: '" + Text + "'";
        }
        public string Text { get; }
    }

    // - Divide by zero error

    sealed class DivideByZeroError : SimulatorError {
        public DivideByZeroError(string text) {
            Text = text;
        }
        public override string ToString()
        {
            return base.ToString() + "Division by 0: '" + Text + "'";
        }
        public string Text { get; }
    }

    // Unimplemented operation error

    sealed class UnimplementedOperationSimulatorError : SimulatorError {
        public UnimplementedOperationSimulatorError(OperationType type) {
            Type = type;
        }
        public override string ToString()
        {
            return base.ToString() + "Unimplemented operation: '" + Type + "'";
        }
        public OperationType Type { get; }
    }

    // AssemblyTranscriber errors
    // - Unimplemented operation error
    
    sealed class UnimplementedOperationAssemblyTranscriberError : AssemblyTranscriberError {
        public UnimplementedOperationAssemblyTranscriberError(OperationType type) {
            Type = type;
        }
        public override string ToString()
        {
            return base.ToString() + "Unimplemented operation: '" + Type + "'";
        }
        public OperationType Type { get; }
    }

}