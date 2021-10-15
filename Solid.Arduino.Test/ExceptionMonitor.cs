using System;
using System.Runtime.ExceptionServices;
using System.Reflection;

namespace Solid.Arduino.Test
{
    class ExceptionMonitor
    {
        public void Setup()
        {
            // Het FirstChanceException event gaat af als er een Exception optreedt in je applicatie.
            // Dit gebeurt nog voordat foutafhandeling (catch) in actie heeft kunnen komen. Dit maakt
            // deze event bij uitstek geschikt om te analyseren in welke mate een applicatie leunt op
            // 'handled exceptions', wat een forse impact op de performance kan hebben.
            AppDomain.CurrentDomain.FirstChanceException += FirstChanceHandler;
        }

        static void FirstChanceHandler(object o, FirstChanceExceptionEventArgs e)
        {
            MethodBase site = e.Exception.TargetSite;
            Console.WriteLine("Thrown in module {0}, by {1}({2})", site.Module, site.DeclaringType, site.ToString());
            Console.WriteLine("Stack trace:\n{0}", e.Exception.StackTrace);
        }
    }
}
