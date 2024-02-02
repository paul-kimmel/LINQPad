<Query Kind="Program" />

void Main()
{
	Tree<int> tree = new PrettyTree<int>(10);
	tree.Insert(3);
	
	tree.Insert(11);
	tree.Insert(-1);
	tree.Insert(55);
	tree.Insert(19);
	tree.Insert(21);
	tree.Insert(33);
	tree.Insert(2);
	//Console.WriteLine(tree);
	
	(tree as PrettyTree<int>).PrintTree();

}

public class PrettyTree<T> : Tree<T> where T : IComparable
{
	public PrettyTree(T o) : base(o) {}
	
	public void PrintTree()
	{
		StringBuilder builder = new StringBuilder();
		var max = GetMaxWidth();
		
		for(int i=0; i<max; i++)
		{
			var levelNodes = GetLevelNodes(i);
			builder.AppendLine(string.Join(" ", levelNodes.Select(o=>o.Data)));
		}
		
		Console.WriteLine(builder.ToString());
	}
	
	private void Traverse(Node<T> node, List<Node<T>> results)
	{
		if (node == null) return;
		
		Traverse(node.LeftChild, results);

		results.Add(node);

		Traverse(node.RightChild, results);
	}
	
	private List<Node<T>> GetLevelNodes(int level)
	{
		List<Node<T>> results = new List<Node<T>>();
		Traverse(Root,results);
		return results.Where(o => o.Level == level).ToList();
	}
	

	public int GetMaxWidth()
	{
		List<Node<T>> results = new List<Node<T>>();
		Traverse(Root, results);
		
		return results.Count > 0 ? results.Max(o=>o.Level) : 0;
	}
}


public class Tree<T> where T : System.IComparable
{
	public Tree(T o)
	{
		Root.Data = o;	
	}
	
	public virtual void Print(Node<T> node)
	{
		Console.WriteLine("{0}: {1}", node.Level, node.Data);
	}
	
	public void Infix()
	{
		Infix(Root);
	}
	
	private void Infix(Node<T> node)
	{
		if(node == null) return;
		Infix(node.LeftChild);
		
		Print(node);
		
		Infix(node.RightChild);
	}
	
	public void Prefix()
	{
		Prefix(Root);		
	}
	
	private void Prefix(Node<T> node)
	{
		if (node == null) return;
		if (node == null) return;
		Print(node);
		Prefix(node.LeftChild);
		Prefix(node.RightChild);
	}

	public void Postfix()
	{
		Postfix(Root);
	}
	
	private void Postfix(Node<T> node)
	{
		if(node == null) return;
		Postfix(node.LeftChild);
		
		Postfix(node.RightChild);
		
		Print(node);
	}
	
	
	public Node<T> Root { get; set; } = new Node<T>();

	public void Insert(T value)
	{
		InsertAt(Root, new Node<T>() { Data = value });
	}
	
	public void Insert(Node<T> node)
	{
		InsertAt(Root, node);
	}
	
	private void InsertAt(Node<T> current, Node<T> node)
	{
		node.Level++;
		
		if(node.Data.CompareTo(current.Data) < 0)
		{
			if(current.LeftChild == null)
			{
				current.LeftChild = node;
				node.Parent = current.LeftChild;
				return;
			}
			
			InsertAt(current.LeftChild, node);
		}
		
		if(node.Data.CompareTo(current.Data) > 0)
		{
			if (current.RightChild == null)
			{
				current.RightChild = node;
				node.Parent = current.RightChild;
				return;
			}
			
			InsertAt(current.RightChild, node);
		}
	}
	
}

public class Node<T>
{
	public T Data{ get; set; }
	public int Level{ get; set; } = 0;
	public Node<T> Parent{ get; set; }
	public Node<T> LeftChild{ get; set; }
	public Node<T> RightChild{ get; set; }
	
}