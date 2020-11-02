using System;
using Gtk;
using System.Collections.Generic;
using Lab1;
using System.Xml.Linq;
using System.Xml;



public partial class MainWindow : Window
{
    Dictionary<string, MyEntry> data;
    Table table;
    Stack<Entry> RowTitles = new Stack<Entry>();
    Stack<Entry> ColTitles = new Stack<Entry>();

    private uint Cols;
    private uint Rows;


    public MainWindow() : base(WindowType.Toplevel)
    {
        Build();
        BuildDefaultTable(9, 11);
    }
    private void BuildDefaultTable(uint rows, uint cols)
    {
        if(table != null)
            scrolledwindow.Remove(table);

        data = new Dictionary<string, MyEntry>();
        table = new Table(rows, cols, false);
        scrolledwindow.Add(table);
        table.Attach(new MyEntry() { Sensitive = false }, 0, 1, 0, 1);

        //columns titles
        Cols = cols;
        for (uint i = 0; i < cols; i++)
            AddColTitle(i);

        //rows titles
        Rows = rows;
        for (uint i = 0; i < rows; i++)
            AddRowTitle(i);

        //cells
        for (uint j = 0; j < rows; j++)
            AddCellsOnRow(j);

        scrolledwindow.ShowAll();
    }

    private void AddRowTitle(uint row)
    {
        Entry en = new Entry(row.ToString()) { Sensitive = false };
        table.Attach(en, 0, 1, row + 1, row + 2);
        RowTitles.Push(en);
    }

    private void AddColTitle(uint col)
    {
        string s = Class26NumSystem.To26(col);
        Entry en = new Entry(s) { Sensitive = false };
        table.Attach(en, col + 1, col + 2, 0, 1);
        ColTitles.Push(en);
    }

    private void AddCellsOnRow(uint row)
    {
        for (uint i = 0; i < Cols; i++)
        {
            string s = Class26NumSystem.To26(i) + row.ToString();
            data.Add(s, new MyEntry("") { Name = s });
            data[s].Expand = false;
            table.Attach(data[s], i + 1, i + 2, row + 1, row + 2);

            data[s].FocusInEvent += MyEntryFocused;
            data[s].Activated += MyEntryActivated;
            data[s].FocusOutEvent += MyEntryNotFocused;
            data[s].Changed += MyEntryChanged;
        }
    }

    private void AddCellsOnCol(uint col)
    {
        for (uint j = 0; j < Rows; j++)
        {
            string s = Class26NumSystem.To26(col) + j.ToString();
            data.Add(s, new MyEntry("") { Name = s });
            data[s].Expand = false;
            table.Attach(data[s], col + 1, col + 2, j + 1, j + 2);

            data[s].FocusInEvent += MyEntryFocused;
            data[s].Activated += MyEntryActivated;
            data[s].FocusOutEvent += MyEntryNotFocused;
            data[s].Changed += MyEntryChanged;
        }
    }

    private void AddRow()
    {
        table.Resize(Rows, Cols);
        AddRowTitle(Rows);
        AddCellsOnRow(Rows);
        ++Rows;
    }


    private void AddCol()
    {
        table.Resize(Rows, Cols);
        AddColTitle(Cols);
        AddCellsOnCol(Cols);
        ++Cols;
    }

