using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConsoleMancala;
using ConsoleMenu;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;
using System.Net.Sockets;

public class WebGameClient
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private HubConnection _connection;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private string _sessionId = "";
    private bool _debug;
    private bool _showZeros;
    private int _role;
    private int _showMoveTime;
    private bool _canMove;
    private int _winner = 0;
    private int[] board = new int[15];
    private List<int[]> history = new List<int[]>();
    private List<int> movesHistory = new List<int>();
    private string[] _avaliableGames;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private TaskCompletionSource<bool> _gameEnded;
    private readonly SemaphoreSlim _drawLock = new(1, 1);
    private bool _gameStarted;
    private bool _waitingForResult;
    private ConsoleColor consoleColor;
    private ConsoleColor selectColor;
    private string? OwnNick;
    private string? OpponentsNick;
    private string? SpaceBeforeOwnNick;
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
        movesHistory = new List<int>();
        _gameStarted = false;
        _waitingForResult = false;
        _showMoveTime = Convert.ToInt16(settings["Show opponent's turn time(in ms)"].ToString());
        _showMoveTime = _showMoveTime < 0 ? _showMoveTime * -1 : _showMoveTime;
        _debug = settings["Show debug"].ToString() == "True";
        _showZeros = settings["Show zeros"].ToString() == "True";
        consoleColor = (ConsoleColor)((ColorOption)settings["Default color"]).GetColor();
        selectColor = (ConsoleColor)((ColorOption)settings["Menu select color"]).GetColor();
        string[] options = new string[4] {"Quick game", "Public games list", "Join game", "Create game"};
        string nick = settings["Nickname"].ToString() ?? "Anonymous";
        var url = settings["Connect ip"];

        _connection = new HubConnectionBuilder()
            .WithUrl($"{url}/game")
            .Build();

        RegisterHandlers();

        try
        {
            await _connection.StartAsync();
            string choice = Menu.MenuShow(Menu.Paginate(options, options.Length), 0, "", selectColor, consoleColor);
            switch(choice)
            {
                case "Quick game":
                    await _connection.InvokeAsync("QuickGame", nick);
                    break;
                case "Public games list":
                    _waitingForResult = true;
                    await _connection.InvokeAsync("ListPublicGames");
                    while(_waitingForResult) await Task.Delay(50);
                    if(_avaliableGames.Length == 1 && _avaliableGames[0] == "") return;
                    do {
                        _sessionId = Menu.MenuShow(Menu.Paginate(_avaliableGames, 8), 0, "", selectColor, consoleColor);
                        Console.WriteLine("Trying to connect to game...");
                        _waitingForResult = true;
                        await _connection.InvokeAsync("ListPublicGames");
                        while(_waitingForResult) await Task.Delay(50);
                        if(_avaliableGames.Length == 1 && _avaliableGames[0] == "") return;
                        if(!_avaliableGames.Contains(_sessionId))
                        {
                            Console.WriteLine("\x1b[3J");
                            Console.Clear();
                            Console.WriteLine("Can't connect to game: game is full");
                            Console.WriteLine("Press any key to return to games list");
                            Console.ReadKey();
                        }
                    }
                    while(!_avaliableGames.Contains(_sessionId));
                    await _connection.InvokeAsync("JoinGame", _sessionId, nick);
                    break;
                case "Join game":
                    Console.WriteLine("\x1b[3J");
                    Console.Clear();
                    Console.WriteLine("Enter Game ID to join:");
                    _sessionId = Console.ReadLine() ?? "";
                    await _connection.InvokeAsync("JoinGame", _sessionId, nick);
                    break;
                case "Create game":
                    Console.WriteLine("Enter your game id(or leave blank so it will be generated):");
                    string newSessionId = Console.ReadLine() ?? "";
                    BoolOption publicGame = new BoolOption(true);
                    Console.WriteLine("Is game public? < "+publicGame.ToString()+" >");
                    ConsoleKeyInfo k;
                    bool publicityIsntChosen = true;
                    while(publicityIsntChosen)
                    {
                        k = Console.ReadKey();
                        switch(k.Key)
                        {
                            case ConsoleKey.RightArrow: case ConsoleKey.LeftArrow:
                                publicGame.NextValue();
                                Console.WriteLine("\x1b[3J");
                                Console.Clear();
                                Console.WriteLine("Enter your game id(or leave blank so it will be generated):");
                                Console.WriteLine(newSessionId);
                                Console.WriteLine("Is game public? < "+publicGame.ToString()+" >");
                                break;
                            case ConsoleKey.Enter:
                                publicityIsntChosen = false;
                                break;
                        }
                    }
                    await _connection.InvokeAsync("CreateGame", newSessionId, publicGame.ToString(), nick);
                    break;
                case "":
                    return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Connection failed: " + ex.Message);
            return;
        }

        var inputLoop = InputLoopAsync();

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
                        Console.WriteLine(OpponentsNick);
                        Board.ShowBoard(history[boardStateIndex], _role, -1, false, false, _showZeros);
                        Console.WriteLine(SpaceBeforeOwnNick+OwnNick);
                        if (_winner != 3) Console.WriteLine(_winner == _role ? "You won!" : "You lost");
                        else Console.WriteLine("Draw.");
                        break;
                    }
                    if(boardStateIndex == history.Count-1)
                    {
                        boardStateIndex = 0;
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        Console.WriteLine(OpponentsNick);
                        Board.ShowBoard(history[boardStateIndex], _role, movesHistory[boardStateIndex], false, false, _showZeros);
                        Console.WriteLine(SpaceBeforeOwnNick+OwnNick);
                        if (_winner != 3) Console.WriteLine(_winner == _role ? "You won!" : "You lost");
                        else Console.WriteLine("Draw.");
                        break;
                    }

                    boardStateIndex++;
                    Console.WriteLine("\x1b[3J");
                    Console.Clear();
                    Console.WriteLine(OpponentsNick);
                    Board.ShowBoard(history[boardStateIndex], _role, movesHistory[boardStateIndex], false, false, _showZeros);
                    Console.WriteLine(SpaceBeforeOwnNick+OwnNick);
                    if (_winner != 3) Console.WriteLine(_winner == _role ? "You won!" : "You lost");
                    else Console.WriteLine("Draw.");

                    break;
                case ConsoleKey.LeftArrow: case ConsoleKey.A:
                    if(boardStateIndex == 0)
                    {
                        boardStateIndex = history.Count-1;
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        Console.WriteLine(OpponentsNick);
                        Board.ShowBoard(history[boardStateIndex], _role, -1, false, false, _showZeros);
                        Console.WriteLine(SpaceBeforeOwnNick+OwnNick);
                        if (_winner != 3) Console.WriteLine(_winner == _role ? "You won!" : "You lost");
                        else Console.WriteLine("Draw.");
                        break;
                    }

                    boardStateIndex--;
                    Console.WriteLine("\x1b[3J");
                    Console.Clear();
                    Console.WriteLine(OpponentsNick);
                    Board.ShowBoard(history[boardStateIndex], _role, movesHistory[boardStateIndex], false, false, _showZeros);
                    Console.WriteLine(SpaceBeforeOwnNick+OwnNick);
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

    private void RegisterHandlers()
    {
        _connection.On<string>("AvaliableGames", (avaliableGameList) => 
        {
            _avaliableGames = avaliableGameList.Split(",");
            _waitingForResult = false;
        });

        _connection.On<string>("sessionID", (sessionId) => 
        {
            _sessionId = _sessionId == "" ? sessionId : _sessionId;
        });
        
        _connection.On<string>("Role", (role) =>
        {
            _role = Convert.ToInt16(role);
        });

        _connection.On<string>("Nicknames", (nicknames) => 
        {
            if(_role == 1)
            {
                OwnNick = nicknames.Split(",")[0];
                OpponentsNick = nicknames.Split(",")[1];
            }
            if(_role == 2)
            {
                OwnNick = nicknames.Split(",")[1];
                OpponentsNick = nicknames.Split(",")[0];
            }
            SpaceBeforeOwnNick = "";
            int nickLen = (OwnNick??"").Length == 0 ? 39 : (OwnNick??"").Length;
            for(int i=0; i<39-nickLen; i++) SpaceBeforeOwnNick += " ";
        });

        _connection.On<string>("WaitingForOpponent", (sessionId) =>
        {
            Console.WriteLine("\x1b[3J");
            Console.Clear();
            Console.WriteLine("Created game with id:\n"+sessionId);
            Console.WriteLine("Share it to your opponent");
            Console.WriteLine("Waiting for opponent...");
        });

        _connection.On<string>("GameState", async (state) =>
        {
            if(!_gameStarted) _gameStarted = true;
            await _drawLock.WaitAsync();

            try
            {
                string[] gamestate = state.Split(",");
                int[] bc = new int[board.Length];
                for(int i=0; i<board.Length; i++) bc[i] = Convert.ToInt16(gamestate[i]);
                if(gamestate[gamestate.Length-1] != "")
                {
                    int previousMove = Convert.ToInt16(gamestate[gamestate.Length-1]);
                    movesHistory.Add(previousMove);
                    if(board[board.Length-1] != _role)
                    {
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        Console.WriteLine(OpponentsNick);
                        Board.ShowBoard(board, _role, previousMove, _debug, false, _showZeros);
                        Console.WriteLine(SpaceBeforeOwnNick+OwnNick);
                        await Task.Delay(_showMoveTime);
                        await Board.AnimateMove(board, _role, previousMove, _showMoveTime, _debug, _showZeros, OwnNick, OpponentsNick, SpaceBeforeOwnNick);
                    }
                }
                history.Add(bc);
                Console.WriteLine("\x1b[3J");
                Console.Clear();
                board = bc.ToArray();
                Console.WriteLine(OpponentsNick);
                Board.ShowBoard(board, _role, -1, _debug, false, _showZeros);
                Console.WriteLine(SpaceBeforeOwnNick+OwnNick);
                Console.Write("Turn: ");
                Console.WriteLine(board[board.Length-1]);
                Console.Write("Role: ");
                Console.WriteLine(_role);
                Console.Write("{ ");
                for(int j=0; j<board.Length; j++) Console.Write(Convert.ToString(board[j])+", ");
                Console.Write(" }");
                _canMove = board[board.Length-1] == _role;
                Console.WriteLine(_canMove ? "It's Your turn" : "It's Opponent's turn");
                _waitingForResult = _canMove;
            }
            finally
            {
                _drawLock.Release();
            }
        });

        _connection.On<string>("GameOver", (msg) =>
        {
            _canMove = false;
            _gameEnded.TrySetResult(true);
            _winner = Convert.ToInt16(msg);
        });

        _connection.On<string>("Error", (msg) =>
        {
            if(msg == "Game full") 
            {
                Console.WriteLine("Error: "+msg);
                _gameEnded.TrySetResult(true);
            }
            if(msg == "Game with that id already exists")
            {
                Console.WriteLine("Error: "+msg);
            }
        });
    }

    private async Task InputLoopAsync()
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
                i = turn == 1 ? 3 : 10;
                keyNotSelected = true;
                Console.WriteLine("\x1b[3J");
                Console.Clear();
                Console.WriteLine(OpponentsNick);
                Board.ShowBoard(board, _role, i, _debug, false, _showZeros);
                Console.WriteLine(SpaceBeforeOwnNick+OwnNick);
                Console.Write("Turn: ");
                Console.WriteLine(board[board.Length-1]);
                Console.Write("Role: ");
                Console.WriteLine(_role);
                Console.Write("{ ");
                for(int j=0; j<board.Length; j++) Console.Write(Convert.ToString(board[j])+", ");
                Console.Write(" }");
                Console.WriteLine("It's Your turn");
                while(keyNotSelected)
                {
                    keyInfo = Console.ReadKey();
                    switch(keyInfo.Key)
                    {
                        case ConsoleKey.RightArrow: case ConsoleKey.D:
                            Console.WriteLine("\x1b[3J");
                            Console.Clear();
                            i = turn == 1 ? (i == 5 ? 0 : i+1) : (i == 12 ? 7 : i+1);
                            Console.WriteLine(OpponentsNick);
                            Board.ShowBoard(board, _role, i, _debug, false, _showZeros);
                            Console.WriteLine(SpaceBeforeOwnNick+OwnNick);
                            Console.Write("Turn: ");
                            Console.WriteLine(board[board.Length-1]);
                            Console.Write("Role: ");
                            Console.WriteLine(_role);
                            Console.Write("{ ");
                            for(int j=0; j<board.Length; j++) Console.Write(Convert.ToString(board[j])+", ");
                            Console.Write(" }");
                            Console.WriteLine("It's Your turn");
                            break;
                        case ConsoleKey.LeftArrow: case ConsoleKey.A:
                            Console.WriteLine("\x1b[3J");
                            Console.Clear();
                            i = turn == 1 ? (i == 0 ? 5 : i-1) : (i == 7 ? 12 : i-1);
                            Console.WriteLine(OpponentsNick);
                            Board.ShowBoard(board, _role, i, _debug, false, _showZeros);
                            Console.WriteLine(SpaceBeforeOwnNick+OwnNick);
                            Console.Write("Turn: ");
                            Console.WriteLine(board[board.Length-1]);
                            Console.Write("Role: ");
                            Console.WriteLine(_role);
                            Console.Write("{ ");
                            for(int j=0; j<board.Length; j++) Console.Write(Convert.ToString(board[j])+", ");
                            Console.Write(" }");
                            Console.WriteLine("It's Your turn");
                            break;
                        case ConsoleKey.Enter:
                            if(board[i] != 0) keyNotSelected = false;
                            break;
                    }
                }
                int move = turn == 1 ? i : i-7;
                await _connection.InvokeAsync("MakeMove", _sessionId, move.ToString());
                _canMove = false;
                _waitingForResult = true;
            }
            else
            {
                await _drawLock.WaitAsync();
                try
                {
                    if(_gameStarted && !_waitingForResult)
                    {
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        Console.WriteLine(OpponentsNick);
                        Board.ShowBoard(board, _role, -1, _debug, false, _showZeros);
                        Console.WriteLine(SpaceBeforeOwnNick+OwnNick);
                        Console.Write("Turn: ");
                        Console.WriteLine(board[board.Length-1]);
                        Console.Write("Role: ");
                        Console.WriteLine(_role);
                        Console.Write("Waiting for opponent's turn");
                        for(int k=0; k<3; k++)
                        {
                            Console.Write(".");
                            await Task.Delay(300);
                        }
                    }
                    else
                    {await Task.Delay(100);}
                }
                finally
                {
                    _drawLock.Release();
                }
            }
        }
        Console.WriteLine("\x1b[3J");
        Console.Clear();
        Console.WriteLine(OpponentsNick);
        Board.ShowBoard(board, _role, -1, _debug, false, _showZeros);
        Console.WriteLine(SpaceBeforeOwnNick+OwnNick);
        Console.WriteLine("Game Over: "+(_winner == _role ? "You won!" : "You lost"));
    }
    public static string GetLocalIPv4Address(string port)
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return "http://"+ip.ToString()+":"+port;
                }
            }
            return "http://192.168.0.18"+":"+port; // Fallback to loopback address
        }
        catch
        {
            return "http://192.168.0.18"+":"+port; // Fallback to loopback address on error
        }
    }
}
