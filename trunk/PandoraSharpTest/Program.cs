using System;
using PandoraSharp;

namespace PandoraSharpTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var p = new Pandora();

            try
            {
                //p.Sync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }


            Console.WriteLine("testing");
        }
    }
}