var overlaps = File.ReadLines("data.txt")
    .Select(line => {
        var split = line.Split(':', '|');
        return split[1].Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Intersect(split[2].Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Count();
    }).ToList();

Console.WriteLine(overlaps.Sum(g => g == 0 ? 0 : 1 << (g - 1)));

var multipliers = Enumerable.Repeat(1, overlaps.Count).ToArray();
for (int i = 0; i < overlaps.Count; ++i)
for (int j = i + 1; j <= i + overlaps[i]; ++j)
    multipliers[j] += multipliers[i];
Console.WriteLine(multipliers.Sum());