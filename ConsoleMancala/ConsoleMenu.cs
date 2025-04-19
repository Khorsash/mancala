using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleMenu
{
    abstract class SettingOption
    {
        public abstract void NextValue();
        public abstract void PreviousValue();
    }

    class StringOption: SettingOption
    {
        private string[] Options;
        private int ValueIndex;
        public StringOption(string[] options, int valueIndex)
        {
            Options = options;
            ValueIndex = valueIndex;
        }
        public override void NextValue()
        {
            ValueIndex = (ValueIndex+1) % Options.Length;
        }
        public override void PreviousValue()
        {
            ValueIndex = ValueIndex == 0 ? Options.Length-1 : ValueIndex-1;
        }
        public override string ToString()
        {
            return Options[ValueIndex];
        }
    }
    class BoolOption: SettingOption
    {
        private bool Value;
        public BoolOption(bool value)
        {
            Value = value;
        }
        public override void NextValue()
        {
            Value = !Value;
        }
        public override void PreviousValue()
        {
            Value = !Value;
        }
        public override string ToString()
        {
            return Value.ToString();
        }
    }
    class IntOption: SettingOption
    {
        private int Value;
        private int Step;

        public IntOption(int value, int step)
        {
            Value = value;
            Step = step;
        }
        public override void NextValue()
        {
            Value += Step;
        }
        public override void PreviousValue()
        {
            Value -= Step;
        }
        public override string ToString()
        {
            return Value.ToString();
        }
    }
    class DoubleOption: SettingOption
    {
        private double Value;
        private double Step;

        public DoubleOption(double value, double step)
        {
            Value = value;
            Step = step;
        }
        public override void NextValue()
        {
            Value += Step;
        }
        public override void PreviousValue()
        {
            Value -= Step;
        }
        public override string ToString()
        {
            return Value.ToString();
        }
    }
    class ColorOption: SettingOption
    {
        private int[] Colors;
        private int ColorIndex;
        public ColorOption(int[] colors, int valueIndex)
        {
            Colors = colors;
            ColorIndex = valueIndex;
        }
        public override void NextValue()
        {
            ColorIndex = (ColorIndex+1) % Colors.Length;
        }
        public override void PreviousValue()
        {
            ColorIndex = ColorIndex == 0 ? Colors.Length-1 : ColorIndex-1;
        }
        public override string ToString()
        {
            switch(Colors[ColorIndex])
            {
                case 0:
                    return "black";
                case 1:
                    return "dark blue";
                case 2:
                    return "dark green";
                case 3:
                    return "dark cyan";
                case 4:
                    return "dark red";
                case 5:
                    return "dark magenta";
                case 6:
                    return "dark yellow";
                case 7:
                    return "gray";
                case 8:
                    return "dark gray";
                case 9:
                    return "blue";
                case 10:
                    return "green";
                case 11:
                    return "cyan";
                case 12:
                    return "red";
                case 13:
                    return "magenta";
                case 14:
                    return "yellow";
                case 15:
                    return "white";
                default:
                    return "";   
            }
        }
        public int GetColor()
        {
            return Colors[ColorIndex];
        }
    }
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
        ///         - Arrow Left to go to previous page<br/>
        ///         - Escape to return default value<br/>
        ///         - Enter to confirm choice
        /// 
        /// </summary> 
        /// <returns>T of choice</returns>
        public static T MenuShow<T>(T[][] pages, int pageIndex = 0, string title = "", ConsoleColor selectionColor = ConsoleColor.Green)
        {
            // BlockConsole();
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
        public static void ShowSettings(Dictionary<string, SettingOption> settings, int selected, ConsoleColor selectionColor=ConsoleColor.Green)
        {
            string[] settingNames = settings.Keys.ToArray();
            ConsoleColor valueSelectedColor = ConsoleColor.White;
            for(int i=0; i<settingNames.Length; i++)
            {
                if(i==selected) Console.ForegroundColor = selectionColor;
                else Console.ForegroundColor = default;
                Console.Write(settingNames[i]+": ");
                if(i==selected) Console.ForegroundColor = valueSelectedColor;
                Console.Write("< ");
                Console.Write(settings[settingNames[i]]);
                Console.WriteLine(" >");
            }

        }
        public static void ChangeSettings(Dictionary<string, SettingOption> settings, ConsoleColor selectionColor = ConsoleColor.Green)
        {
            string[] settingNames = settings.Keys.ToArray();
            int currentSetting = 0;
            Console.WriteLine("\x1b[3J");
            Console.Clear();
            ShowSettings(settings, currentSetting, selectionColor);
            bool notConfirmed = true;
            while(notConfirmed)
            {
                ConsoleKeyInfo key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.DownArrow:
                        currentSetting = (currentSetting+1) % settingNames.Length;
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        ShowSettings(settings, currentSetting, selectionColor);
                        break;
                    case ConsoleKey.UpArrow:
                        currentSetting = currentSetting == 0 ? settingNames.Length-1 : currentSetting-1;
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        ShowSettings(settings, currentSetting, selectionColor);
                        break;
                    case ConsoleKey.LeftArrow:
                        settings[settingNames[currentSetting]].PreviousValue();
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        ShowSettings(settings, currentSetting, selectionColor);
                        break;
                    case ConsoleKey.RightArrow:
                        settings[settingNames[currentSetting]].NextValue();
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        ShowSettings(settings, currentSetting, selectionColor);
                        break;
                    case ConsoleKey.Escape:
                        Console.WriteLine("\x1b[3J");
                        Console.Clear();
                        return;
                }
            }
        }
    }
}