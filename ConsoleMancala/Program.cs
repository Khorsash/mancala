using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleMenu;

namespace ConsoleMancala
{
    
    class Program
    {
        static void ClearConsole()
        {
            Console.WriteLine("\x1b[3J");
            Console.Clear();
        }
        static int[] MakeTurn(int[] board, int pitIndx)
        {
            int turn = board[board.Length-1];
            int startIndx = turn == 1 ? pitIndx : pitIndx + 7;

            if(startIndx > 13 || board.Length != 15 || board[startIndx] == 0) 
            {return board;}

            int[] finalBoard = board.ToArray();
            int opponentsBar = turn == 1 ? 13 : 6;
            int ownBar = turn == 1 ? 6 : 13;
            int piecesCount = board[startIndx] == 1 ? 1 : board[startIndx]-1;
            finalBoard[startIndx] = board[startIndx] == 1 ? 0 : 1;
            int currentIndex = startIndx;
            while(piecesCount>0)
            {
                currentIndex = (currentIndex+1)%(board.Length-1);
                if(currentIndex == opponentsBar) continue;
                finalBoard[currentIndex]++;
                piecesCount--;
            }

            if((turn == 1 ? (currentIndex<6) : (currentIndex>6&&currentIndex<13)) && finalBoard[currentIndex] == 1 && finalBoard[12-currentIndex] != 0)
            {
                finalBoard[ownBar] += finalBoard[12-currentIndex] + 1;
                finalBoard[12-currentIndex] = 0;
                finalBoard[currentIndex] = 0;
            }

            if((turn == 1 ? (currentIndex>6&&currentIndex<13) : (currentIndex<6)) && finalBoard[currentIndex] % 2 == 0)
            {
                finalBoard[ownBar] += finalBoard[currentIndex];
                finalBoard[currentIndex] = 0;
            }
            
            if(finalBoard.AsSpan(0, 6).ToArray().Sum() == 0)
            {
                finalBoard[6] += finalBoard.AsSpan(7, 6).ToArray().Sum();
                for(int i=7; i<13; i++) finalBoard[i] = 0;
            }
            else 
            {
                if(finalBoard.AsSpan(7, 6).ToArray().Sum() == 0)
                {
                    finalBoard[13] += finalBoard.AsSpan(0, 6).ToArray().Sum();
                    for(int i=0; i<6; i++) finalBoard[i] = 0;
                }
            }
            
            if(currentIndex == ownBar)
            {return finalBoard;}
            
            finalBoard[board.Length-1] = turn == 1 ? 2 : 1;

            return finalBoard;
        }
        static void HotSitGame(Dictionary<string, SettingOption> settings) 
        {
            int[] board = new int[15] {4, 4, 4, 4, 4, 4, 0, 4, 4, 4, 4, 4, 4, 0, 1};
            bool debug = settings["Show debug"].ToString() == "True";
            List<int[]> history = new List<int[]>();
            history.Add(board);
            List<int> movesHistory = new List<int>();
            bool gameRunning = true;
            bool keyNotSelected;
            int turn;
            int i;
            int winner = 1;
            while(gameRunning)
            {
                turn = board[board.Length-1];
                ClearConsole();
                Console.WriteLine("Now's player "+Convert.ToString(turn)+" turn");
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
                ClearConsole();
                ConsoleKeyInfo keyInfo;
                keyNotSelected = true;
                i = turn == 1 ? 3 : 10;
                Board.ShowBoard(board, 0, i, debug);
                while(keyNotSelected)
                {
                    keyInfo = Console.ReadKey();
                    switch(keyInfo.Key)
                    {
                        case ConsoleKey.RightArrow: case ConsoleKey.D:
                            ClearConsole();
                            i = turn == 1 ? (i == 5 ? 0 : i+1) : (i == 12 ? 7 : i+1);
                            Board.ShowBoard(board, 0, i, debug);
                            break;
                        case ConsoleKey.LeftArrow: case ConsoleKey.A:
                            ClearConsole();
                            i = turn == 1 ? (i == 0 ? 5 : i-1) : (i == 7 ? 12 : i-1);
                            Board.ShowBoard(board, 0, i, debug);
                            break;
                        case ConsoleKey.Enter:
                            if(board[i] != 0) keyNotSelected = false;
                            break;
                    }
                }
                board = MakeTurn(board, turn == 1 ? i : i-7);
                history.Add(board);
                movesHistory.Add(i);
                if(board.AsSpan(0, 6).ToArray().Sum()+board.AsSpan(7, 6).ToArray().Sum() == 0)
                {
                    winner = board[6] > board[13] ? 1 : 2;
                    if (board[6] == board[13]) winner = 3;
                    gameRunning = false;
                }
            }
            ClearConsole();
            Board.ShowBoard(board, 0, 0, debug);
            if (winner == 1 || winner == 2) 
            {Console.WriteLine("Player "+Convert.ToString(winner)+" won!\nSwitch between moves with Right and Left arrow keys\nPress Escape to exit to menu");}
            else
            {Console.WriteLine("Draw.\nSwitch between moves with Right and Left arrow keys\nPress Escape to exit to menu");}
            ConsoleKeyInfo keyInfo2;
            bool analyzingParty = true;
            int boardStateIndex = history.Count-1;
            while(analyzingParty)
            {
                keyInfo2 = Console.ReadKey();
                switch(keyInfo2.Key)
                {
                    case ConsoleKey.RightArrow: case ConsoleKey.D:
                        if(boardStateIndex == history.Count-2)
                        {
                            boardStateIndex = history.Count-1;
                            ClearConsole();
                            Board.ShowBoard(history[boardStateIndex], 0);
                            if (winner == 1 || winner == 2) Console.WriteLine("Player "+Convert.ToString(winner)+" won!");
                            else Console.WriteLine("Draw.");
                            break;
                        }
                        if(boardStateIndex == history.Count-1)
                        {
                            boardStateIndex = 0;
                            ClearConsole();
                            Board.ShowBoard(history[boardStateIndex], 0, movesHistory[boardStateIndex]);
                            if (winner == 1 || winner == 2) Console.WriteLine("Player "+Convert.ToString(winner)+" won!");
                            else Console.WriteLine("Draw.");
                            break;
                        }

                        boardStateIndex++;
                        ClearConsole();
                        Board.ShowBoard(history[boardStateIndex], 0, movesHistory[boardStateIndex]);
                        if (winner == 1 || winner == 2) Console.WriteLine("Player "+Convert.ToString(winner)+" won!");
                        else Console.WriteLine("Draw.");

                        break;
                    case ConsoleKey.LeftArrow: case ConsoleKey.A:
                        if(boardStateIndex == 0)
                        {
                            boardStateIndex = history.Count-1;
                            ClearConsole();
                            Board.ShowBoard(history[boardStateIndex], 0);
                            if (winner == 1 || winner == 2) Console.WriteLine("Player "+Convert.ToString(winner)+" won!");
                            else Console.WriteLine("Draw.");
                            break;
                        }

                        boardStateIndex--;
                        ClearConsole();
                        Board.ShowBoard(history[boardStateIndex], 0, movesHistory[boardStateIndex]);
                        if (winner == 1 || winner == 2) Console.WriteLine("Player "+Convert.ToString(winner)+" won!");
                        else Console.WriteLine("Draw.");

                        break;
                    case ConsoleKey.Escape:
                        ClearConsole();
                        analyzingParty = false;
                        break;
                
                }
            }

        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            string[] MenuOptions = new string[4] {"New Hot seat game", "New online game", "Settings", "Exit"};

            Dictionary<string, SettingOption> settings = new Dictionary<string, SettingOption>();
            int[] menuSelectColors = new int[15];
            for(int i=1; i<16; i++) menuSelectColors[i-1] = i;
            settings["Show debug"] = new BoolOption(false);
            settings["Menu select color"] = new ColorOption(menuSelectColors, 13);
            settings["Settings select color"] = new ColorOption(menuSelectColors, 13);

            bool Running = true;
            while(Running)
            {
                string choice = Menu.MenuShow(Menu.Paginate(MenuOptions, 4), 0, "", 
                                            (ConsoleColor)((ColorOption)settings["Menu select color"]).GetColor());
                switch(choice)
                {
                    case "New Hot seat game":
                        HotSitGame(settings);
                        break;
                    case "Settings":
                        Menu.ChangeSettings(settings, 
                                            (ConsoleColor)((ColorOption)settings["Settings select color"]).GetColor());
                        break;
                    case "Exit":
                        Running = false;
                        ClearConsole();
                        break;
                }
            }
        }
    }
}