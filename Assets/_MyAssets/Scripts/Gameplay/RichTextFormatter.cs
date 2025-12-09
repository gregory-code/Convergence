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
            // colors in hex, you can change hex or use named colors supported by TMP
            { "physical", "<color=#FF8C00>{0}</color>" },            // orange
            { "magic",    "<color=#1E90FF>{0}</color>" },            // blue
            { "health",   "<color=#FF0000>{0}</color>" },            // red
            { "heal",     "<color=#FF0000>{0}</color>" },            // red
            { "healed",   "<color=#FF0000>{0}</color>" },            // red
            { "Swift",    "<u><color=#FFFF00>{0}</color></u>" }      // yellow and underlined
        };

        // Sort keys by descending length to avoid partial matches (e.g., "healed" before "heal")
        var orderedKeys = map.Keys.OrderByDescending(k => k.Length).Select(Regex.Escape);

        // \b ensures whole word matching; allows punctuation immediately around words
        var pattern = @"\b(" + string.Join("|", orderedKeys) + @")\b";

        // Replacement evaluator that preserves the original matched text's casing
        string Evaluator(Match m)
        {
            var matchedText = m.Value;
            // Find corresponding map key (case-insensitive): use First because map is case-insensitive
            var key = map.Keys.First(k => string.Equals(k, matchedText, StringComparison.OrdinalIgnoreCase));
            var template = map[key];
            return string.Format(template, matchedText);
        }

        string result = Regex.Replace(input, pattern, new MatchEvaluator(Evaluator), RegexOptions.IgnoreCase);
        tmp.text = result;
    }
}