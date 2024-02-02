<Query Kind="Program">
  <Namespace>Xunit</Namespace>
  <Namespace>System.Security.Policy</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Drawing.Text</Namespace>
  <Namespace>static UserQuery</Namespace>
  <Namespace>System.ComponentModel</Namespace>
</Query>

#load "xunit"
#define SHOW_PAINT

void Main()
{
    //RunTests();  // Call RunTests() or press Alt+Shift+T to initiate testing.
    //GameOfFifteen.Run();
    GameOfFifteen.ManualScramble();
    
}    

private static void DoSomeRandomFrontierStuff()
{
    var random = new Random(DateTime.Now.Millisecond);
        
    var o = new Frontier<int>(new Stack<int>());
    for(var i = 0; i < 100; i++)
    {
        o.Add(random.Next(1000));    
    }
    
    o.Count.Dump();
    var value = o.Next();
    o.Add(value);
    value.Dump();
    Console.WriteLine($"Contains {value}: {o.Contains(value)}");
    o.Dump();
}


public class Node
{
    public int?[] State;
    public Node Parent{ get; set; }
    public Move Action{ get; set; }
    public string ActionText{ get; set; }
    

    private static readonly Node _empty = new Node() { State = null, Parent = null, Action = Move.None, ActionText = "None" };
    public static Node Empty{ get { return _empty; }}
}

public static class FrontierExtension
{
    public static bool ContainsState(this Frontier<Node> frontier, int?[] state)
    {
        var enumerator = frontier.GetEnumerator();
        while(enumerator.MoveNext())
        {
            if(enumerator.Current.State.SequenceEqual(state)) return true;
        }
        return false;
    }
}

//depth first search (Stack) or breadth first (Queue)
public class Frontier<T> : ICollection<T> 
{
    private Stack<T> stack{ get; set; } = null;
    private Queue<T> queue{ get; set; } = null;
    
    public Frontier(Stack<T> stack)
    {   //depth first implementation
        this.stack = stack;
    }
    
    public Frontier(Queue<T> queue)
    {
        //breadth first implementation
        this.queue = queue;
    }
    
    public void Clear()
    {
        if(stack != null)
            stack.Clear();
        else
            queue.Clear();
    }
    
    public bool IsReadOnly => false;

    public int Count => stack != null ? stack.Count : queue.Count;

    public void Add(T item)
    {
        if (stack != null)
            stack.Push(item);
        else
            queue.Enqueue(item);
    }

    public bool IsEmpty()
    {
        return stack != null ? stack.Count == 0 : queue.Count == 0;
    }

    public T Next()
    {
        return stack != null ? stack.Pop() : queue.Dequeue();

    }

    public bool Remove(T item)
    {
        return stack != null ? stack.Peek() != null : queue.Peek() != null;
    }

    public bool Contains(T item)
    {
        var enumerator = GetEnumerator();
        
        while(enumerator.MoveNext())
        {   
            var o = enumerator.Current;
            if(o.Equals(item))
                return true;
        }
        
        return false;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return stack != null ? stack.GetEnumerator() : queue.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return stack != null ? ((IEnumerable)stack).GetEnumerator():
            ((IEnumerable)queue).GetEnumerator();
    }
}


public class ExploredSet : List<int?[]> 
{ 
    public bool ContainsState(int?[] state)
    {
        var enumerator = this.GetEnumerator();
        while(enumerator.MoveNext())
        {
            if(enumerator.Current.SequenceEqual(state)) return true;
        }
        
        return false;
    }
}

public enum Move
{
    None,
    Up, 
    Down,
    Left, 
    Right
}

public class Moveable
{
    public int X { get; set; }
    public int Y { get; set; }
    public int? Value{ get; set; }
    public Move Move{ get; set; }

    private static readonly Moveable _empty = new Moveable() { X = -1, Y = -1, Value = -1, Move = Move.None};
    public static Moveable Empty { get { return _empty; }}
}


public class GameOfFifteen
{
    private int gridSize = 16;

    private int?[] Goal = new int?[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, (int?)null};

