using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleMancala;
using ConsoleMenu;
using Microsoft.AspNetCore.SignalR.Client;

public class WebGameClient
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private HubConnection _connection;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private string _sessionId = "";
    private int _role;
    private bool _canMove;
    private int _winner = 0;
    private int[] board = new int[15];
    private List<int[]> history = new List<int[]>();
    private List<int> movesHistory = new List<int>();
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private TaskCompletionSource<bool> _gameEnded;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public WebGameClient()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        history = new List<int[]>();
        movesHistory = new List<int>();
    }

    public async Task RunAsync(Dictionary<string, SettingOption> settings)
    {
        _gameEnded = new TaskCompletionSource<bool>();
        board = new int[15];
        history = new List<int[]>();
        bool debug = settings["Show debug"].ToString() == "True" ? true : false;

        Console.WriteLine("Enter server URL (default: http://192.168.0.18:5214): ");
        var url = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(url)) url = "http://192.168.0.18:5214";

        _connection = new HubConnectionBuilder()
            .WithUrl($"{url}/game")
            .Build();

        RegisterHandlers(debug);

        try
        {
            await _connection.StartAsync();

            Console.WriteLine("Enter Game ID to join (or leave blank to create new):");
            _sessionId = Console.ReadLine() ?? "";
            _sessionId = string.IsNullOrEmpty(_sessionId) || string.IsNullOrWhiteSpace(_sessionId)
                        ? Guid.NewGuid().ToString("N").Substring(0, 8) 
                        : _sessionId;

            await _connection.InvokeAsync("JoinGame", _sessionId);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Connection failed: " + ex.Message);
            return;
        }

        var inputLoop = InputLoopAsync(debug);

        await _gameEnded.Task;

        await _connection.StopAsync();
        await _connection.DisposeAsync();

        bool analyzingParty = true;
        int boardStateIndex = history.Count-1;
        ConsoleKeyInfo keyInfo;
        while(analyzingParty)
        {
            keyInfo = Console.ReadKey();
            switch(keyInfo.Key)
            {
                case ConsoleKey.RightArrow: case ConsoleKey.D:
                    if(boardStateIndex == history.Count-2)
                    {
                        boardStateIndex = history.Count-1;
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        Board.ShowBoard(history[boardStateIndex], _role);
                        if (_winner != 3) Console.WriteLine(_winner == _role ? "You won!" : "You lost");
                        else Console.WriteLine("Draw.");
                        break;
                    }
                    if(boardStateIndex == history.Count-1)
                    {
                        boardStateIndex = 0;
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        Board.ShowBoard(history[boardStateIndex], _role, movesHistory[boardStateIndex]);
                        if (_winner != 3) Console.WriteLine(_winner == _role ? "You won!" : "You lost");
                        else Console.WriteLine("Draw.");
                        break;
                    }

                    boardStateIndex++;
                    Console.WriteLine("\x1b[3J");
                    Console.Clear();
                    Board.ShowBoard(history[boardStateIndex], _role, movesHistory[boardStateIndex]);
                    if (_winner != 3) Console.WriteLine(_winner == _role ? "You won!" : "You lost");
                    else Console.WriteLine("Draw.");

                    break;
                case ConsoleKey.LeftArrow: case ConsoleKey.A:
                    if(boardStateIndex == 0)
                    {
                        boardStateIndex = history.Count-1;
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        Board.ShowBoard(history[boardStateIndex], 0);
                        if (_winner != 3) Console.WriteLine(_winner == _role ? "You won!" : "You lost");
                        else Console.WriteLine("Draw.");
                        break;
                    }

                    boardStateIndex--;
                    Console.WriteLine("\x1b[3J");
                    Console.Clear();
                    Board.ShowBoard(history[boardStateIndex], 0, movesHistory[boardStateIndex]);
                    if (_winner != 3) Console.WriteLine(_winner == _role ? "You won!" : "You lost");
                    else Console.WriteLine("Draw.");

                    break;
                case ConsoleKey.Escape:
                    Console.WriteLine("\x1b[3J");
                    Console.Clear();
                    analyzingParty = false;
                    break;
            
            }
        }
    }

    private void RegisterHandlers(bool debug=false)
    {
        _connection.On<string>("Role", (role) =>
        {
            _role = Convert.ToInt16(role);
        });

        _connection.On("WaitingForOpponent", () =>
        {
            Console.WriteLine("\x1b[3J");
            Console.Clear();
            Console.WriteLine("Created game with id:\n"+_sessionId);
            Console.WriteLine("Share it to your opponent");
            Console.WriteLine("Waiting for opponent...");
        });

        _connection.On<string>("GameState", (state) =>
        {
            string[] gamestate = state.Split(",");
            for(int i=0; i<board.Length; i++) board[i] = Convert.ToInt16(gamestate[i]);
            history.Add(board);
            Console.WriteLine("\x1b[3J");
            Console.Clear();
            Board.ShowBoard(board, _role, -1, debug);
        });

        _connection.On<string>("MadeMove", (move) => 
        {
            if(_canMove) movesHistory.Add(Convert.ToInt16(move));
        });

        _connection.On("CanMove", () =>
        {
            _canMove = true;
        });

        _connection.On<string>("GameOver", (msg) =>
        {
            Console.WriteLine("Game Over: "+(msg == _role.ToString() ? "You won!" : "You lost"));
            _canMove = false;
            _gameEnded.TrySetResult(true);
            _winner = Convert.ToInt16(msg);
        });

        _connection.On<string>("Error", (msg) =>
        {
            if(msg == "Game full") 
            {
                Console.WriteLine($"Error: {msg}");
                _gameEnded.TrySetResult(true);
            }
        });
    }

    private async Task InputLoopAsync(bool debug=false)
    {
        int i;
        bool keyNotSelected;
        int turn;
        ConsoleKeyInfo keyInfo;
        while (!_gameEnded.Task.IsCompleted)
        {
            if (_canMove)
            {
                turn = board[board.Length-1];
                Console.WriteLine("\x1b[3J");
                Console.Clear();
                i = turn == 1 ? 3 : 10;
                keyNotSelected = true;
                Board.ShowBoard(board, _role, i, debug);
                while(keyNotSelected)
                {
                    keyInfo = Console.ReadKey();
                    switch(keyInfo.Key)
                    {
                        case ConsoleKey.RightArrow: case ConsoleKey.D:
                            Console.WriteLine("\x1b[3J");
                            Console.Clear();
                            i = turn == 1 ? (i == 5 ? 0 : i+1) : (i == 12 ? 7 : i+1);
                            Board.ShowBoard(board, _role, i, debug);
                            break;
                        case ConsoleKey.LeftArrow: case ConsoleKey.A:
                            Console.WriteLine("\x1b[3J");
                            Console.Clear();
                            i = turn == 1 ? (i == 0 ? 5 : i-1) : (i == 7 ? 12 : i-1);
                            Board.ShowBoard(board, _role, i, debug);
                            break;
                        case ConsoleKey.Enter:
                            if(board[i] != 0) keyNotSelected = false;
                            break;
                    }
                }
                int move = turn == 1 ? i : i-7;
                await _connection.InvokeAsync("MakeMove", _sessionId, move.ToString());
                movesHistory.Add(move);
                _canMove = false;
            }
            else
            {
                await Task.Delay(100);
            }
        }
    }
}
