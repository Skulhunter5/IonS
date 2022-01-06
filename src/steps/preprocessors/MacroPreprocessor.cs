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
        private Macro[] _macros;

        public MacroPreprocessor(Word[] words) {
            _words = words;
        }

        private Macro GetMacro(string key) {
            foreach(Macro macro in _macros) if(macro.Key.Text == key) return macro;
            return null;
        }

        private Macro GetMacro(List<Macro> macros, string key) {
            foreach(Macro macro in macros) if(macro.Key.Text == key) return macro;
            return null;
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
            List<Macro> macros = new List<Macro>();
            for(int i = 0; i < _words.Length; i++) {
                Word word = _words[i];
                if(word.Text == "macro") {
                    if(i++ == _words.Length-1) return new IncompleteMacroDefinitionError(word, null);
                    Word key = _words[i];

                    if(Keyword.isReserved(key.Text) || long.TryParse(key.Text, out long _)) return new InvalidMacroKeyError(key);

                    if(i++ == _words.Length-2) return new IncompleteMacroDefinitionError(word, key);

                    Macro macro = GetMacro(macros, key.Text);
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
                    macros.Add(new Macro(key, words1.ToArray()));
                } else words.Add(word);
            }
            _words = words.ToArray();
            _macros = macros.ToArray();
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