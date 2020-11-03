using System;
using Gtk;
using System.Collections.Generic;
namespace Lab1
{
    public class Parser
    {
        readonly List<string> operators = new List<string> { "+", "-", "*", "/", "^" };
        readonly List<string> bkts = new List<string> { "(", ")" };
        const string INC = "inc";
        const string DEC = "dec";
        const string MMAX = "mmax";
        const string MMIN = "mmin";

        Dictionary<string, MyEntry> data;
        MyEntry CurentCell;

        public Parser(Dictionary<string, MyEntry> data, MyEntry CurentCell)
        {
            this.data = data;
            this.CurentCell = CurentCell;
            CurentCell.IOnCells = new List<MyEntry>();
        }
        private byte GetPriority(string s)
        {
            switch (s)
            {
                case "+":
                case "-":
                    return 1;
                case "*":
                case "/":
                    return 2;
                case "^":
                    return 3;
                case "inc":
                case "dec":
                    return 4;
                default:
                    return 0;
            }
        }
        private string RemoveSpaces(string input)
        {
            string res = "";
            foreach (var item in input.Split(' '))
            {
                res += item;
            }
            return res;
        }
        private Token[] Tokenaiser(string input)
        {
            List<Token> res = new List<Token>();
            input = RemoveSpaces(input);
            if (input.Contains("(-"))
                input = input.Replace("(-", "(0-");

            if (input[0] == '-')
                input = input.Insert(0, "0");

            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsNumber(input[i]))
                {
                    string temp = input[i].ToString();
                    i++;
                    for (; i < input.Length; i++)
                    {
                        if (char.IsNumber(input[i]))
                            temp += input[i].ToString();
                        else break;
                    }
                    res.Add(new Token(TokenType.NUM, temp));
                    i--;
                }
                else if (operators.Contains(input[i].ToString())) //"+", "-", "*", "/", "^"
                {
                    res.Add(new Token(TokenType.OPERATOR, input[i].ToString()));
                }
                else if (bkts.Contains(input[i].ToString())) // "(", ")"
                {
                    if (input[i] == ')')
                        res.Add(new Token(TokenType.CLOSE_BKT, input[i].ToString()));
                    else
                        res.Add(new Token(TokenType.OPEN_BKT, input[i].ToString()));
                }
                else if (input[i] == INC[0] || input[i] == DEC[0]) // inc or dec
                {
                    string temp = "";
                    for (int j = 0; i < input.Length && j < INC.Length; i++)
                    {
                        temp += input[i];
                        j++;
                    }
                    if (temp == INC)
                        res.Add(new Token(TokenType.INC, temp));
                    else if (temp == DEC)
                        res.Add(new Token(TokenType.DEC, temp));
                    else
                    {
                        throw new Exception();
                    }
                    i--;
                }
                else if (input[i] == MMAX[0]) // mmin or mmax
                {
                    string temp = "";
                    for (int j = 0; i < input.Length && j < MMAX.Length; i++)
                    {
                        temp += input[i];
                        j++;
                    }
                    if (temp != MMAX && temp != MMIN)
                    {
                        throw new Exception();
                    }
                    else
                    {
                        if (input[i] != '(')
                        {
                            throw new Exception();
                        }
                        i++;
                        int bktsCount = 0;
                        string args = "";
                        for (; i < input.Length && (input[i] != ')' || bktsCount != 0); i++)
                        {
                            if (input[i] == ')')
                                bktsCount--;
                            else if (input[i] == '(')
                                bktsCount++;
                            args += input[i].ToString();
                        }
                        if (i == input.Length)
                        {
                            throw new Exception();
                        }
                        res.Add(new Token(TokenType.NUM, CountMFunc(temp, args).ToString()));
                    }
                }
                else if (input[i] >= 'A' && input[i] <= 'Z') //reference on another cell
                {
                    string adress = "";
                    for (; i < input.Length && input[i] >= 'A' && input[i] <= 'Z'; i++)
                    {
                        adress += input[i].ToString();
                    }
                    if (i == input.Length)
                    {
                        throw new Exception();
                    }
                    for (; i < input.Length && char.IsNumber(input[i]); i++)
                    {
                        adress += input[i].ToString();
                    }
                    i--;
                    res.Add(new Token(TokenType.CELL_REF, adress));
                }
                else
                {
                    throw new Exception();
                }
            }
            return res.ToArray();
        }
        private int CountMFunc(string func, string args)
        {
            if (args.Contains("!"))
            {
                throw new Exception();
            }
            int bktsCount = 0;
            bool startReplace = true;
            char[] charArr = args.ToCharArray();
            for (int i = 0; i < charArr.Length; i++)
            {
                if (!startReplace)
                {
                    if (charArr[i] == '(')
                        bktsCount++;
                    else if (charArr[i] == ')')
                        bktsCount--;
                }

                if (startReplace)
                {
                    if (charArr[i] == ',')
                        charArr[i] = '!';
                }
                startReplace |= bktsCount == 0;
                if (i + 3 < charArr.Length)
                    if (new string(new char[] { charArr[i], charArr[i + 1], charArr[i + 2], charArr[i + 3] }) == MMIN ||
                 new string(new char[] { charArr[i], charArr[i + 1], charArr[i + 2], charArr[i + 3] }) == MMAX)
                    {
                        startReplace = false;
                        i += 3;
                    }

            }
            args = new string(charArr);
            string[] arrArgs = args.Split('!');
            if (arrArgs.Length == 1)
            {
                if (int.TryParse(Parse(arrArgs[0]), out int a))
                    return a;
                else
                    throw new Exception();
            }
            Stack<string> stack = new Stack<string>();
            stack.Push("");
            for (int i = 0; i < arrArgs.Length; i++)
            {
                stack.Push(Parse(arrArgs[i]).ToString());
            }
            switch (func)
            {
                case MMAX:
                    return mmax(stack);
                case MMIN:
                    return mmin(stack);
                default:
                    return 0;
            }
        }
        private int mmax(Stack<string> args)
        {
            int res = 0;
            int.TryParse(args.Pop(), out int a);
            int.TryParse(args.Pop(), out int b);
            res = Math.Max(a, b);
            while (args.Peek() != "")
            {
                res = Math.Max(res, int.Parse(args.Pop()));
            }
            return res;
        }
        private int mmin(Stack<string> args)
        {
            int res = 0;
            int.TryParse(args.Pop(), out int a);
            int.TryParse(args.Pop(), out int b);
            res = Math.Min(a, b);
            while (args.Peek() != "")
            {
                res = Math.Min(res, int.Parse(args.Pop()));
            }
            return res;
        }


        private Token[] RPN(string input)
        {
            List<Token> res = new List<Token>();
            Stack<Token> stack = new Stack<Token>();
            stack.Push(new Token(TokenType.END, ""));
            foreach (var token in Tokenaiser(input))
            {
                switch (token.type)
                {
                    case TokenType.CELL_REF:
                    case TokenType.NUM:
                        res.Add(token);
                        break;

                    case TokenType.OPEN_BKT:
                        stack.Push(token);
                        break;

                    case TokenType.CLOSE_BKT:
                        while (stack.Peek().type != TokenType.OPEN_BKT && stack.Peek().type != TokenType.END)
                        {
                            res.Add(stack.Pop());
                        }
                        if (stack.Peek().type == TokenType.END)
                        {
                            throw new Exception();
                        }
                        stack.Pop();
                        break;

                    case TokenType.OPERATOR:

                        while (stack.Peek().type != TokenType.END && GetPriority(token.value) <= GetPriority(stack.Peek().value))
                        {
                            res.Add(stack.Pop());
                        }
                        stack.Push(token);
                        break;

                    case TokenType.INC:
                    case TokenType.DEC:
                        stack.Push(token);
                        break;

                }
            }

            while (stack.Peek().type != TokenType.END)
            {
                switch (stack.Peek().type)
                {
                    case TokenType.OPERATOR:
                    case TokenType.INC:
                    case TokenType.DEC:
                        break;
                    default:
                        throw new Exception();
                }
                res.Add(stack.Pop());
            }
            return res.ToArray();
        }

        private bool HasRef(MyEntry entry, MyEntry Ref)
        {
            bool res = false;
            if(entry.IOnCells != null)
            {
                foreach (var item in entry.IOnCells)
                {
                    if (HasRef(item, Ref))
                        return true;
                }
            }
            return res;
        }

        private void CheckRefRecursion(MyEntry entry)
        {
            if (HasRef(entry, entry))
                entry.Result = "ERROR";
        }

        private int CalcRPN(Token[] tokens)
        {

            Stack<Token> stack = new Stack<Token>();
            stack.Push(new Token(TokenType.END, ""));

            foreach (var token in tokens)
            {
                switch (token.type)
                {
                    case TokenType.CELL_REF:
                        stack.Push(new Token(TokenType.NUM, data[token.value].Result));

                        if (!data[token.value].CellsOnMe.Contains(CurentCell) && CurentCell != data[token.value])
                            data[token.value].CellsOnMe.Add(CurentCell);

                        if (!CurentCell.IOnCells.Contains(data[token.value]) && CurentCell != data[token.value])
                            CurentCell.IOnCells.Add(data[token.value]);

                        CheckRefRecursion(CurentCell);
                        break;

                    case TokenType.NUM:
                        stack.Push(token);
                        break;

                    case TokenType.OPERATOR:
                        int leftOperand = 0, rightOperand = 0;
                        if (stack.Peek().type != TokenType.END)
                        {
                            rightOperand = int.Parse(stack.Pop().value);
                            if (stack.Peek().type != TokenType.END)
                                leftOperand = int.Parse(stack.Pop().value);
                            else
                            {
                                    throw new Exception();
                            }
                        }
                        else
                        {
                            throw new Exception();
                        }
                        stack.Push(new Token(TokenType.NUM,
                            Calculate(leftOperand, char.Parse(token.value), rightOperand).ToString()));
                        break;

                    case TokenType.INC:
                    case TokenType.DEC:
                        int operand = 0;
                        if (stack.Peek().type != TokenType.END)
                        {
                            operand = int.Parse(stack.Pop().value);
                        }
                        else
                        {
                            throw new Exception();
                        }
                        stack.Push(new Token(TokenType.NUM, PostfixCalculate(operand, token.value).ToString()));
                        break;
                    default:
                        break;
                }
            }
            return int.Parse(stack.Pop().value);
        }
        private int inc(int a)
        {
            return a + 1;
        }
        private int dec(int a)
        {
            return a - 1;
        }
        private int PostfixCalculate(int operand, string _operator)
        {
            switch (_operator)
            {
                case "inc":
                    return inc(operand);
                case "dec":
                    return dec(operand);
                default:
                    throw new Exception();
            }
        }
        private int Calculate(int leftOperand, char _operator, int rightOperand)
        {
            checked
            {
                switch (_operator)
                {
                    case '+':
                        return leftOperand + rightOperand;
                    case '-':
                        return leftOperand - rightOperand;
                    case '*':
                        return leftOperand * rightOperand;
                    case '/':
                        if (rightOperand == 0)
                            throw new Exception();
                        return leftOperand / rightOperand;
                    case '^':
                        int res = 1;
                        for (; rightOperand > 0; rightOperand--)
                            res *= leftOperand;
                        return res;
                    default:
                        throw new Exception();
                }
            }


        }
        public string Parse(string input) 
        {
            try
            {
                return CalcRPN(RPN(input)).ToString();
            }
            catch (Exception)
            {
                return "ERROR";
            }
        }
    }
}
