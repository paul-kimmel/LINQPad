<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Windows.Forms.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Accessibility.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Deployment.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Serialization.Formatters.Soap.dll</Reference>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Drawing.Drawing2D</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
</Query>

void Main()
{
	
	Tree<int> tree = new PrettyTree<int>(5);
  //Tree<int> tree = new Tree<int>(5);
	
	Random random = new Random(DateTime.Now.Millisecond);
	for(int i = 0; i < 50; i++)
    //for(int i = 0; i < 100000; i++)
    //for(int i = 0; i < 100; i++)
	{
		//tree.Insert(random.Next(-100000, 100000));
        tree.Insert(random.Next(-100, 100));
	}
	
	tree.Print(tree.Root);
}

public class PrettyTree<T> : Tree<T> where T : IComparable
{
	public PrettyTree(T o) : base(o) { }

	private Bitmap bitmap{get; set; } = new Bitmap(dimension,dimension);
	private Graphics graphics = null;

	public override void Print(Node<T> node)
	{
		using (graphics = Graphics.FromImage(bitmap))
		{
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			
			var max = GetMaxWidth();

			Action<Node<T>> action = n=> PrintNode(n);

			Traverse(node, action);
			
			bitmap.Dump();
		}
	}

	private static Font font = new Font("Times New Roman", 12);

	protected virtual void PrintNode(Node<T> node)
	{
		DrawEllipse(node);
		DrawConnector(node);
		DrawContent(node);
	}

	protected virtual void DrawEllipse(Node<T> node)
	{
		graphics.FillEllipse(Brushes.Silver, new Rectangle(node.GetX(), node.GetY(), node.GetNodeDimenion(font, node_dimension), node_dimension));
		graphics.DrawEllipse(Pens.Black, new Rectangle(node.GetX(), node.GetY(), node.GetNodeDimenion(font, node_dimension), node_dimension));
	}

	protected virtual void DrawConnector(Node<T> node)
	{
		if (node.NeedsConnector())
			graphics.DrawLine(Pens.Black, node.GetConnectionPoint(font), node.GetParentConnectionPoint(font));
	}
	
	protected virtual void DrawContent(Node<T> node)
	{
		graphics.DrawString(node.Data.ToString(), font, Brushes.Black, node.GetX() + 3, node.GetY() + 1);
	}

	

	private void Traverse(Node<T> node, Action<Node<T>> action)
	{
		if (node == null) return;
		
		Traverse(node.Left, action);
	
		action(node);
				
		Traverse(node.Right, action);
		
	}

	private void Traverse(Node<T> node, List<Node<T>> results)
	{
		if (node == null) return;

		Traverse(node.Left, results);

		results.Add(node);

		Traverse(node.Right, results);
	}


	private List<Node<T>> GetLevelNodes(int level)
	{
		List<Node<T>> results = new List<Node<T>>();
		Traverse(Root, results);
		return results.Where(o => o.Level == level).ToList();
	}


	public int GetMaxWidth()
	{
		return dimension;
	}
}

public class Tree<T> where T : System.IComparable
{
	//public const int dimension = 3500;
    public const int dimension = 1000;
    
	public const int node_dimension = 20;

	public Tree(T o)
	{
		Root.Data = o;
	}
	
	public void Infix()
	{
		Infix(Root);
	}

	public virtual void Print(Node<T> node)
	{
		Console.WriteLine($"{node.Level}: {node.Data}");
	}

	private void Infix(Node<T> node)
	{
		if (node == null) return;
		Infix(node.Left);

		Print(node);

		Infix(node.Right);
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
		Prefix(node.Left);
		Prefix(node.Right);
	}

	public void Postfix()
	{
		Postfix(Root);
	}

	private void Postfix(Node<T> node)
	{
		if (node == null) return;
		Postfix(node.Left);

		Postfix(node.Right);

		Print(node);
	}
	
	public Node<T> Root { get; set; } = new Node<T>(dimension, node_dimension);

	public void Insert(T value)
	{
		InsertAt(Root, new Node<T>(dimension, node_dimension) { Data = value });
	}

	public void Insert(Node<T> node)
	{
		InsertAt(Root, node);
	}

	private void InsertAt(Node<T> current, Node<T> node)
	{
		node.Level++;

		if(CheckInsertLeft(current, node)) return;
		if(CheckInsertRight(current, node)) return;
	}
	
		
	private bool CheckInsertRight(Node<T> current, Node<T> node)
	{
		if (ShouldInsertRight(current, node))
		{
			if (current.Right == null)
			{
				current.InsertRight(node);
				return true;
			}

			InsertAt(current.Right, node);
		}
		
		return false;
	}

