using System;

namespace BuildStamp
{
    public class ProgramOutput
    {
        public void WriteOutputLine(string line = null)
        {
            if (line == null)
                Console.Out.WriteLine();
            else
                Console.Out.WriteLine(line);
        }

        public void WriteErrorLine(string line = null)
        {
            if (line == null)
                Console.Error.WriteLine();
            else
                Console.Error.WriteLine(line);
        }
    }
}
