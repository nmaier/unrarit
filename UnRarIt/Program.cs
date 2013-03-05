using System;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using NMaier.GetOptNet;

[assembly:CLSCompliant(true)]
namespace UnRarIt
{
  internal static class Program
  {
    [STAThread]
    private static void Main(string[] args)
    {
      var options = new Options();
      try {
        options.Parse(args);

        Application.EnableVisualStyles();
        Application.VisualStyleState = VisualStyleState.ClientAndNonClientAreasEnabled;
        Application.SetCompatibleTextRenderingDefault(true);
        Application.Run(new Main(options.Auto, options.Dir, options.Args));
      }
      catch (GetOptException) {
        MessageBox.Show(
                    "You provided invalid options. Please mind the usage as described below:\n\n" +
                    options.AssembleUsage(73),
                    "Usage",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
        );
      }
    }


    [GetOptOptions(AcceptPrefixType = ArgumentPrefixType.Dashes, CaseType = ArgumentCaseType.OnlyLower)]
    private class Options : GetOpt
    {
      [Parameters]
      public string[] Args = new string[0];
      [Argument(Helpvar = "DIRECTORY", Helptext = "Specifies the directory to extract to")]
      [ShortArgument('d')]
      public string Dir = string.Empty;
      [Argument(Helptext = "Extract all files and exit")]
      [ShortArgument('a')]
      public bool Auto = false;
    }
  }
}
