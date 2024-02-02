<Query Kind="Program">
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Drawing.Drawing2D</Namespace>
  <Namespace>System.Drawing.Text</Namespace>
  <Namespace>System.Drawing.Imaging</Namespace>
  <Namespace>System.ComponentModel</Namespace>
  <Namespace>Xunit</Namespace>
</Query>

#load "xunit"

void RunTests()
{
    DummyTest();
}

[Fact]
public void DummyTest()
{
  Assert.True(false, "Hello World!");
}


void Main()
{
  //RunTests();  // Call RunTests() or press Alt+Shift+T to initiate testing.

#region Random Runs  
  
//    SpyVsSpy.DrawEmptyGridTest();
//    SpyVsSpy.DrawMatrixWithOverlayTest();
//    SpyVsSpy.DrawToCustomContainer();
//    SpyVsSpy.DrawToDump();
//
    SpyVsSpy.Run(13);
//    SpyVsSpy.RandomWin(9);
//    SpyVsSpy.SeededWin(13, new int[] { 1, 3, 12, 10, 7, 2, 11, 5, 8, 13, 9, 4, 6});
//    SpyVsSpy.SeededWin(13, new int[] { 1, 3, 12, 10, 7, 2, 11, 5, 8, 13, 9, 4, 6});
//    SpyVsSpy.SmartRandom(13);
  
#endregion  
  //SpyVsSpy.Run(13);
  
  //TODO: Add smart moves
  //TODO: Track statistics
  //TODO: Maybe add highighter over connections

}

public class SpyVsSpy : IListener
{
  public bool Listening => true;
  protected DumpContainer Container { get; private set; } = ContainerCreate();
  protected Matrix Matrix { get; set; }
  public int GridSize { get; set; }
  

#region Constructors, Startups and Factories  
  protected SpyVsSpy(int size)
  {
    GridSize = size;
    InitializeMatrix(size);
  }

  private void InitializeMatrix(int size)
  {
    Matrix = new Matrix(size);
  }

  private static DumpContainer ContainerCreate()
  {
    var o = new DumpContainer();
    o.Dump();
    return o;
  }


  public static void Run(int size)
  {
    CheckMaxSizeLimit(size);

    var o = new SpyVsSpy(size);
    Broadcaster.Add(o);
    o.Run();
  }

  private void Run()
  {
    //TryBestMove(7200000);
    TryPhaseRunner();
  }

  private static void CheckMaxSizeLimit(int size)
  {
    if (size > 999)
      throw new ArgumentOutOfRangeException("size", "Maximum size is 999");
  }

  #endregion

