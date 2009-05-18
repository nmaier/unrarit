using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NMaier.GetOptNet;

namespace UnRarIt
{
    static class Program
    {
        [GetOptOptions(AcceptPrefixType=ArgumentPrefixType.Dashes)]
        class Options : GetOpt
        {
            [Parameters]
            public string[] Args = new string[0];

            [Argument]
            [ShortArgument('a')]
            public bool Auto = false;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Options options = new Options();
            try
            {
                options.Parse(args);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Main(options.Auto, options.Args));
            }
            catch (GetOptException)
            {
                MessageBox.Show(
                    options.AssembleUsage(60),
                    "Usage",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                    );
            }
        }
    }
}
