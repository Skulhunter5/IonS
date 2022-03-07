using System;
using System.Text;
using System.Collections.Generic;

namespace IonS {

    class ParseResult {
        public ParseResult(CodeBlock root, List<string> strings, List<Variable> variables, Dictionary<string, Dictionary<string, Procedure>> procedures) {
            Root = root;
            Strings = strings;
            Variables = variables;
            Procedures = procedures;
        }

        public CodeBlock Root { get; }
        public List<string> Strings { get; }
        public List<Variable> Variables { get; }
        public Dictionary<string, Dictionary<string, Procedure>> Procedures { get; }

    }

    class Parser {

        private readonly string _text, _source;
        private List<Word> _words;
        private int _position;

        private Dictionary<string, Dictionary<string, Procedure>> _procs;
        private Dictionary<string, Struct> _structs;
        private List<Variable> _vars;
        private List<string> _strings;

        private readonly Assembler _assembler;

        private ulong nextIota = 0;
        private ulong Iota(bool reset) {
            ulong val = nextIota++;
            if(reset) nextIota = 0;
            return val;
        }

        public Parser(string text, string source, Assembler assembler)
        {
            _text = text;
            _source = source;
            _assembler = assembler;
        }

        private Word Peek(int offset) {
            var index = _position + offset;
            if(index >= _words.Count) return null;
            return _words[index];
        }