    public static void ManualScramble()
    {
        var game = new GameOfFifteen(16);
        Console.WriteLine();
        game.Draw();
        game.ManualScramble(25);
        game.Runner();
    }

    private void ManualScramble(int iterations)
    {
        var random = new Random(DateTime.Now.Millisecond);
        
        for (var i = 0; i < iterations; i++)
        {
            var moves = this.GetTilesThatCanMove();
            var move = moves[random.Next(0, moves.Count)];
            this.Positions = this.GetFutureState(move.X, move.Y, move.Move);
            this.Draw(this.Positions);
            Thread.Sleep(200);
        }
    }
    
    public static void Run()
    {
        var game = new GameOfFifteen(16);
        Console.WriteLine();
        game.Runner();
    }
    
    private void Runner()
    {
        var timer = Stopwatch.StartNew();
        try
        {
            var solution = FindSolution();
            solution.Dump();
        }
        finally
        {
            timer.Stop();
            Console.WriteLine($"Processing took {timer.Elapsed}");
        }
    }

    
    private void SlowAndVerbose<T>(int numberExplored, Frontier<T> frontier, ExploredSet explored)
    {
        #if SHOW_PAINT
        if (numberExplored > 500000 || (numberExplored % 500 == 0))
        #endif
        {
            this.Draw($"Tried: {numberExplored}, Frontier: {frontier.Count}, Explored: {explored.Count}");
            Thread.Sleep(100);
        }
    }
    
    private (List<int?[]>, List<Move>, List<string>) FindSolution()
    {
        var frontier = new Frontier<Node>(new Queue<Node>());
        frontier.Add(new Node() { State = GetState(), Action = Move.None, Parent = Node.Empty });
        var explored = new ExploredSet();
        var numberExplored = 0;
        
        while (true)
        {
            IsTruthy(frontier.IsEmpty(), () => throw new Exception("No solution"));
            
            var node = frontier.Next();
            this.Positions = node.State;
            numberExplored += 1;
            
            SlowAndVerbose(numberExplored, frontier, explored);
            
            if(IsGoal(this.Positions))
            {
                this.Draw($"Goal @{numberExplored}, Frontier: {frontier.Count}, Explored: {explored.Count}");
                return ReconstructPath(node);

            }

            explored.Add((int?[])node.State.Clone());
            
            foreach (var item in this.GetTilesThatCanMove())
            {
                var state = this.GetFutureState(item.X, item.Y, item.Move);
                //this.Draw(state);
                //item.Dump();
                if(frontier.ContainsState(state) == false && explored.ContainsState(state) == false)
                {
                    frontier.Add(new Node()
                    {
                        State = state,
                        Parent = node,
                        Action = item.Move,
                        ActionText = $"{item.Value} moves {item.Move}" });
                }
            }
            
        }
    }

    private (List<int?[]>, List<Move>, List<string>) ReconstructPath(Node node)
    {
        var actions = new List<Move>();
        var states = new List<int?[]>();
        var moves = new List<string>();

        while (node.Parent != Node.Empty)
        {
            states.Add(node.State);
            actions.Add(node.Action);
            moves.Add(node.ActionText);
            node = node.Parent;
        }
        actions.Reverse();
        states.Reverse();
        moves.Reverse();
        return (states, actions, moves);
    }

    public GameOfFifteen(int size)
    {
        gridSize = size;
        Init();
    }
    
    public bool IsGoal()
    {
        return Positions.SequenceEqual(Goal);
    }
    
    public bool IsGoal(int?[] positions)
    {
        return Goal.SequenceEqual(positions);
    }
    
    
    private void DebugWriteLine(Exception ex)
    {
#if KUNK
        Debug.WriteLine(ex.Message);
#endif        
    }
    
    private void Init()
    {
        GameOfFifteenBoard.GridSize = gridSize;
        Positions = GameOfFifteenBoard.RandomlyPopulate();
        
        Goal = new int?[gridSize];
        for(var i=0; i<gridSize; i++)
        {
            Goal[i] = i+1;    
        }
        Goal[gridSize-1] = (int?)null;
    }
    
    public int?[] Positions { get; protected set; }

