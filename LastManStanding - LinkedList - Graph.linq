<Query Kind="Program">
  <Namespace>Xunit</Namespace>
</Query>

#load "xunit"

void Main()
{
    //RunTests();  // Call RunTests() or press Alt+Shift+T to initiate testing.

    var watch = new Stopwatch();
    watch.Start();
    Console.WriteLine(RunningLoose + Environment.NewLine);
    $"\r\n{RunBruteForceRule(RunningLoose.Split(',').ToList())} is/are the last man standing".Dump();
    watch.Stop();
    Console.WriteLine(watch.Elapsed);
}

static string RunningLoose { get; set; } = "fox,bug,chicken,grass,sheep";

public string RunBruteForceRule(List<string> animals)
{
    var stack = new Stack<string>();

    foreach (var item in animals)
    {
        var biota = item;
    Loop:
        
        if (stack.Count > 0 && Rules.Any(x => x.StartsWith(biota) && x.EndsWith(stack.Peek())))
        {
            Trace(biota, stack.Pop());//discard and keep current biota
                                      //stack.Pop(); 
            goto Loop;
        }
        else if (stack.Count > 0 && Rules.Any(x => x.StartsWith(stack.Peek()) && x.EndsWith(biota)))
        {
            Trace(stack.Peek(), biota);
            biota = stack.Pop(); //pop top of stack to current biota
            goto Loop;
        }
        else
        {
            stack.Push(biota);
        }
    }
    
    return string.Join(',', stack.Reverse());
    
    void Trace(string predator, string prey)
    {
        Console.WriteLine($"{predator} eats {prey}");
    }
}


static readonly string[] Rules = new string[]
    {
        "antelope eats grass",
        "big-fish eats little-fish",
        "bug eats leaves",
        "bear eats big-fish",
        "bear eats bug",
        "bear eats chicken",
        "bear eats cow",
        "bear eats leaves",
        "bear eats sheep",
        "chicken eats bug",
        "cow eats grass",
        "fox eats chicken",
        "fox eats sheep",
        "giraffe eats leaves",
        "lion eats antelope",
        "lion eats cow",
        "panda eats leaves",
        "sheep eats grass"
    };

#region private::Tests

[Fact] void BruteForceRule_Test() => Assert.True(RunBruteForceRule(RunningLoose.Split(',').ToList()) == "fox");

#endregion