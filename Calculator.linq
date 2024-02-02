<Query Kind="Program">
  <Namespace>Xunit</Namespace>
</Query>

#load "xunit"

void Main()
{
	RunTests();  // Call RunTests() or press Alt+Shift+T to initiate testing.

	var y = Calculator.Calculate("2 - 2 * 3");
	y.Dump();

	var z = Calculator.Calculate("(4+51)*(6-2)"); //(55) * (4) == 220
	z.Dump();
	
}

public class Calculator
{
	private static readonly char LEFT_PARENTHESES = '(';
	
	public static double Calculate(string expression)
	{
		Announce();
		try
		{
			return Announce(PerformCalculate(expression), expression);
		}
		catch(Exception ex)
		{
			Announce(default(double), "empty");
			OutputWriter.TraceLine(ex.Message);
			throw ex;
		}
	}

	private static void Announce()
	{
		OutputWriter.TraceLine(new string('*', 40));
	}
	
	private static double Announce(double result, string expression)
	{
		OutputWriter.TraceLine($"{expression} => {result}");
		OutputWriter.TraceLine(new string('*', 40));
		OutputWriter.TraceLine();
		return result;
	}

	public static double PerformCalculate(string expression)
	{
		if (string.IsNullOrWhiteSpace(expression))
			throw new ArgumentNullException(expression);

		Clear();
		
		try
		{
			Tokenize(expression);
			PostFix();
			//Dump();
			
			return CalculateRpn();
		}
		catch (Exception ex)
		{
			OutputWriter.TraceLine(ex.Message);
		}

		return 0.0;
	}


	private static double CalculateRpn()
	{
		var rpn = PostfixStack.Reverse();
		var result = new TokenStack();
		
		foreach(var token in rpn)
		{
			if(token.IsNumber())
				result.Push(token);
			else if(token.IsBinaryOperator())
				PushBinaryOperation(result, token);
			else if(token.IsUnaryOperator())
				PushUnaryOperation(result, token);
			else
				//TODO: Consider just throwing it away???
				throw new ArgumentOutOfRangeException("token", token.ToString());
				
		}
		
		if(result.Count != 1)
			throw new ArgumentException("Expression did not yield a result");
			
		return (result.Pop() as Operand).GetValue();
		
	}

	private static void PushUnaryOperation(TokenStack target, Token o)
	{
		target.Push(o.PerformOp(target.Pop()));
	}

	private static void PushBinaryOperation(TokenStack target, Token o)
	{
		var rhs = target.Pop();
		var lhs = target.Pop();

		target.Push(o.PerformOp(lhs, rhs));
	}

	private static void RestackTokens()
	{
		foreach(var token in PostfixStack.Reverse())
		{
			if(token.IsNumber())
				Tokens.Push(token);
			else
				OperatorTokens.Push(token);
		}
	}

	private static void PostFix()
	{
		foreach(var o in Tokens.Reverse().ToList())
			PostfixStack.Push(o);
			
		foreach(var o in OperatorTokens.ToList())
			PostfixStack.Push(o);
			
		Tokens.Clear();
		OperatorTokens.Clear();
	}

	private static void Clear()
	{
		Tokens.Clear();
		OperatorTokens.Clear();
		PostfixStack.Clear();
	}
	
	private static void Dump()
	{
		Tokens.Dump();
		OperatorTokens.Dump();
		PostfixStack.Dump();
	}

	private static TokenStack PostfixStack { get; set; } = new TokenStack();
	private static TokenStack Tokens{ get; set; } = new TokenStack();
	private static TokenStack OperatorTokens{ get; set; } = new TokenStack();
	
