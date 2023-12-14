using Pos = (int y, int x);
var data = File.ReadLines("data.txt").ToList();

var emptyRows = Enumerable.Range(0, data.Count).Where(r => data[r].All(c => c == '.')).ToHashSet();
var emptyCols = Enumerable.Range(0, data[0].Length).Where(c => 
    Enumerable.Range(0, data.Count).All(r => data[r][c] == '.')).ToHashSet();
HashSet<Pos> galaxies = (from row in Enumerable.Range(0, data.Count)
                from col in Enumerable.Range(0, data[row].Length)
                where data[row][col] == '#'
                select (row, col)).ToHashSet();

long FindSumOfSteps(Pos source, long magnification) => galaxies.Sum(target => 
        Enumerable.Range(Math.Min(target.y, source.y), Math.Abs(target.y - source.y))
            .Sum(y => emptyRows.Contains(y) ? magnification : 1) +
        Enumerable.Range(Math.Min(target.x, source.x), Math.Abs(target.x - source.x))
            .Sum(x => emptyCols.Contains(x) ? magnification : 1)
    );

Console.WriteLine(galaxies.Select(g => FindSumOfSteps(g, 2)).Sum()/2);
Console.WriteLine(galaxies.Select(g => FindSumOfSteps(g, 1000000)).Sum()/2);