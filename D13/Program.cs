var puzzles = File.ReadAllText("data.txt").ReplaceLineEndings("\n").Split("\n\n")
    .Select(lines => lines.Split('\n')).ToArray();

Console.WriteLine(puzzles.Select(p => SolvePuzzle(p, 0)).Sum());
Console.WriteLine(puzzles.Select(p => SolvePuzzle(p, 1)).Sum());

long SolvePuzzle(string[] lines, int errors) {
    var w = lines[0].Length;
    var h = lines.Length;
    for (var col = 1; col < w; ++col)
        if (Enumerable.Range(col, w-col).Sum(cmpB => {
                var cmpA = 2 * col - 1 - cmpB;
                return cmpA < 0 ?
                    0 :
                    Enumerable.Range(0, h).Count(y =>
                        lines[y][cmpA] != lines[y][cmpB]);
            }) == errors)
            return col;
    for (var row = 1; row < h; ++row)
        if (Enumerable.Range(row, h-row).Sum(cmpB => {
                var cmpA = 2 * row - 1 - cmpB;
                return cmpA < 0 ?
                    0 :
                    Enumerable.Range(0, w).Count(x =>
                        lines[cmpA][x] != lines[cmpB][x]);
            }) == errors)
            return row * 100;
    return 0;
}