    private void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        if (!AskBeforeContinue())
            a.RetVal = true;
        else
        {
            a.RetVal = false;
            Application.Quit();
        }

    }

    private void BtnAddRowClicked(object sender, EventArgs e)
    {
        AddRow();
        table.ShowAll();
    }

    private void BtnAddColClicked(object sender, EventArgs e)
    {
        AddCol();
        table.ShowAll();
    }

    protected void BtnDelRowClicked(object sender, EventArgs e)
    {
        if (Rows == 1)
        {
            MessageDialog md = new MessageDialog(this, DialogFlags.DestroyWithParent, MessageType.Error,
                ButtonsType.Ok, "Табличка замала для видалення рядків")
            { Title = "Упс...", Modal = true };
            md.Run();
            md.Destroy();
            return;
        }
        bool noRef = true;
        for (uint i = 0; i < Cols && noRef; i++)
        {
            string s = Class26NumSystem.To26(i) + (Rows - 1).ToString();
            if (data[s].CellsOnMe.Count != 0)
                noRef = false;
        }
        if (!noRef)
        {
            MessageDialog md = new MessageDialog(this,
               DialogFlags.DestroyWithParent, MessageType.Question,
               ButtonsType.Cancel, $"\tВи впевнені, що хочете видалити рядок {Rows - 1}?" +
               "\nСхоже що на деякі його клітинки посилаються інші клітинки")
            { Modal = true };

            md.AddButton("Ok", ResponseType.Accept);
            int response = md.Run();
            md.Destroy();
            if (response == (int)ResponseType.Accept)
            {
                for (uint i = 0; i < Cols; i++)
                {
                    string s = Class26NumSystem.To26(i) + (Rows - 1).ToString();
                    data[s].Text = "";
                    data[s].Expression = "";
                    UpdateCellsWithRef(data[s]);
                }
                DeleteLastRow();
            }
        }
        else
            DeleteLastRow();
        table.ShowAll();
    }

    private void DeleteLastRow()
    {
        table.Remove(RowTitles.Pop());
        for (uint i = 0; i < Cols; i++)
        {
            string s = Class26NumSystem.To26(i) + (Rows - 1).ToString();
            table.Remove(data[s]);
            data.Remove(s);
        }
        table.Resize(--Rows, Cols);
    }

    protected void BtnDelColClicked(object sender, EventArgs e)
    {
        if (Cols == 1)
        {
            MessageDialog md = new MessageDialog(this, DialogFlags.DestroyWithParent, MessageType.Error,
                ButtonsType.Ok, "Табличка замала для видалення стовпчиків")
            { Title = "Упс...", Modal = true };
            md.Run();
            md.Destroy();
            return;
        }
        bool noRef = true;
        for (uint i = 0; i < Rows && noRef; i++)
        {
            string s = Class26NumSystem.To26(Cols - 1) + (i).ToString();
            if (data[s].CellsOnMe.Count != 0)
                noRef = false;
        }
        if (!noRef)
        {
            MessageDialog md = new MessageDialog(this,
               DialogFlags.DestroyWithParent, MessageType.Question,
               ButtonsType.Cancel, $"\tВи впевнені, що хочете видалити стовпчик {Cols - 1}?" +
               "\nСхоже що на деякі його клітинки посилаються інші клітинки")
            { Modal = true };

            md.AddButton("Ok", ResponseType.Accept);
            int response = md.Run();
            md.Destroy();
            if (response == (int)ResponseType.Accept)
            {
                for (uint i = 0; i < Rows; i++)
                {
                    string s = Class26NumSystem.To26(Cols - 1) + (i).ToString();
                    data[s].Text = "";
                    data[s].Expression = "";
                    UpdateCellsWithRef(data[s]);
                }
                DeleteLastCol();
            }
        }
        else
            DeleteLastCol();
        table.ShowAll();
    }


    private void DeleteLastCol()
    {
        table.Remove(ColTitles.Pop());
        for (uint i = 0; i < Rows; i++)
        {
            string s = Class26NumSystem.To26(Cols - 1) + (i).ToString();
            table.Remove(data[s]);
            data.Remove(s);
        }
        table.Resize(Rows, --Cols);
    }

    private void MyEntryFocused(object sender, EventArgs e)
    {
        MyEntry entry = (MyEntry)sender;
        if (entry.HasExpression)
        {
            entry.Text = entry.Expression;
        }
        MainEntry.Text = entry.Result;

    }

    private void MyEntryChanged(object sender, EventArgs e)
    {
        MyEntry entry = (MyEntry)sender;
        if (!entry.HasExpression)
        {
            entry.Result = entry.Text;
            MainEntry.Text = entry.Result;
        }
    }

    private void MyEntryActivated(object sender, EventArgs e)
    {
        MyEntry entry = (MyEntry)sender;
        entry.Expression = entry.Text;
        if (entry.IOnCells != null)
        {
            foreach (var item in entry.IOnCells) //del every ref on this cell 
            {
                item.CellsOnMe.Remove(entry);
            }
        }
        if (entry.HasExpression)
        { // Parsing expression 

            entry.Result = new Parser(data, entry).Parse(entry.Expression.Substring(1));
        }
        else
        {
            entry.Result = entry.Text;
        }
        MainEntry.Text = entry.Result;
        UpdateCellsWithRef(entry);
    }

    private void MyEntryNotFocused(object sender, EventArgs e)
    {
        MyEntry entry = (MyEntry)sender;
        if (entry.HasExpression)
        {
            entry.Text = entry.Result;
        }
    }

    private void UpdateCellsWithRef(MyEntry Ref)// Parsing every cell that have ref on this cell
    {
        UpdateCells(Ref, Ref);
    }

    private void UpdateCells(MyEntry startRef, MyEntry Ref)
    {
        foreach (MyEntry item in Ref.CellsOnMe)
        {
            item.Result = new Parser(data, item).Parse(item.Expression.Substring(1));
            item.Text = item.Result;
            if (item != startRef)
                UpdateCells(startRef, item);
        }
    }


    private bool IsEmpty(MyEntry entry)
    {
        return entry.Text == "" && entry.Expression == "" && entry.Result == "" &&
            entry.IOnCells == null && entry.CellsOnMe.Count == 0;
    }
    private void SaveActivated(object sender, EventArgs e)
    {
        SaveTable();
    }

    protected void OpenActivated(object sender, EventArgs e)
    {
        if (!AskBeforeContinue())
            return;

        FileChooserDialog dialog = new FileChooserDialog("Відкрити...", this, FileChooserAction.Save,
        "Cancel", ResponseType.Cancel, "Open", ResponseType.Apply);
        try
        {
            OpenTable(dialog);
        }
        catch (Exception)
        {
            MessageDialog dg = new MessageDialog(this, DialogFlags.DestroyWithParent,
                MessageType.Error, ButtonsType.Ok, "Схоже що з файлом щось не так...")
            { Title = "Упс...", Modal = true };
            dg.Run();
            dg.Destroy();
        }
        dialog.Destroy();
    }

    protected void CloseActivated(object sender, EventArgs e)
    {
        if (!AskBeforeContinue())
            return;
        BuildDefaultTable(9, 11);
    }

    protected void InfoActivated(object sender, EventArgs e)
    {
        MessageDialog dialog = new MessageDialog(this, DialogFlags.DestroyWithParent,
        MessageType.Info, ButtonsType.Ok, "Щоб записати текст в клітинку після введення натисніть на Enter." +
            "\n Щоб дати команду обробити текст як математичний вираз перед виразом введіть '='." +
            "\n\n\tДоступні операції/функції:" +
            "\n\t\t +\t\tx+y" +
            "\n\t\t -\t\tx-y" +
            "\n\t\t *\t\tx*y" +
            "\n\t\t /\t\tx/y" +
            "\n\t\t ^\t\tx^y" +
            "\n\t\t inc\t\tinc(x) = x + 1" +
            "\n\t\t dec\t\tdec(x) = x - 1" +
            "\n\t\t mmax(x1,...,xN)" +
            "\n\t\t mmin(x1,...,xN)" +
            "\nПосилання на комірки (наприклад, =A0)");
        dialog.Run();
        dialog.Destroy();
    }

    protected void AboutA_Activated(object sender, EventArgs e)
    {
        AboutDialog about = new AboutDialog()
        {
            ProgramName = "My Table",
            Authors = new string[] { "Карапуд Максим К-26" },
            Version = "Версія 1.0.0",
            Logo = new Gdk.Pixbuf("/home/fiakenel/Изображения/Lab1.png", 100, 100)
        };
        about.Run();
        about.Destroy();
    }


    private bool AskBeforeContinue()
    {
        MessageDialog dialog = new MessageDialog(this, DialogFlags.DestroyWithParent,
            MessageType.Question, ButtonsType.Cancel, "Ви впевнені що хочете продовжити?" +
                "\nНе збережені дані буде втрачено");
        dialog.AddButton("Продовжити без збереження", 2);
        dialog.AddButton("Зберегти і продовжити", 1);
        int response = dialog.Run();
        dialog.Destroy();
        switch (response)
        {
            case (int)ResponseType.Cancel:
                return false;
            case 1:
                SaveTable();
                break;
            case 2:
                break;
            default:
                break;
        }
        return true;
    }

    private void OpenTable(FileChooserDialog dialog)
    {

        int reply = dialog.Run();
        if (reply == (int)ResponseType.Apply)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(dialog.Filename);//load file

            XmlElement xRoot = xDoc.DocumentElement;

            if (xRoot.Attributes.Count < 2)
                throw new Exception();
            Rows = uint.Parse(xRoot.Attributes.GetNamedItem("Rows").Value);
            Cols = uint.Parse(xRoot.Attributes.GetNamedItem("Cols").Value);

            BuildDefaultTable(Rows, Cols);
            foreach (XmlNode xnode in xRoot) // Load cells
            {
                string name = xnode.Attributes.GetNamedItem("Name").Value;

                data[name].Text = xnode.Attributes.GetNamedItem("Text").Value;
                data[name].Result = xnode.Attributes.GetNamedItem("Result").Value;
                data[name].Expression = xnode.Attributes.GetNamedItem("Expression").Value;

                if (xnode.HasChildNodes)
                {
                    foreach (XmlNode childnode in xnode.ChildNodes)
                    {
                        if (childnode.Name == "CellsOnMe") //Load CellsOnMe list for cell
                        {
                            foreach (XmlNode CellsOnMeElem in childnode.ChildNodes)
                            {
                                data[name].CellsOnMe.Add(data[CellsOnMeElem.Name]);
                            }
                        }
                        if (childnode.Name == "IOnCells")//Load IOnCells list for cell
                        {
                            data[name].IOnCells = new List<MyEntry>();
                            foreach (XmlNode IOnCellsElem in childnode.ChildNodes)
                            {
                                data[name].IOnCells.Add(data[IOnCellsElem.Name]);
                            }
                        }
                    }
                }
            }
        }
    }
    private void SaveTable()
    {
        FileChooserDialog dialog = new FileChooserDialog("Зберегти як...", this, FileChooserAction.Save,
        "Cancel", ResponseType.Cancel, "Save", ResponseType.Apply);

        int reply = dialog.Run();
        if (reply == (int)ResponseType.Apply)
        {
            XDocument document = new XDocument();

            // Create root elem
            XElement STable = new XElement("table");

            // create first attributes
            XAttribute SRows = new XAttribute("Rows", Rows);
            XAttribute SCols = new XAttribute("Cols", Cols);
            STable.Add(SRows);
            STable.Add(SCols);

            foreach (var cell in data)
            {
                if (IsEmpty(cell.Value))//go to next cell if this cell is empty
                    continue;
                XElement SCell = new XElement("cell");

                SCell.Add(new XAttribute("Name", cell.Key));//Save the name of cell

                if (cell.Value.IOnCells != null && cell.Value.IOnCells.Count != 0)
                {//Save IOnCells list
                    XElement SIOnCells = new XElement("IOnCells");

                    foreach (var subCell in cell.Value.IOnCells)
                    {
                        SIOnCells.Add(new XElement(subCell.Name));
                    }
                    SCell.Add(SIOnCells);
                }
                if (cell.Value.CellsOnMe.Count != 0)
                {//save CellsOnMe list
                    XElement SCellsOnMe = new XElement("CellsOnMe");
                    foreach (var subCell in cell.Value.CellsOnMe)
                    {
                        SCellsOnMe.Add(new XElement(subCell.Name));
                    }
                    SCell.Add(SCellsOnMe);
                }

                SCell.Add(new XAttribute("Expression", cell.Value.Expression));//Save attributes of cell
                SCell.Add(new XAttribute("Text", cell.Value.Text));
                SCell.Add(new XAttribute("Result", cell.Value.Result));

                STable.Add(SCell);
            }

            document.Add(STable);
            document.Save(dialog.Filename);
        }
        dialog.Destroy();
    }
}
