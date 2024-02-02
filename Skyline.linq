<Query Kind="Program">
  <NuGetReference>JustMock</NuGetReference>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Drawing.Text</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Telerik.JustMock</Namespace>
  <Namespace>Xunit</Namespace>
</Query>

#load "xunit"

void Main()
{
    //RunTests();  // Call RunTests() or press Alt+Shift+T to initiate testing.

    int[,] __city = {{0, 0, 0, 0},
                     {0, 0, 0, 0},
                     {0, 0, 0, 0},
                     {0, 0, 0, 0}};
                   
    int[,] city = {{3, 0, 8, 4}, 
                   {2, 4, 5, 7}, 
                   {9, 2, 6, 3},
                   {0, 3, 1, 0}};
                   
    int[,] _city = {{3, 0, 8},
                   {2, 4, 5},
                   {9, 2, 6},
                   {0, 3, 1}};

    int[][] city2 ={new int[]{3, 0, 8, 4}, 
                    new int[]{2, 4, 5, 7}, 
                    new int[]{9, 2, 6, 3},
                    new int[]{0, 3, 1, 0}};


    CityPlanner.Adjustment(city2).Dump();
    
    
    CityPlanner.Adjustment(city).Dump();
    ColoredSkylineRenderer.Run(city);
    
    var result = CityPlanner.IncreaseSkyline(city);
    result.Dump();
    ColoredSkylineRenderer.Run(result);
}

public static class CityPlanner
{
    public static int[,] IncreaseSkyline(this int[,] city)
    {
        for (int i = 0; i < city.Length; i++)
        {
            int j = city.GetJ(i);
            int k = city.GetK(i);
            int height = city.GetHeight(i);
            int minmax = city.GetMinMax(i);
            city[j, k] = minmax;
        }
        
        return city;
        
    }

    public static int Adjustment(this int[,] city)
    {
        int adjusted = 0;
        
        for(int i = 0; i < city.Length; i++)
        {
            adjusted += city.GetMinMax(i) - city.GetHeight(i);
            var j = city.GetJ(i);
            var k = city.GetK(i);
            var colmax = city.GetCol(k).Max();
            var rowmax = city.GetRow(j).Max();

            Console.WriteLine($"i:{i}, j:{j}, k:{k}, colmax:{colmax}, rowmax:{rowmax}, minmax:{city.GetMinMax(i)}");
            
        }

        return adjusted;
    }
    
    public static int Adjustment(this int[][] city)
    {
        int adjusted = 0;
        
        var flat = city.SelectMany(a => a);
        
        for(int i=0; i < flat.Count(); i++)
        {
            var minmax = Math.Min(city[i /city.Length].Max(), 
                flat.Where((x, n) => n % city.Length == i % city.Length).Max()); 
                                        
            adjusted += minmax - city[i/city.Length][i % city.Length];
        }
        
        return adjusted;
    }
    
    public static int BoundCol(this int[,] city)
    {
        return city.GetUpperBound(1) + 1;
    }

    public static int BoundRow(this int[,] city)
    {
        return city.GetUpperBound(0) + 1;
    }

    public static int GetMinMax(this int[,] city, int i)
    {
        return city.GetMinMax(city.GetJ(i), city.GetK(i));
    }
    
    public static int GetJ(this int[,] city, int i)
    {
        return i / city.BoundRow();
    }

    public static int GetK(this int[,] city, int i)
    {
        return i % city.BoundCol();
    }

    public static int GetHeight(this int[,] city, int i)
    {
        return city[i/city.BoundCol(), i%city.BoundCol()];
    }
    
    public static int GetMinMax(this int[,] city, int row, int col)
    {
        return Math.Min(city.GetCol(col).Max(), city.GetRow(row).Max());
    }
    
    public static IEnumerable<int> GetRow(this int[,] city, int row = 0)
    {
        for(int i=0; i<city.GetLength(1); i++)
            yield return city[row, i];
    }

    public static IEnumerable<int> GetCol(this int[,] city, int col = 0)
    {
        for (int i = 0; i < city.GetLength(0); i++)
            yield return city[i, col];
    }
}

public class ColoredSkylineRenderer : SkylineRenderer
{
    public static void Run(int[,] city)
    {
        var renderer = new ColoredSkylineRenderer();
        renderer.Draw(city);
    }

    protected virtual void DrawRectangle(Graphics graphics, string text, int x, int y, int w)
    {
        var rect = GetRectangle(x, y, w);
        graphics.FillRectangle(GetFillColor(text), rect);
        graphics.DrawRectangle(Pens.Black, GetRectangle(x, y, w));
    }

    protected Brush GetFillColor(string text)
    {
        int number;
        if (Int32.TryParse(text, out number) == false) return Brushes.White;
        return GetFillColor(number);
    }

    protected Brush GetFillColor(int digit) => digit switch
    {
        1 => Brushes.LightSteelBlue,
        var x when (x == 0 || x == 2) => Brushes.LightYellow,
        var x when In(x, new int[] { 1, 3 }) => Brushes.LightGreen,
        var x when In(x, new int[] { 6, 7 }) => Brushes.LightCoral,
        var x when In(x, new int[] { 4, 5, 7 }) => Brushes.LightGoldenrodYellow,
        var x when In(x, new int[] { 8, 9 }) => Brushes.LightSkyBlue,
        _ => Brushes.White

    };