        private Word Current {
            get {
                if(_position >= _words.Count) return null;
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
            string signature = procedure.GetArgsSignature();
            if(_procs.ContainsKey(procedure.Name.Text)) {
                if(_procs[procedure.Name.Text].ContainsKey(signature)) return new ProcedureRedefinitionError(_procs[procedure.Name.Text][signature].Name, procedure.Name);
            } else _procs.Add(procedure.Name.Text, new Dictionary<string, Procedure>());
            _procs[procedure.Name.Text].Add(signature, procedure);
            return null;
        }

        private bool ProcedureExists(string name) {
            if(!_procs.ContainsKey(name)) return false;
            return true;
        }
        private bool ProcedureExists(string name, string signature) { // CHECKIFNECESSARY
            if(!_procs.ContainsKey(name)) return false;
            if(!_procs[name].ContainsKey(signature)) return false;
            return true;
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
            return Current.Text == "{" && Current.Type == WordType.Word;
        }

        private bool IsBraceClose() {
            return Current.Text == "}" && Current.Type == WordType.Word;
        }

        private CodeBlock ParseCodeBlock(Scope parentScope, Scope newScope, BreakableBlock breakableBlock, Procedure currentProcedure) {
            bool root = parentScope == null;
            CodeBlock block = new CodeBlock(newScope, Current != null ? Current.Position : null); // TODO: check if last null can be replaced with 'new Position(_source, 1, 1)'
            if(Current == null) {
                if(root) return block;
                ErrorSystem.AddError_i(new ExpectedCodeBlockError(_words[_words.Count-1].Position.Derive(0, _words[_words.Count-1].Text.Length+1)));
            }

            if(Current.Type == WordType.Word && Current.Text == ";") {
                NextWord();
                return block;
            }

            block.Start = Current.Position;
            if(IsBraceOpen() || root) {
                Position openBracePosition = Current.Position;
                if(IsBraceOpen() && !root) NextWord();
                while(Current != null && (!IsBraceClose() || root)) {
                    if(IsBraceClose()) break;
                    ParseOperation(block.Operations, block.Scope, breakableBlock, currentProcedure);
                }
                if(!root) {
                    if(Current == null) ErrorSystem.AddError_i(new EOFInCodeBlockError(openBracePosition));
                    if(!IsBraceClose()) ErrorSystem.AddError_i(new EOFInCodeBlockError(openBracePosition));
                    block.End = Current.Position;
                    NextWord();
                }
            } else {
                block.End = Current.Position;
                ParseOperation(block.Operations, block.Scope, breakableBlock, currentProcedure);
            }
            return block;
        }

        private Error ParseProcedure(List<Operation> operations, Scope scope, BreakableBlock breakableBlock, Procedure currentProcedure, bool isInlined) {
            if(currentProcedure != null || scope.Parent != null) return new ProcedureNotInGlobalScopeError(Current.Position);
            Word procWord = Current;
            NextWord();

            if(Current == null) return new IncompleteProcedureError(procWord, null);
            Word name = Current;
            // TODO: Check that the name is valid for nasm aswell
            if(!Utils.IsValidIdentifier(name.Text)) return new InvalidProcedureNameError(name);
            NextWord();

            if(Current == null) return new IncompleteProcedureError(procWord, name);
            if(Current.Type != WordType.Word) return new InvalidProcedureParametersError(name, Current);
            bool dir = false;
            List<DataType> Args = new List<DataType>();
            List<DataType> Rets = new List<DataType>();
            if(Current.Text == "()") NextWord();
            else if(Current.Text == "(") {
                NextWord();
                while(Current.Text != ")" && Current.Type == WordType.Word && Current != null) {
                    if(Current.Text == "--") {
                        if(dir) return new InvalidProcedureParametersError(name, Current);
                        else dir = true;
                    } else {
                        if(Current.Type != WordType.Word || !DataType.TryParse(Current.Text, out DataType dataType)) return new InvalidDataTypeError(Current);
                        if(dir) Rets.Add(dataType);
                        else Args.Add(dataType);
                    }
                    NextWord();
                }
                if(Current == null) return new EOFInProcedureParametersError(name);
                if(Current.Text != ")" || Current.Type != WordType.Word) return new InvalidProcedureParametersError(name, Current);
                NextWord();
            } else return new InvalidProcedureParametersError(name, Current);

            Procedure proc = new Procedure(name, Args.ToArray(), Rets.ToArray(), null, isInlined);
            Error error = RegisterProcedure(proc);
            if(error != null) return error;

            if(Current == null) return new IncompleteProcedureError(procWord, name);

            proc.Body = ParseCodeBlock(scope, new Scope(scope, proc), null, proc);

            return null;
        }

        private void ParseOperation(List<Operation> operations, Scope scope, BreakableBlock breakableBlock, Procedure currentProcedure) {
            if(Current.Type == WordType.String) {
                StringWord stringWord = (StringWord) Current;
                if(stringWord.StringType == "") operations.Add(RegisterString(Current.Text));
                else if(stringWord.StringType == "c") operations.Add(RegisterCStyleString(Current.Text));
                else ErrorSystem.InternalError("Forgot to add a new StringType in Parser.ParseOperation");
            } else if(Current.Type == WordType.Char) operations.Add(new Push_uint64_Operation(Encoding.ASCII.GetBytes(""+Current.Text)[0], Current.Position));
            else if(Current.Text == "exit") operations.Add(new ExitOperation(Current.Position));
            else if(Current.Text == "drop") operations.Add(new DropOperation(1, Current.Position));
            else if(Current.Text == "2drop") operations.Add(new DropOperation(2, Current.Position));
            else if(Current.Text == "dup") operations.Add(new DupOperation(Current.Position));
            else if(Current.Text == "2dup") operations.Add(new Dup2Operation(Current.Position));
            else if(Current.Text == "over") operations.Add(new OverOperation(Current.Position));
            else if(Current.Text == "2over") operations.Add(new Over2Operation(Current.Position));
            else if(Current.Text == "swap") operations.Add(new SwapOperation(Current.Position));
            else if(Current.Text == "2swap") operations.Add(new Swap2Operation(Current.Position));
            else if(Current.Text == "rot") operations.Add(new RotOperation(Current.Position));
            else if(Current.Text == "rrot") operations.Add(new RRotOperation(Current.Position));
            else if(Current.Text == "rot5") operations.Add(new Rot5Operation(Current.Position));
            else if(Current.Text == "rrot5") operations.Add(new RRot5Operation(Current.Position));
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
            else if(Current.Text == "true") operations.Add(new Push_bool_Operation(true, Current.Position));
            else if(Current.Text == "false") operations.Add(new Push_bool_Operation(false, Current.Position));
            else if(Current.Text == "null") operations.Add(new Push_ptr_Operation(0, Current.Position));
            else if(Current.Text == "argc") operations.Add(new ArgcOperation(Current.Position));
            else if(Current.Text == "argv") operations.Add(new ArgvOperation(Current.Position));
            else if(Current.Text == "@") operations.Add(new MemReadOperation(0, Current.Position));
            else if(Current.Text == "!") operations.Add(new MemWriteOperation(0, Current.Position));
            else if(Current.Text == "@[]") operations.Add(new ArrayReadOperation(Current.Position));
            else if(Current.Text == "![]") operations.Add(new ArrayWriteOperation(Current.Position));
            else if(Utils.cttRegex.IsMatch(Current.Text)) {
                if(!int.TryParse(Current.Text.Substring(3, Current.Text.Length - 3), out int n) || n < 1) ErrorSystem.AddError_s(new InvalidCTTIndexError(Current));
                else operations.Add(new CTTOperation(n, Current.Position));
            } else if(Current.Text == "if") {
                Position position = Current.Position;
                NextWord();

                CodeBlock block = ParseCodeBlock(scope, new Scope(scope, currentProcedure), breakableBlock, currentProcedure);
                IfBlock ifBlock = new IfBlock(block, null, position);

                while(Current != null && Current.Type == WordType.Word && Current.Text == "else*") {
                    NextWord();

                    block = ParseCodeBlock(scope, new Scope(scope, currentProcedure), breakableBlock, currentProcedure);
                    ifBlock.Conditions.Add(block);

                    if(Current.Text != "if") ErrorSystem.AddError_i(new MissingIfError(Current.Position));
                    NextWord();

                    block = ParseCodeBlock(scope, new Scope(scope, currentProcedure), breakableBlock, currentProcedure);
                    ifBlock.Conditionals.Add(block);
                }

                if(Current != null && Current.Type == WordType.Word && Current.Text == "else") {
                    NextWord();

                    block = ParseCodeBlock(scope, new Scope(scope, currentProcedure), breakableBlock, currentProcedure);
                    ifBlock.BlockElse = block;
                }

                operations.Add(ifBlock);
                return;
            } else if(Current.Text == "while") {
                Position position = Current.Position;
                NextWord();

                WhileBlock whileBlock = new WhileBlock(null, null, position);

                CodeBlock block = ParseCodeBlock(scope, new Scope(scope, currentProcedure), breakableBlock, currentProcedure);
                whileBlock.Condition = block;

                if(Current.Type == WordType.Word && Current.Text != "do") ErrorSystem.AddError_i(new MissingDoError(Current.Position));
                NextWord();

                block = ParseCodeBlock(scope, new Scope(scope, currentProcedure), whileBlock, currentProcedure);
                whileBlock.Block = block;

                operations.Add(whileBlock);
                return;
            } else if(Current.Text == "do") {
                Position position = Current.Position;
                NextWord();

                DoWhileBlock doWhileBlock = new DoWhileBlock(null, null, position);

                CodeBlock block = ParseCodeBlock(scope, new Scope(scope, currentProcedure), doWhileBlock, currentProcedure);
                doWhileBlock.Block = block;

                if(Current.Type == WordType.Word && Current.Text != "while") ErrorSystem.AddError_i(new MissingWhileError(Current.Position));
                NextWord();

                block = ParseCodeBlock(scope, new Scope(scope, currentProcedure), breakableBlock, currentProcedure);
                doWhileBlock.Condition = block;

                operations.Add(doWhileBlock);
                return;
            } else if(Current.Text == "switch") {
                Position position = Current.Position;
                NextWord();

                SwitchBlock switchBlock = new SwitchBlock(position);

                if(Current.Type != WordType.Word || Current.Text != "{") ErrorSystem.AddError_i(new MissingOpeningBraceError(switchBlock, Current.Position));
                NextWord();

                while(Current.Type == WordType.Word && Current.Text == "case") {
                    NextWord();

                    CodeBlock block = ParseCodeBlock(scope, new Scope(scope, currentProcedure), breakableBlock, currentProcedure);
                    switchBlock.Cases.Add(block);

                    block = ParseCodeBlock(scope, new Scope(scope, currentProcedure), switchBlock, currentProcedure);
                    switchBlock.Blocks.Add(block);
                }

                if(Current.Type == WordType.Word && Current.Text == "default") {
                    NextWord();
                    switchBlock.DefaultBlock = ParseCodeBlock(scope, new Scope(scope, currentProcedure), switchBlock, currentProcedure);
                }

                if(Current.Type != WordType.Word || Current.Text != "}") ErrorSystem.AddError_i(new MissingClosingBraceError(switchBlock, Current.Position));
                NextWord();

                operations.Add(switchBlock);
                return;
            } else if(Current.Text == "continue") {
                if(breakableBlock == null || breakableBlock.BlockType == BlockType.Switch) ErrorSystem.AddError_s(new InvalidContinueError(Current.Position));
                else operations.Add(new ContinueOperation(breakableBlock, Current.Position));
            } else if(Current.Text == "break") {
                if(breakableBlock == null) ErrorSystem.AddError_s(new InvalidBreakError(Current.Position));
                else operations.Add(new BreakOperation(breakableBlock, Current.Position));
            } else if(Current.Text == "var") {
                Word varWord = Current;
                NextWord();
                if(Current == null) ErrorSystem.AddError_i(new IncompleteVariableDeclarationError(varWord, null));

                Word identifier = Current;
                if(!Utils.IsValidIdentifier(identifier)) ErrorSystem.AddError_i(new InvalidVariableIdentifierError(identifier));
                
                NextWord();
                if(Current.Text == null) ErrorSystem.AddError_i(new IncompleteVariableDeclarationError(varWord, identifier));

                // TODO: add support for endings like K or KB or something similar
                if(Utils.binaryRegex.IsMatch(Current.Text))                         RegisterVariable(scope, new DirectVariable(identifier, Convert.ToUInt32(Current.Text.Replace("_", ""), 2)));
                else if(Utils.octalRegex.IsMatch(Current.Text))                     RegisterVariable(scope, new DirectVariable(identifier, Convert.ToUInt32(Current.Text.Replace("_", ""), 8)));
                else if(Utils.decimalRegex.IsMatch(Current.Text))                   RegisterVariable(scope, new DirectVariable(identifier, Convert.ToUInt32(Current.Text.Replace("_", ""), 10)));
                else if(Utils.hexadecimalRegex.IsMatch(Current.Text))               RegisterVariable(scope, new DirectVariable(identifier, Convert.ToUInt32(Current.Text.Replace("_", ""), 16)));
                else if(_structs.ContainsKey(Current.Text))                         RegisterVariable(scope, new StructVariable(identifier, _structs[Current.Text]));
                else if(DataType.TryParse(Current.Text, out DataType dataType))     RegisterVariable(scope, new DataTypeVariable(identifier, dataType));
                else ErrorSystem.AddError_s(new InvalidVariableByteSizeError(Current));
            } else if(Utils.writeBytesRegex.IsMatch(Current.Text)) {
                string amountStr = Current.Text.Substring(1);
                bool isByte = byte.TryParse(amountStr, out byte amount);
                
                if(!isByte || (amount != 8 && amount != 16 && amount != 32 && amount != 64)) ErrorSystem.AddError_s(new InvalidMemReadWriteAmountError(amountStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1)));
                else operations.Add(new MemWriteOperation(amount, Current.Position));
            } else if(Utils.readBytesRegex.IsMatch(Current.Text)) {
                string amountStr = Current.Text.Substring(1);
                bool isByte = byte.TryParse(amountStr, out byte amount);

                if(!isByte || (amount != 8 && amount != 16 && amount != 32 && amount != 64)) ErrorSystem.AddError_s(new InvalidMemReadWriteAmountError(amountStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 1)));
                else operations.Add(new MemReadOperation(amount, Current.Position));
            } else if(Current.Text == "here") {
                string text = Current.Position.ToString();
                operations.Add(new StringOperation(_strings.Count, text.Length, Current.Position));
                _strings.Add(text);
            } else if(Current.Text == "chere") {
                operations.Add(new CStyleStringOperation(_strings.Count, Current.Position));
                _strings.Add(Current.Position + "\0");
            } else if(Utils.syscallRegex.IsMatch(Current.Text)) {
                string argcStr = Current.Text.Substring(7, Current.Text.Length - 7);
                if(!int.TryParse(argcStr, out int argc) || argc > 6 || argc < 0) ErrorSystem.AddError_s(new InvalidSyscallArgcError(argcStr, new Position(Current.Position.File, Current.Position.Line, Current.Position.Column + 7)));
                operations.Add(new SyscallOperation(argc, Current.Position));
            } else if(IsBraceOpen()) {
                CodeBlock block = ParseCodeBlock(scope, new Scope(scope, currentProcedure), breakableBlock, currentProcedure);
                operations.Add(block);
                return;
            } else if(Current.Text == "proc") {
                ParseProcedure(operations, scope, breakableBlock, currentProcedure, false);
                return;
            } else if(Current.Text == "inline") {
                NextWord();
                if(Current.Text != "proc") ErrorSystem.AddError_i(new MissingProcAfterInlineError(Current));
                ParseProcedure(operations, scope, breakableBlock, currentProcedure, true);
                return;
            } else if(Current.Text == "return") {
                if(currentProcedure == null) ErrorSystem.AddError_s(new ReturnOutsideProcedureError(Current.Position));
                operations.Add(new ReturnOperation(currentProcedure, scope, Current.Position));
            } else if(Current.Text == "cast") {
                Word castWord = Current;
                NextWord();

                if(Current == null) ErrorSystem.AddError_i(new IncompleteMultiCastError(castWord.Position));
                if(Current.Type != WordType.Word) ErrorSystem.AddError_i(new InvalidMultiCastError(castWord.Position));
                List<DataType> dataTypes = new List<DataType>();
                if(Current.Text == "(") {
                    NextWord();
                    while(Current.Text != ")" && Current.Type == WordType.Word && Current != null) {
                        if(Current.Type != WordType.Word) ErrorSystem.AddError_i(new InvalidMultiCastError(castWord.Position));

                        if(Utils.wildcardRegex.IsMatch(Current.Text)) dataTypes.Add(DataType.I_NONE);
                        else if(!DataType.TryParse(Current.Text, out DataType dataType)) ErrorSystem.AddError_s(new InvalidDataTypeError(Current));
                        else dataTypes.Add(dataType);

                        NextWord();
                    }
                    if(Current == null) ErrorSystem.AddError_i(new IncompleteMultiCastError(castWord.Position));
                    if(Current.Text != ")" || Current.Type != WordType.Word) ErrorSystem.AddError_i(new InvalidMultiCastError(castWord.Position));
                    NextWord();
                } else {
                    ErrorSystem.AddError_i(new IncompleteMultiCastError(castWord.Position));
                    return;
                }

                operations.Add(new MultiCastOperation(dataTypes.ToArray(), castWord.Position));
                return;
            } else if(Current.Text.StartsWith("cast(") && Current.Text.EndsWith(")")) {
                string dataTypeStr = Current.Text.Substring(5, Current.Text.Length-6);
                if(!DataType.TryParse(dataTypeStr, out DataType dataType)) ErrorSystem.AddError_i(new InvalidDataTypeError(new Word(new Position(Current.Position.File, Current.Position.Line, Current.Position.Column+5), dataTypeStr)));
                else operations.Add(new SingleCastOperation(dataType, Current.Position));
            } else if(Current.Text == "assert") {
                Position assertPosition = Current.Position;

                NextWord();
                if(Current == null) ErrorSystem.AddError_i(new IncompleteAssertError(assertPosition));
                CodeBlock block = ParseCodeBlock(scope, new Scope(scope, currentProcedure), breakableBlock, currentProcedure);
                CodeBlock condition = block;

                if(Current == null) ErrorSystem.AddError_i(new IncompleteAssertError(assertPosition));
                block = ParseCodeBlock(scope, new Scope(scope, currentProcedure), breakableBlock, currentProcedure);
                CodeBlock response = block;
                
                string text = assertPosition.ToString() + ": Error: Static assertion failed: ";
                operations.Add(new AssertOperation(condition, response, text.Length, _strings.Count, assertPosition));
                _strings.Add(text);

                return;
            } else if(Current.Text == "iota" || Current.Text == "reset") {
                operations.Add(new Push_uint64_Operation(Iota(Current.Text == "reset"), Current.Position));
            } else if(Current.Text == "let" || Current.Text == "peek") {
                Word bindWord = Current;
                NextWord();

                if(Current == null) ErrorSystem.AddError_i(new IncompleteBindingError(bindWord));
                if(Current.Type != WordType.Word) ErrorSystem.AddError_i(new InvalidBindingError(Current));
                List<Binding> bindings = new List<Binding>();
                if(Current.Text == "(") {
                    NextWord();
                    while(Current.Text != ")" && Current.Type == WordType.Word && Current != null) {
                        if(Utils.wildcardRegex.IsMatch(Current.Text)) bindings.Add(null);
                        else if(!Utils.IsValidIdentifier(Current)) ErrorSystem.AddError_s(new InvalidBindingError(Current));
                        else bindings.Add(new Binding(Current));

                        NextWord();
                    }
                    if(Current == null) ErrorSystem.AddError_i(new EOFInBindingListError(bindWord));
                    if(Current.Text != ")" || Current.Type != WordType.Word) ErrorSystem.AddError_i(new InvalidBindingListError(bindWord));
                    NextWord();
                } else {
                    if(Utils.wildcardRegex.IsMatch(Current.Text)) bindings.Add(null);
                    else if(!Utils.IsValidIdentifier(Current.Text)) ErrorSystem.AddError_s(new InvalidBindingError(Current));
                    else bindings.Add(new Binding(Current));

                    NextWord();
                }

                BindingScope bindingScope = new BindingScope(scope, currentProcedure, bindings);
                CodeBlock block = ParseCodeBlock(scope, bindingScope, breakableBlock, currentProcedure);
                BindingBlock bindingBlock = new BindingBlock(bindingScope, block, bindWord.Text == "let" ? BindingType.Let : BindingType.Peek, bindWord.Position);

                operations.Add(bindingBlock);
                return;
            } else if(Current.Text == "struct") {
                Word structWord = Current;
                NextWord();

                if(Current == null) ErrorSystem.AddError_i(new IncompleteStructDefinitionError(structWord));
                if(Current.Type != WordType.Word) ErrorSystem.AddError_i(new InvalidIdentifierError(Current));
                Word nameWord = Current;
                NextWord();

                Struct structt = new Struct(nameWord);
                while(Current != null && Current.Type == WordType.Word && Current.Text != "end") { // TODO: implement braces as block-marker for struct-definitions aswell
                    if(!DataType.TryParse(Current.Text, out DataType dataType)) ErrorSystem.AddError_s(new InvalidDataTypeError(Current));
                    NextWord();

                    if(Current == null) ErrorSystem.AddError_i(new IncompleteStructDefinitionError(structWord));
                    if(Current.Type != WordType.Word || Current.Text != ":") ErrorSystem.AddError_i(new MissingColonInStructDefinitionError(Current.Position));
                    NextWord();
                    
                    if(Current == null) ErrorSystem.AddError_i(new IncompleteStructDefinitionError(structWord));
                    if(!Utils.IsValidIdentifier(Current)) ErrorSystem.AddError_s(new InvalidIdentifierError(Current));
                    else if(structt.HasField(Current.Text)) ErrorSystem.AddError_s(new StructFieldRedefinitionError(Current, structt.GetField(Current.Text).Identifier.Position));
                    else structt.AddField(new StructField(Current, dataType, structt.GetByteSize()));
                    NextWord();
                }
                if(Current == null || Current.Type != WordType.Word || Current.Text != "end") ErrorSystem.AddError_i(new IncompleteStructDefinitionError(structWord));

                _structs.Add(structt.Name.Text, structt);
            } else if(Current.Text.StartsWith("sizeof(") && Current.Text.EndsWith(")")) {
                string type = Current.Text.Substring(7, Current.Text.Length-8);
                if(DataType.TryParse(type, out DataType dataType)) operations.Add(new Push_uint64_Operation((ulong) dataType.GetByteSize(), Current.Position));
                else if(_structs.ContainsKey(type)) operations.Add(new Push_uint64_Operation((ulong) _structs[type].GetByteSize(), Current.Position));
                else ErrorSystem.AddError_s(new InvalidTypeError(Current));
            } else {
                // TODO: add overflow protection for binary and hexadecimal numbers
                if(Utils.binaryRegex.IsMatch(Current.Text)) operations.Add(new Push_uint64_Operation(Convert.ToUInt64(Current.Text.Substring(2, Current.Text.Length-2).Replace("_", ""), 2), Current.Position));
                else if(Utils.octalRegex.IsMatch(Current.Text)) operations.Add(new Push_uint64_Operation(Convert.ToUInt64(Current.Text.Substring(1, Current.Text.Length-1).Replace("_", ""), 8), Current.Position));
                else if(Utils.decimalRegex.IsMatch(Current.Text)) operations.Add(new Push_uint64_Operation(Convert.ToUInt64(Current.Text.Replace("_", ""), 10), Current.Position));
                else if(Utils.hexadecimalRegex.IsMatch(Current.Text)) operations.Add(new Push_uint64_Operation(Convert.ToUInt64(Current.Text.Substring(2, Current.Text.Length-2).Replace("_", ""), 16), Current.Position));
                else {
                    Variable var = scope.GetVariable(Current.Text);
                    if(var != null) {
                        if(var.GetType() == typeof(Binding)) operations.Add(new PushBindingOperation((Binding) var, scope.GetBindingOffset((Binding) var), Current.Position));
                        else operations.Add(new VariableAccessOperation(var, Current.Position));
                    } else if(ProcedureExists(Current.Text)) operations.Add(new ProcedureCallOperation(Current, currentProcedure, Current.Position));
                    else {
                        if(Current.Text.StartsWith("@") || Current.Text.StartsWith("!")) { // CWDTODO: complete this step and add safeguards
                            string[] tokens = Current.Text.Substring(1).Split(".");
                            if(tokens.Length == 2 && _structs.ContainsKey(tokens[0])) {
                                Struct structt = _structs[tokens[0]];
                                if(!structt.HasField(tokens[1])) {
                                    Console.WriteLine("ERROR: Invalid field"); // CWDTODO: move into an error class
                                    Environment.Exit(1);
                                }
                                operations.Add(
                                    Current.Text[0] == '@'
                                        ? new StructFieldReadOperation(structt.GetField(tokens[1]), Current.Position)
                                        : new StructFieldWriteOperation(structt.GetField(tokens[1]), Current.Position)
                                );
                                NextWord();
                                return;
                            }
                        }
                        ErrorSystem.AddError_s(new UnexpectedWordError(Current));
                    }
                }
            }
            NextWord();
            return;
        }

        public ParseResult Parse() {
            List<Word> words;
            words = new Lexer(_text, _source).run();
            if(ErrorSystem.ShouldTerminateAfterStep()) ErrorSystem.WriteAndExit();
            
            words = new Preprocessor(_source, words, _assembler).run();
            if(ErrorSystem.ShouldTerminateAfterStep()) ErrorSystem.WriteAndExit();
            _words = words;

            _procs = new Dictionary<string, Dictionary<string, Procedure>>();
            _structs = new Dictionary<string, Struct>();
            _vars = new List<Variable>();
            _strings = new List<string>();

            // Actual parsing
            CodeBlock root = ParseCodeBlock(null, new Scope(null, null), null, null);

            new TypeChecker(root, _procs).run();
            if(ErrorSystem.ShouldTerminateAfterStep()) ErrorSystem.WriteAndExit();

            foreach(string name in _procs.Keys) {
                Dictionary<string, Procedure> overloads = _procs[name];
                foreach(string signature in overloads.Keys) if(overloads[signature].IsInlined) overloads.Remove(signature);
            }
            
            new Optimizer(root, _procs).run();

            return new ParseResult(root, _strings, _vars, _procs);
        }

    }

}