    public int?[] GetFutureState(int x, int y, Move move)
    {
        var clone = GetState();
        var result = (int?[])MoveTile(x, y, move, false).Clone();

        for (int i = 0; i < clone.Count(); i++)
        {
            Positions[i] = clone[i];
        }
        
        return result;
    }

    public int?[] MoveTile(int x, int y, Move move, bool draw = true)
    {
        GuardX(x);
        GuardY(y);
        int width = GetWidth(gridSize);

        //right now any move whether its blank or not
        switch (move)
        {
            case Move.Up:
                MoveUp(x, y, width, draw);
                break;
            case Move.Down:
                MoveDown(x, y, width, draw);
                break;
            case Move.Left:
                MoveLeft(x, y, width, draw);
                break;
            case Move.Right:
                MoveRight(x, y, width, draw);
                break;
                
            default:
                Debug.WriteLine($"MoveTitle: {move}");
                break;
        }
        
        return Positions;
    }

    public List<Moveable> GetTilesThatCanMove()
    {
        int width = GetWidth();
        
        var list = new List<Moveable>();
        for(int y = 0; y < width; y++)
            for(int x = 0; x < width; x++)
            {
                var result = CanMove(x, y);
                if(result != Moveable.Empty)
                    list.Add(result);
            }
        
        return list;
    }
    
    public Moveable CanMove(int x, int y)
    {
        int width = GetWidth();
        int? value = GetValue(x, y);
        
        if(CanMoveDown(x, y, width))
            return new Moveable() { X = x, Y = y, Value = value, Move = Move.Down };
        if (CanMoveUp(x, y, width))
            return new Moveable() { X = x, Y = y, Value = value, Move = Move.Up };
        if (CanMoveLeft(x, y, width))
            return new Moveable() { X = x, Y = y, Value = value, Move = Move.Left };
        if (CanMoveRight(x, y, width))
            return new Moveable() { X = x, Y = y, Value = value, Move = Move.Right };
            
        return Moveable.Empty;
    }

    public int? GetValue(int x, int y)
    {
        try
        {   
            int k = GetWidth() * y + x;
            return Positions[k].HasValue ? Positions[k].Value : (int?)null;
        }
        catch(Exception ex)
        {
            DebugWriteLine(ex);
            return Int32.MinValue;
        }
    }

    public bool CanMoveDown(int x, int y, int width)
    {
        return y + 1 < width && CanMoveTo(width * (y + 1) + x);
    }
   

    void MoveDown(int x, int y, int width, bool draw = true)
    {
        int oldX = width * y + x;
        int newX = width * (y + 1) + x ;

        if (y + 1 >= 0 && CanMoveTo(newX))
        {
            Swap(oldX, newX);
            if(draw) Draw();
        }
        else
        {
            Debug.WriteLine("Target tile is not empty");
        }
    }

    public bool CanMoveUp(int x, int y, int width)
    {
        return y - 1 >= 0 && CanMoveTo(width * (y - 1) + x);
    }

    

    void MoveUp(int x, int y, int width, bool draw = true)
    {
        int oldX = width * y + x;
        int newX = width * (y - 1) + x;

        if (y - 1 >= 0 && CanMoveTo(newX))
        {
            Swap(oldX, newX);
            if(draw) Draw();
        }
        else
        {
            Debug.WriteLine("Target tile is not empty");
        }
    }

    public bool CanMoveLeft(int x, int y, int width)
    {
        return x - 1 >= 0 && CanMoveTo(width * y + x - 1);
    }

    public bool CanMoveTo(int newX)
    {
        try
        {
            return Positions[newX].HasValue == false;
        }
        catch (Exception ex)
        {
            DebugWriteLine(ex);
            return false;
        }
    }
    
    void MoveLeft(int x, int y, int width, bool draw = true)
    {
        int oldX = width * y + x;
        int newX = width * y + x - 1;

        if (x - 1 >= 0 && CanMoveTo(newX))
        {
            Swap(oldX, newX);
            if(draw) Draw();
        }
        else
        {
            Debug.WriteLine("Target tile is not empty");
        }
    }

