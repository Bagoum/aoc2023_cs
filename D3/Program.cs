var data = File.ReadLines("data.txt").Select(s => s.ToCharArray()).ToArray();
Dictionary<(int, int), List<int>> gears = new();
bool PartExistsInRange(int r0, int r1, int c0, int c1, int value) {
    r1 = Math.Clamp(r1, 0, data.Length - 1);
    c1 = Math.Clamp(c1, 0, data[0].Length - 1);
    bool found = false;
    for (int r = Math.Clamp(r0, 0, data.Length - 1); r <= r1; ++r)
    for (int c = Math.Clamp(c0, 0, data[0].Length - 1); c <= c1; ++c)
        if (!char.IsLetterOrDigit(data[r][c]) && data[r][c] != '.') {
            if (!gears.TryGetValue((r, c), out var lis))
                gears[(r, c)] = lis = new();
            lis.Add(value);
            found = true;
        }
    return found;
}

var sum = 0;
for (int r = 0; r < data.Length; ++r)
for (int c = 0; c < data[r].Length;)
    if (char.IsDigit(data[r][c]))
        for (int c1 = c; c1 <= data[r].Length;)
            if (c1 < data[r].Length && char.IsDigit(data[r][c1])) {
                ++c1;
            } else {
                var value = int.Parse(data[r][c..c1]);
                if (PartExistsInRange(r - 1, r + 1, c - 1, c1, value))
                    sum += value;
                c = c1;
                break;
            }
    else
        ++c;
Console.WriteLine(sum);

Console.WriteLine(gears
    .Where(kv => kv.Value.Count == 2)
    .Aggregate(0, (acc, kv) => acc + kv.Value[0] * kv.Value[1]));