	public static void Tokenize(string expression)
	{
		string current = "";
		
		char lastToken = '\0';
		
		foreach(var o in expression)
		{
			if(Char.IsWhiteSpace(o)) continue;
			
			if(ShouldTreatLikeUnaryMinus(lastToken, o, current))
			{
				current += o;
			}
			else if (IsPartOfNumber(o))
			{
				current += o;
			}
			else if(o.IsRightParentheses())
			{
				CheckPushToken(current);
				var pop = OperatorTokens.Pop();
				while(pop is LeftParenthesesOperator == false)
				{
					Tokens.Push(pop);			
					if(OperatorTokens.TryPop(out pop) == false)
						break;
				}
				current = "";
			}
			else //handles LEFT_PARENTHESES too
			{
				CheckPushToken(current);
				PushOperator(o);
				current = "";
			}

			lastToken = o;
			WriteTokenState();
			
		}

		if (string.IsNullOrWhiteSpace(current) == false)
		{
			Tokens.Push(current);
			WriteTokenState();
		}
	}

	private static bool ShouldTreatLikeUnaryMinus(char lastToken, char o, string current)
	{
		return (lastToken == '\0' || Operator.IsOperator(lastToken)) && o == '-' && current == "";
	}

	private static bool IsPartOfNumber(char o)
	{
		return Char.IsDigit(o) || Char.IsNumber(o) || o.IsDecimalPoint();
	}
	
	private static void WriteTokenState()
	{
		OutputWriter.TraceLine($"{Tokens.ToString()}, {OperatorTokens.ToString()}");
	}

	private static void PushOperator(char o )
	{
		PushOperator(o.ToString());
	}

	private static void CheckPushToken(string token)
	{
		if(string.IsNullOrEmpty(token)) return;
		Tokens.Push(token);
	}
	
	private static void PushOperator(string token)
	{
		var result = TokenFactory.Create(token);
		
		Token peek;
		if (OperatorTokens.TryPeek(out peek))
			CheckPopPushHigherOrderOperator(result, peek);
		
		OperatorTokens.Push(result);				
	}

	private static void CheckPopPushHigherOrderOperator(Token source, Token target)
	{
		if (HasHigherOperatorPrecedence(source, target))
			Tokens.Push(OperatorTokens.Pop());
	}

	private static bool HasHigherOperatorPrecedence(Token source, Token target)
	{
		return target.Precedence > source.Precedence
		&& target.IsParenthesesOperator() == false;
	}
	
}

public enum PrecedenceEnum
{
	None,
	Low,
	Medium,
	High
}

public static class CharExtensions
{
	private static readonly char DECIMAL_POINT = '.';
	private static readonly char RIGHT_PARENTHESES = ')';

	public static bool IsDecimalPoint(this Char o)
	{
		return o == DECIMAL_POINT;
	}

	public static bool IsRightParentheses(this Char o)
	{
		return o == RIGHT_PARENTHESES;
	}
}

public static class TokenExtensions
{
	public static Operand PerformOp(this Token token, params Token[] tokens)
	{
		return token is BinaryOperator ? 
			(token as BinaryOperator).PerformOp(tokens) :
			(token as UnaryOperator).PerformOp(tokens);
	}
}

public class TokenStack : Stack<Token>
{
	public void Push(string token)
	{
		Push(TokenFactory.Create(token));		
	}

	public void Push(char token)
	{
		Push(token.ToString());
	}
	
	public override string ToString()
	{
		return string.Join<Token>(",", this.Reverse().ToArray());
	}
}

public class TokenFactory
{
	private static readonly IToken[] Tokens = new IToken[]
	{
		new NumericOperand(),
		new FactorialOperator(),
		new SquareRootOperator(),
		new ExponentOperator(),
		new AdditionOperator(),
		new SubtractionOperator(),
		new DivisionOperator(),
		new MultiplicationOperator(),
		new UnaryNegationOperator(),
		new LeftParenthesesOperator(),
		new RightParenthesesOperator()
	};
	
	public static Token Create(string token)
	{
		foreach(var o in Tokens)
		{
			if(o.Wants(token))
			{
				Type type = o.GetType();
				var result = (Token)Assembly.GetAssembly(type).CreateInstance(type.FullName);
				result.Text = token;
				return result;
			}
		}

		return new DummyToken() { Text = token };
	}
}


public interface IToken
{
	bool Wants(string token);
	string Text { get; set; }
	PrecedenceEnum Precedence { get; set; }
}

