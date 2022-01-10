using System.Collections.Generic;
using System.Text;

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
            for(int i = 0; i < _strings.Count; i++) if(_strings[i] == text) return new StringOperation(i, text.Length);
            _strings.Add(text);
            return new StringOperation(_strings.Count - 1, text.Length);
        }

        private Operation RegisterCStyleString(string text) {
            text = text + "\0";
            for(int i = 0; i < _strings.Count; i++) if(_strings[i] == text) return new CStyleStringOperation(i);
            _strings.Add(text);
            return new CStyleStringOperation(_strings.Count - 1);
        }

        private bool IsBraceOpen() {
            return Current.Text == "{" && Current.GetType() != typeof(StringWord) && Current.GetType() != typeof(CharWord);
        }

        private bool IsBraceClose() {
            return Current.Text == "}" && Current.GetType() != typeof(StringWord) && Current.GetType() != typeof(CharWord);
        }

        private ParseBlockResult ParseBlock(Scope scope, BreakableBlock breakableBlock, Procedure currentProcedure) {
            bool root = scope == null;
            CodeBlock block = new CodeBlock(scope, currentProcedure);
            if(Current == null) {
                if(root) return new ParseBlockResult(block, null);
                return new ParseBlockResult(null, new EOFInCodeBlockError());
            }
            if(IsBraceOpen() || root) {
                if(IsBraceOpen()) NextWord();
                while(Current != null && (!IsBraceClose() || root)) {
                    if(IsBraceClose()) break;
                    var error = ParseOperation(block.Operations, block.Scope, breakableBlock, currentProcedure);
                    if(error != null) return new ParseBlockResult(null, error);
                }
                if(!root) {
                    if(!IsBraceClose()) return new ParseBlockResult(null, new EOFInCodeBlockError());
                    NextWord();
                }
            } else {
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
            if(Keyword.isReserved(name.Text) || long.TryParse(name.Text, out long _)) return new InvalidProcedureNameError(currentProcedure.Name);
            NextWord();

            if(Current == null) return new IncompleteProcedureError(procWord, name);
            Word argsWord = Current;
            string[] args = Current.Text.Split("-");
            if(args.Length != 2) return new InvalidProcedureArgsError(argsWord);
            if(!int.TryParse(args[0], out int argc) || argc < 0 || argc > Utils.FreeUseRegisters.Length) return new InvalidProcedureArgsError(argsWord);
            if(!int.TryParse(args[1], out int rvc) || rvc < 0 || rvc > Utils.FreeUseRegisters.Length) return new InvalidProcedureArgsError(argsWord);
            NextWord();

            Procedure proc = new Procedure(name, argc, rvc, null, isInlined);
            Error error = RegisterProcedure(proc);
            if(error != null) return error;

            if(Current == null) return new IncompleteProcedureError(procWord, name);

            ParseBlockResult result = ParseBlock(scope, null, proc);
            if(result.Error != null) return result.Error;
            proc.Body = result.Block;

            return null;
        }

        private Error ParseOperation(List<Operation> operations, Scope scope, BreakableBlock breakableBlock, Procedure currentProcedure) {
            if(Current.Text == "exit") operations.Add(new ExitOperation());
            else if(Current.Text == "putc") operations.Add(new Put_char_Operation());
            else if(Current.Text == "drop") operations.Add(new DropOperation());
            else if(Current.Text == "2drop") operations.Add(new Drop2Operation());
            else if(Current.Text == "dup") operations.Add(new DupOperation());
            else if(Current.Text == "2dup") operations.Add(new Dup2Operation());
            else if(Current.Text == "over") operations.Add(new OverOperation());
            else if(Current.Text == "2over") operations.Add(new Over2Operation());
            else if(Current.Text == "swap") operations.Add(new SwapOperation());
            else if(Current.Text == "2swap") operations.Add(new Swap2Operation());
            else if(Current.Text == "rot") operations.Add(new RotateOperation());
            else if(Current.Text == "2rot") operations.Add(new Rotate2Operation());
            else if(Current.Text == "rot5") operations.Add(new Rotate5Operation());
            else if(Current.Text == "2rot5") operations.Add(new Rotate52Operation());
            else if(Current.Text == "++") operations.Add(new IncrementOperation());
            else if(Current.Text == "--") operations.Add(new DecrementOperation());
            else if(Current.Text == "+") operations.Add(new AddOperation());
            else if(Current.Text == "-") operations.Add(new SubtractOperation());
            else if(Current.Text == "*") operations.Add(new MultiplyOperation());
            else if(Current.Text == "/") operations.Add(new DivideOperation());
            else if(Current.Text == "%") operations.Add(new ModuloOperation());
            else if(Current.Text == "/%") operations.Add(new DivModOperation());
            else if(Current.Text == "<<") operations.Add(new ShLOperation());
            else if(Current.Text == ">>") operations.Add(new ShROperation());
            else if(Current.Text == "&") operations.Add(new BitAndOperation());
            else if(Current.Text == "|") operations.Add(new BitOrOperation());
            else if(Current.Text == "^") operations.Add(new BitXorOperation());
            else if(Current.Text == "inv") operations.Add(new BitInvOperation());
            else if(Current.Text == "&&") operations.Add(new AndOperation());
            else if(Current.Text == "||") operations.Add(new OrOperation());
            else if(Current.Text == "not") operations.Add(new NotOperation());
            else if(Current.Text == "min") operations.Add(new MinOperation());
            else if(Current.Text == "max") operations.Add(new MaxOperation());
            else if(Current.Text == "==") operations.Add(new ComparisonOperation(ComparisonType.EQ));
            else if(Current.Text == "!=") operations.Add(new ComparisonOperation(ComparisonType.NEQ));
            else if(Current.Text == "<") operations.Add(new ComparisonOperation(ComparisonType.B));
            else if(Current.Text == ">") operations.Add(new ComparisonOperation(ComparisonType.A));
            else if(Current.Text == "<=") operations.Add(new ComparisonOperation(ComparisonType.BEQ));
            else if(Current.Text == ">=") operations.Add(new ComparisonOperation(ComparisonType.AEQ));
            else if(Current.Text == ".") operations.Add(new DumpOperation());
            else if(Current.Text == "true") operations.Add(new Push_uint64_Operation(1));
            else if(Current.Text == "false") operations.Add(new Push_uint64_Operation(0));
            else if(Current.Text.StartsWith("ctt")) {
                if(!uint.TryParse(Current.Text.Substring(3, Current.Text.Length - 3), out uint n) || n < 1) return new InvalidCTTIndexError(Current);
                operations.Add(new CTTOperation(n));
            } else if(Current.Text == "if") {
                NextWord();
                ParseBlockResult result = ParseBlock(scope, breakableBlock, currentProcedure);
                if(result.Error != null) return result.Error;
                CodeBlock blockIf = result.Block;
                IfBlock ifBlock = new IfBlock(blockIf, null);
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
                NextWord();
                WhileBlock whileBlock = new WhileBlock(null, null);
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
                NextWord();
                DoWhileBlock doWhileBlock = new DoWhileBlock(null, null);
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

                operations.Add(new MemWriteOperation(amount));
            } else if(Current.Text.StartsWith("@")) {
                string amountStr = Current.Text.Substring(1);
                bool isByte = byte.TryParse(amountStr, out byte amount);
                if(!isByte) return new InvalidMemReadWriteAmountError(amountStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1));

                if(!(amount == 8 || amount == 16 || amount == 32 || amount == 64)) return new InvalidMemReadWriteAmountError(amountStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1));

                operations.Add(new MemReadOperation(amount));
            } else if(Current.GetType() == typeof(StringWord)) {
                StringWord stringWord = (StringWord) Current;
                if(stringWord.StringType == "") operations.Add(RegisterString(Current.Text));
                else if(stringWord.StringType == "c") operations.Add(RegisterCStyleString(Current.Text));
                else return new InternalParserError("Forgot to add a new StringType in Parser.ParseOperation");
            } else if(Current.GetType() == typeof(CharWord)) {
                operations.Add(new Push_uint64_Operation(Encoding.ASCII.GetBytes(""+Current.Text)[0]));
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
                operations.Add(new ReturnOperation(currentProcedure.Id));
            } else {
                if(ulong.TryParse(Current.Text, out ulong value)) operations.Add(new Push_uint64_Operation(value));
                else {
                    Variable var = scope.GetVariable(Current.Text);
                    if(var != null) operations.Add(new VariableAccessOperation(var.Id));
                    else {
                        Procedure proc = GetProcedure(Current.Text, true);
                        if(proc != null) {
                            operations.Add(new ProcedureCallOperation(proc));
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

            List<string> toRemove = new List<string>();
            foreach(string proc in _procs.Keys) if(_procs[proc].IsInlined) toRemove.Add(proc);
            foreach(string proc in toRemove) _procs.Remove(proc);

            return new ParseResult(parseResult.Block, _strings, _vars, _procs, null);
        }

    }

}