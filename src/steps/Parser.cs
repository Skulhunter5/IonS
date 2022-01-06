using System.Collections.Generic;
using System.Text;

namespace IonS {

    class ParseResult : Result {
        public ParseResult(List<Variable> variables, List<string> strings, List<Operation> operations, Error error) : base(error) {
            Variables = variables;
            Strings = strings;
            Operations = operations;
        }
        public List<Variable> Variables { get; }
        public List<string> Strings { get; }
        public List<Operation> Operations { get; }
    }

    class ParseResult2 : Result {
        public ParseResult2(CodeBlock root, List<string> strings, List<Variable> variables, Error error) : base(error) {
            Root = root;
            Strings = strings;
            Variables = variables;
        }
        public CodeBlock Root { get; }
        public List<string> Strings { get; }
        public List<Variable> Variables { get; }
    }

    class ParseBlockResult : Result {
        public ParseBlockResult(CodeBlock block, Error error) : base(error) {
            Block = block;
        }
        public CodeBlock Block { get; }
    }

    class Parser {
        private readonly string _text, _source;
        private Word[] _words;
        private int _position;

        private List<Variable> _vars;
        private List<string> _strings;

        public Parser(string text, string source)
        {
            _text = text;
            _source = source;
        }

        private Word Peek(int offset) {
            var index = _position + offset;
            if(index >= _words.Length) return null;
            return _words[index];
        }

        private Word Current {
            get {
                if(_position >= _words.Length) return null;
                return _words[_position];
            }
        }

        private Word NextWord() {
            _position++;
            return Current;
        }

        private Variable GetVariable(string identifier) {
            foreach(Variable var in _vars) if(var.Identifier.Text == identifier) return var;
            return null;
        }

        private Error RegisterVariable(Scope scope, Variable var) {
            var error = scope.RegisterVariable(var);
            if(error != null) return error;
            _vars.Add(var);
            return null;
        }

        private ParseBlockResult ParseBlock(Scope scope, BreakableBlock breakableBlock) {
            bool root = scope == null;
            CodeBlock block = new CodeBlock(scope);
            if(Current == null) {
                if(root) return new ParseBlockResult(block, null);
                return new ParseBlockResult(null, new EOFInCodeBlockError());
            }
            if(Current.Text == "{" || root) {
                if(Current.Text == "{") NextWord();
                while(Current != null && (Current.Text != "}" || root)) {
                    if(Current.Text == "}") break;
                    var error = ParseOperation(block.Operations, block.Scope, breakableBlock);
                    if(error != null) return new ParseBlockResult(null, error);
                }
                if(!root) {
                    if(Current.Text != "}") return new ParseBlockResult(null, new EOFInCodeBlockError());
                    NextWord();
                }
            } else {
                var error = ParseOperation(block.Operations, block.Scope, breakableBlock);
                if(error != null) return new ParseBlockResult(null, error);
            }
            return new ParseBlockResult(block, null);
        }