public abstract class Token : IToken
{
	public string Name { get { return GetName(); } }
	public string TokenName => GetTokenName();
	
	protected virtual string GetTokenName()
	{
		return Name;		
	}

	public override string ToString()
	{
		return TokenName;
	}

	private string GetName()
	{
		return this.GetType().Name;
	}
	
	public PrecedenceEnum Precedence{get; set;}
	public string Text{ get; set; }

	public bool IsNumeric
	{
		get { return IsNumber(); }
	}
	
	public bool IsNumber()
	{
		return this is NumericOperand;		
	}
	
	public bool IsOperator()
	{
		return this is Operator;
	}
	
	public bool IsParenthesesOperator()
	{
		return this is LeftParenthesesOperator || this is RightParenthesesOperator;
	}

	public bool IsBinaric
	{
		get { return IsBinaryOperator(); }
	}
	
	public bool IsBinaryOperator()
	{
		return this is BinaryOperator;
	}

	public bool IsUnaric
	{
		get { return IsUnaryOperator(); }
	}
	
	public bool IsUnaryOperator()
	{
		return this is UnaryOperator;
	}
	
	public virtual bool Wants(string token)
	{
		try
		{
			return DoWants(token);
		}
		catch(Exception ex)
		{
			OutputWriter.TraceLine(ex.Message);
		}
		return false;
	}
	
	protected abstract bool DoWants(string token);
}

public class DummyToken : Token
{
	protected override bool DoWants(string token)
	{
		return true;
	}
}


public class NumericOperand : Operand
{
	protected override bool DoWants(string token)
	{
		double result;
		if(Double.TryParse(token, out result))
			return true;
			
		return false;
	}

	public static Operand Create(double o)
	{
		return new NumericOperand() { Text = o.ToString()};
	}
	
	public override string ToString()
	{
		return GetValue().ToString();
	}
}

public interface IOperator
{
	Operand PerformOp(params Operand[] operands);
	Operand PerformOp(params Token[] tokens);
}

public abstract class Operator : Token, IOperator
{
	private static readonly char[] operators = { '+', '-', '*', '/', '(', ')', '^', '!', '√' };
	public static bool IsOperator(char token)
	{
		return operators.Contains(token);
	}
	
	public static bool IsOperator(string token)
	{
		if(token.Length != 1)
			throw new ArgumentException(token);
			
		return IsOperator(token[0]);
	}

	public virtual Operand PerformOp(params Token[] tokens)
	{
		var operands = new List<Operand>();
		Array.ForEach(tokens, o => operands.Add(o as Operand));
		return PerformOp(operands.ToArray());	
	}

	protected void CheckParameterLength(Operand[] operands, int length = 1)
	{
		if (operands.Length != length)
			throw new ArgumentException($"Wrong number of arguments: {operands.Length}");
	}

	protected void CheckOperandType(Operand[] operands, int index = 0)
	{
		if (operands[index] is NumericOperand == false)
			throw new ArgumentException($"Argument is not numeric: {operands[index]}");
	}

	public abstract Operand PerformOp(params Operand[] operands);
	
}

public class LeftParenthesesOperator : Operator
{
	public LeftParenthesesOperator()
	{
		Precedence = PrecedenceEnum.High;
	}
	
	protected override bool DoWants(string token)
	{
		return Operator.IsOperator(token) && token == "(";
	}

	public override Operand PerformOp(params Operand[] operands)
	{
		return Operand.Empty; // for now
	}
	
	protected override string GetTokenName()
	{
		return "(";
	}
}

public class RightParenthesesOperator : Operator
{
	public RightParenthesesOperator()
	{
		Precedence = PrecedenceEnum.High;
	}
	
	protected override bool DoWants(string token)
	{
		return Operator.IsOperator(token) && token == ")";
	}

	public override Operand PerformOp(params Operand[] operands)
	{
		return Operand.Empty; // for now
	}

	protected override string GetTokenName()
	{
		return ")";
	}
}


public class FactorialOperator : UnaryOperator 
{
	public FactorialOperator()
	{
		Precedence = PrecedenceEnum.High;
	}
	
