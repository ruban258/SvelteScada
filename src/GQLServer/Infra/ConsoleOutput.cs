using System;

namespace GQLServer.infra
{
    public interface IOutput
    {
        void WriteLine(object obj);
        void WriteLine(string msg);
        void WriteLine(string msg, params object[] parameters);
    }

    public class ConsoleOutput : IOutput
    {
        public void WriteLine(object obj) => Console.WriteLine(obj);
        public void WriteLine(string msg) => Console.WriteLine(msg);
        public void WriteLine(string msg, params object[] parameters) => Console.WriteLine(msg, parameters);
    }
}