        private Error ParseOperation(List<Operation> operations, Scope scope, BreakableBlock breakableBlock) {
            if(Current.Text == "exit") {
                operations.Add(new ExitOperation());
            } else if(Current.Text == "putc") {
                operations.Add(new Put_char_Operation());
            } else if(Current.Text == "drop") {
                operations.Add(new DropOperation());
            } else if(Current.Text == "2drop") {
                operations.Add(new Drop2Operation());
            } else if(Current.Text == "dup") {
                operations.Add(new DupOperation());
            } else if(Current.Text == "2dup") {
                operations.Add(new Dup2Operation());
            } else if(Current.Text == "over") {
                operations.Add(new OverOperation());
            } else if(Current.Text == "2over") {
                operations.Add(new Over2Operation());
            } else if(Current.Text == "swap") {
                operations.Add(new SwapOperation());
            } else if(Current.Text == "2swap") {
                operations.Add(new Swap2Operation());
            } else if(Current.Text == "rot") {
                operations.Add(new RotateOperation());
            } else if(Current.Text == "2rot") {
                operations.Add(new Rotate2Operation());
            } else if(Current.Text == "+") {
                operations.Add(new AddOperation());
            } else if(Current.Text == "-") {
                operations.Add(new SubtractOperation());
            } else if(Current.Text == "*") {
                operations.Add(new MultiplyOperation());
            } else if(Current.Text == "/") {
                operations.Add(new DivideOperation());
            } else if(Current.Text == "%") {
                operations.Add(new ModuloOperation());
            } else if(Current.Text == "/%") {
                operations.Add(new DivModOperation());
            } else if(Current.Text == "<<") {
                operations.Add(new ShLOperation());
            } else if(Current.Text == ">>") {
                operations.Add(new ShROperation());
            } else if(Current.Text == "&") {
                operations.Add(new BitAndOperation());
            } else if(Current.Text == "|") {
                operations.Add(new BitOrOperation());
            } else if(Current.Text == "min") {
                operations.Add(new MinOperation());
            } else if(Current.Text == "max") {
                operations.Add(new MaxOperation());
            } else if(Current.Text == "==") {
                operations.Add(new ComparisonOperation(ComparisonType.EQ));
            } else if(Current.Text == "!=") {
                operations.Add(new ComparisonOperation(ComparisonType.NEQ));
            } else if(Current.Text == "<") {
                operations.Add(new ComparisonOperation(ComparisonType.B));
            } else if(Current.Text == ">") {
                operations.Add(new ComparisonOperation(ComparisonType.A));
            } else if(Current.Text == "<=") {
                operations.Add(new ComparisonOperation(ComparisonType.BEQ));
            } else if(Current.Text == ">=") {
                operations.Add(new ComparisonOperation(ComparisonType.AEQ));
            } else if(Current.Text == ".") {
                operations.Add(new DumpOperation());
            } else if(Current.Text == "if") {
                NextWord();
                ParseBlockResult result = ParseBlock(scope, breakableBlock);
                if(result.Error != null) return result.Error;
                CodeBlock blockIf = result.Block;
                CodeBlock blockElse = null;
                if(Current.Text == "else") {
                    NextWord();
                    result = ParseBlock(scope, breakableBlock);
                    if(result.Error != null) return result.Error;
                    blockElse = result.Block;
                }
                operations.Add(new IfBlock(blockIf, blockElse));
                return null;
            } else if(Current.Text == "while") { // CWD
                NextWord();
                WhileBlock whileBlock = new WhileBlock(null, null);
                ParseBlockResult result = ParseBlock(scope, breakableBlock);
                if(result.Error != null) return result.Error;
                whileBlock.Condition = result.Block;
                if(Current.Text != "do") return new MissingDoError(Current.Position);
                NextWord();
                result = ParseBlock(scope, whileBlock);
                if(result.Error != null) return result.Error;
                whileBlock.Block = result.Block;
                operations.Add(whileBlock);
                return null;
            } else if(Current.Text == "do") {
                NextWord();
                DoWhileBlock doWhileBlock = new DoWhileBlock(null, null);
                ParseBlockResult result = ParseBlock(scope, doWhileBlock);
                if(result.Error != null) return result.Error;
                doWhileBlock.Block = result.Block;
                if(Current.Text != "while") return new MissingWhileError(Current.Position);
                NextWord();
                result = ParseBlock(scope, breakableBlock);
                if(result.Error != null) return result.Error;
                doWhileBlock.Condition = result.Block;
                operations.Add(doWhileBlock);
                return null;
            } else if(Current.Text == "continue") {
                if(breakableBlock == null) return new InvalidContinueError(Current.Position);
                operations.Add(new ContinueOperation(breakableBlock));
            } else if(Current.Text == "break") {
                if(breakableBlock == null) return new InvalidBreakError(Current.Position);
                operations.Add(new BreakOperation(breakableBlock));
            } else if(Current.Text == "var") {
                Word varWord = Current;
                NextWord();
                if(Current == null) return new IncompleteVariableDeclarationError(varWord, null);

                Word identifier = Current;
                // TODO: Check that the identifier is valid for nasm aswell
                if(Keyword.isReserved(identifier.Text) || long.TryParse(identifier.Text, out long _)) return new InvalidVariableIdentifierError(identifier);
                Variable var = GetVariable(identifier.Text);
                if(var != null) return new VariableRedeclarationError(var.Identifier, identifier);

                NextWord();
                if(Current.Text == null) return new IncompleteVariableDeclarationError(varWord, identifier);

                if(byte.TryParse(Current.Text, out byte bytesize)) {
                    //_vars.Add(new Variable(identifier, bytesize));
                    Error error = RegisterVariable(scope, new Variable(identifier, bytesize));
                    if(error != null) return error;
                } else new InvalidVariableBytesizeError(Current);
            } else if(Current.Text.StartsWith("!")) {
                string amountStr = Current.Text.Substring(1);
                bool isByte = byte.TryParse(amountStr, out byte amount);
                if(!isByte) return new InvalidMemReadWriteAmountError(amountStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1));

                if(!(amount == 8 || amount == 16 || amount == 32 || amount == 64)) return new InvalidMemReadWriteAmountError(amountStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1));

