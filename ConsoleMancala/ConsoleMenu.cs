using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleMenu
{
    class Menu
    {
        public static T[][] Paginate<T>(IEnumerable<T> array, int pageSize)
        {
            if (pageSize <= 0)
            {throw new ArgumentException("Page size must be greater than 0");}
            if (array == null)
            {throw new ArgumentNullException("Array is null");}

            T[] list = array.ToArray();
            int count = list.Length;

            if (count == 0)
            {throw new ArgumentException("Array is empty");}

            int pageCount = count % pageSize != 0 ?((count - count % pageSize) / pageSize) + 1 : count / pageSize;
            T[][] pages = new T[pageCount][];
            for (int i=0; i<pageCount; i++)
            {
                pages[i] = new T[pageSize];
                int start = i * pageSize;
                int end = start + pageSize < count ? start + pageSize : count;
                int j = 0;
                for (int k=start; k<end; k++)
                {
                    pages[i][j] = list[k];
                    j++;
                }
            }
            return pages;
        }
        private static void BlockConsole()
        {
#pragma warning disable CA1416 // Validate platform compatibility
            Console.BufferHeight = Console.WindowHeight;
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
            Console.BufferWidth = Console.WindowWidth;
#pragma warning restore CA1416 // Validate platform compatibility
            Console.SetCursorPosition(0, 0);
        }
        private static void ShowPage<T>(T[] page, string title = "", ConsoleColor selectionColor = ConsoleColor.Green)
        {
            Console.WriteLine("\x1b[3J");
            Console.Clear();
            bool cond = false;
            int YOffset = title != "" ? title.Split("\n").Length : 0;
            if (title != "") Console.WriteLine(title);
            for (int i=0; i<page.Length; i++)
            {
                if (cond)
                {
                    Console.ForegroundColor = default;
                    cond = false;
                }
                if (i == 0)
                {
                    Console.ForegroundColor = selectionColor;
                    cond = true;
                    Console.WriteLine("> " + Convert.ToString(page[i]));
                    continue;
                }
                Console.WriteLine("  " + Convert.ToString(page[i]));
            }
            Console.ForegroundColor = default;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Console.SetCursorPosition(Convert.ToString(page[0]).Length + 2, YOffset);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
        /// <summary>
        ///     Shows menu in console with paginated options<br/><br/>
        ///     Controls are:<br/>
        ///         - Arrow Up to go up<br/>
        ///         - Arrow Down to go down<br/>
        ///         - Arrow Right to go to next page<br/>
        ///         - Arrow Left to go to previos page<br/>
        ///         - Escape to return default value<br/>
        ///         - Enter to confirm choice
        /// 
        /// </summary> 
        /// <returns>T of choice</returns>
        public static T MenuShow<T>(T[][] pages, int pageIndex = 0, string title = "", ConsoleColor selectionColor = ConsoleColor.Green)
        {
            BlockConsole();
            int choice = 0;
            
            int YOffset = title != "" ? title.Split("\n").Length : 0;

            int pi = pageIndex;
            ShowPage(pages[pi], title, selectionColor);
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        Console.SetCursorPosition(0, choice + YOffset);
                        Console.ForegroundColor = default;
                        Console.Write("  " + pages[pi][choice]);
                        if (choice > 0)
                        {choice--;}
                        else
                        {choice = pages[pi].Length - 1;}
                        Console.SetCursorPosition(0, choice + YOffset);
                        Console.ForegroundColor = selectionColor;
                        Console.Write("> " + pages[pi][choice]);
                        Console.ForegroundColor = default;
                        break;
                    case ConsoleKey.DownArrow:
                        Console.SetCursorPosition(0, choice + YOffset);
                        Console.ForegroundColor = default;
                        Console.Write("  " + pages[pi][choice]);
                        if (choice < pages[pi].Length - 1)
                        {choice++;}
                        else
                        {choice = 0;}
                        Console.SetCursorPosition(0, choice + YOffset);
                        Console.ForegroundColor = selectionColor;
                        Console.Write("> " + pages[pi][choice]);
                        Console.ForegroundColor = default;
                        break;
                    case ConsoleKey.Enter:
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        return pages[pi][choice];
                    case ConsoleKey.LeftArrow:
                        if (pi > 0)
                        {pi--;}
                        else
                        {pi = pages.Length - 1;}
                        ShowPage(pages[pi], title, selectionColor);
                        choice = 0;
                        break;
                    case ConsoleKey.RightArrow:
                        if (pi < pages.Length - 1)
                        {pi++;}
                        else
                        {pi = 0;}
                        ShowPage(pages[pi], title, selectionColor);
                        choice = 0;
                        break;
                    case ConsoleKey.Escape:
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        T? x = default;
                        return x == null ? pages[0][0] : x;
                }
            }
        }
    }
}