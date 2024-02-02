<Query Kind="Program">
  <Namespace>Xunit</Namespace>
</Query>

#load "xunit"

void Main()
{
  RunTests();  // Call RunTests() or press Alt+Shift+T to initiate testing.

	var list = new LinkedList<int>();
  list.Print();
  list.Count().Dump();
  list.Append(709);
	list.Prepend(5);
  list.Prepend(6);
  list.Append(9);
  list.Append(7);
  list.Prepend(3);
  list.Count().Dump();
  list.Print();
  list.Dump();
  
}

#region private::Tests

[Fact] void CountFirstTest() => Assert.True(new LinkedList<int>().Count() == 0);

[Fact] void CountLastTest()  
{
  var list = new LinkedList<string>();
  list.Prepend("Hello World!");
  Assert.True(list.Count() == 1);
}

[Fact]
void PrintEmptyTest()
{
  var list = new LinkedList<string>();
  list.Print();
  Assert.True(true /* if I got here I passed */);
}

[Fact]
void PrintNotEmptyTest()
{
  var list = new LinkedList<string>();
  list.Append("Jello Mold!");
  list.Print();
  Assert.True(true /* if I got here I passed */);
}

[Fact]
void AppendTest()
{
  var list = new LinkedList<string>();
  list.Append("Jello Mold!");
  Assert.True(list.Count() == 1);
}

[Fact]
void ToListTest()
{
  var list = new LinkedList<int>();
  list.Append(1);
  list.Append(2);
  list.Append(3);
  
  Assert.True(list.ToList()[1] == 2);
}

#endregion

public class LinkedList<T>
{
  public Node<T> Head { get; set; }

  public List<T> ToList()
  {
    var list = new List<T>();
    Traverse((x) => list.Add(x.Data));
    return list;
  }

  public void Prepend(T data)
  {
    Prepend(new Node<T>(data));
  }

  public void Prepend(Node<T> node)
  {
    node.Next = Head;
    Head = node;
  }

  public long Count()
  {
    long count = 0;
    Traverse((node) => count++);
    return count;
  }

  public void Print()
  {
    if(Head != null)
      Traverse((x, i) => x.Print(i));
    else
      Debug.WriteLine("List empty.");
  }

  public void Traverse(Action<Node<T>> action)
  {
    Traverse((x, i) => action(x));
  }

  public void Traverse(Action<Node<T>, int> action)
  {
    if (Head == null) return;

    var current = Head;
    bool _continue = true;
    int index = 0;

    while (_continue)
    {
      _continue = current.Next != null;

      action(current, ++index);
      current = current.Next;
    }
  }

  public void Append(T data)
  {
    Append(new Node<T>(data));
  }

  public void Append(Node<T> node)
  {
    if(Head == null)
      Head = node;
    else
      Traverse((x) => { if (x.Next == null) x.Next = node; });
  }
}


public class Node<T>
{
  public T Data { get; set; }
  public Node<T> Next { get; set; }
  public Node(T data)
  {
    Data = data;
  }
  
  public void Print()
  {
    Debug.Write($"{Data}=>");
    if (this.Next == null)
      Debug.Write($"(end)");
  }

  public void Print(int i)
  {
    Debug.Write($"{i}:{Data}=>");
    if (this.Next == null)
      Debug.Write($"(end)");
  }
}

