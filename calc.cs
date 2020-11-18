using System;
using System.Collections.Generic;
using System.Linq;

namespace PT3
{
   using static Functs;
   public interface IExpr
   {
      double Compute(IReadOnlyDictionary<string, double> variableValues);
      IEnumerable<string> Variables { get; }
      bool IsConstant { get; }
      bool IsPolynom { get; }
   }
   public abstract class Expr : IExpr
   {
      public abstract double Compute(IReadOnlyDictionary<string, double> variableValues);
      public abstract IEnumerable<string> Variables { get; }
      public abstract bool IsConstant { get; }
      public abstract bool IsPolynom { get; }
      public abstract Expr Dif(Variable var, IReadOnlyDictionary<string, double> variableValues);
      public static Expr Parse(string[] expr)
      {
         Stack<Expr> opds = new Stack<Expr>();
         Stack<string> oprs = new Stack<string>();
         for (int i = 0; i < expr.Length; i++)
         {
            if (expr[i] != "(")
               if (expr[i] == ")")
               {
                  string temp_opr = oprs.Pop();
                  switch (temp_opr)
                  {
                     case "+":
                        {
                           Expr texpr = opds.Pop() + opds.Pop();
                           opds.Push(texpr);
                           break;
                        }
                     case "*":
                        {
                           Expr texpr = opds.Pop() * opds.Pop();
                           opds.Push(texpr);
                           break;
                        }
                     case "/":
                        {
                           Expr texpr = opds.Pop() / opds.Pop();
                           opds.Push(texpr);
                           break;
                        }
                     case "-":
                        {
                           Expr texpr = -opds.Pop();
                           opds.Push(texpr);
                           break;
                        }
                     case "Sh":
                        {
                           Expr texpr = Sh(opds.Pop());
                           opds.Push(texpr);
                           break;
                        }
                     case "Ch":
                        {
                           Expr texpr = Ch(opds.Pop());
                           opds.Push(texpr);
                           break;
                        }
                     case "Th":
                        {
                           Expr texpr = Th(opds.Pop());
                           opds.Push(texpr);
                           break;
                        }
                     case "Cth":
                        {
                           Expr texpr = Cth(opds.Pop());
                           opds.Push(texpr);
                           break;
                        }
                     case "Int":
                        {
                           Expr texpr = Int(opds.Pop(), opds.Pop(), opds.Pop());
                           opds.Push(texpr);
                           break;
                        }
                  }
               }
               else
               if (expr[i] == "+" || expr[i] == "*" || expr[i] == "-" || expr[i] == "/" ||
                  expr[i] == "Sh" || expr[i] == "Ch" || expr[i] == "Th" || expr[i] == "Cth")
                  oprs.Push(expr[i]);
               else
                  if (double.TryParse(expr[i], out double d))
                  opds.Push(new Constant(double.Parse(expr[i])));
               else
                  opds.Push(new Variable(expr[i]));

         }
         return opds.Pop();
      }

