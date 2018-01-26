using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STVRogue.Utils
{
    class Utils
    {
    }

    public class Logger
    {
        /* You can change the behavior of this logger. */
        public static void log(String s)
        {
            //Console.Out.WriteLine("** " + s);
        }
    }

    public class RandomGenerator
    {
        static private Random rnd_ = null ; 
        static public Random rnd { 
            get { if (rnd_==null) rnd_ = new Random();
                  return rnd_ ; }
        }


        static public void initializeWithSeed(int seed)
        {
            rnd_ = new Random(seed);
        }

          
    }

    public static class MyListExtensions
    {
        public static T GetRandomItem<T>(this List<T> xs)
        {
            if(xs.Count > 0)
                return xs[RandomGenerator.rnd.Next(xs.Count)];
            return default(T);
        }


    }
    
    public static class ReplayDirectory
    {
        public static string Get(string fileName)
        {
            string returnString = "";
            string s = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Replace(@"file:\", string.Empty);
            string[] split = s.Split('\\');
            for (int i = 0; i < split.Length - 3; i++)
            {
                returnString += split[i] + '\\';
            }
            return returnString + "Replay\\" + fileName;
        }
    }
}
