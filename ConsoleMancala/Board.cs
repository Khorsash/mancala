using System;

namespace ConsoleMancala
{
    class Board
    {
        public static int TurnEndIndex(int[] board, int pitIndx)
        {
            int turn = board[board.Length-1];
            int startIndx = turn == 1 ? pitIndx : pitIndx + 7;
            int opponentsBar = turn == 1 ? 13 : 6;
            int piecesCount = board[startIndx] == 1 ? 1 : board[startIndx]-1;
            int currentIndex = startIndx;
            while(piecesCount>0)
            {
                currentIndex = (currentIndex+1)%(board.Length-1);
                if(currentIndex == opponentsBar) continue;
                piecesCount--;
            }
            return currentIndex;
        }
        public static string IntTo2DigitString(int num)
        {
            return num<10 ? "0"+Convert.ToString(num) : Convert.ToString(num%100);
        }
        // that's actually what grok made ok(relatively all another expirience)
        // shitty ascii art, lmao

        // * |‾‾| [  ] [  ] [  ] [  ] [  ] [  ] |‾‾|
        // * |00| ----------------------------- |00|
        // * |__| [  ] [  ] [  ] [  ] [  ] [  ] |__|
        public static void ShowBoard(int[] board, int showFromPerspective=0, int selected=-1, bool debug=false)
        {
            int pitsCount = (board.Length-3)/2;
            int turn = showFromPerspective != 0 ? showFromPerspective : board[board.Length-1];
            int turnEnd = selected != -1 ? TurnEndIndex(board, board[board.Length-1] == 1 ? selected : selected-7) : -1;
            ConsoleColor consoleColor = ConsoleColor.DarkGray;
            Console.Write("|‾‾|");
            ConsoleColor selectColor = ConsoleColor.Green;
            ConsoleColor selectColor2 = ConsoleColor.Green;
            ConsoleColor endIndexColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = consoleColor;
            if(turn == 1)
            {
                for(int i=board.Length-3; i>pitsCount; i--)
                {
                    if(selected != -1 && i == turnEnd)
                    {
                        if(i == selected) Console.ForegroundColor = selectColor; 
                        Console.Write(" [");
                        Console.ForegroundColor = endIndexColor;
                        Console.Write(board[i]>0?IntTo2DigitString(board[i]):"  ");
                        Console.ForegroundColor = consoleColor;
                        if(i == selected) Console.ForegroundColor = selectColor;
                        Console.Write("]");
                        Console.ForegroundColor = consoleColor;
                        continue;
                    }
                    if(selected != -1 && i == selected)
                    {
                        Console.ForegroundColor = selectColor; 
                        Console.Write(" [");
                        Console.ForegroundColor = selectColor2;
                        Console.Write(board[i]>0?IntTo2DigitString(board[i]):"  ");
                        Console.ForegroundColor = selectColor;
                        Console.Write("]");
                        Console.ForegroundColor = consoleColor;
                        continue;
                    }
                    Console.Write(" [");
                    Console.Write(board[i]>0?IntTo2DigitString(board[i]):"  ");
                    Console.Write("]");
                }
                Console.Write(" ");
                
            }
            else
            {
                for(int i=pitsCount-1; i>-1; i--)
                {
                    if(selected != -1 && i == turnEnd)
                    {
                        if(i == selected) Console.ForegroundColor = selectColor; 
                        Console.Write(" [");
                        Console.ForegroundColor = endIndexColor;
                        Console.Write(board[i]>0?IntTo2DigitString(board[i]):"  ");
                        Console.ForegroundColor = consoleColor;
                        if(i == selected) Console.ForegroundColor = selectColor;
                        Console.Write("]");
                        Console.ForegroundColor = consoleColor;
                        continue;
                    }
                    if(selected != -1 && i == selected)
                    {
                        Console.ForegroundColor = selectColor; 
                        Console.Write(" [");
                        Console.ForegroundColor = selectColor2;
                        Console.Write(board[i]>0?IntTo2DigitString(board[i]):"  ");
                        Console.ForegroundColor = selectColor;
                        Console.Write("]");
                        Console.ForegroundColor = consoleColor;
                        continue;
                    }
                    Console.Write(" [");
                    Console.Write(board[i]>0?IntTo2DigitString(board[i]):"  ");
                    Console.Write("]");
                }
                Console.Write(" ");
            }
            Console.Write("|‾‾|");
            Console.WriteLine();
            int opponentsBar = turn == 1 ? board[board.Length-2] : board[pitsCount];
            if(selected != -1 && (turn == 1 ? 13 : 6) == turnEnd)
            {
                Console.Write(" |");
                Console.ForegroundColor = endIndexColor;
                Console.Write(IntTo2DigitString(opponentsBar));
                Console.ForegroundColor = consoleColor;
                Console.Write("|");
            }
            else
            {Console.Write("|"+IntTo2DigitString(opponentsBar)+"| ");}
            for(int i=0; i<(pitsCount*5)-1; i++) Console.Write("-");
            int ownBar = turn == 1 ? board[pitsCount] : board[board.Length-2];
            if(selected != -1 && (turn == 1 ? 6 : 13) == turnEnd)
            {
                Console.Write(" |");
                Console.ForegroundColor = endIndexColor;
                Console.Write(IntTo2DigitString(ownBar));
                Console.ForegroundColor = consoleColor;
                Console.Write("|");
            }
            else
            {Console.Write(" |"+IntTo2DigitString(ownBar)+"|");}
            Console.WriteLine();
            Console.Write("|__|");
            if(turn == 1)
            {
                for(int i=0; i<pitsCount; i++)
                {
                    if(selected != -1 && i == turnEnd)
                    {
                        if(i == selected) Console.ForegroundColor = selectColor; 
                        Console.Write(" [");
                        Console.ForegroundColor = endIndexColor;
                        Console.Write(board[i]>0?IntTo2DigitString(board[i]):"  ");
                        Console.ForegroundColor = consoleColor;
                        if(i == selected) Console.ForegroundColor = selectColor;
                        Console.Write("]");
                        Console.ForegroundColor = consoleColor;
                        continue;
                    }
                    if(selected != -1 && i == selected)
                    {
                        Console.ForegroundColor = selectColor; 
                        Console.Write(" [");
                        Console.ForegroundColor = selectColor2;
                        Console.Write(board[i]>0?IntTo2DigitString(board[i]):"  ");
                        Console.ForegroundColor = selectColor;
                        Console.Write("]");
                        Console.ForegroundColor = consoleColor;
                        continue;
                    }
                    Console.Write(" [");
                    Console.Write(board[i]>0?IntTo2DigitString(board[i]):"  ");
                    Console.Write("]");
                }
                Console.Write(" ");
            }
            else
            {
                for(int i=pitsCount+1; i<board.Length-2; i++)
                {
                    if(selected != -1 && i == turnEnd)
                    {
                        if(i == selected) Console.ForegroundColor = selectColor; 
                        Console.Write(" [");
                        Console.ForegroundColor = endIndexColor;
                        Console.Write(board[i]>0?IntTo2DigitString(board[i]):"  ");
                        Console.ForegroundColor = consoleColor;
                        if(i == selected) Console.ForegroundColor = selectColor;
                        Console.Write("]");
                        Console.ForegroundColor = consoleColor;
                        continue;
                    }
                    if(selected != -1 && i == selected)
                    {
                        Console.ForegroundColor = selectColor; 
                        Console.Write(" [");
                        Console.ForegroundColor = selectColor2;
                        Console.Write(board[i]>0?IntTo2DigitString(board[i]):"  ");
                        Console.ForegroundColor = selectColor;
                        Console.Write("]");
                        Console.ForegroundColor = consoleColor;
                        continue;
                    }
                    Console.Write(" [");
                    Console.Write(board[i]>0?IntTo2DigitString(board[i]):"  ");
                    Console.Write("]");
                }
                Console.Write(" ");
            }
            Console.Write("|__|");
            Console.WriteLine();

            // uncomment to debug 
            
            if(debug)
            {
                Console.WriteLine();
                Console.Write("{ ");
                for(int i=0; i<board.Length; i++) Console.Write(Convert.ToString(board[i])+", ");
                Console.Write(" }");
                Console.WriteLine();
                Console.Write("selected: ");
                Console.WriteLine(selected);
                Console.Write("choice index given to function: ");
                Console.WriteLine(turn == 1 ? selected : selected-7);
                Console.Write("endIndex: ");
                Console.WriteLine(turnEnd);
            }
        }
    }
}