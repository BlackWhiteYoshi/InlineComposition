# InlineComposition

A source generator that merges the content of other classes into one class.
A simple workaround for struct inheritance or multiple inheritance.

- Inlined members are fields, properties, events and methods (including constructor and finalizer).
- Attributes and summaries of inlined members get also inlined.
- Inheritance and implements declaration are also inlined.
- Mixing classes and structs works fine (inline struct in class and vice versa).

For documentation or sourcecode see [github.com/BlackWhiteYoshi/InlineComposition](https://github.com/BlackWhiteYoshi/InlineComposition).
