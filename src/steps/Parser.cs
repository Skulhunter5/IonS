using System.Collections.Generic;
using System.Text;

using System; // TEMPORARY

namespace IonS {

    class ParseResult : Result {
        public ParseResult(CodeBlock root, List<string> strings, List<Variable> variables, Dictionary<string, Procedure> procedures, Error error) : base(error) {
            Root = root;
            Strings = strings;
            Variables = variables;
            Procedures = procedures;
        }
        public CodeBlock Root { get; }
        public List<string> Strings { get; }
        public List<Variable> Variables { get; }
        public Dictionary<string, Procedure> Procedures { get; }
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
        private Dictionary<string, Procedure> _procs;
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

        private Error RegisterVariable(Scope scope, Variable var) {
            var error = scope.RegisterVariable(var);
            if(error != null) return error;
            if(scope.Procedure == null) _vars.Add(var);
            else scope.Procedure.Variables.Add(var);
            return null;
        }

        private Error RegisterProcedure(Procedure procedure) {
            if(_procs.ContainsKey(procedure.Name.Text)) return new ProcedureRedefinitionError(_procs[procedure.Name.Text].Name, procedure.Name);
            _procs.Add(procedure.Name.Text, procedure);
            return null;
        }

        private Procedure GetProcedure(string name, bool use) {
            _procs.TryGetValue(name, out Procedure procedure);
            return procedure;
        }

        private void UseProcedure(Procedure currentProcedure, Procedure procedure) {
            if(currentProcedure == null) {
                procedure.IsUsed = true;
                foreach(Procedure proc in procedure.UsedProcedures) proc.IsUsed = true;
            } else if(!currentProcedure.UsedProcedures.Contains(procedure)) currentProcedure.UsedProcedures.Add(procedure);
        }

        private Operation RegisterString(string text) {
            for(int i = 0; i < _strings.Count; i++) if(_strings[i] == text) return new StringOperation(i, text.Length, Current.Position);
            _strings.Add(text);
            return new StringOperation(_strings.Count - 1, text.Length, Current.Position);
        }

        private Operation RegisterCStyleString(string text) {
            text = text + "\0";
            for(int i = 0; i < _strings.Count; i++) if(_strings[i] == text) return new CStyleStringOperation(i, Current.Position);
            _strings.Add(text);
            return new CStyleStringOperation(_strings.Count - 1, Current.Position);
        }

        private bool IsBraceOpen() {
            return Current.Text == "{" && Current.GetType() != typeof(StringWord) && Current.GetType() != typeof(CharWord);
        }

        private bool IsBraceClose() {
            return Current.Text == "}" && Current.GetType() != typeof(StringWord) && Current.GetType() != typeof(CharWord);
        }

        private ParseBlockResult ParseBlock(Scope scope, BreakableBlock breakableBlock, Procedure currentProcedure) {
            bool root = scope == null;
            CodeBlock block = new CodeBlock(scope, currentProcedure, Current != null ? Current.Position : null);
            if(Current == null) {
                if(root) return new ParseBlockResult(block, null);
                return new ParseBlockResult(null, new MissingCodeBlockError());
            }
            block.Start = Current.Position;
            if(IsBraceOpen() || root) {
                Position openBracePosition = Current.Position;
                if(IsBraceOpen()) NextWord();
                while(Current != null && (!IsBraceClose() || root)) {
                    if(IsBraceClose()) break;
                    var error = ParseOperation(block.Operations, block.Scope, breakableBlock, currentProcedure);
                    if(error != null) return new ParseBlockResult(null, error);
                }
                if(!root) {
                    if(!IsBraceClose()) return new ParseBlockResult(null, new EOFInCodeBlockError(openBracePosition));
                    block.End = Current.Position;
                    NextWord();
                }
            } else {
                block.End = Current.Position;
                var error = ParseOperation(block.Operations, block.Scope, breakableBlock, currentProcedure);
                if(error != null) return new ParseBlockResult(null, error);
            }
            return new ParseBlockResult(block, null);
        }

