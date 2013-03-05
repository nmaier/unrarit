using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;

namespace UnRarIt.Utilities
{
  public static class Tools
  {
    private readonly static Regex camelcase = new Regex(@"([a-z\d])([A-Z])", RegexOptions.Compiled);

    private readonly static Regex underscores = new Regex(@"-|_", RegexOptions.Compiled);

    private readonly static Regex whitespace = new Regex(@"\s{2,}", RegexOptions.Compiled);

    private readonly static MatchEvaluator CleanNameEvaluator = CleanNameME;


    private static string CleanNameME(Match m)
    {
      return String.Format(CultureInfo.InvariantCulture, "{0} {1}", m.Groups[1], m.Groups[2]);
    }


    public static string CleanName(string name)
    {
      if (string.IsNullOrEmpty(name)) {
        return name;
      }
      name = camelcase.Replace(name, CleanNameEvaluator);

      name = underscores.Replace(name, " ");
      name = whitespace.Replace(name, " ");
      return name.Trim();
    }
  }
}