  public void Listen(object o, bool slow = false)
  {
    try
    {
      Container.UpdateContent(o);
      Slowsky(slow);
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
  }
  
  private void Slowsky(bool slow = false)
  {
#if DEBUG
		if(slow)
		{
			Thread.Sleep(25);	
		}
#endif
  }
  
  public void TryPhaseRunner(int timeout = 100000)
  {
    try
    {
      List<int[]> seeds = GetThreeCollisionSet();
      seeds = GetTwoCollisionSet(seeds);
      seeds = GetOneCollisionSet(seeds);
      seeds = GetWinningSet(seeds);
    }
    catch(WinnningMoveException ex)
    {
      Console.WriteLine(ex.Message);
    }
    catch(Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
  }
  
  private void BestRandomMove(int[] seeds = null)
  {
    Matrix.BestRandomMove();
  }
  
  public List<int[]> GetThreeCollisionSet(int timeout = 100000)
  {
    TryOneRandomSwap();
    var results = GetCollisionSet(Matrix.GetWinningMoves().ToArray(), 3, BestRandomMove);
    CheckFail(results, 3);
    return results;
  }


  public List<int[]> GetTwoCollisionSet(List<int[]> threes, int timeout = 100000)
  {
    return GetCollisionSet(threes, 2);
  }
  
  public List<int[]> GetOneCollisionSet(List<int[]> twos, int timeout = 100000)
  {
    return GetCollisionSet(twos, 1);
  }
  
  public List<int[]> GetWinningSet(List<int[]> ones, int timeout = 100000)
  {
    return GetCollisionSet(ones, 0);
  }

  private List<int[]> GetCollisionSet(List<int[]> sets, int countLimit, int timeout = 100000)
  {
    var result = new List<int[]>();
    foreach (var set in sets)
    {
      result.AddRange(GetCollisionSet(set, countLimit));
    }
    
    Console.WriteLine($"Found {result.Count} collisions for sets of {countLimit}");
    CheckFail(result, countLimit);
    return result;
  }
  
  private void CheckFail(List<int[]> seeds, int countLimit)
  {
    if (seeds != null && seeds.Count == 0)
      throw new FailedSieveException($"Fail: Found {seeds.Count} collisions for sets of {countLimit}");
  }

  private static readonly Random _swapRandom = new Random(DateTime.Now.Millisecond);
  private int[] RandomSwapCopy(int[] seeds)
  {
    var copy = new int[seeds.Length];
    
    seeds.CopyTo(copy, 0);
    
    var first = _swapRandom.Next(0, GridSize);
    var second = _swapRandom.Next(0, GridSize);
    var temp = copy[first];
    copy[first] = copy[second];
    copy[second] = temp;

#if !DEBUG
    CheckThrowCritical(copy);
    Console.WriteLine($"Randomized {string.Join(" ", copy)}");
#endif
    return copy;
  }
 
  private void CheckIsWinner(List<ICollision> crashes)
  {
    if (crashes != null && crashes.Count == 0)
    {
      throw new WinnningMoveException($"Results: {Matrix.WinningMove()}, Spy Killings: {crashes.Count}");
    }
  }

  private void CheckIsWinner(int[] seeds)
  {
    if(seeds == null || seeds.Length == 0) return;
    SeedSwap(seeds);
    CheckIsWinner(CrashTestDummy.FindAll(Matrix));
  }

  private void SeedSwapper(int[] seeds)
  {
    SeedSwap(RandomSwapCopy(seeds));
  }
  
  private List<int[]> GetCollisionSet(int[] seeds, int countLimit, Action<int[]> swap = null, int timeout = 100000)
  {
    Action<int[]> report = (spores) =>
    {
      Console.WriteLine($"Checking {string.Join(" ", spores)} for sets of {countLimit}");
    };
    
    report(seeds);
    
    CheckIsWinner(seeds);

    using (var timer = Create(timeout))
    {
      var results = new List<int[]>();

      while (timer.Enabled)
      {
        if (swap == null)
          SeedSwapper(seeds);
        else
          swap(seeds);
          
        //report(Matrix.GetMarkers().ToArray());
        
        var crashes = CrashTestDummy.FindAll(Matrix);
        Matrix.Connect(crashes);
        Broadcaster.Broadcast(Matrix._Bitmap.Value);
        CheckIsWinner(crashes);

        if (crashes.Count <= countLimit)
        {
          int[] configuration = Matrix.GetWinningMoves().ToArray();
          //if (results.IsDuplication(configuration) == false) // duplicates seem to help exhaustively check
          {
            results.Add(configuration);
            Console.WriteLine($"Results: {Matrix.WinningMove()}, Collisions: {crashes.Count}");
          }
          
        }

        //Console.ReadLine();
        //Thread.Sleep(2500);
      }

      return results;
    }
  }

  public void TryBestMove(int timeout = 100000)
  {
    var timer = Create(timeout);
    bool won = false;

    Matrix.Draw();

    //int[] seeds = { 9, 2, 11, 13, 3, 12, 4, 7, 10, 5, 1, 8, 6 }; //3
    //int[] seeds = { 9, 2, 10, 13, 3, 12, 4, 7, 11, 5, 1, 8, 6 }; //2
    //int[] seeds = { 9, 2, 11, 13, 3, 10, 4, 7, 12, 5, 1, 8, 6};  //2
    //int[] seeds = { 9, 2, 10, 5, 3, 12, 4, 7, 11, 13, 1, 8, 6}; // 2
    //int[] seeds = {9, 2, 11, 13, 3, 7, 4, 10, 12, 5, 1, 8, 6};  //1
    //int[] seeds = {9, 2, 8, 13, 3, 7, 4, 10, 12, 5, 1, 11, 6};  //1
    //int[] seeds = {9, 2, 8, 13, 1, 7, 4, 10, 12, 5, 3, 11, 6};  //1
    //int[] seeds = {8, 2, 9, 13, 3, 7, 4, 10, 12, 5, 1, 11, 6 }; //1 - Leads to win
    //int[] seeds = {9, 2, 8, 13, 3, 7, 11, 10, 12, 5, 1, 4, 6};  //1
    //int[] seeds = { 8, 2, 9, 13, 1, 7, 4, 10, 12, 5, 3, 11, 6};   //0 Win!
    //int[] seeds = { 1, 3, 12, 10, 7, 2, 11, 5, 8, 13, 9, 4, 6}; //0 Hackerrank Win!
    //int[] seeds = { 5, 8, 13, 1, 10, 7, 2, 4, 9, 12, 3, 11, 6 }; //0 Candidate
    int[] seeds = { 6, 4, 1, 8, 11, 13, 3, 7, 2, 10, 12, 9, 5}; //0 win
    
    Random random = new Random(DateTime.Now.Millisecond);
    
    while (timer.Enabled && won == false)
    {
      SeedSwap(seeds);
      if (CrashTestDummy.FindAll(Matrix).Count == 0)
      {
        won = true;
        break;
      }
      //TryOneRandomSwap();
      var copy = new int[seeds.Length];
      seeds.CopyTo(copy, 0);
      //swap any two 
      var first = random.Next(0, GridSize);
      var second = random.Next(0, GridSize);
      var temp = copy[first];
      copy[first] = copy[second];
      copy[second] = temp;
      
      SeedSwap(copy);
      
      //Matrix.BestRandomMove();
      
      var crashes = CrashTestDummy.FindAll(Matrix);
      Matrix.Connect(crashes);

      Broadcaster.Broadcast(Matrix._Bitmap.Value);
      
      Console.WriteLine($"Results: {Matrix.WinningMove()}, Spy Killings: {crashes.Count}");
      
      //Console.ReadLine();
      //Thread.Sleep(2500);
      
      if (crashes.Count == 0)
        won = true;
    }

    Console.WriteLine(won ? $"You Won: {Matrix.WinningMove()}!" : "You ran out of time!");
  }

 

  private void SwapCurrent(Point current)
  {
    Swap(current.X, current.Y, current.X, current.Y % Matrix.GetUpperBound() == 0 ? 1 : current.Y % Matrix.GetUpperBound());
  }

  void Swap(int fromC, int fromR, int toC, int toR)
  {
    Matrix.Swap(fromC, fromR, toC, toR);
  }

  
  private static System.Timers.Timer Create(int timeout = 10000)
  {
    var timer = new System.Timers.Timer(timeout);

    timer.Elapsed += (sender, args) =>
    {
      (sender as System.Timers.Timer).Stop();
    };

    timer.Start();
    return timer;
  }


  object ToDump()
  {
    return Matrix._Bitmap.Value;
  }
  

  public void Reset()
  {
    InitializeMatrix(GridSize);
    DrawMatrix();
  }

  private bool isSolved = false;
  private bool IsSolved()
  {
    return isSolved;
  }

  public void DrawMatrix()
  {
    var watch = new Stopwatch();

    try
    {
      watch.Start();
      Matrix.Draw();
      Container.UpdateContent(Matrix._Bitmap.Value);
    }
    finally
    {
      watch.Stop();
      Console.WriteLine($"Operation Time: {watch.ElapsedMilliseconds}ms");
    }
  }

#region Random Run Scenarios

  public static void DrawEmptyGridTest()
  {
    var o = new SpyVsSpy(13);

    o.Swap(1, 1, 10, 10);
    o.Swap(4, 1, 5, 2);
    o.DrawMatrix();
  }

  public static void DrawToCustomContainer()
  {
    var o = new SpyVsSpy(25);
    var picture = new System.Windows.Forms.PictureBox();
    var output = PanelManager.DisplayControl(picture, "My Results");
    
    o.Matrix.Draw();
    picture.Image = o.Matrix._Bitmap.Value;
  }

  public static void DrawToDump()
  {
    var o = new SpyVsSpy(13);
    o.Dump();
  }
  
  public static void SeededWin(int size, int[] seeds)
  {
    if (size > 999)
      throw new ArgumentOutOfRangeException("size", "Maximum size is 999");

    var o = new SpyVsSpy(size);
    Broadcaster.Add(o);
    o.SeededWin(seeds);
  }

  private void SeededWin(int[] seeds)
  {
    SeedSwap(seeds);
    var crashes = CrashTestDummy.FindAll(Matrix);

    foreach (var crash in crashes)
    {
      Matrix.Connect(crash.Collisions.First(), crash.Collisions.Last());
    }

    Broadcaster.Broadcast(Matrix._Bitmap.Value);

    Console.WriteLine(crashes.Count == 0 ? "You won!" : "Something went wrong!");
  }

  public static void RandomWin(int size)
  {
    if (size > 999)
      throw new ArgumentOutOfRangeException("size", "Maximum size is 999");

    var o = new SpyVsSpy(size);
    Broadcaster.Add(o);
    o.RandomWin();
  }

  private void RandomWin()
  {
    var timer = Create(10000);
    var won = false;

    while (timer.Enabled && !won)
    {
      TryOneRandomSwap();
      var crashes = CrashTestDummy.FindAll(Matrix);

      foreach (var crash in crashes)
      {
        Matrix.Connect(crash.Collisions.First(), crash.Collisions.Last());
      }
      
      Broadcaster.Broadcast(Matrix._Bitmap.Value);
      if (crashes.Count == 0)
        won = true;
    }

    Console.WriteLine(won ? "You Won!" : "You ran out of time!");
  }

  public static void SmartRandom(int size)
  {
    if (size > 999)
      throw new ArgumentOutOfRangeException("size", "Maximum size is 999");

    var o = new SpyVsSpy(size);
    Broadcaster.Add(o);
    o.SmartRandomWin();
  }

  private void SmartRandomWin()
  {
    var timer = Create(10000);
    var won = false;

    while (timer.Enabled && !won)
    {
      TryOneSmartRandomSwap();
      var crashes = CrashTestDummy.FindAll(Matrix);

      foreach (var crash in crashes)
      {
        Matrix.Connect(crash.Collisions.First(), crash.Collisions.Last());
      }
      
      Broadcaster.Broadcast(Matrix._Bitmap.Value);
      if (crashes.Count == 0)
        won = true;
    }

    Console.WriteLine(won ? "You Won!" : "You ran out of time!");
  }



  private void SeedSwap(int[] seeds)
  {
    Matrix.Draw();
    for (int column = Matrix.GetLowerBound(), i = 0; column < Matrix.GetUpperBound(); column++, i++)
    {
      var row = GetCurrentRow(column);
      Swap(column, row, column, seeds[i]);
      Matrix.Draw();
      Broadcaster.Broadcast(Matrix._Bitmap.Value);
    }
  }

  private void TryOneSmartRandomSwap()
  {
    var list = new Random(DateTime.Now.Millisecond).Shuffle(Enumerable.Range(1, Matrix.GetUpperBound() - 1).ToArray());

    Matrix.Draw();

    for (int column = Matrix.GetLowerBound(), i = 0; column < Matrix.GetUpperBound(); column++, i++)
    {
      var row = GetCurrentRow(column);
      Swap(column, row, column, list[i]);
      Matrix.Draw();
      Broadcaster.Broadcast(Matrix._Bitmap.Value);
    }

    Broadcaster.Broadcast(Matrix._Bitmap.Value);
  }

  private void TryOneRandomSwap()
  {
    Matrix.Draw();
    var rows = Matrix.GetMarkers();
    
    for (int column = Matrix.GetLowerBound(), rIndex = 0; column < Matrix.GetUpperBound(); column++, rIndex++)
    {
      var row = rows[rIndex];
      RandomSwap(column, row);
    }
    
    Matrix.Draw();
    Broadcaster.Broadcast(Matrix._Bitmap.Value);
  }


  private static readonly Random random = new Random(DateTime.Now.Millisecond);
  private void RandomSwap(int column , int row)
  {
    try
    {
      Matrix.Swap(column, row, column, random.Next(Matrix.GetLowerBound(), Matrix.GetUpperBound()));
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
  }
  #endregion

  private int GetCurrentRow(int current)
  {
    for (int row = Matrix.GetLowerBound(); row < Matrix.GetUpperBound(); row++)
    {
      if (Matrix.IsHit(current, row)) return row;
    }

    throw new Exception("Cannot find marker in any row");
  }
}

public class Position
{
  public PointF Location { get; protected set; } = PointF.Empty;
  public SizeF Size { get; protected set; } = SizeF.Empty;
  public RectangleF Rectangle { get; protected set; } = RectangleF.Empty;


  public static float DefaultHeight { get; set; } = 20f;
  public static float DefaultWidth { get; set; } = 60f;
  public static float DefaultPad { get; set; } = 10f;


  protected Position()
  {

  }

  public static Position GetNextPosition(int column, int row)
  {
    return new Position(GetNextLocation(column, row));
  }

  private static PointF GetNextLocation(int column, int row)
  {
    return new PointF((DefaultWidth + DefaultPad) * column, (DefaultHeight + DefaultPad) * row);
  }

  public Position(PointF location)
  {
    this.Location = location;
    this.Size = new SizeF(DefaultWidth, DefaultHeight);
    this.Rectangle = new RectangleF(this.Location, this.Size);
  }

  public Position(float X, float Y)
  {
    this.Location = new PointF(X, Y);
    this.Size = new SizeF(DefaultWidth, DefaultHeight);
    this.Rectangle = new RectangleF(this.Location, this.Size);
  }

  public Position(float X, float Y, float width, float height)
  {
    this.Location = new PointF(X, Y);
    this.Size = new SizeF(width, height);
    this.Rectangle = new RectangleF(this.Location, this.Size);
  }

  public Position(PointF location, SizeF size)
  {
    this.Location = location;
    this.Size = size;
    this.Rectangle = new RectangleF(this.Location, this.Size);
  }

  public PointF GetCenter(SizeF actualSize)
  {
    var x = actualSize.Width / 2;
    return new PointF(x, x);
  }

  public float GetRadius(SizeF actualSize)
  {
    return (float)(GetCircumference(actualSize) / (2 * Math.PI));
  }

  private float GetCircumference(SizeF actualSize)
  {
    var circumference = (float)(actualSize.Width * Math.PI);
    return circumference;
  }

  public PointF GetClockPoint(SizeF actualSize, float radian)
  {
    var center = GetCenter(actualSize);
    var radius = GetRadius(actualSize);

    float x = (float)(center.X + (radius * Math.Cos(radian)));
    float y = (float)(center.Y + (radius * Math.Sin(radian)));


    return new PointF(x, y);
  }

  public PointF GetClockPoint(SizeF actualSize, int angle)
  {
    if (angle < -360 || angle > 360)
      throw new ArgumentOutOfRangeException("angle", angle, $"Angle {angle} should be between -360 and 360");

    var center = GetCenter(actualSize);
    var radius = GetRadius(actualSize);
    var radian = angle * Math.PI / 180;

    float x = (float)(center.X + (radius * Math.Cos(radian)));
    float y = (float)(center.Y + (radius * Math.Sin(radian)));


    return new PointF(x, y);
  }

  public float X { get { return Location.X; } }
  public float Y { get { return Location.Y; } }

  public float Width { get { return Size.Width; } }
  public float Height { get { return Size.Height; } }

  private static readonly Position _empty = new Position();
  public static Position Empty
  {
    get { return _empty; }
  }
}

public static class Factory
{
  public static Graphics Create(Bitmap bitmap)
  {
    var o = Graphics.FromImage(bitmap);
    o.CompositingMode = CompositingMode.SourceOver;
    o.CompositingQuality = CompositingQuality.HighSpeed;
    o.InterpolationMode = InterpolationMode.High;
    o.PixelOffsetMode = PixelOffsetMode.HighSpeed;
    o.SmoothingMode = SmoothingMode.AntiAlias;
    o.TextRenderingHint = TextRenderingHint.AntiAlias;
    return o;
  }
}

public class MatrixItem
{
  public string Value { get; set; } = "*";

  public int Column { get; protected set; } = -1;
  public int Row { get; protected set; } = -1;

  public Position Position { get; protected set; } = Position.Empty;
  public string Marker { get; protected set; } = "S";
  public static Font Font { get; protected set; } = new Font("Consolas", 10f, FontStyle.Regular);
  public static Font BoldFont { get; protected set; } = new Font("Consolas", 10f, FontStyle.Bold);

  public MatrixItem(int column, int row, Position position, string value = "*", string marker = "S")
  {
    this.Column = column;
    this.Row = row;
    this.Value = value;
    this.Position = position;
    this.Marker = marker;
  }

  protected MatrixItem()
  {
    this.Column = -1;
    this.Row = -1;
    this.Value = "Error";
  }

  public bool IsHit()
  {
    return Value == Marker;
  }

 

  public bool ContainsMarker()
  {
    return Value == Marker;
  }

  public Bitmap Circle(Bitmap bitmap, Pen pen = null)
  {
    using (Graphics graphics = Factory.Create(bitmap))
    {
      Pen mypen = pen == null ? Pens.Silver : pen;

      var size = graphics.MeasureString(this.Value, Font);
      if (Column != 0 && Row != 0)
      {
        graphics.DrawEllipse(mypen, Position.X, Position.Y, size.Width, size.Width);
#if DEBUG
				//bitmap = DrawConnector(bitmap, size, 90);
#endif
      }
      return bitmap = Draw(bitmap);
    }
  }

  public Bitmap Draw(Bitmap bitmap)
  {
    using (Graphics graphics = Factory.Create(bitmap))
    {
      if(Value == Marker)
      {
        graphics.DrawString(Value.ToString(), BoldFont, Brushes.Red, Position.Rectangle);  
      }
      else
      {
        graphics.DrawString(Value.ToString(), Font, Brushes.Black, Position.Rectangle);
      }
      
#if DEBUG
			//graphics.DrawRectangle(Pens.Green, Position.Rectangle.X , Position.Rectangle.Y, Position.Rectangle.Width, Position.Rectangle.Height);
#endif
      return bitmap;
    }
  }

  public Bitmap DrawConnector(Bitmap bitmap, SizeF actualSize, float radian)
  {
    if (Column == 0 || Row == 0) return bitmap;

    using (Graphics graphics = Factory.Create(bitmap))
    {
      var point = GetClockPoint(actualSize, radian);
      //convert back to silver
      graphics.FillEllipse(Brushes.Black, Position.Rectangle.X + point.X - 2f, Position.Rectangle.Y + point.Y - 1.5f, 3, 3);

      return bitmap = Draw(bitmap);
    }
  }

  public Bitmap DrawConnector(Bitmap bitmap, SizeF actualSize, int angle)
  {
    if (Column == 0 || Row == 0) return bitmap;

    using (Graphics graphics = Factory.Create(bitmap))
    {
      var point = GetClockPoint(actualSize, angle);
      graphics.FillEllipse(Brushes.Black, Position.Rectangle.X + point.X - 2f, Position.Rectangle.Y + point.Y - 1.5f, 3, 3);

      return bitmap = Draw(bitmap);
    }
  }


  public PointF GetClockPoint(SizeF actualSize, int angle)
  {
    const int MOVE_ORIGIN_TO_TOP = 90;
    return Position.GetClockPoint(actualSize, angle - MOVE_ORIGIN_TO_TOP);
  }

  public PointF GetClockPoint(SizeF actualSize, float radian)
  {
    return Position.GetClockPoint(actualSize, radian);
  }

  private static readonly MatrixItem _empty = new MatrixItem();
  public static MatrixItem Empty { get { return _empty; } }

}


public class Matrix
{
  public int Size { get; protected set; }
  private MatrixItem[,] matrix;
  public Lazy<Bitmap> _Bitmap = new Lazy<Bitmap>(() => new Bitmap(1000, 450, PixelFormat.Format32bppPArgb));

  public Matrix(int size)
  {
    Size = size + 1;
    Initialize();
  }

  private void Initialize()
  {
    matrix = new MatrixItem[Size, Size];
    InitializeColumnHeaders();
    InitializeRowHeaders();
    InitializeDefaultMarkers();
    InitializeGrid();
  }

  public MatrixItem this[int column, int row]
  {
    get
    {
      try
      {
        return matrix[column, row];
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.Message);
        return MatrixItem.Empty;
      }
    }
  }

  public List<int> GetMarkers()
  {
    return matrix.Cast<MatrixItem>().Where(o => o.IsHit()).Select(o => o.Row).ToList<int>();
  }

  private void InitializeGrid()
  {
    for (int column = GetLowerBound(); column < GetUpperBound(); column++)
      for (int row = GetLowerBound() + 1; row < GetUpperBound(); row++)
      {
        matrix[column, row] = new MatrixItem(column, row, Position.GetNextPosition(column, row));
      }
  }

  private void InitializeDefaultMarkers()
  {
    for (int column = GetLowerBound(); column < GetUpperBound(); column++)
    {
      matrix[column, GetLowerBound()] = new MatrixItem(column, GetLowerBound(), Position.GetNextPosition(column, GetLowerBound()), "S");
    }
  }

  private void InitializeRowHeaders()
  {
    for (int row = 1; row < Size; row++)
    {
      SetRowHeader(row, new MatrixItem(0, row, Position.GetNextPosition(0, row), row.ToString()));
    }
  }

  private void SetRowHeader(int row, MatrixItem item)
  {
    matrix[0, row] = item;
  }

  private void SetColumnHeader(int column, MatrixItem item)
  {
    matrix[column, 0] = item;
  }

  private void InitializeColumnHeaders()
  {
    SetColumnHeader(0, new MatrixItem(0, 0, new Position(0f, 0f), string.Empty));
    for (int column = 1; column < Size; column++)
    {
      SetColumnHeader(column, new MatrixItem(column, 0, Position.GetNextPosition(column, 0), column.ToString()));
    }
  }

  public void Reset()
  {
    Initialize();
  }

  public int GetAbsolutelowerBound() => 0;
  public int GetLowerBound() => 1;
  public int GetUpperBound() => Size;

  public string GetColumnHeader(int column)
  {
    if (column < 0 || column >= Size)
      throw new ArgumentOutOfRangeException("column", column, $"Column {column} is out of range");

    return matrix[column, 0].Value.ToString();
  }

  public string GetRowHeader(int row)
  {
    if (row < 0 || row >= Size)
      throw new ArgumentOutOfRangeException("row", row, $"Row {row} is out of range");

    return matrix[0, row].Value.ToString();
  }


  public void Draw()
  {
    using (Graphics graphics = Factory.Create(_Bitmap.Value))
    {
      graphics.Clear(Color.White);
#if DEBUG
			//graphics.DrawRectangle(Pens.Red, 0, 0, maxWidth - 2, maxHeight - 2);
#endif
      for (int column = GetAbsolutelowerBound(); column < Size; column++)
        for (int row = GetAbsolutelowerBound(); row < Size; row++)
        {
          matrix[column, row].Circle(_Bitmap.Value);
        }
    }
  }

  public static PointF Add(PointF source, PointF target)
  {
    return new PointF(source.X + target.X, source.Y + target.Y);
  }

  public static int GetAngle(PointF source, PointF target)
  {
    if (source.X < target.X) return 90;
    if (source.X == target.X) return 180;
    return 270;
  }
  
  

  public void Connect(MatrixItem source, MatrixItem target)
  {
    try
    {
      using (var graphics = Factory.Create(_Bitmap.Value))
      {
        var angle = GetAngle(source.Position.Location, target.Position.Location);

        var size = graphics.MeasureString(source.Value, MatrixItem.Font);
        source.DrawConnector(_Bitmap.Value, size, angle);
        var point1 = Add(source.GetClockPoint(size, angle), source.Position.Location);

        var x = source.Position.X - target.Position.X;
        var y = source.Position.Y - target.Position.Y;
        var radian = Math.Atan2(y, x);


        size = graphics.MeasureString(target.Value, MatrixItem.Font);
        target.DrawConnector(_Bitmap.Value, size, (float)radian);
        var point2 = Add(target.GetClockPoint(size, (float)radian), target.Position.Location);

        graphics.DrawLine(Pens.Gray, point1, point2);
      }
      Broadcaster.Broadcast(_Bitmap.Value);
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
  }

  public void Connect(int column1, int row1, int column2, int row2)
  {
    try
    {
      Connect(matrix[column1, row1], matrix[column2, row2]);
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
  }

  public bool IsHit(int column, int row)
  {
    try
    {
      return matrix[column, row].IsHit();
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
      return false;
    }
  }

  public void Swap(int fromC, int fromR, int toC, int toR)
  {
    CheckCoordinates(fromC, fromR, toC, toR);
    try
    {
      string temp = this[toC, toR].Value;
      this[toC, toR].Value = this[fromC, fromR].Value;
      this[fromC, fromR].Value = temp;
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
  }

  public void CheckCoordinates(int fromC, int fromR, int toC, int toR)
  {
    void Check(int coordinate)
    {
      if (coordinate < GetLowerBound() || coordinate > GetUpperBound())
        throw new ArgumentOutOfRangeException("coordinate", $"Coordinate {coordinate} is out of range");
    }
    
    Check(fromC);
    Check(fromR);
    Check(toC);
    Check(toR);
    
  }

  public List<int> GetWinningMoves()
  {
    return this.GetMarkers();
  }

  public string WinningMove()
  {
    return string.Join(" ", GetWinningMoves());
  }

  private static readonly Random eggs = new Random(DateTime.Now.Millisecond);
  public void BestRandomMove()
  {
    var columns = eggs.Shuffle(Enumerable.Range(1, GetUpperBound() - 1).ToArray());

    for (var c = 0; c < columns.Count; c++)
    {
      var rows = GetMarkers();
      BestMove(columns[c], rows[columns[c] - 1]);
    }

    Draw();
    Broadcaster.Broadcast(_Bitmap.Value);
  }


  public void BestMove()
  {
    for (var column = GetLowerBound(); column < GetUpperBound(); column++)
    {
      var rows = GetMarkers();
      BestMove(column, rows[column - 1]);
    }

    Draw();
    Broadcaster.Broadcast(_Bitmap.Value);
  }

  private void BestMove(int column, int row)
  {
    int lowRow = row;
    Tuple<List<ICollision>, int> fewestCrashes = Tuple.Create(new List<ICollision>(), Int32.MaxValue);
    
        
    for(int r = GetLowerBound(); r < GetUpperBound(); r++)
    {
      Swap(column, row, column, r);
      Update();
      row = r;
      
      var crashes = CrashTestDummy.CountCollisions(this, column, row);
      Connect(crashes.Item1);

      if (crashes.Item2 < fewestCrashes.Item2)
      {
        fewestCrashes = crashes;
        lowRow = row;
      }
    }

    if (fewestCrashes != null)
    {
      Swap(column, row, column, lowRow);
      Update();
    }
      
    Connect(fewestCrashes.Item1);
  }
  
  private void Update()
  {
    Draw();
    Broadcaster.Broadcast(_Bitmap.Value);
  }
  
  public void Connect(List<ICollision> crashes)
  {
    foreach (var crash in crashes)
    {
      Connect(crash.Collisions.First(), crash.Collisions.Last());
    }
  }
}


public interface IListener
{
  void Listen(object o, bool slow);
  bool Listening { get; }
}

public class Broadcaster
{

  private static List<IListener> Listeners = new List<IListener>();

  public static void Broadcast(object o, bool slow = false)
  {
    foreach (var listener in Listeners)
    {
      if (listener.Listening)
        listener.Listen(o, slow);
    }
  }

  public static void Add(IListener listener)
  {
    if (Listeners.Contains(listener)) return;
    Listeners.Add(listener);
  }
}

/* Collision Elements */

public interface ICollision
{
  bool ContainsCollisions(Matrix o);
  List<ICollision> FindCollisions(Matrix o);
  List<MatrixItem> Collisions{ get; set; }
  [Description("Find a position (verices) where this item does not collide.")]
  MatrixItem HuntTheCalydonianBoar(MatrixItem item, Matrix o);
  string Name { get; }
}


public static class CrashTestDummy
{
  private static readonly ICollision[] collisionTests = new ICollision[]{
    new DiagonalUpCollision(), new DiagonalDownCollision(), 
    new VerticalUpCollision(), new VerticalDownCollision(),
    new LinearDownCollision(), new LinearUpCollision(),
    new HorizontalLeftCollision(), new HorizontalRightCollision()
  };
  
  public static List<ICollision> Find(Matrix o, params ICollision[] tests)
  {
    var list = new List<ICollision>();
    foreach (var test in tests)
    {
      list.AddRange(test.FindCollisions(o));
    }
    return list;
  }
  
  public static List<ICollision> FindAll(Matrix o)
  {
    var list = new List<ICollision>();
    foreach(var test in collisionTests)
    {
      list.AddRange(test.FindCollisions(o));
    }

    return list;
  }

  public static Tuple<List<ICollision>, int> CountCollisions(Matrix o, int column, int row)
  {
    var list = new List<ICollision>();
    foreach (var test in collisionTests)
    {
      list.AddRange(test.FindCollisions(o));
    }
    
    var columnCollisions = list.Where(x => x.Collisions.Any(y => y.Column == column && y.Row == row)).ToList();
    return Tuple.Create(columnCollisions, columnCollisions.Count);
  }

  public static bool HasCollisions(Matrix o)
  {
    return FindAll(o).Count == 0;
  }
}

public abstract class AbstractCollision : ICollision
{
  public List<MatrixItem> Collisions { get; set; } = new List<MatrixItem>();

  public virtual bool ContainsCollisions(Matrix o)
  {
    return FindCollisions(o).Count > 0;
  }

  public abstract List<ICollision> FindCollisions(Matrix o);

  public virtual MatrixItem HuntTheCalydonianBoar(MatrixItem item, Matrix o)
  {
    return MatrixItem.Empty;
  }

  protected IEnumerable<MatrixItem> MarkedItems(Matrix o)
  {
    for (int column = o.GetLowerBound(); column < o.GetUpperBound(); column++)
    {
      for (int row = o.GetLowerBound(); row < o.GetUpperBound(); row++)
      {
        if (o[column, row].IsHit())
          yield return o[column, row];
      }
    }
  }

  public virtual string Name { get; protected set; } = "AbstractCollision";
}

public class DiagonalUpCollision : AbstractCollision
{
  public DiagonalUpCollision() { }
  protected DiagonalUpCollision(MatrixItem collider1, MatrixItem collider2)
  {
    Collisions.Add(collider1);
    Collisions.Add(collider2);
  }

  public override List<ICollision> FindCollisions(Matrix o)
  {
    var results = new List<ICollision>();

    foreach (var item in MarkedItems(o))
    {
      int row = item.Row;
      
      for (int column = item.Column + 1; column < o.GetUpperBound(); column++)
      {
        row--;
        if( row < o.GetLowerBound()) break;
        if (o[column, row].IsHit())
        {
          results.Add(new DiagonalUpCollision(item, o[column, row]));
        }
  
      }
    }

    return results;
  }

}

public class DiagonalDownCollision : AbstractCollision
{
  public DiagonalDownCollision() { }
  protected DiagonalDownCollision(MatrixItem collider1, MatrixItem collider2)
  {
    Collisions.Add(collider1);
    Collisions.Add(collider2);
  }

  public override List<ICollision> FindCollisions(Matrix o)
  {
    var results = new List<ICollision>();

    foreach (var item in MarkedItems(o))
    {
      int row = item.Row;
      
      for (int column = item.Column + 1; column < o.GetUpperBound(); column++)
      {
        row++;
        if( row >= o.GetUpperBound()) break;
        
        if (o[column, row].IsHit())
        {
          results.Add(new DiagonalDownCollision(item, o[column, row]));
        }

      }
    }

    return results;
  }
}

public class VerticalUpCollision : AbstractCollision
{
  public VerticalUpCollision() { }
  protected VerticalUpCollision(MatrixItem collider1, MatrixItem collider2)
  {
    Collisions.Add(collider1);
    Collisions.Add(collider2);
  }

  public override List<ICollision> FindCollisions(Matrix o)
  {
    var results = new List<ICollision>();

    foreach (var item in MarkedItems(o))
    {
      for (int row = item.Row - 1; row >= o.GetLowerBound(); row--)
      {
        if (o[item.Column, row].IsHit())
        {
          results.Add(new VerticalUpCollision(item, o[item.Column, row]));
        }
      }
    }

    return results;
  }
}

public class VerticalDownCollision : AbstractCollision
{
  public VerticalDownCollision() { }
  protected VerticalDownCollision(MatrixItem collider1, MatrixItem collider2)
  {
    Collisions.Add(collider1);
    Collisions.Add(collider2);
  }

  public override List<ICollision> FindCollisions(Matrix o)
  {
    var results = new List<ICollision>();

    foreach (var item in MarkedItems(o))
    {
      for (int row = item.Row + 1; row < o.GetUpperBound(); row++)
      {
        if (o[item.Column, row].IsHit())
        {
          results.Add(new VerticalDownCollision(item, o[item.Column, row]));
        }
      }
    }

    return results;
  }
}

public class HorizontalLeftCollision : AbstractCollision
{
  public HorizontalLeftCollision() { }
  protected HorizontalLeftCollision(MatrixItem collider1, MatrixItem collider2)
  {
    Collisions.Add(collider1);
    Collisions.Add(collider2);
  }

  public override List<ICollision> FindCollisions(Matrix o)
  {
    var results = new List<ICollision>();

    foreach (var item in MarkedItems(o))
    {
      for (int column = item.Column - 1; column >= o.GetLowerBound(); column--)
      {
        if (o[column, item.Row].IsHit())
        {
          results.Add(new HorizontalLeftCollision(item, o[column, item.Row]));
        }
      }
    }

    return results;
  }
}

public class HorizontalRightCollision : AbstractCollision
{
  public HorizontalRightCollision() { }
  protected HorizontalRightCollision(MatrixItem collider1, MatrixItem collider2)
  {
    Collisions.Add(collider1);
    Collisions.Add(collider2);
  }

  public override List<ICollision> FindCollisions(Matrix o)
  {
    var results = new List<ICollision>();

    foreach (var item in MarkedItems(o))
    {
      for (int column = item.Column + 1; column < o.GetUpperBound(); column++)
      {
        if (o[column, item.Row].IsHit())
        {
          results.Add(new HorizontalRightCollision(item, o[column, item.Row]));
        }
      }
    }

    return results;
  }
}

public class LinearUpCollision : AbstractCollision
{
  public LinearUpCollision() { }
  protected LinearUpCollision(params MatrixItem[] args)
  {
    Collisions.AddRange(args);
  }

  public override List<ICollision> FindCollisions(Matrix o)
  {
    var results = new List<ICollision>();
    var hits = new List<MatrixItem>();

    foreach (var item in MarkedItems(o))
    {
      hits.Clear();
      hits.Add(item);

      int row = item.Row;
      for (int column = item.Column + 1; column < o.GetUpperBound(); column += 1)
      {
        row -= 2;
        if(row >= o.GetLowerBound())
        {
          if (o[column, row].IsHit())
          {
            hits.Add(o[column, row]);
          }
        }
      }

      if (hits.Count > 2)
        results.Add(new LinearUpCollision(hits.ToArray()));

    }

    return results;
  }
}

public class LinearDownCollision : AbstractCollision
{
  public LinearDownCollision() { }
  protected LinearDownCollision(params MatrixItem[] args)
  {
    Collisions.AddRange(args);
  }

  public override List<ICollision> FindCollisions(Matrix o)
  {
    var results = new List<ICollision>();
    var hits = new List<MatrixItem>();

    foreach (var item in MarkedItems(o))
    {
      hits.Clear();
      hits.Add(item);

      int row = item.Row;
      for (int column = item.Column + 1; column < o.GetUpperBound(); column += 1)
      {
        row += 2;
        if(row < o.GetUpperBound())
        {
          if (o[column, row].IsHit())
          {
            hits.Add(o[column, row]);
          }
        }
      }

      if (hits.Count > 2)
        results.Add(new LinearDownCollision(hits.ToArray()));

    }

    return results;
  }
}

public static class RandomExtensions
{
  
  public static List<int> Shuffle(this Random random, int[] array)
  {
    int n = array.Length;
    while (n > 1)
    {
      int k = random.Next(n--);
      int temp = array[n];
      array[n] = array[k];
      array[k] = temp;
    }
  
    return new List<int>(array);
  }
  
  public static bool HasDuplicates(this int[] k)
  {
    if(k == null || k.Length == 0) return false;
    return (k.Distinct().Count() != k.Length);
  }
  
  public static bool IsDuplication(this List<int[]> list, int[] proposedConfiguration)
  {
    try
    {
      var result = list.Any(x => x.Except(proposedConfiguration).Count() == 0);
      if(result)
        Debug.WriteLine($"Random duplication {string.Join(" ", proposedConfiguration)} ignored.");
        
      return result;
    }
    catch(Exception ex)
    {
      Debug.WriteLine(ex.Message);
      return false;
    }
    
  }

}

public class WinnningMoveException : Exception
{
  public WinnningMoveException(string message) : base(message) {}
}

public class FailedSieveException : Exception
{
  public FailedSieveException(string message) : base(message) { }
}

/* End Collision Elements */

#region private::Tests

[Fact] void Test_Xunit() => Assert.True (1 + 1 == 2);

#endregion