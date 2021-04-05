using System;
using System.IO;
using System.Text;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            var filename = "/Users/shyamsundarnavvaneethan/RiderProjects/ConsoleApp1/ConsoleApp1/input.csv";
            var bytes = Encoding.UTF8.GetBytes(File.ReadAllText(filename));
            var tokenizer = new Tokenizer(bytes);
            int maxRows = 0;
            int maxCols = 0;
            while (tokenizer.HasMoreTokens())
            {
                maxRows++;
                int noCols = 0;
                while (true)
                {
                    var token = tokenizer.NextToken();
                    if (token == Token.ERROR)
                    {
                        Console.Write(tokenizer.NextValue());
                        System.Environment.Exit(1);
                    }
                    if (tokenizer.IsEob() || tokenizer.IsEol())
                    {
                        break;
                    }
                    if (token == Token.COMMA)
                    {
                        noCols++;
                        if (noCols > maxCols) maxCols = noCols;
                    }
                }
            }
            var cells = new TextCell[maxRows,maxCols+1];
            tokenizer.Reset();

            int row = 0;
            int col = 0;
            while (tokenizer.HasMoreTokens())
            {
                var token = tokenizer.NextToken();
                if (token == Token.STR)
                {
                    cells[row, col] = new TextCell(tokenizer.NextValue());
                } else if (token == Token.COMMA)
                {
                    col++;
                } else if (token == Token.EOL)
                {
                    row++;
                    col = 0;
                }
            }
            DrawCells(cells, maxRows, maxCols+1);
            
        }
        
        public static void DrawCells( TextCell[,] cells, int rows, int cols)
        {
            int[] maxRowLines = new int[rows];
            int[] maxColSizes = new int[cols];
            
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (cells[row, col] != null)
                    {
                        if (cells[row, col].NumberOfRows > maxRowLines[row])
                            maxRowLines[row] = cells[row, col].NumberOfRows;
                        if (cells[row, col].MaxColLength > maxColSizes[col])
                            maxColSizes[col] = cells[row, col].MaxColLength;
                    }
                }
            }

            //draw header line
            // header
            Console.Write("┌"); // start top left
            for (int col = 0; col < cols; col++)
            {
                if (col!=0)
                {
                    Console.Write("┬"); // column seperator - T
                }
                for (int x = 0; x < maxColSizes[col]; x++)
                {
                    Console.Write("─");
                }
            }
            Console.WriteLine("┐"); // end of table top right

            for (int row = 0; row < rows; row++)
            {
                if (row != 0)
                {
                    // divider
                    Console.Write("├"); // middle start |- pipe
                    for (int col = 0; col < cols; col++)
                    {
                        if (col != 0)
                        {
                            Console.Write("┼"); // column seperator  -|-
                        }

                        for (int x = 0; x < maxColSizes[col]; x++)
                        {
                            Console.Write("─");
                        }
                    }
                    Console.WriteLine("┤"); // end of table top right -|
                }

                for (int y = maxRowLines[row];  y >0 ; y--)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        Console.Write("│");
                        var cell = cells[row, col];
                        var outString = cell!=null && cell.lines.Length >= y ? cell.lines[cell.lines.Length-y] : "";
                        Console.Write(outString);
                        for (int x = 0; x < (maxColSizes[col] - outString.Length); x++)
                        {
                            Console.Write(" ");
                        }
                    }
                    Console.WriteLine("│");
                }
            }
            Console.Write("└"); // L
            for (int col = 0; col < cols; col++)
            {
                if(col!=0)
                    Console.Write("┴"); // L
                        
                for (int x = 0; x < maxColSizes[col]; x++)
                {
                    Console.Write("─");
                }
            }
            Console.WriteLine("┘"); // bottom right
        }
    }
    

    public class TextCell
    {
        public int NumberOfRows;
        public int MaxColLength;
        public string[] lines;

        public TextCell(string line)
        { 
            lines = line.Split("\n");
            NumberOfRows = lines.Length;
            MaxColLength = 0;
            foreach (var singleLine in lines)
            {
                MaxColLength = MaxColLength < singleLine.Length ? singleLine.Length : MaxColLength;
            }
        }
    }
    
    public enum Token
    {
        COMMA, 
        STR,
        EOL,
        EOF,
        ERROR
    }
    public class Tokenizer
    {
        private byte[] _buffer;
        private int index = -1;
        private Token nextToken;
        private string nextValue;

        public Tokenizer(byte[] buffer)
        {
            this._buffer = buffer;
        }

        public bool HasMoreTokens()
        {
            return index < _buffer.Length;
        }

        public void Reset()
        {
            index = -1;
            nextValue = null;
        }
        
        private  bool IsQuote()
        {
            return _buffer[index] == '"' || _buffer[index] == '\'';
        }
        
        private  bool IsComma()
        {
            return _buffer[index] == ',';
        }

        public bool IsEob()
        {
            return index >= _buffer.Length;
        }

        public bool isCR()
        {
            return _buffer[index] == '\r';
        }

        public bool isLF()
        {
            return _buffer[index] == '\n';
        }

        public bool isCRLF()
        {
            if (isCR())
            {
                index++;
                if (!IsEob())
                {
                    if (isLF())
                    {
                        return true;
                    }
                }
                index--;
            }
            return false;
        }
        public bool IsEol()
        {
            return isCRLF() || isCR() || isLF();
        }

        public string NextValue()
        {
            return nextValue;
        }
        public Token NextToken()
        {
            if (IsEob())
                return Token.EOF;
            while (!IsEob())
            {
                index++;
                if (IsEob())
                    return Token.EOF;
                if (IsEol())
                    return (nextToken = Token.EOL);
                if (IsComma())
                    return (nextToken = Token.COMMA);
                if (IsQuote())
                {
                    var stringBuilder = new StringBuilder();
                    while (!IsEob())
                    {
                        index++;
                        if(IsEob()) break;
                        if (IsQuote())
                        {
                            nextToken = Token.STR;
                            nextValue = stringBuilder.ToString();
                            return nextToken;
                        }
                        stringBuilder.Append((char)_buffer[index]);
                    }
                    if (!IsEob()) continue;
                    nextToken = Token.ERROR;
                    nextValue = "End of file reached error before matching delimiter";
                    return Token.ERROR;
                }
                else
                {
                    var stringBuilder = new StringBuilder();
                    while (!IsEob() && !IsEol() &&!IsComma())
                    {
                        stringBuilder.Append((char)_buffer[index]);
                        index++;
                    }
                    nextToken = Token.STR;
                    nextValue = stringBuilder.ToString();
                    index--;// push back
                    return nextToken;
                }
            }
            return nextToken = Token.EOF;
        }
    }
}