	protected override bool DoWants(string token)
	{
		return Operator.IsOperator(token) && token == "!";
	}

	public override Operand PerformOp(params Operand[] operands)
	{
		Guard(operands);
		
		double value = operands[0].GetValue();
		OutputWriter.TraceLine($"Factorial({value})");
		return NumericOperand.Create(Factorial(value));
		
	}

	protected double Factorial(double n)
	{
		return n > 1 ? n * Factorial(n - 1) : n;
	}

	protected override string GetTokenName()
	{
		return "!";
	}
}

public class SquareRootOperator : UnaryOperator 
{
	public SquareRootOperator()
	{
		Precedence = PrecedenceEnum.Medium;
	}

	protected override bool DoWants(string token)
	{
		return Operator.IsOperator(token) && token == "√";
	}

	public override Operand PerformOp(params Operand[] operands)
	{
		Guard(operands);

		double value = operands[0].GetValue();
		OutputWriter.TraceLine($"Sqrt({value})");

		return NumericOperand.Create(Math.Sqrt(value));
	}

	protected override string GetTokenName()
	{
		return "√";
	}
}

public class UnaryNegationOperator : UnaryOperator 
{
	public UnaryNegationOperator()
	{
		Precedence = PrecedenceEnum.High;
	}
	
	protected override bool DoWants(string token)
	{
		return Operator.IsOperator(token) && token == "-";
	}

	public override Operand PerformOp(params Operand[] operands)
	{
		Guard(operands);
		
		double value = operands[0].GetValue();
		OutputWriter.TraceLine($"Neg({value})");

		return NumericOperand.Create(value * -1);
	}

	protected override string GetTokenName()
	{
		return "-";
	}
}

public class ExponentOperator : BinaryOperator 
{
	public ExponentOperator()
	{
		Precedence = PrecedenceEnum.High;
	}
	
	protected override bool DoWants(string token)
	{
		return Operator.IsOperator(token) && token == "^";
	}

	public override Operand PerformOp(params Operand[] operands)
	{
		Guard(operands);

		double value1 = operands[0].GetValue();
		double value2 = operands[1].GetValue();
		OutputWriter.TraceLine($"Exp({value1}, {value2})");

		return NumericOperand.Create(Math.Pow(value1, value2));
	}

	protected override string GetTokenName()
	{
		return "^";
	}
}

public class AdditionOperator : BinaryOperator 
{
	public AdditionOperator()
	{
		Precedence = PrecedenceEnum.Low;
	}
	
	protected override bool DoWants(string token)
	{
		return Operator.IsOperator(token) && token == "+";
	}

	public override Operand PerformOp(params Operand[] operands)
	{
		Guard(operands);

		double value1 = operands[0].GetValue();
		double value2 = operands[1].GetValue();
		OutputWriter.TraceLine($"Add({value1}, {value2})");

		return NumericOperand.Create(value1 + value2);
	}

	protected override string GetTokenName()
	{
		return "+";
	}
}

public class SubtractionOperator : BinaryOperator 
{
	public SubtractionOperator()
	{
		Precedence = PrecedenceEnum.Low;
	}
	
	protected override bool DoWants(string token)
	{
		return Operator.IsOperator(token) && token == "-";
	}

	public override Operand PerformOp(params Operand[] operands)
	{
		Guard(operands);

		double value1 = operands[0].GetValue();
		double value2 = operands[1].GetValue();
		OutputWriter.TraceLine($"Subtract({value1}, {value2})");
		
		return NumericOperand.Create(value1 - value2);

	}

	protected override string GetTokenName()
	{
		return "-";
	}
}

public class DivisionOperator : BinaryOperator 
{
	public DivisionOperator()
	{
		Precedence = PrecedenceEnum.Medium;
	}
	
	protected override bool DoWants(string token)
	{
		return Operator.IsOperator(token) && token == "/";
	}

