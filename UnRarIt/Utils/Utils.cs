using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace UnRarIt.Utils
{
    public class Utils
    {
        static private Regex camelcase = new Regex(@"([a-z\d])([A-Z])", RegexOptions.Compiled);
        static private Regex underscores = new Regex(@"-|_", RegexOptions.Compiled);
        static private Regex whitespace = new Regex(@"\s{2,}", RegexOptions.Compiled);
        static private string CleanNameME(Match m)
        {
            return String.Format("{0} {1}", m.Groups[1], m.Groups[2]);
        }
        static private MatchEvaluator CleanNameEvaluator = new MatchEvaluator(CleanNameME);
        static public string CleanName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            name = camelcase.Replace(name, CleanNameEvaluator);

            name = underscores.Replace(name, " ");
            name = whitespace.Replace(name, " ");
            return name.Trim();
        }

    }
}