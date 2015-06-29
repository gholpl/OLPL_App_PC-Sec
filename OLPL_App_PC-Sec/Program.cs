using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OLPL_App_PC_Sec
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string [] args)
        {
            settings st1 = new settings();
            functions fn1 = new functions();
            results rs1 = new results();
            st1.baseKey = "SOFTWARE\\OLPL";
            st1.appKey = "PC-Sec";
            fn1.regCreateKeys(st1);
            st1 = fn1.getSettings(st1);
            st1.logFile = @"c:\temp\log.txt";
            st1.mode = 1;
            st1.resultURL = "https://api.olpl.org/api/sec";
            if (args.Length > 0)
            {
                if (args[0].ToUpper() == "ADMIN")
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Form1());
                }
            }   
           else
            {
                Console.WriteLine("Run application on client");
                rs1 = fn1.checkMaintUser(st1,rs1);
                rs1.Result_Admin_User = fn1.checkAdministrator(st1, "Administrator");
                rs1.Result_Admin_Group = fn1.checkAdministrators(st1);


                fn1.sendResults(st1, rs1);
                Thread.Sleep(6000);
                Application.Exit();
            }
        }
    }
}
