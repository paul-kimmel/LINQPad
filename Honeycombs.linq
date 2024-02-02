<Query Kind="Program">
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Drawing.Drawing2D</Namespace>
</Query>

void Main()
{
  List<Hexagon> list = new List<Hexagon>();
  
  
  for(int x = 0; x < 25; x++)
  {
    for(float y = 0; y < 25; y++)
    {
      list.Add(new Hexagon(x, y, Color.FromArgb(90, Color.Orange)));
    }
  }
  list.Paint();
  
}

public static class HexagonExtensions
{
  public static void Paint(this List<Hexagon> list)
  {
    Array.ForEach(list.ToArray(), o => o.Paint());
    Hexagon.GetBitmap().Dump();
  }
}

public class Hexagon
{
  public static readonly int dimension = 2000;
  //public static readonly int node_dimension = 60;
  protected static readonly Bitmap bitmap = new Bitmap(dimension, dimension);
  private Graphics graphics = null;
  private static readonly int FontSize = 10;
  
  protected static FontFamily fontFamily = new FontFamily("Calibri");
  protected static Font font = new Font(fontFamily, FontSize, FontStyle.Regular,
   GraphicsUnit.Point);
   
  private int row;
  private float col;
  
  public static Bitmap GetBitmap()
  {
    return bitmap;
  }

  public Hexagon(int row, float col)
  {
    this.row = row;
    this.col = col;
    Points = HexToPoints(Height, row, col); 
  }

  public Hexagon(int row, float col, Color color)
  {
    this.row = row;
    this.col = col;
    Points = HexToPoints(Height, row, col);
    Brush = new SolidBrush(color);
  }


  public string Title{ get; set;} = "Toast";
  public PointF[] Points{ get; protected set;}
  public static int Height{ get; set; } = 80;
  public Brush Brush{ get; set; } = new SolidBrush(Color.White);
  
  public void Paint()
  {
    FontFamily fontFamily = new FontFamily("Calibri");
    Font font = new Font(fontFamily, 10, FontStyle.Regular,
     GraphicsUnit.Point);

    using (var graphics = Graphics.FromImage(bitmap))
    {
      graphics.SmoothingMode = SmoothingMode.AntiAlias;
      graphics.FillPolygon(Brush, Points);
      DrawHexagon(graphics);
      DrawLabel(graphics);
      DrawTitle(graphics);
    }
  }

  protected virtual void DrawTitle(Graphics graphics)
  {
    var path = new GraphicsPath();
    path.AddPolygon(Points);
    
    var region = new Region(path);
    
    //we can use this to show the whole title
    graphics.SetClip(region, CombineMode.Replace); 
    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
    graphics.DrawString(Title, font, Brushes.Black, new PointF(Points[1].X- 8, Points[1].Y + FontSize));
    
  }

  protected virtual void DrawLabel(Graphics graphics)
  {
    graphics.DrawString($"{row},{col}", font, Brushes.Black, Points[1]);
  }

  protected virtual void DrawHexagon(Graphics graphics)
  {
    graphics.DrawPolygon(Pens.Black, Points);
  }

  protected virtual float HexWidth(float height)
  {
    return (float)(4 * (height / 2 / Math.Sqrt(3)));
  }


  protected virtual PointF[] HexToPoints(float height, float row, float col)
  {
    // Start with the leftmost corner of the upper left hexagon.
    float width = HexWidth(height);
    float y = height / 2;
    float x = 0;

    // Move down the required number of rows.
    y += row * height;

    // If the column is odd, move down half a hex more.
    if (col % 2 == 1) y += height / 2;

    // Move over for the column number.
    x += col * (width * 0.75f);

    // Generate the points.
    return new PointF[]
        {
            new PointF(x, y),
            new PointF(x + width * 0.25f, y - height / 2),
            new PointF(x + width * 0.75f, y - height / 2),
            new PointF(x + width, y),
            new PointF(x + width * 0.75f, y + height / 2),
            new PointF(x + width * 0.25f, y + height / 2),
        };
  }
}
