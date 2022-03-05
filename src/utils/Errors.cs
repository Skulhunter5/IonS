using System;
using System.Collections.Generic;

namespace IonS {

    abstract class Error {}

    abstract class GeneralError : Error {
        public override string ToString() {
            return "[General]: ";
        }
    }

    abstract class LexerError : Error {
        public override string ToString() {
            return "[Lexer]: ";
        }
    }

    abstract class PreprocessorError : Error {
        public override string ToString() {
            return "[Preprocessor]: ";
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

    // General errors
    // - Invalid DataType error

    sealed class InvalidDataTypeError : GeneralError {
        public InvalidDataTypeError(Word dataType) {
            DataType = dataType;
        }

        public Word DataType { get; }

        public override string ToString() {
            return base.ToString() + "Invalid DataType: " + DataType;
        }
    }

    // - Invalid type error

    sealed class InvalidTypeError : GeneralError {
        public InvalidTypeError(Word type) {
            Type = type;
        }

        public Word Type { get; }

        public override string ToString() {
            return base.ToString() + "Invalid type: " + Type;
        }
    }

    // Lexer errors
    // - Invalid string literal errors

    sealed class InvalidStringTypeError : LexerError {
        public InvalidStringTypeError(string type, Position position) {
            Type = type;
            Position = position;
        }
        
        public string Type { get; }
        public Position Position { get; }
        
        public override string ToString() {
            return base.ToString() + "Invalid string type: '" + Type + "' at " + Position;
        }
    }

    sealed class EOFInStringLiteralError : LexerError {
        public EOFInStringLiteralError(Position start) {
            Start = start;
        }
        
        public Position Start { get; }

        public override string ToString() {
            return base.ToString() + "EOF inside string literal: starts at " + Start;
        }
    }

    // - Invalid escape character error

    sealed class InvalidEscapeCharacterError : LexerError {
        public InvalidEscapeCharacterError(string text, Position position) {
            Text = text;
            Position = position;
        }
        
        public string Text { get; }
        public Position Position { get; }
        
        public override string ToString() {
            return base.ToString() + "Invalid escape character: '" + Text + "' at " + Position;
        }
    }

    // - Invalid char error

    sealed class InvalidCharError : LexerError {
        public InvalidCharError(Word word) {
            Word = word;
        }
        
        public Word Word { get; }
        
        public override string ToString() {
            return base.ToString() + "Invalid char: " + Word;
        }
    }

    // Preprocessor errors
    // - Unknown preprocessor directive error

    sealed class UnknownPreprocessorDirectiveError : PreprocessorError {
        public UnknownPreprocessorDirectiveError(Word word) {
            Word = word;
        }
        
        public Word Word { get; }
        
        public override string ToString() {
            return base.ToString() + "Unknown preprocessor directive: " + Word;
        }
    }

    // - Unexpected preprocessor directive error

    sealed class UnexpectedPreprocessorDirectiveError : PreprocessorError {
        public UnexpectedPreprocessorDirectiveError(Word word) {
            Word = word;
        }
        
        public Word Word { get; }
        
        public override string ToString() {
            return base.ToString() + "Unexpected preprocessor directive: " + Word;
        }
    }

    // - Incomplete preprocessor directive error

    sealed class IncompletePreprocessorDirectiveError : PreprocessorError {
        public IncompletePreprocessorDirectiveError(Word word) {
            Word = word;
        }
        
        public Word Word { get; }
        
        public override string ToString() {
            return base.ToString() + "Incomplete preprocessor directive: " + Word;
        }
    }

    // - Missing preprocessor directive error

    sealed class MissingPreprocessorDirectiveError : PreprocessorError {
        public MissingPreprocessorDirectiveError(string missing, Word word) {
            Missing = missing;
            Word = word;
        }
        
        public string Missing { get; }
        public Word Word { get; }
        
        public override string ToString() {
            return base.ToString() + "Missing preprocessor directive: '" + Missing + "' for " + Word;
        }
    }
    
    // - Symbol redefinition error

    sealed class PreprocessorSymbolRedefinitionError : PreprocessorError {
        public PreprocessorSymbolRedefinitionError(Word word, Position originalPosition) {
            Word = word;
            OriginalPosition = originalPosition;
        }
        
        public Word Word { get; }
        public Position OriginalPosition { get; }
        
        public override string ToString() {
            return base.ToString() + "Trying to redefine preprocessor symbol: '" + Word.Text + "' (" + OriginalPosition + ") at " + Word.Position;
        }
    }
    
    // - Symbol redefinition error

    sealed class UnclosedPreprocessorDirectivesError : PreprocessorError {
        public UnclosedPreprocessorDirectivesError(Stack<Word> openDirectives) {
            OpenDirectives = openDirectives;
        }
        
        public Stack<Word> OpenDirectives { get; }
        
        public override string ToString() {
            string msg = "";
            foreach(Word directive in OpenDirectives) msg += "\n- " + directive + "";
            return base.ToString() + "Unclosed preprocessor directives:" + msg;
        }
    }
    
    // - Invalid symbol error

    sealed class InvalidSymbolError : PreprocessorError {
        public InvalidSymbolError(Word symbol, bool isEnvironmentSymbol) {
            Symbol = symbol;
            IsEnvironmentSymbol = isEnvironmentSymbol;
        }
        
        public Word Symbol { get; }
        public bool IsEnvironmentSymbol { get; }
        
        public override string ToString() {
            if(IsEnvironmentSymbol) return base.ToString() + "Invalid symbol: environment symbol: " + Symbol.Text;
            return base.ToString() + "Invalid symbol: " + Symbol;
        }
    }

    // - EOF in block comment error

    sealed class EOFInBlockCommentError : PreprocessorError {
        public EOFInBlockCommentError(Position start) {
            Start = start;
        }
        
        public Position Start { get; }
        
        public override string ToString() {
            return base.ToString() + "EOF in block comment: starts at " + Start;
        }
    }

    // - File not found error

    sealed class FileNotFoundError : PreprocessorError {
        public FileNotFoundError(string path, string directory, Position position) {
            Path = path;
            Directory = directory;
            Position = position;
        }
        
        public string Path { get; }
        public string Directory { get; }
        public Position Position { get; }
        
        public override string ToString() {
            return base.ToString() + "File not found: '" + Path + "' " + (Directory != null ? "(cwd: " + Directory + ") " : "") + "at " + Position;
        }
    }

    // - Incomplete include error

    sealed class IncompleteIncludeError : PreprocessorError {
        public IncompleteIncludeError(Position position) {
            Position = position;
        }
        
        public Position Position { get; }
        
        public override string ToString() {
            return base.ToString() + "Incomplete include: at " + Position;
        }
    }

    // - File path not a string literal error

    sealed class FilePathNotAStringLiteralError : PreprocessorError {
        public FilePathNotAStringLiteralError(Word word) {
            Word = word;
        }
        
        public Word Word { get; }
        
        public override string ToString() {
            return base.ToString() + "File path not a string: " + Word;
        }
    }

    // - Macro errors

    sealed class IncompleteMacroDefinitionError : PreprocessorError {
        public IncompleteMacroDefinitionError(Word macroWord, Word key) {
            MacroWord = macroWord;
            Key = key;
        }
        
        public Word MacroWord { get; }
        public Word Key { get; }
        
        public override string ToString() {
            return base.ToString() + "Incomplete macro: " + (Key != null ? Key : "at " + MacroWord.Position);
        }
    }

    sealed class InvalidMacroKeyError : PreprocessorError {
        public InvalidMacroKeyError(Word key) {
            Key = key;
        }
        
        public Word Key { get; }
        
        public override string ToString() {
            return base.ToString() + "Invalid key for macro: " + Key;
        }
    }

    sealed class MacroRedefinitionError : PreprocessorError {
        public MacroRedefinitionError(Word o, Word n) {
            Old = o;
            New = n;
        }
        
        public Word Old { get; }
        public Word New { get; }
        
        public override string ToString() {
            return base.ToString() + "Trying to redifine macro: '" + Old.Text + "' (" + Old.Position + ") at " + New.Position;
        }
    }

    // Parser errors
    // - Internal parser error

    sealed class InternalParserError : ParserError {
        public InternalParserError(string message) {
            Message = message;
        }
        
        public string Message { get; }
        
        public override string ToString() {
            return base.ToString() + "Internal error: " + Message;
        }
    }

    // - Unexpected word error

    sealed class UnexpectedWordError : ParserError {
        public UnexpectedWordError(Word word) {
            Word = word;
        }
        
        public Word Word { get; }
        
        public override string ToString() {
            return base.ToString() + "Unexpected word: " + Word;
        }
    }

    // - Incomplete assert error

    sealed class IncompleteAssertError : ParserError {
        public IncompleteAssertError(Position position) {
            Position = position;
        }
        
        public Position Position { get; }
        
        public override string ToString() {
            return base.ToString() + "Incomplete assert: at " + Position;
        }
    }

    // - Missing opening brace error

    sealed class MissingOpeningBraceError : ParserError {
        public MissingOpeningBraceError(Operation operation, Position position) {
            Operation = operation;
            Position = position;
        }
        
        public Operation Operation { get; }
        public Position Position { get; }
        
        public override string ToString() {
            return base.ToString() + "Missing opening brace: at " + Position + " for " + Operation;
        }
    }

    // - Missing closing brace error

    sealed class MissingClosingBraceError : ParserError {
        public MissingClosingBraceError(Operation operation, Position position) {
            Operation = operation;
            Position = position;
        }
        
        public Operation Operation { get; }
        public Position Position { get; }
        
        public override string ToString() {
            return base.ToString() + "Missing closing brace: at " + Position + " for " + Operation;
        }
    }

    // - Incomplete multicast error

    sealed class IncompleteMultiCastOperation : ParserError {
        public IncompleteMultiCastOperation(Position position) {
            Position = position;
        }
        
        public Position Position { get; }
        
        public override string ToString() {
            return base.ToString() + "Incomplete multicast: at " + Position;
        }
    }

    // - Incomplete multicast error

    sealed class InvalidMultiCastError : ParserError {
        public InvalidMultiCastError(Position position) {
            Position = position;
        }
        
        public Position Position { get; }
        
        public override string ToString() {
            return base.ToString() + "Invalid multicast: at " + Position;
        }
    }

    // - Block errors

    sealed class MissingIfError : ParserError {
        public MissingIfError(Position position) {
            Position = position;
        }
        
        public Position Position { get; }
        
        public override string ToString() {
            return base.ToString() + "Missing if: at " + Position;
        }
    }

    sealed class MissingDoError : ParserError {
        public MissingDoError(Position position) {
            Position = position;
        }
        
        public Position Position { get; }
        
        public override string ToString() {
            return base.ToString() + "Missing do marker: at " + Position;
        }
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

    sealed class InvalidVariableByteSizeError : ParserError {
        public InvalidVariableByteSizeError(Word byteSize) {
            ByteSize = byteSize;
        }
        
        public Word ByteSize { get; }
        
        public override string ToString() {
            return base.ToString() + "Invalid bytesize for variable: " + ByteSize;
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

    sealed class InvalidProcedureParametersError : ParserError {
        public InvalidProcedureParametersError(Word name) {
            Name = name;
        }
        public InvalidProcedureParametersError(Word name, Word wrong) {
            Name = name;
            Wrong = wrong;
        }
        
        public Word Name { get; }
        public Word Wrong { get; }
        
        public override string ToString() {
            return base.ToString() + "Invalid parameters for procedure: " + (Wrong != null ? Wrong + " for " + Name : Name);
        }
    }

    sealed class EOFInProcedureParametersError : ParserError {
        public EOFInProcedureParametersError(Word nameWord) {
            NameWord = nameWord;
        }
        
        public Word NameWord { get; }
        
        public override string ToString() {
            return base.ToString() + "EOF while parsing parameters for procedure: " + NameWord;
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

    // - Incomplete binding error

    sealed class IncompleteBindingError : ParserError {
        public IncompleteBindingError(Word bindWord) {
            BindWord = bindWord;
        }
        
        public Word BindWord { get; }
        
        public override string ToString() {
            return base.ToString() + "Incomplete binding: " + BindWord;
        }
    }

    // - Incomplete binding error

    sealed class InvalidBindingError : ParserError {
        public InvalidBindingError(Word bind) {
            Bind = bind;
        }
        
        public Word Bind { get; }
        
        public override string ToString() {
            return base.ToString() + "Invalid binding: " + Bind;
        }
    }

    // - Invalid binding list error

    sealed class InvalidBindingListError : ParserError {
        public InvalidBindingListError(Word bindWord) {
            BindWord = bindWord;
        }
        
        public Word BindWord { get; }
        
        public override string ToString() {
            return base.ToString() + "Invalid binding list for binding: " + BindWord;
        }
    }

    // - EOF in binding list error

    sealed class EOFInBindingListError : ParserError {
        public EOFInBindingListError(Word bindWord) {
            BindWord = bindWord;
        }
        
        public Word BindWord { get; }
        
        public override string ToString() {
            return base.ToString() + "EOF while parsing bindings list: " + BindWord;
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

    // - Incomplete struct definition error

    sealed class IncompleteStructDefinitionError : ParserError {
        public IncompleteStructDefinitionError(Word structWord) {
            StructWord = structWord;
        }

        public Word StructWord { get; }

        public override string ToString() {
            return base.ToString() + "Incomplete struct definition error: " + StructWord;
        }
    }

    // - Missing colon in struct definition error

    sealed class MissingColonInStructDefinitionError : ParserError {
        public MissingColonInStructDefinitionError(Position position) {
            Position = position;
        }

        public Position Position { get; }

        public override string ToString() {
            return base.ToString() + "Missing colon in struct definition: " + Position;
        }
    }

    // - Invalid identifier error

    sealed class InvalidIdentifierError : ParserError {
        public InvalidIdentifierError(Word word) {
            Word = word;
        }

        public Word Word { get; }

        public override string ToString() {
            return base.ToString() + "Invalid identifier: " + Word;
        }
    }

    // StructFieldRedefinitionError

    sealed class StructFieldRedefinitionError : ParserError {
        public StructFieldRedefinitionError(Word word, Position originalPosition) {
            Word = word;
            OriginalPosition = originalPosition;
        }
        
        public Word Word { get; }
        public Position OriginalPosition { get; }
        
        public override string ToString() {
            return base.ToString() + "Redefining struct field: '" + Word.Text + "' (" + OriginalPosition + ") at " + Word.Position;
        }
    }

    // TypeChecker errors

    sealed class StackUnderflowError : TypeCheckerError {
        public StackUnderflowError(Operation operation) {
            Operation = operation;
        }

        public Operation Operation { get; }

        public override string ToString() {
            return base.ToString() + "Stack underflow: during " + Operation;
        }
    }

    sealed class UnexpectedDataTypeError : TypeCheckerError {
        public UnexpectedDataTypeError(DataType gotType, DataType expectedType, Operation operation) {
            GotType = gotType;
            ExpectedType = expectedType;
            ExpectedString = null;
            Operation = operation;
        }
        public UnexpectedDataTypeError(DataType gotType, string expectedString, Operation operation) {
            GotType = gotType;
            ExpectedString = expectedString;
            Operation = operation;
        }

        public DataType GotType { get; }
        public DataType ExpectedType { get; }
        public string ExpectedString { get; }
        public Operation Operation { get; }

        public override string ToString() {
            return base.ToString() + "Unexpected DataType on stack: expected " + (ExpectedString != null ? ExpectedString : ExpectedType) + " but got " + GotType + " during " + Operation;
        }
    }

    sealed class InvalidReturnDataError : TypeCheckerError {
        public InvalidReturnDataError(DataType[] gotTypes, Procedure procedure) {
            GotTypes = gotTypes;
            Procedure = procedure;
        }
        public InvalidReturnDataError(DataType[] gotTypes, Procedure procedure, ReturnOperation operation) {
            GotTypes = gotTypes;
            Procedure = procedure;
            Operation = operation;
        }

        public DataType[] GotTypes { get; }
        public Procedure Procedure { get; }
        public ReturnOperation Operation { get; }

        public override string ToString() {
            string msg = "";
            for(int i = 0; i < Procedure.Rets.Length; i++) msg += Procedure.Rets[i] + (i < Procedure.Rets.Length - 1 ? ", " : "");
            msg += "] but got [";
            for(int i = 0; i < GotTypes.Length; i++) msg += GotTypes[i] + (i < GotTypes.Length - 1 ? ", " : "");
            return base.ToString() + "Invalid return arguments: expected [" + msg + "] "  + (Operation != null ? "for return at " + Operation.Position + " in " : "at the end of ") + Procedure;
        }
    }

    sealed class NonMatchingSignaturesError : TypeCheckerError {
        public NonMatchingSignaturesError(List<TypeCheckContract> contracts, Block block) {
            Contracts = contracts;
            Block = block;
        }

        public List<TypeCheckContract> Contracts { get; }
        public Block Block { get; }

        public override string ToString() { // TODO: make this error message easier to read
            string[] strs = new string[Contracts.Count];
            for(int i = 0; i < strs.Length; i++) strs[i] = "[" + String.Join(", ", Contracts[i].Stack) + "]";
            return base.ToString() + "Not all code paths have the same signature: " + Block + "\n- " + String.Join("\n- ", strs) + "\n";
        }
    }

    sealed class SignatureMustBeNoneError : TypeCheckerError {
        public SignatureMustBeNoneError(TypeCheckContract reference, TypeCheckContract actual, CodeBlock block) {
            Reference = reference;
            Actual = actual;
            Block = block;
        }

        public TypeCheckContract Reference { get; }
        public TypeCheckContract Actual { get; }
        public CodeBlock Block { get; }

        public override string ToString() { // TODO: make this error message easier to read
            return base.ToString() + "Signature of CodeBlock must be none: expected [" + String.Join(", ", Reference.Stack) + "] but got [" + String.Join(", ", Actual.Stack) + "] after " + Block;
        }
    }

    sealed class SignatureMustBeNoneBeforeBreakActionError : TypeCheckerError {
        public SignatureMustBeNoneBeforeBreakActionError(TypeCheckContract reference, TypeCheckContract actual, BreakOperation breakOperation) {
            Reference = reference;
            Actual = actual;
            BreakOperation = breakOperation;
        }
        public SignatureMustBeNoneBeforeBreakActionError(TypeCheckContract reference, TypeCheckContract actual, ContinueOperation continueOperation) {
            Reference = reference;
            Actual = actual;
            ContinueOperation = continueOperation;
        }

        public TypeCheckContract Reference { get; }
        public TypeCheckContract Actual { get; }
        public BreakOperation BreakOperation { get; }
        public ContinueOperation ContinueOperation { get; }

        public override string ToString() { // TODO: make this error message easier to read
            return base.ToString() + "Signature before break action must be none: expected [" + String.Join(", ", Reference.Stack) + "] but got [" + String.Join(", ", Actual.Stack) + "] before " + (BreakOperation != null ? BreakOperation : ContinueOperation);
        }
    }

    // Unknown procedure overload error

    sealed class UnknownProcedureOverloadError : TypeCheckerError { // TODO: add stack view or to make debugging easier
        public UnknownProcedureOverloadError(Word name) {
            Name = name;
        }

        public Word Name { get; }

        public override string ToString() {
            return base.ToString() + "Unknown procedure overload for: " + Name;
        }
    }

    // Expected DataType error

    sealed class ExpectedDataTypeError : TypeCheckerError {
        public ExpectedDataTypeError(string msg, Operation operation) {
            Msg = msg;
            Operation = operation;
        }

        public string Msg { get; }
        public Operation Operation { get; }

        public override string ToString() {
            return base.ToString() + "Expected " + Msg + " for " + Operation;
        }
    }

}