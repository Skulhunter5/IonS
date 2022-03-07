using System;
using System.IO;
using System.Collections.Generic;

namespace IonS {

    class Macro {
        public Macro(Word key, List<Word> words) {
            Key = key;
            Words = words;
        }

        public Word Key { get; }
        public List<Word> Words { get; }
        
    }

    class Preprocessor {

        private readonly string _source;
        private readonly List<Word> _words;

        private readonly Assembler _assembler;

        private int i;
        private List<Word> words;

        private Dictionary<string, Position> _symbols;
        private Dictionary<string, Macro> _macros;

        public Preprocessor(string source, List<Word> words, Assembler assembler) {
            _source = source;
            _words = words;
            _assembler = assembler;
        }

        private Macro GetMacro(string key) {
            _macros.TryGetValue(key, out Macro macro);
            return macro;
        }

        private void CollectMacro() {
            int start = i;

            Word word = words[i];

            if(i++ == words.Count-1) ErrorSystem.AddError_i(new IncompleteMacroDefinitionError(word, null));
            Word key = words[i];

            if(!Utils.IsValidIdentifier(key)) ErrorSystem.AddError_i(new InvalidMacroKeyError(key));

            if(i++ == words.Count-2) ErrorSystem.AddError_i(new IncompleteMacroDefinitionError(word, key));

            Macro macro = GetMacro(key.Text);
            if(macro != null) ErrorSystem.AddError_i(new MacroRedefinitionError(macro.Key, key));

            List<Word> words1 = new List<Word>();
            int openBraces = 0;
            if(words[i].Text != "{") words1.Add(words[i]);
            else {
                while(true) {
                    i++;
                    if(i >= words.Count) ErrorSystem.AddError_i(new IncompleteMacroDefinitionError(word, key));
                    if(words[i].Text == "{") openBraces++;
                    if(words[i].Text == "}") {
                        if(openBraces > 0) openBraces--;
                        else break;
                    }
                    words1.Add(words[i]);
                }
            }
            _macros.Add(key.Text, new Macro(key, words1));
            i++;
            words.RemoveRange(start, i - start);
            i = start;
        }

        private void ExpandMacro(Macro macro, List<Word> words) {
            foreach(Word word in macro.Words) word.ExpandedFrom = words[i];
            words.RemoveAt(i);
            words.InsertRange(i, macro.Words);
        }

        // Includes

        private string AlterPath(string original, string alterBy) {
            string temp = "";

            temp = Path.GetDirectoryName(original);
            string[] steps = alterBy.Split('/');
            if(steps[0] != ".") return alterBy;
            for(int i = 1; i < steps.Length; i++) {
                string step = steps[i];
                if(step == "..") temp = new DirectoryInfo(temp).Parent.ToString();
                else temp += "\\" + step;
            }

            return temp;
        }

        private void Include() {
            Word nameWord = words[i+1];
            words.RemoveRange(i, 2);

            if(nameWord.GetType() != typeof(StringWord) || ((StringWord) nameWord).StringType != "") ErrorSystem.AddError_i(new FilePathNotAStringLiteralError(nameWord));
            string filename = AlterPath(_source, nameWord.Text);
            
            if(!File.Exists(filename)) ErrorSystem.AddError_i(new FileNotFoundError(filename, Directory.GetCurrentDirectory(), nameWord.Position));
            string path = Path.GetFullPath(filename);
            string text = File.ReadAllText(path).Replace("\r\n", "\n");
            List<Word> rwords = new Lexer(text, path).run();
            if(ErrorSystem.ShouldTerminateAfterStep()) ErrorSystem.WriteAndExit();

            foreach(Word word in rwords) if(word.IncludedFrom == null) word.IncludedFrom = new Position[] {word.Position, nameWord.Position};
            words.InsertRange(i, rwords);
        }

        private static readonly List<string> PredefinedSymbols = new List<string> {
            "OS_LINUX",
            "ARCH_X86_64",
        };
        private void InitSymbols() {
            _symbols = new Dictionary<string, Position>();
            if(_assembler == Assembler.nasm_linux_x86_64 || _assembler == Assembler.fasm_linux_x86_64 || _assembler == Assembler.iasm_linux_x86_64) {
                _symbols.Add("OS_LINUX", null);
                _symbols.Add("ARCH_X86_64", null);
            } else throw new NotImplementedException();
        }

        // RUN