      public static implicit operator Expr(double Variables) => new Constant(Variables);
      public static Expr operator +(Expr op1, Expr op2) => new Sum(op1, op2);
      public static Expr operator *(Expr op1, Expr op2) => new Mult(op1, op2);
      public static Expr operator /(Expr op1, Expr op2) => new Div(op1, op2);
      public static Expr operator -(Expr op1) => new Min(op1);
   }
   public class Functs
   {
      public static Sh Sh(Expr a) => new Sh(a);
      public static Ch Ch(Expr a) => new Ch(a);
      public static Th Th(Expr a) => new Th(a);
      public static Cth Cth(Expr a) => new Cth(a);
      public static Int Int(Expr a, Expr l, Expr r) => new Int(a, l, r);
   }
   public class Variable : Expr
   {
      public string Name { get; }
      public Variable(string name) { Name = name; }
      public override double Compute(IReadOnlyDictionary<string, double> variableValues) => variableValues[Name];
      public override IEnumerable<string> Variables => new[] { Name };
      public override bool IsConstant => false;
      public override bool IsPolynom => true;
      public override Expr Dif(Variable var, IReadOnlyDictionary<string, double> variableValues) => Name == var.Name ? Compute(variableValues) : 0;
      public override string ToString() => Name;
   }
   public class Constant : Expr
   {
      public double Value { get; }
      public Constant(double value) { Value = value; }
      public override double Compute(IReadOnlyDictionary<string, double> variableValues) => Value;
      public override IEnumerable<string> Variables => new string[0];
      public override bool IsConstant => true;
      public override bool IsPolynom => true;
      public override Expr Dif(Variable var, IReadOnlyDictionary<string, double> variableValues) => 0;
      public override string ToString() => Value.ToString();
   }
   public abstract class UnaryOperation : Expr
   {
      protected Expr A;
      public UnaryOperation(Expr a) { A = a; }
      public override abstract double Compute(IReadOnlyDictionary<string, double> variableValues);
      public override IEnumerable<string> Variables => A.Variables;
      public override bool IsConstant => A.IsConstant;
      public override bool IsPolynom => A.IsPolynom;
      public abstract override Expr Dif(Variable var, IReadOnlyDictionary<string, double> variableValues);
   }
   public abstract class BinaryOperation : Expr
   {
      protected Expr A, B;
      public BinaryOperation(Expr a, Expr b) { A = a; B = b; }
      public override abstract double Compute(IReadOnlyDictionary<string, double> variableValues);
      public override IEnumerable<string> Variables => A.Variables.Concat(B.Variables).Distinct();
      public override bool IsConstant => A.IsConstant && B.IsConstant;
      public override bool IsPolynom => true;
      public abstract override Expr Dif(Variable var, IReadOnlyDictionary<string, double> variableValues);
   }
   public abstract class Function : Expr
   {
      protected Expr A;
      public Function(Expr a) { A = a; }
      public override abstract double Compute(IReadOnlyDictionary<string, double> variableValues);
      public override IEnumerable<string> Variables => A.Variables;
      public override bool IsConstant => A.IsConstant;
      public override bool IsPolynom => A.IsConstant;
      public abstract override Expr Dif(Variable var, IReadOnlyDictionary<string, double> variableValues);
   }
   public class Sum : BinaryOperation
   {
      public Sum(Expr a, Expr b) : base(a, b) { }
      public override double Compute(IReadOnlyDictionary<string, double> variableValues) =>
         A.Compute(variableValues) + B.Compute(variableValues);

      public override Expr Dif(Variable var, IReadOnlyDictionary<string, double> variableValues) => A.Dif(var, variableValues) + B.Dif(var, variableValues);
      public override string ToString() => $"( {A} + {B} ) ";
   }
   public class Mult : BinaryOperation
   {
      public Mult(Expr a, Expr b) : base(a, b) { }
      public override double Compute(IReadOnlyDictionary<string, double> variableValues) =>
         A.Compute(variableValues) * B.Compute(variableValues);
      public override Expr Dif(Variable var, IReadOnlyDictionary<string, double> variableValues) => A.Dif(var, variableValues) * B + B.Dif(var, variableValues) * A;
      public override string ToString() => $"( {A} * {B} ) ";
   }
   public class Div : BinaryOperation
   {
      public Div(Expr a, Expr b) : base(a, b) { }
      public override double Compute(IReadOnlyDictionary<string, double> variableValues)
      {
         if (B.Compute(variableValues) != 0)
            return A.Compute(variableValues) / B.Compute(variableValues);
         else
            throw new DivideByZeroException("Невозможно поделить число на нуль!");
      }
      public override Expr Dif(Variable var, IReadOnlyDictionary<string, double> variableValues) => A.Dif(var, variableValues) * B + (-B.Dif(var, variableValues) * A) / (B * B);
      public override string ToString() => $"( {A} / {B} ) ";
   }
   public class Min : UnaryOperation
   {
      public Min(Expr a) : base(a) { }
      public override double Compute(IReadOnlyDictionary<string, double> variableValues) =>
         -A.Compute(variableValues);
      public override Expr Dif(Variable var, IReadOnlyDictionary<string, double> variableValues) => -(A.Dif(var, variableValues));
      public override string ToString() => $"( - {A} ) ";
   }

