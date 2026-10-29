using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CollectionTracker
{
    public class InvalidDataException : Exception
    {
        public InvalidDataException(string message) : base(message) { }
    }

    public class GameEntry
    {
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidDataException("Название не может быть пустым.");
                if (value.Contains(";")) // Эксперимент 1: Защита от разделителя
                    throw new InvalidDataException("Символ ';' использовать нельзя.");
                _name = value;
            }
        }
        public int Playtime { get; set; }
        public int Rating { get; set; }
        public string Category { get; set; }

        public GameEntry(string name, int playtime, int rating, string category)
        {
            Name = name;
            Playtime = playtime;
            Rating = rating;
            Category = category;
        }

        public override string ToString() => $"{Name};{Playtime};{Rating};{Category}";
    }

    public class GameLog
    {
        public List<GameEntry> Items { get; private set; } = new();

        public void Load(string path)
        {
            Items.Clear();
            if (!File.Exists(path)) return;

            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                // Эксперимент 2: Обработка испорченных данных
                try
                {
                    var parts = line.Split(';');
                    if (parts.Length != 4) continue; // Пропускаем мусорные строки

                    if (!int.TryParse(parts[1], out int playtime)) continue;
                    if (!int.TryParse(parts[2], out int rating)) continue;

                    Items.Add(new GameEntry(parts[0], playtime, rating, parts[3]));
                }
                catch { /* Тихо пропускаем битую строку */ }
            }
        }

        public void Save(string path)
        {
            var lines = Items.Select(i => i.ToString()).ToArray();
            File.WriteAllLines(path, lines);
        }

        public void Add(GameEntry e) => Items.Add(e);

        public List<GameEntry> Search(string needle) =>
            Items.Where(i => i.Name.ToLower().Contains(needle.ToLower())).ToList();

        public double AveragePlaytime() => Items.Count > 0 ? Items.Average(i => i.Playtime) : 0;
    }

    class Program
    {
        static GameLog log = new();
        static string dataPath = "games.txt";

        static void Main()
        {
            log.Load(dataPath);
            Menu();
        }

        static void Menu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("1. Таблица | 2. Добавить | 3. Поиск | 4. Среднее | 0. Выход");
                var ch = Console.ReadLine();
                try
                {
                    switch (ch)
                    {
                        case "1": PrintTable(); break;
                        case "2": AddItem(); break;
                        case "3": SearchItem(); break;
                        case "4": PrintAverage(); break;
                        case "0": log.Save(dataPath); return;
                    }
                }
                catch (InvalidDataException ex) { Console.WriteLine($"Ошибка: {ex.Message}"); }
                catch (Exception ex) { Console.WriteLine($"Системная ошибка: {ex.Message}"); }
                Console.WriteLine("\nEnter...");
                Console.ReadLine();
            }
        }

        static void PrintTable()
        {
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0,-25} | {1,-8} | {2,-6} | {3}",
                "Игра", "Время", "Оценка", "Тип"));
            Console.WriteLine(new string('-', 60));
            foreach (var g in log.Items)
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0,-25} | {1,-8} | {2,-6} | {3}",
                    g.Name, g.Playtime, g.Rating, g.Category));
        }

        static void AddItem()
        {
            Console.Write("Название: "); var n = Console.ReadLine() ?? "";
            Console.Write("Время (мин): "); int.TryParse(Console.ReadLine(), out int t);
            Console.Write("Оценка: "); int.TryParse(Console.ReadLine(), out int r);
            Console.Write("Тип: "); var c = Console.ReadLine() ?? "";
            log.Add(new GameEntry(n, t, r, c));
        }

        static void SearchItem()
        {
            Console.Write("Часть названия: "); var q = Console.ReadLine() ?? "";
            var res = log.Search(q);
            foreach (var g in res) Console.WriteLine(g.Name);
        }

        static void PrintAverage()
        {
            var avg = log.AveragePlaytime();
            Console.WriteLine($"Без формата: {avg}");
            Console.WriteLine($"С форматом F2: {avg:F2}");
        }
    }
}