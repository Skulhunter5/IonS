namespace IonS {

    class PreprocessorResult : Result {
        public PreprocessorResult(Word[] words, Error error) : base(error) {
            Words = words;
        }
        public Word[] Words { get; }
    }

}