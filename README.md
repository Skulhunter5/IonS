# IonS
IonS is a compiled stackbased programming language.\
Technically the "compiler" is just a transcriber that generates nasm x86_64 assembly though.\
\
IonS is currently written in C# but I want to make it a self-hosted language.

## Grammar
In IonS every operation is executed one after another, which results in something like ``` 1 2 + . ``` which outputs ```3```.
Step by step:
- ```1```: pushes 1 onto the stack\
Stack: [1]
- ```2```: pushes 2 onto the stack\
Stack: [1, 2]
- ```+```: adds the two topmost items and pushes the result back onto the stack\
Stack: [3]
- ```.```: prints the top of the stack to stdout\
Stack: []
### Operations
* A simple integer such as ```0```, ```1``` or ```12345``` so generally ```n``` pushes that integer n onto the stack.
``` Python
Push(n)
```
* ```+```
Adds the topmost 2 items and pushes the result back onto the stack\
[x, x, a, b] -> [x, a+b]
``` Python
b = Pop()
a = Pop()
Push(a + b)
```
* ```-```
Subtracts the topmost 2 items and pushes the result back onto the stack\
[x, a, b] -> [x, a-b]
``` Python
b = Pop()
a = Pop()
Push(a - b)
```
* ```*```
Multiplies the topmost 2 items and pushes the result back onto the stack\
[x, a, b] -> [x, a*b]
``` Python
b = Pop()
a = Pop()
Push(a * b)
```
* ```/```
Divides the topmost 2 items (integer division) and pushes the result back onto the stack\
[x, a, b] -> [x, a//b]
``` Python
b = Pop()
a = Pop()
Push(a / b)
```
* ```%```
Calculates the modulo of the topmost 2 items and pushes the result back onto the stack\
[x, a, b] -> [x, a%b]
``` Python
b = Pop()
a = Pop()
Push(a % b)
```
* ```.```
Pops the topmost item off the stack and prints it to stdout\
[x, a] -> [x]\
stdout: ```a```
``` Python
a = Pop()
Print(a)
```
* ```drop```
Pops the topmost item off the stack\
[x, a] -> [x]
``` Python
Pop()
```
* ```2drop```
Pops the topmost 2 items off the stack\
[x, a, b] -> [x]
``` Python
Pop()
Pop()
```
* ```dup```
Duplicates the topmost item\
[x, a] -> [x, a, a]
``` Python
a = Pop()
Push(a)
Push(a)
```
* ```dup```
Duplicates the topmost 2 items\
[x, a, b] -> [x, a, b, a, b]
``` Python
b = Pop()
a = Pop()
Push(a)
Push(b)
Push(a)
Push(b)
```
* ```over```
Copies the second item to the top of the stack\
[x, a, b] -> [x, a, b, a]
``` Python
b = Pop()
a = Pop()
Push(a)
Push(b)
Push(a)
```
* ```2over```
Copies the third and fourth item to the top of the stack\
[x, a, b, c, d] -> [x, a, b, c, d, a, b]
``` Python
d = Pop()
c = Pop()
b = Pop()
a = Pop()
Push(a)
Push(b)
Push(c)
Push(d)
Push(a)
Push(b)
```
* ```exit```
Exits with the topmost item as the exitcode
### Control statements
* **if-statement**:\
Syntax: ```... if 'if-block' end ...```\
Only executes the 'if-block' if the topmost item is not 0, otherwise it jumps after the end-marker
* **while-loop**:\
Syntax: ```... while 'condition' do 'while-block' end ...```\
Executes the 'while-block' as long as the condition is true\
The while-statement executes the condition and does a check just like the if-statement but when the 'while-block' is done, it jumps back to the beginning of the 'condition' (An example can be found in the tests directory)

## Additional information
* The program always exits with the topmost item as the exitcode.\
It is recommended to always end the program with ```0 exit``` to not get a stack underflow situation.