# Internal changes
- Redo stuff like ShLOperation.TypeCheck
- Move argument-, binding-, ... parsing into "parselist" function
- See if linear comparison of every object in a list is faster or slower than checking if a key exists inside a Dictionary (useful for Keyword.isKeyword())

# Implement for IonS
- look into capturing all words and strings through regex or maybe do list-parsing through regex
- Rework Error-Warning-System to maybe allow for single syntax error but multiple other errors and also allow warnings (see TypeChecker & ExitOperation)
- Rework words/tokens
- Implement function prototypes
- Implement ; as empty block
- Implement heap-allocation handler
- Implement structs
- Make switch statements viable (just assume a fallthrough at every point and then check if all endings have the same signature) \
-> conditionally breaking or non-breaking cases have to have a signature of none \
-> all breaks have to have the same signature

# Implement in IonS

# Notes
- Add HasSkipped to TypeCheckContract
- REWORK THE IMPLICIT CAST SYSTEM (e.g. everything can be cast to boolean without a warning (at least for an if, while, ...))

# TODO
- check that variable names aren't strings/chars
- check that variable names are actuallly valid (correct and normalize invalid identifier errors)
- check for duplicate identifier usage (also cross-reference between e.g. variables and procedures (maybe store all used identifiers in one List/Dictionary/HashSet/...))
- normalize Incomplete/EOF errors
- make signature errors more readable
- normalize some parsing-steps (like generalizing: *keyword* *identifier*)