	private bool CheckInsertLeft(Node<T> current, Node<T> node)
	{
		if (ShouldInsertLeft(current, node))
		{
			if (current.Left == null)
			{
				current.InsertLeft(node);
				return true;
			}

			InsertAt(current.Left, node);
		}
		
		return false;
	}

	private bool ShouldInsertLeft(Node<T> current, Node<T> node)
	{
		return node.Data.CompareTo(current.Data) < 0;
	}

	private bool ShouldInsertRight(Node<T> current, Node<T> node)
	{
		return node.Data.CompareTo(current.Data) > 0;
	}
}


private interface ILevelState
{
	int GetX();
}

private class DefaultLevelState<T> : ILevelState
{ 
	public DefaultLevelState(Node<T> node)
	{
		this.Node = node;
	}
	
	protected virtual int LevelOffset()
	{
		const int PADDING = 10;
		return (int)(PADDING * Level() / 2);
	}
	
	
	protected Node<T> Node{ get; set; }
	public virtual int GetX()
	{
		if (IsNull()) return Node.x;
		
		if (IsLeft())
			return GetLeftX();
		else if (IsRight())
			return GetRightX();
		
		RaiseOrphan();
		return 0;
	}
	
	private int GetLeftX()
	{
		return Parent.GetX() - NodeDimensionMultipler() - LevelOffset();
	}
	
	private int GetRightX()
	{
		return Parent.GetX() + NodeDimensionMultipler() + LevelOffset();
	}

	protected int NodeDimensionMultipler()
	{
		return Node.NodeDimension * 2;
	}

	protected Node<T> Parent { get { return Node.Parent; }}
	
	protected void RaiseOrphan()
	{
		throw new Exception("Orphan");
	}
	
	protected bool IsNull()
	{
		return Node.Parent == null;
	}
	
	protected bool IsLeft()
	{
		return Node.Parent.Left == Node;
	}
	
	protected bool IsRight()
	{
		return Node.Parent.Right == Node;
	}
	
	protected int Level()
	{
		return Node.Level;
	}
}

private class Level2State<T> : DefaultLevelState<T>
{

	public Level2State(Node<T> node) : base(node)
	{}

	public override int GetX()
	{
		return base.GetX();
	}

	protected override int LevelOffset()
	{
		return -10;
	}

}

public class Node<T>
{
	public Node()
	{ }

	public Node(int dimension, int node_dimension)
	{
		NodeDimension = node_dimension;
		x = dimension / 2;
		y = 0;
		MaxWidth = dimension;
		MaxHeight = dimension;
	}

	public int NodeDimension { get; protected set; }
	public int MaxWidth { get; protected set; }
	public int MaxHeight{ get; protected set; }
	
	public T Data { get; set; }
	
	public Node<T> Left{ get; set; }
	public Node<T> Right{ get; set; }
	public int Level { get; set; } = 0;
	public Node<T> Parent { get; set; }

	internal int x;
	internal int y;

	private ILevelState Create()
	{
		return Level == 2 ? new Level2State<T>(this) : new DefaultLevelState<T>(this);
	}
	
	public int GetX()
	{
		return Create().GetX();
	}
	
	public int GetY()
	{
		if (Parent == null) return y;
		
		if(CanInsert())
			return GetVerticalOffset();

		throw new Exception("Orphan");
		
	}
	
	private int GetVerticalOffset()
	{
		return (int)(Parent.GetY() + NodeDimension * 1.5);
	}
	
	private bool CanInsert()
	{
		return Parent.Left == this || Parent.Right == this;
	}

	public int GetNodeDimenion(Font font, int node_dimension)
	{
		var length = GetWorldLength(font);
		return length > node_dimension ? length : node_dimension;
	}

	protected int GetWorldLength(Font font)
	{
		return TextRenderer.MeasureText(Data.ToString(), font).Width + 2;
	}

	public Point GetParentConnectionPoint(Font font)
	{
		return new Point(Parent.GetX() + GetWorldLength(font)/2, Parent.GetY() + NodeDimension );
	}

	public Point GetConnectionPoint(Font font)
	{
		return new Point(GetX() + GetWorldLength(font)/2, GetY());
	}

	public bool NeedsConnector()
	{
		return Parent != null;
	}

	internal void InsertRight(Node<T> node) 
	{
		Right = node;
		if(node != null) node.Parent = this;
	}

	internal void InsertLeft(Node<T> node) 
	{
		Left = node;
		if(node != null) node.Parent = this;
	}

	private static readonly Node<T> _empty = new Node<T>();
	public static Node<T> Empty
	{
		get { return _empty; }
	}

}