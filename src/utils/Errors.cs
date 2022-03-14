using System;
using System.Collections.Generic;

namespace IonS {

    abstract class Error {}

    abstract class GeneralError : Error {
        public override string ToString() {
            return "[General] ERROR: ";
        }
    }

    abstract class LexerError : Error {
        public override string ToString() {
            return "[Lexer] ERROR: ";
        }
    }

    abstract class PreprocessorError : Error {
        public override string ToString() {
            return "[Preprocessor] ERROR: ";
        }
    }

    abstract class ParserError : Error {
        public override string ToString() {
            return "[Parser] ERROR: ";
        }
    }

    abstract class TypeCheckerError : Error {
        public override string ToString() {
            return "[TypeChecker] ERROR: ";
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

    // - Variable errors

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

    sealed class InvalidVariableByteSizeError : ParserError {
        public InvalidVariableByteSizeError(Word byteSize) {
            ByteSize = byteSize;
        }
        
        public Word ByteSize { get; }
        
        public override string ToString() {
            return base.ToString() + "Invalid bytesize for variable: " + ByteSize;
        }
    }

    // - Function errors

    sealed class FunctionNotInGlobalScopeError : ParserError {
        public FunctionNotInGlobalScopeError(Position position) {
            Position = position;
        }
        
        public Position Position { get; }
        
        public override string ToString() {
            return base.ToString() + "Function inside a local scope: at " + Position;
        }
    }

    sealed class FunctionRedefinitionError : ParserError {
        public FunctionRedefinitionError(Word o, Word n) {
            Old = o;
            New = n;
        }
        
        public Word Old { get; }
        public Word New { get; }
        
        public override string ToString() {
            return base.ToString() + "Trying to redefine Function: '" + Old.Text + "' (" + Old.Position + ") at " + New.Position;
        }
    }

    sealed class ExpectedFunctionAfterInlineError : ParserError {
        public ExpectedFunctionAfterInlineError(Word word) {
            Word = word;
        }
        
        public Word Word { get; }
        
        public override string ToString() {
            return base.ToString() + "Expected 'function' after 'inline': got " + Word;
        }
    }

    // - Return outside Function error

    sealed class ReturnOutsideFunctionError : ParserError {
        public ReturnOutsideFunctionError(Position position) {
            Position = position;
        }
        
        public Position Position { get; }
        
        public override string ToString() {
            return base.ToString() + "Return ouside of a Function: at " + Position;
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
        public InvalidReturnDataError(DataType[] gotTypes, Function function) {
            GotTypes = gotTypes;
            Function = function;
        }
        public InvalidReturnDataError(DataType[] gotTypes, Function function, ReturnOperation operation) {
            GotTypes = gotTypes;
            Function = function;
            Operation = operation;
        }

        public DataType[] GotTypes { get; }
        public Function Function { get; }
        public ReturnOperation Operation { get; }

        public override string ToString() {
            string msg = "";
            for(int i = 0; i < Function.RetSig.Size; i++) msg += Function.RetSig.Types[i] + (i < Function.RetSig.Size - 1 ? ", " : "");
            msg += "] but got [";
            for(int i = 0; i < GotTypes.Length; i++) msg += GotTypes[i] + (i < GotTypes.Length - 1 ? ", " : "");
            return base.ToString() + "Invalid return arguments: expected [" + msg + "] "  + (Operation != null ? "for return at " + Operation.Position + " in " : "at the end of ") + Function;
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

    // Unknown Function overload error

    sealed class UnknownFunctionOverloadError : TypeCheckerError { // TODO: add stack view or to make debugging easier
        public UnknownFunctionOverloadError(Word name) {
            Name = name;
        }

        public Word Name { get; }

        public override string ToString() {
            return base.ToString() + "Unknown Function overload for: " + Name;
        }
    }

    // Cannot push inlined function address error

    sealed class CannotPushInlinedFunctionAddressError : TypeCheckerError {
        public CannotPushInlinedFunctionAddressError(Word name, Position position) {
            Name = name;
            Position = position;
        }

        public Word Name { get; }
        public Position Position { get; }

        public override string ToString() {
            return base.ToString() + "Can't push the address of an inlined function: " + Name.Text + " at " + Position;
        }
    }

    // Expected errors

    sealed class ExpectedError : TypeCheckerError {
        public ExpectedError(string expected, Position position) {
            Expected = expected;
            Position = position;
        }

        public string Expected { get; }
        public Position Position { get; }

        public override string ToString() {
            return base.ToString() + "Expected " + Expected + " at " + Position;
        }
    }

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

    sealed class ExpectedCodeBlockError : TypeCheckerError {
        public ExpectedCodeBlockError(Position position) {
            Position = position;
        }

        public Position Position { get; }

        public override string ToString() {
            return base.ToString() + "Expected CodeBlock at " + Position;
        }
    }

}