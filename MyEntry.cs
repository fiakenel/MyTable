using System.Collections.Generic;
using Gtk;
namespace Lab1
{
    public class MyEntry : Entry
    {
        public List<MyEntry> CellsOnMe = new List<MyEntry>(); //Cells with ref on this cell
        public List<MyEntry> IOnCells; //Cell refs in this cell

        private string expression = "";
        public string Expression
        {
            set
            {
                if (value.Length > 0 && value[0] == '=')
                    expression = value;
                else expression = "";
            }
            get
            {
                return expression;
            }
        }
        public bool HasExpression
        {
            get
            {
                if (expression.Length > 0 && expression[0] == '=')
                    return true;

                return false;
            }
        }
        public string Result { get; set; } = "";
        public MyEntry() { }
        public MyEntry(string initialText) : base(initialText) { }


    }
}
