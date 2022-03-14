# Internal changes
- Redo stuff like ShLOperation.TypeCheck
- Move argument-, binding-, ... parsing into "parselist" function
- See if linear comparison of every object in a list is faster or slower than checking if a key exists inside a Dictionary (useful for Keyword.isKeyword()) or check if a HashSet would help

# Implement for IonS
- look into capturing all words and strings through regex or maybe do list-parsing through regex
- Rework words/tokens
- Implement function prototypes
- Make switch statements viable (just assume a fallthrough at every point and then check if all endings have the same signature) \
-> conditionally breaking or non-breaking cases have to have a signature of none \
-> all breaks have to have the same signature

# Notes
- Add HasSkipped to TypeCheckContract
- REWORK THE IMPLICIT CAST SYSTEM (e.g. everything can be cast to boolean without a warning (at least for an if, while, ...))

# TODO
- Create a signature class and create a call operation like ```(n)``` \
create Signatures, DataType.Function and call ```ptr<function<Signature>>``` | create DataType.Address and make them callable \
maybe add capability of unknown number of arguments at compiletime with ```(...)```
- Make int64 instead of uint64 the standard type and add possibility to input uint64 (maybe with uint64:(...) and make this a general concept like allowing this for pointers too)
- Add error prorities to make sure errors causing other errors are put further up in the list
- Rework which errors are instantly terminal and which aren't (see Invalid...Error)
- Generalize incomplete block detection (create new function with block-start as parameter, check if the next word == null; if so, throw error, otherwise just return the text of that word/that word)
- check that variable names are actually valid (correct and normalize invalid identifier errors)
- check for duplicate identifier usage (also cross-reference between e.g. variables and functions (maybe store all used identifiers in one List/Dictionary/HashSet/...))
- normalize Incomplete/EOF errors
- make signature errors more readable
- normalize some parsing-steps (like generalizing: *keyword* *identifier*)
- do something against newlines/tabulators/etc ruining error messages