    public bool In<T>(T x, T[] a)
    {
        return a.Contains(x);
    }

    protected override void DrawNumberedTile(Graphics graphics, string text, int x, int y, int w)
    {
        DrawRectangle(graphics, text, x, y, w);
        DrawString(graphics, text, x, y, w);
    }

}

public class SkylineRenderer
{
    public int GridSize = 16;
    private Bitmap bitmap;
    private readonly int width = 401;

    public void Draw(int[,] positions)
    {
        var list = new List<int?>();
        foreach (var item in positions)
            list.Add(item);


        Draw(list.ToArray());
    }

    public void Draw(int?[] positions, string? message = null)
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

    //experimental
    public void DrawNumberedTile(int?[] positions, int k, int x, int y)
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


    void DrawGameBoard(Graphics graphics, int width) => graphics.DrawRectangle(Pens.Black, new Rectangle(0, 0, width - 1, width - 1));

    protected virtual void DrawNumberedTile(Graphics graphics, string text, int x, int y, int w)
    {
        DrawRectangle(graphics, x, y, w);
        DrawString(graphics, text, x, y, w);
    }

    protected void DrawString(Graphics graphics, string text, int x, int y, int w) => graphics.DrawString(text, font, Brushes.Black,
                    GetTextPoint(x, y, w));

    protected void DrawRectangle(Graphics graphics, int x, int y, int w)
    {
        var rect = GetRectangle(x, y, w);
        graphics.FillRectangle(Brushes.White, rect);
        graphics.DrawRectangle(Pens.Black, GetRectangle(x, y, w));
    }


    protected Rectangle GetRectangle(int x, int y, int w) => new Rectangle(GetX(x, w), GetY(y, w), w, w);
    PointF GetTextPoint(int x, int y, int w) => new Point(GetTextX(x, w), GetTextY(y, w));


    private int GetX(int x, int width) => GetPosition(x, width);
    private int GetY(int y, int width) => GetPosition(y, width);

    int GetPosition(int k, int width)
    {
        return k * width;
    }

    private int GetTextX(int x, int width) => GetTextPosition(x, width);
    private int GetTextY(int y, int width) => GetTextPosition(y, width);

    int GetTextPosition(int k, int width)
    {
        return k * width + width / 3;
    }

    private Font font = new Font(new FontFamily("Arial"), 16);
    private Font exploredFont = new Font(new FontFamily("Arial"), 8);

    void Guard(int?[] positions)
    {
        Noop();
        //Guard(IsPerfectSquare(positions.Length), 
        //    () => throw new ArgumentOutOfRangeException("positions", "Not a perfect square"));
    }

    void Noop()
    {
        return;
    }

    void Guard(bool test, Action action)
    {
        if (test == false)
            action();
    }

    private bool IsPerfectSquare(int number)
    {
        return (Math.Sqrt(number) % 1 == 0);
    }

    public void Shuffle<T>(Random random, T[] array)
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


    void InitializeGrid(int?[] positions)
    {
        Draw(positions);
    }

    public List<int?> CreatePositions() => new List<int?>(GridSize);
}


#region private::Tests

private static readonly int[,] cityScape = { { 3, 0, 8, 4 }, { 2, 4, 5, 7 }, { 9, 2, 6, 3 }, { 0, 3, 1, 0 } };
private static readonly int[,] cityScapeRectangle = {{3, 0, 8},
                                                     {2, 4, 5},
                                                     {9, 2, 6},
                                                     {0, 3, 1}};
    
[Fact]
void Test_Xunit() => Assert.True(
    CityPlanner.Adjustment(cityScape) == 35);
       
[Fact]
void Test_GetRow() => Assert.True(CityPlanner.GetRow(cityScape, 2).SequenceEqual(new int[] { 9, 2, 6, 3 }));

[Fact]
void Test_RowMax() => Assert.True(cityScape.GetRow(2).Max() == 9);

[Fact]
void Test_GetCol() => Assert.True(CityPlanner.GetCol(cityScape, 0).SequenceEqual(new int[] { 3, 2, 9, 0 }));

[Fact]
void Test_GetMinMaxInversion_Test() 
{
    Assert.True(CityPlanner.GetMinMax(cityScape, 0, 1) == 4); 
}


[Fact]
void Test_GetMinMax()
{
    Assert.True(CityPlanner.GetMinMax(cityScape, 0, 1)==4);
    
}

[Fact]
void Test_GetBoundColumn()
{
    Assert.True(CityPlanner.BoundCol(cityScapeRectangle) == 3);
}

[Fact]
void Test_GetBoundRow()
{
    Assert.True(CityPlanner.BoundRow(cityScapeRectangle) == 4);
}


[Fact]
void Test_GetRowLength()
{
    Console.WriteLine(cityScapeRectangle.GetLength(0));
    Assert.True(cityScapeRectangle.GetLength(0) == 4);
}

[Fact]
void Test_GetColLength()
{
    Console.WriteLine(cityScapeRectangle.GetLength(1));
    Assert.True(cityScapeRectangle.GetLength(1) == 3);
}

#endregion