    public bool CanMoveRight(int x, int y, int width)
    {
        return x + 1 < width && CanMoveTo(width * y + x + 1);
    }

      
    void MoveRight(int x, int y, int width, bool draw = true)
    {
        int oldX = width * y + x;
        int newX = width * y + x + 1;
        
        if (x + 1 < width && CanMoveTo(newX))
        {
            Swap(oldX, newX);
            if(draw) Draw();
        }
        else
        {
            Debug.WriteLine("Target tile is not empty");    
        }
    }
    
    public void DrawNumberedTile(int x, int y)
    {   
        int k = GetWidth() * y + x;
        GameOfFifteenBoard.DrawNumberedTile(Positions, k, x, y);
    }
    
    public void DrawGoalState()
    {
        GameOfFifteenBoard.Draw(Goal);
    }
    
    
    public void Draw(string? message = null)
    {
        GameOfFifteenBoard.Draw(Positions, message);
    }
        
    
    public void Draw(int?[] positions)
    {
        GameOfFifteenBoard.Draw(positions);
    }

    int?[] Swap(int oldX, int newX)
    {
        int? temp = Positions[oldX];
        Positions[oldX] = Positions[newX];
        Positions[newX] = temp;
        return Positions;
    }

    void GuardY(int y)
    {
        GuardXY(y, "y out of range");
    }

    void GuardX(int x)
    {
        GuardXY(x, "x out of range");
    }
    
    int GetWidth()
    {
        return (int)Math.Sqrt(gridSize);
    }
    
    int GetWidth(int length)
    {
        return (int)Math.Sqrt(length);
    }
    
    void GuardXY(int k, string message)
    {
        Guard(k >= 0 && k < GetWidth(Positions.Length), () => throw new ArgumentException(message, "k"));   
    }
    
    public int?[] GetState()
    {
        return (int?[])Positions.Clone();
    }
    
    void IsTruthy(bool test, Action action)
    {
        if(test)
            action();
    }
    
    void Guard(bool test, Action action)
    {
        if(test == false)
            action();
    }

    
}

public class GameOfFifteenBoard
{
    public static int GridSize = 16;
    private static Bitmap bitmap;
    private static readonly int width = 401;

    public static void Draw(int?[] positions, string? message = null)
    {
        Guard(positions);

        bitmap = new Bitmap(width, width + 20);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

        DrawGameBoard(graphics, width);

        int root = (int)Math.Sqrt(positions.Length);

        for (int y = 0, k = 0; y < root; y++)
            for (int x = 0; x < root; x++)
                DrawNumberedTile(graphics, positions[k++].ToString(), x, y, width / root);

        if (message != null)
            graphics.DrawString(message, exploredFont, Brushes.Green, new Point(0, 401));

        Util.ClearResults();
        Console.WriteLine(Environment.NewLine);
        bitmap.Dump();

    }

    //expiermental
    public static void DrawNumberedTile(int?[] positions, int k, int x, int y)
    {
        if (bitmap == null)
            throw new ArgumentNullException("bitmap");

        using var graphics = Graphics.FromImage(bitmap);
        graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

        int root = (int)Math.Sqrt(positions.Length);

        DrawNumberedTile(graphics, positions[k].ToString(), x, y, width / root);
        Util.ClearResults();
        bitmap.Dump();

    }


    static void DrawGameBoard(Graphics graphics, int width) => graphics.DrawRectangle(Pens.Black, new Rectangle(0, 0, width - 1, width - 1));

    static void DrawNumberedTile(Graphics graphics, string text, int x, int y, int w)
    {
        DrawRectangle(graphics, x, y, w);
        DrawString(graphics, text, x, y, w);
    }

    static void DrawString(Graphics graphics, string text, int x, int y, int w) => graphics.DrawString(text, font, Brushes.Black,
                    GetTextPoint(x, y, w));

    static void DrawRectangle(Graphics graphics, int x, int y, int w)
    {
        var rect = GetRectangle(x, y, w);
        graphics.FillRectangle(Brushes.White, rect);
        graphics.DrawRectangle(Pens.Black, GetRectangle(x, y, w));
    }


