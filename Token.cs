using System;
namespace Lab1
{
    public enum TokenType
    {
        NUM,
        OPERATOR, // "+", "-", "*", "/", "^"
        OPEN_BKT, // "(" 
        CLOSE_BKT,// ")"
        INC, // inc
        DEC, // dec
        CELL_REF, // example: "A1"
        END
    }
    public class Token
    {
        public readonly TokenType type;
        public readonly string value;
        public Token(TokenType type, string value)
        {
            this.type = type;
            this.value = value;
        }
    }
}
