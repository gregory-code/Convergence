using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public static class RichTextFormatter
{
    public static void ApplyFormatting(TextMeshProUGUI tmp, string input)
    {
        if (tmp == null) throw new ArgumentNullException(nameof(tmp));
        if (input == null) input = string.Empty;

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "physical", "<color=#FF8C00>{0}</color>" },            // orange
            { "magic",    "<color=#1E90FF>{0}</color>" },            // blue

            { "health",     "<color=#FF0000>{0}</color>" },          // red
            { "max health", "<color=#FF0000>{0}</color>" },          // red
            { "heal",       "<color=#FF0000>{0}</color>" },          // red
            { "healed",     "<color=#FF0000>{0}</color>" },          // red

            { "defense", "<color=#D0D0D0>{0}</color>" },             // darker white / light gray

            { "exhaust",   "<u><color=#E8D8B0>{0}</color></u>" },    // beige + underline
            { "exhausted", "<u><color=#E8D8B0>{0}</color></u>" },    // beige + underline

            { "energize",  "<u><color=#FFF3A0>{0}</color></u>" },    // light yellow + underline
            { "energized", "<u><color=#FFF3A0>{0}</color></u>" },    // light yellow + underline

            { "spark", "<u><color=#FFD200>{0}</color></u>" },       // darker yellow + underline

            { "swift", "<u><color=#FFFF00>{0}</color></u>" }        // yellow + underline
        };

        // Sort keys by descending length to avoid partial matches
        var orderedKeys = map.Keys
            .OrderByDescending(k => k.Length)
            .Select(Regex.Escape);

        // Word boundary matching
        var pattern = @"\b(" + string.Join("|", orderedKeys) + @")\b";

        string Evaluator(Match m)
        {
            var matchedText = m.Value;
            var key = map.Keys.First(k =>
                string.Equals(k, matchedText, StringComparison.OrdinalIgnoreCase));

            return string.Format(map[key], matchedText);
        }

        tmp.text = Regex.Replace(input, pattern, Evaluator, RegexOptions.IgnoreCase);
    }
}
