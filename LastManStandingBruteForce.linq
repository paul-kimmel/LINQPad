<Query Kind="Program">
  <Namespace>Xunit</Namespace>
</Query>

#load "xunit"

void Main()
{
    //RunTests();  // Call RunTests() or press Alt+Shift+T to initiate testing.

    var watch = new Stopwatch();
    watch.Start();
    RunBruteForceRule(RunningLoose.Split(',').ToList()).Dump(); 
    watch.Stop();
    Console.WriteLine(watch.Elapsed);
}


public string RunBruteForceRule(List<string> animals)
{   
    
    for (var i = 0; i < animals.Count; i++)
    {
        var index = -1;
        
        if (i > 0 && Rules.Any(o => o.StartsWith(animals[i]) && o.EndsWith(animals[i-1])))
            index = i - 1;
            
        else if (i < animals.Count - 1 && Rules.Any(o => o.StartsWith(animals[i]) && o.EndsWith(animals[i+1])))
            index = i + 1;
            
        if(index > -1)
        {
            animals.RemoveAt(index);
            RunBruteForceRule(animals);
        }
    }
    
    return string.Join(',', animals);
}



static string RunningLoose { get; set; } = "fox,bug,chicken,grass,sheep";

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

[Fact] void BruteForceRule_Test() => Assert.True (RunBruteForceRule(RunningLoose.Split(',').ToList()) == "fox");

#endregion