        private Error ParseProcedure(List<Operation> operations, Scope scope, BreakableBlock breakableBlock, Procedure currentProcedure, bool isInlined) {
            if(currentProcedure != null || scope.Parent != null) return new ProcedureNotInGlobalScopeError(Current.Position);
            Word procWord = Current;
            NextWord();

            if(Current == null) return new IncompleteProcedureError(procWord, null);
            Word name = Current;
            // TODO: Check that the name is valid for nasm aswell
            if(Keyword.isReserved(name.Text) || long.TryParse(name.Text, out long _)) return new InvalidProcedureNameError(name);
            NextWord();

            if(Current == null) return new IncompleteProcedureError(procWord, name);
            if(Current.Text != "(" && Current.GetType() != typeof(StringWord)) return new InvalidProcedureParametersError(name, Current);
            NextWord();
            bool dir = false;
            List<DataType> Args = new List<DataType>();
            List<DataType> Rets = new List<DataType>();
            while(Current.Text != ")" && Current.GetType() != typeof(StringWord) && Current != null) {
                if(Current.Text == "--") {
                    if(dir) return new InvalidProcedureParametersError(name, Current);
                    else dir = true;
                } else {
                    if(!EDataType.TryParse(Current.Text, out DataType dataType)) return new InvalidDataTypeError(Current);
                    if(dir) Rets.Add(dataType);
                    else Args.Add(dataType);
                }
                NextWord();
            }
            if(Current == null) return new EOFInProcedureParametersError(name);
            if(Current.Text != ")" || Current.GetType() == typeof(StringWord)) return new InvalidProcedureParametersError(name, Current);
            NextWord();

            Procedure proc = new Procedure(name, Args.ToArray(), Rets.ToArray(), null, isInlined);
            Error error = RegisterProcedure(proc);
            if(error != null) return error;

            if(Current == null) return new IncompleteProcedureError(procWord, name);

            ParseBlockResult result = ParseBlock(scope, null, proc);
            if(result.Error != null) return result.Error;
            proc.Body = result.Block;

            return null;
        }

