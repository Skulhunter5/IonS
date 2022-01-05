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

    class MacroExpansionResult : Result {
        public MacroExpansionResult(Word[] words, Error error) : base(error) {
            Words = words;
        }
        public Word[] Words { get; }
    }

    class MacroPreprocessor {

        private readonly Word[] _words;

        public MacroPreprocessor(Word[] words) {
            _words = words;
        }

        private static Macro GetMacro(List<Macro> macros, string key) {
            foreach(Macro macro in macros) if(macro.Key.Text == key) return macro;
            return null;
        }

        private void expand(List<Macro> macros, Word[] _words, List<Word> words, int index) {
            Word word = _words[index];
            Macro macro = GetMacro(macros, word.Text);
            if(macro == null) return;
            for(int i = 0; i < macro.Words.Length; i++) {
                Word word2 = macro.Words[i];
                Macro macro2 = GetMacro(macros, word2.Text);
                if(macro2 != null) expand(macros, macro.Words, words, i);
                else {
                    word2.ExpandedFrom = word;
                    words.Add(word2);
                }
            }
        }

        public MacroExpansionResult run() {
            List<Word> words = new List<Word>();
            List<Macro> macros = new List<Macro>();
            for(int i = 0; i < _words.Length; i++) {
                Word word = _words[i];
                if(word.Text == "macro") {
                    if(i++ == _words.Length-1) return new MacroExpansionResult(null, new IncompleteMacroDefinitionError(word, null));
                    Word key = _words[i];

                    if(Keyword.isReserved(key.Text) || long.TryParse(key.Text, out long _)) return new MacroExpansionResult(null, new InvalidMacroKeyError(key));

                    if(i++ == _words.Length-2) return new MacroExpansionResult(null, new IncompleteMacroDefinitionError(word, key));

                    Macro macro = GetMacro(macros, key.Text);
                    if(macro != null) return new MacroExpansionResult(null, new MacroRedefinitionError(macro.Key, key));

                    List<Word> words1 = new List<Word>();
                    if(_words[i].Text != "{") words1.Add(_words[i]);
                    else {
                        while(true) {
                            i++;
                            if(i >= _words.Length) return new MacroExpansionResult(null, new IncompleteMacroDefinitionError(word, key));
                            if(_words[i].Text == "}") break;
                            words1.Add(_words[i]);
                        }
                    }
                    macros.Add(new Macro(key, words1.ToArray()));
                    continue;
                } else {
                    Macro macro = GetMacro(macros, word.Text);
                    if(macro != null) {
                        expand(macros, _words, words, i);
                        continue;
                    }
                }
                words.Add(word);
            }
            return new MacroExpansionResult(words.ToArray(), null);
        }
    }

}