	public override Operand PerformOp(params Operand[] operands)
	{
		Guard(operands);


		double value1 = operands[0].GetValue();
		double value2 = operands[1].GetValue();
		OutputWriter.TraceLine($"Divide({value1}, {value2})");
		
		return NumericOperand.Create(value1 / value2);
	}

	protected override string GetTokenName()
	{
		return "/";
	}
}

public class MultiplicationOperator : BinaryOperator 
{
	public MultiplicationOperator()
	{
		Precedence = PrecedenceEnum.Medium;
	}
	
	protected override bool DoWants(string token)
	{
		return Operator.IsOperator(token) && token == "*";
	}

	public override Operand PerformOp(params Operand[] operands)
	{
		Guard(operands);


		double value1 = operands[0].GetValue();
		double value2 = operands[1].GetValue();
		OutputWriter.TraceLine($"Multiply({value1}, {value2})");
		return NumericOperand.Create(value1 * value2);
	}

	protected override string GetTokenName()
	{
		return "*";
	}
}


public abstract class BinaryOperator : Operator 
{
	protected void Guard(Operand[] operands)
	{
		CheckParameterLength(operands, 2);
		CheckOperandType(operands, 0);
		CheckOperandType(operands, 1);
	}

	

}

public abstract class UnaryOperator : Operator 
{
	protected void Guard(Operand[] operands)
	{
		CheckParameterLength(operands, 1);
		CheckOperandType(operands, 0);
	}

}

public class Operand : Token
{
	public double GetValue()
	{
		double result;
		if (Double.TryParse(Text, out result))
			return result;
		return 0.0;
	}

	protected override bool DoWants(string token)
	{
		return false;
	}

	private static readonly Operand _empty = new Operand();
	public static Operand Empty
	{
		get { return _empty; }
	}
}


public class OutputWriter : TextWriter
{
	private TextWriter console;
	private readonly static OutputWriter writer = new OutputWriter();
		 
	public OutputWriter()
	{
		console = Console.Out;
		Console.SetOut(this);
	}
	
	~OutputWriter()
	{
		if(console != null)
			Console.SetOut(console);
	}
	
	public override Encoding Encoding => Encoding.UTF8;

	public override void WriteLine(string s)
	{
		if(console != null)
			console.WriteLine(s);
	}

	public override void WriteLine()
	{
		if (console != null)
			console.WriteLine();
	}

	public override void Write(string s)
	{
		if(console != null)
			console.Write(s);
	}
	
	public static void TraceLine()
	{
		writer.WriteLine();
	}
	
	public static void TraceLine(string s)
	{
		writer.WriteLine(s);
	}

	public static void Trace(string s)
	{
		writer.Write(s);
	}

}


#region private::Tests

[Fact] void Calculate0_Test() => Assert.Throws<ArgumentNullException>(() => Calculator.Calculate(""));

[Fact] void Calculate1_Test() => Assert.True (Calculator.Calculate("5+4") == 9);

[Fact] void Calculate2_Test() => Assert.True (Calculator.Calculate("10-3") == 7);

[Fact] void Calculate3_Test() => Assert.True (Calculator.Calculate("6+4*4") == 22);

[Fact] void Calculate4_Test() => Assert.True (Calculator.Calculate("(4+51)*(3-2)") == 55);

[Fact] void Calculate5_Test() => Assert.True(Calculator.Calculate("3!") == 6);

[Fact] void Calculate6_Test() => Assert.True(Calculator.Calculate("((3-6)*2)") == -6);

[Fact] void Calculate7_Test() => Assert.True (Calculator.Calculate("3!*((3-6)*2)") == -36);

[Fact] void Calculate8_Test() => Assert.True (Calculator.Calculate("3^2/2") == 4.5);

[Fact] void Calculate9_Test() => Assert.True (Calculator.Calculate("3/2^2") == .75);

[Fact] void Calculate10_Test() => Assert.True (Calculator.Calculate("-4+5*8") == 36);

[Fact] void Calculate11_Test() => Assert.True (Calculator.Calculate("3*-6+2") == -16);

[Fact] void Calculate12_Test() => Assert.True (Calculator.Calculate("5-4*8") == -27);

#endregion

