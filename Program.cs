using System;
using System.Collections.Generic;
using System.Linq;

public class QuadTreeAddressSystem
{
    private readonly int depth;
    private readonly Dictionary<string, List<object>> storage;

    public QuadTreeAddressSystem(int depth = 5)
    {
        this.depth = depth;
        this.storage = new Dictionary<string, List<object>>();
    }

    public string PointToAddress(double x, double y)
    {
        double xmin = 0.0, ymin = 0.0;
        double xmax = 1.0, ymax = 1.0;
        char[] address = new char[depth];

        for (int i = 0; i < depth; i++)
        {
            double midX = (xmin + xmax) / 2;
            double midY = (ymin + ymax) / 2;

            // Корректировка выхода за границы
            x = Math.Clamp(x, xmin, xmax - 1e-10);
            y = Math.Clamp(y, ymin, ymax - 1e-10);

            if (x < midX && y >= midY)      // Квадрант A
            {
                address[i] = 'A';
                xmax = midX;
                ymin = midY;
            }
            else if (x >= midX && y >= midY) // Квадрант B
            {
                address[i] = 'B';
                xmin = midX;
                ymin = midY;
            }
            else if (x < midX && y < midY)   // Квадрант C
            {
                address[i] = 'C';
                xmax = midX;
                ymax = midY;
            }
            else                            // Квадрант D
            {
                address[i] = 'D';
                xmin = midX;
                ymax = midY;
            }
        }

        return new string(address);
    }

    public (double xmin, double xmax, double ymin, double ymax) AddressToBBox(string address)
    {
        double xmin = 0.0, ymin = 0.0;
        double xmax = 1.0, ymax = 1.0;

        foreach (char c in address)
        {
            double midX = (xmin + xmax) / 2;
            double midY = (ymin + ymax) / 2;

            switch (c)
            {
                case 'A':
                    xmax = midX;
                    ymin = midY;
                    break;
                case 'B':
                    xmin = midX;
                    ymin = midY;
                    break;
                case 'C':
                    xmax = midX;
                    ymax = midY;
                    break;
                case 'D':
                    xmin = midX;
                    ymax = midY;
                    break;
                default:
                    throw new ArgumentException($"Invalid character in address: '{c}'");
            }
        }

        return (xmin, xmax, ymin, ymax);
    }

    public void AddData(double x, double y, object data)
    {
        string address = PointToAddress(x, y);
        if (!storage.ContainsKey(address))
        {
            storage[address] = new List<object>();
        }
        storage[address].Add(data);
    }

    public List<object> GetByAddress(string address)
    {
        return storage.TryGetValue(address, out var data) ? data : new List<object>();
    }

    public List<object> GetByPrefix(string prefix)
    {
        return storage
            .Where(kvp => kvp.Key.StartsWith(prefix))
            .SelectMany(kvp => kvp.Value)
            .ToList();
    }

    public List<object> QueryRegion(double qxmin, double qxmax, double qymin, double qymax)
    {
        var results = new List<object>();

        foreach (var kvp in storage)
        {
            var (axmin, axmax, aymin, aymax) = AddressToBBox(kvp.Key);

            // Проверка пересечения квадрата с регионом
            bool xOverlap = axmin < qxmax && axmax > qxmin;
            bool yOverlap = aymin < qymax && aymax > qymin;

            if (xOverlap && yOverlap)
            {
                results.AddRange(kvp.Value);
            }
        }

        return results;
    }

    public string PointToAddress3D(double x, double y, double z)
    {
        // Добавьте обработку Z-координаты
        throw new NotImplementedException();
    }

    public (double, double, double, double, double, double) AddressToBBox3D(string address)
    {
        // Возвращайте 6 границ (x,y,z)
        throw new NotImplementedException();
    }
}

// Пример использования
public class Program
{
    public static void Main()
    {
        // Инициализация системы с глубиной 3
        var system = new QuadTreeAddressSystem(depth: 3);
        
        // Добавление данных
        system.AddData(0.2, 0.8, "Объект 1");  // Адрес: AAA
        system.AddData(0.6, 0.7, "Объект 2");  // Адрес: BCA
        system.AddData(0.7, 0.3, "Объект 3");  // Адрес: DDA
        
        // Поиск по адресу
        Console.WriteLine("Данные в квадрате 'BCA': " + 
            string.Join(", ", system.GetByAddress("BCA")));  // ["Объект 2"]
        
        // Поиск по префиксу
        Console.WriteLine("Данные в секторе 'B': " + 
            string.Join(", ", system.GetByPrefix("B")));  // ["Объект 2"]
        
        // Поиск по региону [0.5-1.0]x[0.0-0.5]
        Console.WriteLine("Данные в регионе: " + 
            string.Join(", ", system.QueryRegion(0.5, 1.0, 0.0, 0.5)));  // ["Объект 3"]

        // Преобразование адреса в границы
        while (true)
        {
            Console.Write($"Введите адрес для получения границы квадрата: ");
            var address = Console.ReadLine();
            var bbox = system.AddressToBBox(address);
                Console.WriteLine($"Границы квадрата '{address}': " +
                    $"X: [{bbox.xmin:F10}-{bbox.xmax:F10}], " +
                    $"Y: [{bbox.ymin:F10}-{bbox.ymax:F10}]");
        }
    }
}