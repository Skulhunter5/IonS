using System.IO;
using System.Collections.Generic;

namespace IonS {

    class CommentPreprocessor {

        private readonly Word[] _words;

        public CommentPreprocessor(Word[] words) {
            _words = words;
        }

        public PreprocessorResult run() {
            List<Word> words = new List<Word>();
            for(int i = 0; i < _words.Length; i++) {
                Word word = _words[i];
                if(word.Text == "/*") {
                    Position start = word.Position;
                    while(word.Text != "*/") {
                        i++;
                        if(i >= _words.Length) break;
                        word = _words[i];
                    }
                    if(i >= _words.Length) return new PreprocessorResult(null, new EOFInBlockCommentError(start));
                } else words.Add(word);
            }
            return new PreprocessorResult(words.ToArray(), null);
        }

    }

}