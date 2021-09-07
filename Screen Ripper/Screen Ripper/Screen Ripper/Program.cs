using System.Threading;

namespace Screen_Ripper
{
    static class Program
    {
        static void Main()
        {
            MeltColor melt = new MeltColor();
            Waves melt2 = new Waves();

            while (true)
            {
                melt.Start();
                Thread.Sleep(12000);
                melt.Stop();
                melt2.Start();
                Thread.Sleep(10000);
                melt2.Stop();
            }
        }
    }
}
