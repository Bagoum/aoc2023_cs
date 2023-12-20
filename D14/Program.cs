var data = File.ReadAllLines("data.txt").Select(row => row.ToArray()).ToArray();

var rollN = Roll(data, true, false);
Console.WriteLine(Locations(rollN).Sum(x => x.score));

var prevScores = new List<List<(int, int, int score)>> { Locations(data) };
var itrs = 1000000000;
while (true) {
    data = Roll(
            Roll(
                Roll(
                    Roll(data, true, false) //up
                    , false, false) //left
                , true, true) //down
            , false, true); //right
    var nxtScore = Locations(data);
    for (var period = 1; period <= prevScores.Count; ++period) {
        if (nxtScore.SequenceEqual(prevScores[^period])) {
            Console.WriteLine(
                prevScores[itrs - (1 + (int)(Math.Floor((itrs - prevScores.Count) * 1.0 / period))) * period]
                    .Sum(x => x.score));
            return;
        }
    }
    prevScores.Add(nxtScore);
}


static char[][] Roll(char[][] positions, bool yRoll, bool positive) {
    var result = positions.Select(row => row.Select(c => c == '#' ? '#' : '.').ToArray()).ToArray();
    var h = positions.Length;
    var w = positions[0].Length;
    var dir = positive ? 1 : -1;
    var yRange = Enumerable.Range(0, h);
    if (positive) yRange = yRange.Reverse();
    foreach (var y in yRange) {
        var xRange = Enumerable.Range(0, w);
        if (positive) xRange = xRange.Reverse();
        foreach (var x in xRange) {
            if (positions[y][x] != 'O') continue;
            var scanDir = (yRoll ? dir : 0, yRoll ? 0 : dir);
            var gotoPos = (y, x);
            for (var scan = Add(gotoPos, scanDir);
                 scan.y >= 0 && scan.y < h && scan.x >= 0 && scan.x < w && result[scan.y][scan.x] == '.';
                 scan = Add(scan, scanDir)) {
                gotoPos = scan;
            }
            result[gotoPos.y][gotoPos.x] = 'O';
        }
    }
    return result;
}

static (int y, int x) Add((int y, int x) p1, (int y, int x) p2) => (p1.y + p2.y, p1.x + p2.x);

static List<(int row, int col, int score)> Locations(char[][] positions) =>
    (from row in Enumerable.Range(0, positions.Length)
        from col in Enumerable.Range(0, positions[row].Length)
        where positions[row][col] == 'O'
        select (row, col, positions.Length - row)).ToList();