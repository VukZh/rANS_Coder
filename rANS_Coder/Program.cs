
namespace rANS_Coder
{
    class Program
    {
        static void Main(string[] args)
        {
            MainSolution ms = new MainSolution(args); // инициализируем с аргументами командной строки
            ms.InitSolution(); // читаем файл и в зависимости от его типа кодируем или декодируем
        }
    }
}