                operations.Add(new MemWriteOperation(amount));
            } else if(Current.Text.StartsWith("@")) {
                string amountStr = Current.Text.Substring(1);
                bool isByte = byte.TryParse(amountStr, out byte amount);
                if(!isByte) return new InvalidMemReadWriteAmountError(amountStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1));

                if(!(amount == 8 || amount == 16 || amount == 32 || amount == 64)) return new InvalidMemReadWriteAmountError(amountStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1));

                operations.Add(new MemReadOperation(amount));
            } else if(Current.Text.StartsWith('"')) {
                if(Current.Text.EndsWith("\"")) {
                    string text = Current.Text.Substring(1, Current.Text.Length - 2);
                    operations.Add(new StringOperation(_strings.Count, text.Length));
                    _strings.Add(text);
                } else if(Current.Text.EndsWith("\"c")) {
                    operations.Add(new CStyleStringOperation(_strings.Count));
                    _strings.Add(Current.Text.Substring(1, Current.Text.Length - 3) + "\0");
                }
            } else if(Current.Text.StartsWith("'")) {
                string text = Current.Text.Substring(1, Current.Text.Length-2);
                char c = text[0];
                if(text.Length == 2) {
                    // TODO: factor out into function
                    c = text[1];
                    if(c == 'n') c = '\n';
                    else if(c == 't') c = '\t';
                    else if(c == 'r') c = '\r';
                    else if(c == '\\') c = '\\';
                    else if(c == '"') c = '"';
                    else if(c == '0') c = '\0';
                    else if(c == '\n') return new InvalidEscapeCharacterError("\\n", new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1));
                    else if(c == '\t') return new InvalidEscapeCharacterError("\\t", new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1));
                    else if(c == '\r') return new InvalidEscapeCharacterError("\\r", new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1));
                    else return new InvalidEscapeCharacterError(""+c, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1));
                }
                operations.Add(new Push_uint64_Operation(Encoding.ASCII.GetBytes(""+c)[0]));
            } else if(Current.Text == "here") {
                string text = Current.Position.ToString();
                operations.Add(new StringOperation(_strings.Count, text.Length));
                _strings.Add(text);
            } else if(Current.Text == "chere") {
                operations.Add(new CStyleStringOperation(_strings.Count));
                _strings.Add(Current.Position + "\0");
            } else if(Current.Text.StartsWith("syscall")) {
                string argcStr = Current.Text.Substring(7, Current.Text.Length - 7);
                if(!int.TryParse(argcStr, out int argc)) return new InvalidSyscallArgcError(argcStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 7));
                if(argc >= 0 && argc <= 6) operations.Add(new SyscallOperation(argc));
                else return new InvalidSyscallArgcError(argcStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 7));
            } else {
                if(ulong.TryParse(Current.Text, out ulong value)) operations.Add(new Push_uint64_Operation(value));
                else {
                    Variable var = scope.GetVariable(Current.Text);
                    if(var != null) operations.Add(new VariableAccessOperation(var.Id));
                    else return new UnexpectedWordError(Current);
                }
            }
            NextWord();
            return null;
        }

        public ParseResult2 Parse() {
            var lexingResult = new Lexer(_text, _source).run();
            if(lexingResult.Error != null) return new ParseResult2(null, null, null, lexingResult.Error);
            _words = lexingResult.Words;

            var incPreprocResult = new IncludePreprocessor(_source, _words).run();
            if(incPreprocResult.Error != null) return new ParseResult2(null, null, null, incPreprocResult.Error);
            _words = incPreprocResult.Words;

            var result = new MacroPreprocessor(_words).run();
            if(result.Error != null) return new ParseResult2(null, null, null, result.Error);
            _words = result.Words;

            _vars = new List<Variable>();
            _strings = new List<string>();

            ParseBlockResult parseResult = ParseBlock(null, null);
            if(parseResult.Error != null) return new ParseResult2(null, null, null, parseResult.Error);

            return new ParseResult2(parseResult.Block, _strings, _vars, null);
        }

    }

}