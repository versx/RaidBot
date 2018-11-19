namespace T
{
    using System.Threading.Tasks;

    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var bot = new Bot();
            await bot.Start();

            while (true) { }
        }
    }
}