        private Error ParseOperation(List<Operation> operations, Scope scope, BreakableBlock breakableBlock, Procedure currentProcedure) {
            
            if(Current.GetType() == typeof(StringWord)) {
                StringWord stringWord = (StringWord) Current;
                if(stringWord.StringType == "") operations.Add(RegisterString(Current.Text));
                else if(stringWord.StringType == "c") operations.Add(RegisterCStyleString(Current.Text));
                else return new InternalParserError("Forgot to add a new StringType in Parser.ParseOperation");
            } else if(Current.GetType() == typeof(CharWord)) operations.Add(new Push_uint8_Operation(Encoding.ASCII.GetBytes(""+Current.Text)[0], Current.Position));
            else if(Current.Text == "exit") operations.Add(new ExitOperation(Current.Position));
            else if(Current.Text == "drop") operations.Add(new DropOperation(Current.Position));
            else if(Current.Text == "2drop") operations.Add(new Drop2Operation(Current.Position));
            else if(Current.Text == "dup") operations.Add(new DupOperation(Current.Position));
            else if(Current.Text == "2dup") operations.Add(new Dup2Operation(Current.Position));
            else if(Current.Text == "over") operations.Add(new OverOperation(Current.Position));
            else if(Current.Text == "2over") operations.Add(new Over2Operation(Current.Position));
            else if(Current.Text == "swap") operations.Add(new SwapOperation(Current.Position));
            else if(Current.Text == "2swap") operations.Add(new Swap2Operation(Current.Position));
            else if(Current.Text == "rot") operations.Add(new RotateOperation(Current.Position));
            else if(Current.Text == "2rot") operations.Add(new Rotate2Operation(Current.Position));
            else if(Current.Text == "rot5") operations.Add(new Rotate5Operation(Current.Position));
            else if(Current.Text == "2rot5") operations.Add(new Rotate52Operation(Current.Position));
            else if(Current.Text == "++") operations.Add(new IncrementOperation(Current.Position));
            else if(Current.Text == "--") operations.Add(new DecrementOperation(Current.Position));
            else if(Current.Text == "+") operations.Add(new AddOperation(Current.Position));
            else if(Current.Text == "-") operations.Add(new SubtractOperation(Current.Position));
            else if(Current.Text == "*") operations.Add(new MultiplyOperation(Current.Position));
            else if(Current.Text == "/") operations.Add(new DivideOperation(Current.Position));
            else if(Current.Text == "%") operations.Add(new ModuloOperation(Current.Position));
            else if(Current.Text == "/%") operations.Add(new DivModOperation(Current.Position));
            else if(Current.Text == "<<") operations.Add(new ShLOperation(Current.Position));
            else if(Current.Text == ">>") operations.Add(new ShROperation(Current.Position));
            else if(Current.Text == "&") operations.Add(new BitAndOperation(Current.Position));
            else if(Current.Text == "|") operations.Add(new BitOrOperation(Current.Position));
            else if(Current.Text == "^") operations.Add(new BitXorOperation(Current.Position));
            else if(Current.Text == "inv") operations.Add(new BitInvOperation(Current.Position));
            else if(Current.Text == "&&") operations.Add(new AndOperation(Current.Position));
            else if(Current.Text == "||") operations.Add(new OrOperation(Current.Position));
            else if(Current.Text == "not") operations.Add(new NotOperation(Current.Position));
            else if(Current.Text == "min") operations.Add(new MinOperation(Current.Position));
            else if(Current.Text == "max") operations.Add(new MaxOperation(Current.Position));
            else if(Current.Text == "==") operations.Add(new ComparisonOperation(ComparisonType.EQ, Current.Position));
            else if(Current.Text == "!=") operations.Add(new ComparisonOperation(ComparisonType.NEQ, Current.Position));
            else if(Current.Text == "<") operations.Add(new ComparisonOperation(ComparisonType.B, Current.Position));
            else if(Current.Text == ">") operations.Add(new ComparisonOperation(ComparisonType.A, Current.Position));
            else if(Current.Text == "<=") operations.Add(new ComparisonOperation(ComparisonType.BEQ, Current.Position));
            else if(Current.Text == ">=") operations.Add(new ComparisonOperation(ComparisonType.AEQ, Current.Position));
            else if(Current.Text == ".") operations.Add(new DumpOperation(Current.Position));
            else if(Current.Text == "true") operations.Add(new Push_uint64_Operation(1, Current.Position));
            else if(Current.Text == "false") operations.Add(new Push_uint64_Operation(0, Current.Position));
            else if(Current.Text.StartsWith("ctt")) {
                if(!int.TryParse(Current.Text.Substring(3, Current.Text.Length - 3), out int n) || n < 1) return new InvalidCTTIndexError(Current);
                operations.Add(new CTTOperation(n, Current.Position));
            } else if(Current.Text == "if") {
                Position position = Current.Position;
                NextWord();
                ParseBlockResult result = ParseBlock(scope, breakableBlock, currentProcedure);
                if(result.Error != null) return result.Error;
                CodeBlock blockIf = result.Block;
                IfBlock ifBlock = new IfBlock(blockIf, null, position);
                while(Current.Text == "else*") {
                    NextWord();
                    result = ParseBlock(scope, breakableBlock, currentProcedure);
                    if(result.Error != null) return result.Error;
                    CodeBlock Condition = result.Block;
                    if(Current.Text != "if") return new MissingIfError(Current.Position);
                    NextWord();
                    result = ParseBlock(scope, breakableBlock, currentProcedure);
                    if(result.Error != null) return result.Error;
                    CodeBlock Conditional = result.Block;
                    ifBlock.Conditions.Add(Condition);
                    ifBlock.Conditionals.Add(Conditional);
                }
                if(Current.Text == "else") {
                    NextWord();
                    result = ParseBlock(scope, breakableBlock, currentProcedure);
                    if(result.Error != null) return result.Error;
                    ifBlock.BlockElse = result.Block;
                }
                operations.Add(ifBlock);
                return null;
            } else if(Current.Text == "while") {
                Position position = Current.Position;
                NextWord();
                WhileBlock whileBlock = new WhileBlock(null, null, position);
                ParseBlockResult result = ParseBlock(scope, breakableBlock, currentProcedure);
                if(result.Error != null) return result.Error;
                whileBlock.Condition = result.Block;
                if(Current.Text != "do") return new MissingDoError(Current.Position);
                NextWord();
                result = ParseBlock(scope, whileBlock, currentProcedure);
                if(result.Error != null) return result.Error;
                whileBlock.Block = result.Block;
                operations.Add(whileBlock);
                return null;
            } else if(Current.Text == "do") {
                Position position = Current.Position;
                NextWord();
                DoWhileBlock doWhileBlock = new DoWhileBlock(null, null, position);
                ParseBlockResult result = ParseBlock(scope, doWhileBlock, currentProcedure);
                if(result.Error != null) return result.Error;
                doWhileBlock.Block = result.Block;
                if(Current.Text != "while") return new MissingWhileError(Current.Position);
                NextWord();
                result = ParseBlock(scope, breakableBlock, currentProcedure);
                if(result.Error != null) return result.Error;
                doWhileBlock.Condition = result.Block;
                operations.Add(doWhileBlock);
                return null;
            } else if(Current.Text == "continue") {
                if(breakableBlock == null) return new InvalidContinueError(Current.Position);
                operations.Add(new ContinueOperation(breakableBlock, Current.Position));
            } else if(Current.Text == "break") {
                if(breakableBlock == null) return new InvalidBreakError(Current.Position);
                operations.Add(new BreakOperation(breakableBlock, Current.Position));
            } else if(Current.Text == "var") {
                Word varWord = Current;
                NextWord();
                if(Current == null) return new IncompleteVariableDeclarationError(varWord, null);

                Word identifier = Current;
                // TODO: Check that the identifier is valid for nasm aswell
                if(Keyword.isReserved(identifier.Text) || long.TryParse(identifier.Text, out long _)) return new InvalidVariableIdentifierError(identifier);
                
                NextWord();
                if(Current.Text == null) return new IncompleteVariableDeclarationError(varWord, identifier);

                if(int.TryParse(Current.Text, out int bytesize)) { // TODO: add support for endings like K or KB or something similar
                    Error error = RegisterVariable(scope, new Variable(identifier, bytesize));
                    if(error != null) return error;
                } else new InvalidVariableBytesizeError(Current);
            } else if(Current.Text.StartsWith("!")) {
                string amountStr = Current.Text.Substring(1);
                bool isByte = byte.TryParse(amountStr, out byte amount);
                if(!isByte) return new InvalidMemReadWriteAmountError(amountStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1));

                if(!(amount == 8 || amount == 16 || amount == 32 || amount == 64)) return new InvalidMemReadWriteAmountError(amountStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1));

