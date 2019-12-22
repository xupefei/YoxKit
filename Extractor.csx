#!/usr/bin/env dotnet-script

#load "YoxFile.csx"

using System.Text.RegularExpressions;

string InputPath = @"/Users/paddy/Desktop/MUSICUS/Script Org/";
string InputPattern = @"*.dat";

string OutputPath = @"/Users/paddy/Desktop/MUSICUS/Text Org/";
Directory.CreateDirectory(OutputPath);

foreach (string path in Directory.GetFiles(InputPath, InputPattern))
{
    var file = new YoxFile(File.ReadAllBytes(path));

    using var sw = File.CreateText(Path.Combine(OutputPath, Path.GetFileNameWithoutExtension(path) + ".txt"));

    Prettify(file.Strings).ForEach(sw.WriteLine);
}

static Regex LineMatcher = new Regex(@"^((?:@[A-Z])*)(.*?)((?:@[A-Z])*)$", RegexOptions.Compiled);

private List<string> Prettify(string[] strings)
{
    var res = new List<string>();

    for (int i = 0; i < strings.Length; i++)
    {
        string whole = strings[i];

        string[] lines = whole.Split("@L");

        for (int j = 0; j < lines.Length; j++)
        {
            string line = lines[j];
            var seg = LineMatcher.Match(line);

            string attr = $"[L={seg.Groups[1].Value}, R={seg.Groups[3].Value}]";
            string val = seg.Groups[2].Value;

            string id = (i + 1).ToString("D4") + "+" + (j + 1).ToString("D2");

            res.Add(attr);
            res.Add($"○{id}○{val}");
            res.Add($"●{id}●{val}");
            res.Add("");
        }
    }

    return res;
}