using System.Collections.Generic;

namespace IonS {

    class Macro {
        public Macro(Word key, Word[] words) {
            Key = key;
            Words = words;
        }

        public Word Key { get; }
        public Word[] Words { get; }
    }

    class MacroPreprocessor {

        private Word[] _words;
        private Dictionary<string, Macro> _macros;

        public MacroPreprocessor(Word[] words) {
            _words = words;
        }

        private Macro GetMacro(string key) {
            _macros.TryGetValue(key, out Macro macro);
            return macro;
        }

        private void ExpandMacro(Word[] _words, List<Word> words, int index) {
            Word word = _words[index];
            Macro macro = GetMacro(word.Text);
            if(macro == null) return;
            for(int i = 0; i < macro.Words.Length; i++) {
                Word word2 = macro.Words[i];
                Macro macro2 = GetMacro(word2.Text);
                if(macro2 != null) ExpandMacro(macro.Words, words, i);
                else {
                    word2.ExpandedFrom = word; // TODO: rework ExpandedFrom (see IncludedFrom)
                    words.Add(word2);
                }
            }
        }

        private Error CollectMacros() {
            List<Word> words = new List<Word>();
            _macros = new Dictionary<string, Macro>();
            for(int i = 0; i < _words.Length; i++) {
                Word word = _words[i];
                if(word.Text == "#macro") {
                    if(i++ == _words.Length-1) return new IncompleteMacroDefinitionError(word, null);
                    Word key = _words[i];

                    if(!Keyword.isValidIdenfitier(key.Text)) return new InvalidMacroKeyError(key);

                    if(i++ == _words.Length-2) return new IncompleteMacroDefinitionError(word, key);

                    Macro macro = GetMacro(key.Text);
                    if(macro != null) return new MacroRedefinitionError(macro.Key, key);

                    List<Word> words1 = new List<Word>();
                    int openBraces = 0;
                    if(_words[i].Text != "{") words1.Add(_words[i]);
                    else {
                        while(true) {
                            i++;
                            if(i >= _words.Length) return new IncompleteMacroDefinitionError(word, key);
                            if(_words[i].Text == "{") openBraces++;
                            if(_words[i].Text == "}") {
                                if(openBraces > 0) openBraces--;
                                else break;
                            }
                            words1.Add(_words[i]);
                        }
                    }
                    _macros.Add(key.Text, new Macro(key, words1.ToArray()));
                } else words.Add(word);
            }
            _words = words.ToArray();
            return null;
        }

        public Word[] ExpandMacros() {
            List<Word> words = new List<Word>();
            for(int i = 0; i < _words.Length; i++) {
                Word word = _words[i];
                Macro macro = GetMacro(word.Text);
                if(macro != null) ExpandMacro(_words, words, i);
                else words.Add(word);
            }
            return words.ToArray();
        }

        public PreprocessorResult run() {
            var result = CollectMacros();
            if(result != null) return new PreprocessorResult(null, result);
            return new PreprocessorResult(ExpandMacros(), null);
        }

    }

}