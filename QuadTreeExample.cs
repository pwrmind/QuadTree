
// Example usage
public class QuadTreeExample
{
    public static void Run()
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
            if (string.IsNullOrEmpty(address))
            {
                break;
            }
            var bbox = system.AddressToBBox(address);
            Console.WriteLine($"Границы квадрата '{address}': " +
                $"X: [{bbox.xmin:F10}-{bbox.xmax:F10}], " +
                $"Y: [{bbox.ymin:F10}-{bbox.ymax:F10}]");
        }
    }
}
