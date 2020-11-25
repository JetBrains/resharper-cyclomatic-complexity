public class Foo
{
  public bool Blah { get; set; }

  // C# treats bool values and negation operators as conditionals,
  // bumping the CC incorrectly
  public void Thing(bool val)
  {
    Blah = val;
    Blah = val;
    Blah = val;
    Blah = val;
    Blah = val;
    Blah = val;
    Blah = val;
    Blah = !val;
    Blah = !val;
    Blah = !val;
    Blah = !val;
    Blah = !val;
    Blah = !val;
    Blah = !val;
    Blah = !val;
    Blah = !val;
    Blah = !val;
    Blah = !val;
    Blah = !val;
    Blah = !val;
    Blah = !val;
    Blah = !val;
    Blah = !val;
  }
}
