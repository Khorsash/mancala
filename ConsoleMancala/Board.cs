using System;
using System.Linq;
using System.Threading.Tasks;

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
        public static void ShowBoard(int[] board, int showFromPerspective=0, int selected=-1,
                                      bool debug=false, bool animationFrame=false, bool showZeros=true)
        {
            int pitsCount = (board.Length-3)/2;
            int turn = showFromPerspective != 0 ? showFromPerspective : board[board.Length-1];
            int turnEnd = selected != -1 ? TurnEndIndex(board, board[board.Length-1] == 1 ? selected : selected-7) : -1;
            int add = animationFrame ? 1 : 0;
            bool bs;
            string emptyPitShow = showZeros ? "00" : "  ";
            ConsoleColor consoleColor = ConsoleColor.DarkGray;
            ConsoleColor selectColor = ConsoleColor.Green;
            ConsoleColor selectColor2 = ConsoleColor.Green;
            ConsoleColor endIndexColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = consoleColor;
            if(selected != -1 && selected == (turn == 1 ? board.Length-2 : pitsCount)) Console.ForegroundColor = selectColor;
            Console.Write("|‾‾|");
            Console.ForegroundColor = consoleColor;
            if(turn == 1)
            {
                for(int i=board.Length-3; i>pitsCount; i--)
                {
                    if(selected != -1 && i == turnEnd && !animationFrame)
                    {
                        if(i == selected) Console.ForegroundColor = selectColor; 
                        Console.Write(" [");
                        Console.ForegroundColor = endIndexColor;
                        Console.Write(board[i]>0?IntTo2DigitString(board[i]):"00");
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
                        Console.Write(board[i]>0?IntTo2DigitString(board[i]+add):emptyPitShow);
                        Console.ForegroundColor = selectColor;
                        Console.Write("]");
                        Console.ForegroundColor = consoleColor;
                        continue;
                    }
                    Console.Write(" [");
                    Console.Write(board[i]>0?IntTo2DigitString(board[i]):emptyPitShow);
                    Console.Write("]");
                }
                Console.Write(" ");
                
            }
            else
            {
                for(int i=pitsCount-1; i>-1; i--)
                {
                    if(selected != -1 && i == turnEnd && !animationFrame)
                    {
                        if(i == selected) Console.ForegroundColor = selectColor; 
                        Console.Write(" [");
                        Console.ForegroundColor = endIndexColor;
                        Console.Write(board[i]>0?IntTo2DigitString(board[i]):"00");
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
                        Console.Write(board[i]>0?IntTo2DigitString(board[i]+add):emptyPitShow);
                        Console.ForegroundColor = selectColor;
                        Console.Write("]");
                        Console.ForegroundColor = consoleColor;
                        continue;
                    }
                    Console.Write(" [");
                    Console.Write(board[i]>0?IntTo2DigitString(board[i]):emptyPitShow);
                    Console.Write("]");
                }
                Console.Write(" ");
            }
            if(selected != -1 && selected == (turn == 1 ? pitsCount : board.Length-2)) Console.ForegroundColor = selectColor;
            Console.Write("|‾‾|");
            Console.ForegroundColor = consoleColor;
            Console.WriteLine();
            int opponentsBar = turn == 1 ? board[board.Length-2] : board[pitsCount];
            bs = false;
            if(selected != -1 && (turn == 1 ? board.Length-2 : pitsCount) == turnEnd && !animationFrame)
            {
                Console.Write("|");
                Console.ForegroundColor = endIndexColor;
                Console.Write(IntTo2DigitString(opponentsBar));
                Console.ForegroundColor = consoleColor;
                Console.Write("| ");
                bs = true;
            }
            if(selected != -1 && selected == (turn == 1 ? board.Length-2 : pitsCount))
            {
                Console.ForegroundColor = selectColor;
                Console.Write("|"+IntTo2DigitString(opponentsBar+add)+"| ");
                Console.ForegroundColor = consoleColor;
                bs = true;
            }
            if(!bs)
            {Console.Write("|"+IntTo2DigitString(opponentsBar)+"| ");}
            for(int i=0; i<(pitsCount*5)-1; i++) Console.Write("-");
            int ownBar = turn == 1 ? board[pitsCount] : board[board.Length-2];
            bs = false;
            if(selected != -1 && (turn == 1 ? pitsCount : board.Length-2) == turnEnd && !animationFrame)
            {
                Console.Write(" |");
                Console.ForegroundColor = endIndexColor;
                Console.Write(IntTo2DigitString(ownBar));
                Console.ForegroundColor = consoleColor;
                Console.Write("|");
                bs = true;
            }
            if(selected != -1 && selected == (turn == 1 ? pitsCount : board.Length-2))
            {
                Console.ForegroundColor = selectColor;
                Console.Write(" |"+IntTo2DigitString(ownBar+add)+"|");
                Console.ForegroundColor = consoleColor;
                bs = true;
            }
            if(!bs)
            {Console.Write(" |"+IntTo2DigitString(ownBar)+"|");}
            Console.WriteLine();
            if(selected != -1 && selected == (turn == 1 ? board.Length-2 : pitsCount)) Console.ForegroundColor = selectColor;
            Console.Write("|__|");
            Console.ForegroundColor = consoleColor;
            if(turn == 1)
            {
                for(int i=0; i<pitsCount; i++)
                {
                    if(selected != -1 && i == turnEnd && !animationFrame)
                    {
                        if(i == selected) Console.ForegroundColor = selectColor; 
                        Console.Write(" [");
                        Console.ForegroundColor = endIndexColor;
                        Console.Write(board[i]>0?IntTo2DigitString(board[i]):"00");
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
                        Console.Write(board[i]>0?IntTo2DigitString(board[i]+add):emptyPitShow);
                        Console.ForegroundColor = selectColor;
                        Console.Write("]");
                        Console.ForegroundColor = consoleColor;
                        continue;
                    }
                    Console.Write(" [");
                    Console.Write(board[i]>0?IntTo2DigitString(board[i]):emptyPitShow);
                    Console.Write("]");
                }
                Console.Write(" ");
            }
            else
            {
                for(int i=pitsCount+1; i<board.Length-2; i++)
                {
                    if(selected != -1 && i == turnEnd && !animationFrame)
                    {
                        if(i == selected) Console.ForegroundColor = selectColor; 
                        Console.Write(" [");
                        Console.ForegroundColor = endIndexColor;
                        Console.Write(board[i]>0?IntTo2DigitString(board[i]):"00");
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
                        Console.Write(board[i]>0?IntTo2DigitString(board[i]+add):emptyPitShow);
                        Console.ForegroundColor = selectColor;
                        Console.Write("]");
                        Console.ForegroundColor = consoleColor;
                        continue;
                    }
                    Console.Write(" [");
                    Console.Write(board[i]>0?IntTo2DigitString(board[i]):emptyPitShow);
                    Console.Write("]");
                }
                Console.Write(" ");
            }
            if(selected != -1 && selected == (turn == 1 ? pitsCount : board.Length-2)) Console.ForegroundColor = selectColor;
            Console.Write("|__|");
            Console.ForegroundColor = consoleColor;
            Console.WriteLine();
            
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
        public static async Task AnimateMove(int[] board, int showFromPerspective,
                                              int move, int showMoveTime,
                                               bool debug=false, bool showZeros=false,
                                               string? ownNick="", string? opponentsNick="",
                                               string? spaceBeforeOwnNick="")
        {
            int[] bc = board.ToArray();
            int pitsCount = (bc.Length-3)/2;
            int piecesCount = bc[move];
            int turn = bc[bc.Length-1];
            int currentIndex;
            if(piecesCount == 1)
            {
                bc[move] = -1;
                Console.WriteLine("\x1b[3J");
                Console.Clear();
                Console.WriteLine(opponentsNick);
                ShowBoard(bc, showFromPerspective, move, debug, true, showZeros);
                Console.WriteLine(spaceBeforeOwnNick+ownNick);
                await Task.Delay(showMoveTime);
                bc[move] = 0;
                Console.WriteLine("\x1b[3J");
                Console.Clear();
                Console.WriteLine(opponentsNick ?? "Anonymous");
                ShowBoard(bc, showFromPerspective, (move+1)%(bc.Length-1), debug, true, showZeros);
                Console.WriteLine(spaceBeforeOwnNick+ownNick);
                await Task.Delay(showMoveTime);
                currentIndex = move+1;
            }
            else
            {
                bc[move] = 0;
                currentIndex = move;
                int opponentsBar = turn == 1 ? board.Length-2 : pitsCount;
                while(piecesCount>0)
                {
                    if(currentIndex == opponentsBar)
                    {
                        currentIndex = (currentIndex+1)%(bc.Length-1);
                    }
                    Console.WriteLine("\x1b[3J");
                    Console.Clear();
                    if(debug)
                    {
                        Console.Write("Current index: ");
                        Console.WriteLine(currentIndex);
                    }
                    Console.WriteLine(opponentsNick);
                    ShowBoard(bc, showFromPerspective, currentIndex, debug, true, showZeros);
                    Console.WriteLine(spaceBeforeOwnNick+ownNick);
                    await Task.Delay(showMoveTime);
                    bc[currentIndex]++;
                    piecesCount--;
                    currentIndex = (currentIndex+1)%(bc.Length-1);
                }
            }
            currentIndex = TurnEndIndex(board, turn == 1 ? move : move-7);
            if(board[currentIndex] == 0 
                && (turn == 1 ? currentIndex<6 : currentIndex>6&&currentIndex<13) 
                && board[12-currentIndex] != 0)
            {
                bc[12-currentIndex] -= 1;
                Console.WriteLine("\x1b[3J");
                Console.Clear();
                Console.WriteLine(opponentsNick);
                ShowBoard(bc, showFromPerspective, 12-currentIndex, debug, true, showZeros);
                Console.WriteLine(spaceBeforeOwnNick+ownNick);
                await Task.Delay(showMoveTime);
            }
            if((turn == 1 ? currentIndex>6&&currentIndex<13 : currentIndex<6) 
                 && board[currentIndex] % 2 == 0)
            {
                bc[currentIndex] -= 1;
                Console.WriteLine("\x1b[3J");
                Console.Clear();
                Console.WriteLine(opponentsNick);
                ShowBoard(bc, showFromPerspective, currentIndex, debug, true, showZeros);
                Console.WriteLine(spaceBeforeOwnNick+ownNick);
                await Task.Delay(showMoveTime);
            }
        }
    }
}