using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;

namespace UnRarIt
{
  partial class AboutBox : Form
  {
    public AboutBox()
    {
      InitializeComponent();
      Text = String.Format(CultureInfo.CurrentCulture, "About {0}", AssemblyTitle);
      labelVersion.Text = String.Format(CultureInfo.CurrentCulture, "Version {0}", AssemblyVersion);
      labelCopyright.Text = AssemblyCopyright;
      labelCompanyName.Text = AssemblyCompany;
      textBoxDescription.Text = AssemblyDescription;
    }


    public static string AssemblyCompany
    {
      get
      {
        var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
        if (attributes.Length == 0) {
          return string.Empty;
        }
        return ((AssemblyCompanyAttribute)attributes[0]).Company;
      }
    }
    public static string AssemblyCopyright
    {
      get
      {
        var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
        if (attributes.Length == 0) {
          return string.Empty;
        }
        return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
      }
    }
    public static string AssemblyDescription
    {
      get
      {
        var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
        if (attributes.Length == 0) {
          return string.Empty;
        }
        return ((AssemblyDescriptionAttribute)attributes[0]).Description;
      }
    }
    public static string AssemblyTitle
    {
      get
      {
        var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
        if (attributes.Length > 0) {
          var titleAttribute = (AssemblyTitleAttribute)attributes[0];
          if (!string.IsNullOrEmpty(titleAttribute.Title)) {
            return titleAttribute.Title;
          }
        }
        return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
      }
    }
    public static string AssemblyVersion
    {
      get
      {
        return Assembly.GetExecutingAssembly().GetName().Version.ToString();
      }
    }
  }
}