   public class Sh : Function
   {
      public Sh(Expr a) : base(a) { }
      public override double Compute(IReadOnlyDictionary<string, double> variableValues) => Math.Sinh(A.Compute(variableValues));
      public override Expr Dif(Variable var, IReadOnlyDictionary<string, double> variableValues) => A.Dif(var, variableValues) * Ch(A);
      public override string ToString() => $"( Sh ( {A} ) ) ";
   }
   public class Ch : Function
   {
      public Ch(Expr a) : base(a) { }
      public override double Compute(IReadOnlyDictionary<string, double> variableValues) => Math.Cosh(A.Compute(variableValues));
      public override Expr Dif(Variable var, IReadOnlyDictionary<string, double> variableValues) => A.Dif(var, variableValues) * Sh(A);
      public override string ToString() => $"( Ch ( {A} ) ) ";
   }
   public class Th : Function
   {
      public Th(Expr a) : base(a) { }
      public override double Compute(IReadOnlyDictionary<string, double> variableValues) => Math.Tanh(A.Compute(variableValues));
      public override Expr Dif(Variable var, IReadOnlyDictionary<string, double> variableValues) => A.Dif(var, variableValues) / (Ch(A) * Ch(A));
      public override string ToString() => $"( Th ( {A} ) ) ";
   }
   public class Cth : Function
   {
      public Cth(Expr a) : base(a) { }
      public override double Compute(IReadOnlyDictionary<string, double> variableValues)
      {
         double x = A.Compute(variableValues);
         if (1 / Math.Tanh(x) != double.NaN)
            return 1 / Math.Tanh(x);
         else
            throw new NotFiniteNumberException($"Функция гиперболический котангенс не определена на значении аргумента {x}");
      }
      public override Expr Dif(Variable var, IReadOnlyDictionary<string, double> variableValues) => -A.Dif(var, variableValues) / (Sh(A) * Sh(A));
      public override string ToString() => $"( Cth ( {A} ) ) ";
   }
   public class Int : Expr
   {
      private Expr A;
      private Expr L, R;
      public Int(Expr a, Expr l, Expr r)
      {
         A = a;
         L = l;
         R = r;
      }
      public override IEnumerable<string> Variables => A.Variables;
      public override bool IsConstant => true;
      public override bool IsPolynom => true;
      public override double Compute(IReadOnlyDictionary<string, double> variableValues)
      {
         double sum = 0, step = 0.001, l = L.Compute(variableValues), r = R.Compute(variableValues);
         string str = A.Variables.First();
         var dic = new Dictionary<string, double> { [str] = l };
         for (double i = l; i < r - step; i += step)
         {
            dic[str] = i;
            sum += A.Compute(dic) * step;
         }
         return sum;
      }

      public override Expr Dif(Variable var, IReadOnlyDictionary<string, double> variableValues)
      {
         throw new NotImplementedException();
      }

      public override string ToString() => $"( Int ( {A} , {L} , {R} ) )";
   }
   class Program
   {
      static void Main()
      {
         Console.WriteLine("<1> Ввод новой функции");
         Console.WriteLine("<2> Посчитать значение функции");
         Console.WriteLine("<3> Интегрирование");
         Console.WriteLine("<4> Дифференцирование");
         Console.WriteLine("<0> Выход");

         Expr mainExpr = null;
         Dictionary<string, double> Dic = new Dictionary<string, double>();

         bool exit = false;
         do
         {
            Console.WriteLine("Выберете операцию: ");
            int ch = int.Parse(Console.ReadLine());
            switch (ch)
            {
               case 0:
                  {

                     exit = true;
                     break;
                  }
               case 1:
                  {
                     Console.WriteLine("Введите функцию:");
                     string expr0 = Console.ReadLine();
                     string[] expr = expr0.Split(' ');
                     mainExpr = Expr.Parse(expr);                  
                     break;
                  }
               case 2:
                  {
                     Console.WriteLine("Введите значения всех переменных");
                     for (int i = 0; i < mainExpr.Variables.Where(x => char.IsLetter(char.Parse(x))).Count(); i++)
                     {
                        string s0 = Console.ReadLine();
                        string[] s = s0.Split(' ');
                        Dic.Add(s[0], double.Parse(s[2]));
                     }
                     Console.WriteLine("Значение функции равно:");
                     Console.WriteLine(mainExpr.Compute(Dic));
                     break;
                  }
               case 3:
                  {
                     Console.WriteLine("Выберете нижний предел интегрирования: ");
                     double l = double.Parse(Console.ReadLine());
                     Console.WriteLine("Выберете верхний предел интегрирования: ");
                     double r = double.Parse(Console.ReadLine());
                     Expr Integ = new Int(mainExpr, 1, 2);
                     Console.WriteLine("Результат интегрирования: ");
                     Console.WriteLine(Integ.Compute(Dic));
                     break;
                  }
               case 4:
                  {
                     Console.WriteLine("Выберете переменную дифференицрования: ");
                     string s0 = Console.ReadLine();
                     string[] s = s0.Split(' ');
                     var d = new Variable(s[0]);
                     Dic.Add(s[0], double.Parse(s[2]));
                     Console.WriteLine("Результат дифференцирования: ");
                     Console.WriteLine(mainExpr.Dif(d, Dic));
                     Console.WriteLine(mainExpr.Dif(d, Dic).Compute(Dic));
                     break;
                  }
               default:
                  {
                     Console.WriteLine("Неверная операция!");
                     Console.WriteLine("Выберете операцию: ");
                     ch = int.Parse(Console.ReadLine());
                     break;
                  }
            }

         } while (!exit);
      }
   }
}