                operations.Add(new MemWriteOperation(amount, Current.Position));
            } else if(Current.Text.StartsWith("@")) {
                string amountStr = Current.Text.Substring(1);
                bool isByte = byte.TryParse(amountStr, out byte amount);
                if(!isByte) return new InvalidMemReadWriteAmountError(amountStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1));

                if(!(amount == 8 || amount == 16 || amount == 32 || amount == 64)) return new InvalidMemReadWriteAmountError(amountStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1));

                operations.Add(new MemReadOperation(amount, Current.Position));
            } else if(Current.Text == "here") {
                string text = Current.Position.ToString();
                operations.Add(new StringOperation(_strings.Count, text.Length, Current.Position));
                _strings.Add(text);
            } else if(Current.Text == "chere") {
                operations.Add(new CStyleStringOperation(_strings.Count, Current.Position));
                _strings.Add(Current.Position + "\0");
            } else if(Current.Text.StartsWith("syscall")) {
                string argcStr = Current.Text.Substring(7, Current.Text.Length - 7);
                if(!int.TryParse(argcStr, out int argc)) return new InvalidSyscallArgcError(argcStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 7));
                if(argc >= 0 && argc <= 6) operations.Add(new SyscallOperation(argc, Current.Position));
                else return new InvalidSyscallArgcError(argcStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 7));
            } else if(IsBraceOpen()) {
                var result = ParseBlock(scope, breakableBlock, currentProcedure);
                if(result.Error != null) return result.Error;
                operations.Add(result.Block);
                return null;
            } else if(Current.Text == "proc") return ParseProcedure(operations, scope, breakableBlock, currentProcedure, false);
            else if(Current.Text == "inline") {
                NextWord();
                if(Current.Text != "proc") return new MissingProcAfterInlineError(Current);
                return ParseProcedure(operations, scope, breakableBlock, currentProcedure, true);
            } else if(Current.Text == "return") {
                if(currentProcedure == null) return new ReturnOutsideProcedureError(Current.Position);
                operations.Add(new ReturnOperation(currentProcedure.Id, Current.Position));
            } else {
                if(ulong.TryParse(Current.Text, out ulong value)) operations.Add(new Push_uint64_Operation(value, Current.Position));
                else {
                    Variable var = scope.GetVariable(Current.Text);
                    if(var != null) operations.Add(new VariableAccessOperation(var.Id, Current.Position));
                    else {
                        Procedure proc = GetProcedure(Current.Text, true);
                        if(proc != null) {
                            operations.Add(new ProcedureCallOperation(proc, Current.Position));
                            UseProcedure(currentProcedure, proc);
                        } else return new UnexpectedWordError(Current);
                    }
                }
            }
            NextWord();
            return null;
        }

        public ParseResult Parse() {
            var lexingResult = new Lexer(_text, _source).run();
            if(lexingResult.Error != null) return new ParseResult(null, null, null, null, lexingResult.Error);
            _words = lexingResult.Words;

            var commentResult = new CommentPreprocessor(_words).run();
            if(commentResult.Error != null) return new ParseResult(null, null, null, null, commentResult.Error);
            _words = commentResult.Words;

            var includeResult = new IncludePreprocessor(_source, _words).run();
            if(includeResult.Error != null) return new ParseResult(null, null, null, null, includeResult.Error);
            _words = includeResult.Words;

            var macroResult = new MacroPreprocessor(_words).run();
            if(macroResult.Error != null) return new ParseResult(null, null, null, null, macroResult.Error);
            _words = macroResult.Words;

            _vars = new List<Variable>();
            _procs = new Dictionary<string, Procedure>();
            _strings = new List<string>();

            ParseBlockResult parseResult = ParseBlock(null, null, null);
            if(parseResult.Error != null) return new ParseResult(null, null, null, null, parseResult.Error);

            Error error = new TypeChecker(parseResult.Block, _procs).run();
            if(error != null) return new ParseResult(null, null, null, null, error);

            List<string> toRemove = new List<string>();
            foreach(string proc in _procs.Keys) if(_procs[proc].IsInlined) toRemove.Add(proc);
            foreach(string proc in toRemove) _procs.Remove(proc);

            return new ParseResult(parseResult.Block, _strings, _vars, _procs, null);
        }

    }

}