    static Rectangle GetRectangle(int x, int y, int w) => new Rectangle(GetX(x, w), GetY(y, w), w, w);
    static PointF GetTextPoint(int x, int y, int w) => new Point(GetTextX(x, w), GetTextY(y, w));


    private static int GetX(int x, int width) => GetPosition(x, width);
    private static int GetY(int y, int width) => GetPosition(y, width);

    static int GetPosition(int k, int width)
    {
        return k * width;
    }

    private static int GetTextX(int x, int width) => GetTextPosition(x, width);
    private static int GetTextY(int y, int width) => GetTextPosition(y, width);

    static int GetTextPosition(int k, int width)
    {
        return k * width + width / 3;
    }

    private static Font font = new Font(new FontFamily("Arial"), 16);
    private static Font exploredFont = new Font(new FontFamily("Arial"), 8);

    static void Guard(int?[] positions)
    {
        Guard(IsPerfectSquare(positions.Length), () => throw new ArgumentOutOfRangeException("positions", "Not a perfect square"));
    }
    static void Guard(bool test, Action action)
    {
        if (test == false)
            action();
    }

    private static bool IsPerfectSquare(int number)
    {
        return (Math.Sqrt(number) % 1 == 0);
    }

    [Description("Solvable in three (3)")]  
    public static int?[] SimplyPopulate8()
    {
        return new int?[] { 1, 2, 3, null, 4, 5, 7, 8, 6 };
    }

    [Description("Seemed to find at 177,000 or almost max using DFS")]
    
    public static int?[] SimplyPopulate8_WithSolution()
    {
        return new int?[] { 8, null, 6, 5, 4, 7, 2, 3, 1 };
    }

    public static int?[] SimplyPopulate8_WithSolution2()
    {
        return new int?[] { null, 1, 3, 4, 2, 5, 7, 8, 6 };
    }

    public static int?[] SimplyPopulate15()
    {
        //return new int?[] { 5, 1, 3, 4, 2, null, 7, 8, 9, 6, 19, 12, 13, 14, 11, 15};
        //return new int?[] { 6, 13, 7, 10, 8, 9, 11, null, 15, 2, 12, 5, 14, 3, 1, 4};
        //return new int?[] { 5, 2, 12, null, 8, 11, 13, 3, 14, 1, 10, 15, 7, 6, 4, 9};
        return new int?[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, null, 13, 14, 15};
    }

    public static void Shuffle<T>(Random random, T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = random.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }

    public static int?[] RandomlyPopulate()
    {
        return SimplyPopulate15();
        var list = new List<int>();
        list.AddRange(Enumerable.Range(1, GridSize - 1));

        var result = list.Select(x => x).Cast<int?>().ToList();
        result.Add(null);

        int?[] numbers = result.ToArray();
        Shuffle(new Random(DateTime.Now.Microsecond), numbers);
        return numbers;
    }
        
    public static int?[] RandomlyPopulate_Old()
    {
        var random = new Random(DateTime.Now.Microsecond);
        var positions = CreatePositions();
        var exclusionSet = new HashSet<int>();
        
        var i = 0;
        while(i < GridSize - 1)
        {
            var next = random.Next(1, GridSize);
            if(exclusionSet.Contains(next) == false)
            {
                i++;
                positions.Add(next);
                exclusionSet.Add(next);
            }
        }
        
        positions.Add((int?)null);

        InitializeGrid(positions.ToArray());
        
        return positions.ToArray();
    }

    static void InitializeGrid(int?[] positions)
    {
        Draw(positions);
    }

    public static List<int?> CreatePositions() => new List<int?>(GridSize);
}

#region private::Tests

[Fact] void Test_Xunit() => Assert.True (1 + 1 == 2);

[Fact] void ContainsTest() {
    var o = new Frontier<string>(new Queue<string>());
    o.Add("Hello");
    o.Add("World");
    Assert.True(o.Contains("World"));
}

[Fact] void GameOfFifteen_Positions_Test(){
    GameOfFifteenBoard.RandomlyPopulate();
}

#endregion