        public List<Word> run() {
            words = new List<Word>();
            words.AddRange(_words);

            InitSymbols();

            _macros = new Dictionary<string, Macro>();

            Stack<Word> openDirectives = new Stack<Word>();

            i = 0;
            while(i < words.Count) {
                if(words[i].GetType() == typeof(StringWord) || words[i].GetType() == typeof(CharWord)) i++;

                if(words[i].Text.StartsWith("//")) {
                    int start = i;
                    int line = words[i].Position.Line;
                    while(words[i++].Position.Line == line) if(i >= words.Count) break;
                    if(i < words.Count) i--;
                    words.RemoveRange(start, i - start);
                    i = start;
                    continue;
                }

                if(words[i].Text.StartsWith("/*")) {
                    int start = i;
                    while(!words[i++].Text.EndsWith("*/")) if(i >= words.Count) ErrorSystem.AddError_i(new EOFInBlockCommentError(words[start].Position));
                    words.RemoveRange(start, i - start);
                    i = start;
                    continue;
                }

                if(words[i].Text.StartsWith('#')) {
                    if(words[i].Text == "#macro") {
                        CollectMacro();
                        continue;
                    } else if(words[i].Text == "#include") {
                        Include();
                        continue;
                    } else if(words[i].Text == "#define") { // TODO: make sure the symbol is valid (character-wise)
                        if(i+1 == words.Count) ErrorSystem.AddError_i(new IncompletePreprocessorDirectiveError(words[i]));

                        Word symbolWord = words[i+1];
                        if(!Utils.symbolRegex.Match(symbolWord.Text).Success) ErrorSystem.AddError_i(new InvalidSymbolError(symbolWord, false));
                        if(PredefinedSymbols.Contains(symbolWord.Text)) ErrorSystem.AddError_i(new InvalidSymbolError(symbolWord, true));
                        
                        if(_symbols.ContainsKey(symbolWord.Text)) ErrorSystem.AddError_i(new PreprocessorSymbolRedefinitionError(symbolWord, _symbols.GetValueOrDefault(symbolWord.Text, null)));

                        _symbols.Add(symbolWord.Text, symbolWord.Position);
                        words.RemoveRange(i, 2);

                        continue;
                    } else if(words[i].Text == "#ifdef") { // TODO: make sure the symbol is valid (character-wise)
                        if(i+1 == words.Count) ErrorSystem.AddError_i(new IncompletePreprocessorDirectiveError(words[i]));

                        Word symbolWord = words[i+1];
                        if(!Utils.symbolRegex.Match(symbolWord.Text).Success) ErrorSystem.AddError_i(new InvalidSymbolError(symbolWord, false));
                        
                        if(_symbols.ContainsKey(symbolWord.Text)) {
                            openDirectives.Push(words[i]);
                            words.RemoveRange(i, 2);
                        } else {
                            int start = i++;
                            while(words[i++].Text != "#endif") if(i >= words.Count) ErrorSystem.AddError_i(new MissingPreprocessorDirectiveError("#endif", words[start]));
                            words.RemoveRange(start, i - start);
                            i = start;
                        }

                        continue;
                    } else if(words[i].Text == "#ifndef") { // TODO: make sure the symbol is valid (character-wise)
                        if(i+1 == words.Count) ErrorSystem.AddError_i(new IncompletePreprocessorDirectiveError(words[i]));

                        Word symbolWord = words[i+1];
                        if(!Utils.symbolRegex.Match(symbolWord.Text).Success) ErrorSystem.AddError_i(new InvalidSymbolError(symbolWord, false));

                        if(_symbols.ContainsKey(symbolWord.Text)) {
                            int start = i++;
                            while(words[i++].Text != "#endif") if(i >= words.Count) ErrorSystem.AddError_i(new MissingPreprocessorDirectiveError("#endif", words[start]));
                            words.RemoveRange(start, i - start);
                            i = start;
                        } else {
                            openDirectives.Push(words[i]);
                            words.RemoveRange(i, 2);
                        }

                        continue;
                    } else if(words[i].Text == "#endif") {
                        if(openDirectives.Count == 0) ErrorSystem.AddError_i(new UnexpectedPreprocessorDirectiveError(_words[i]));
                        else {
                            openDirectives.Pop();
                            words.RemoveAt(i);
                            continue;
                        }
                    } else ErrorSystem.AddError_i(new UnknownPreprocessorDirectiveError(_words[i]));
                }

                Macro macro = GetMacro(words[i].Text);
                if(macro != null) ExpandMacro(macro, words);
                else i++;
            }

            if(openDirectives.Count > 0) ErrorSystem.AddError_s(new UnclosedPreprocessorDirectivesError(openDirectives));

            return words;
        }

    }

}