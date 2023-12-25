using System.Globalization;

var data = File.ReadAllLines("data.txt");
Console.WriteLine(FindArea(data.Select(step => {
    var split = step.Split(" ");
    return (long.Parse(split[1]), split[0] switch {
        "R" => Direction.Right,
        "L" => Direction.Left,
        "U" => Direction.Up,
        "D" => Direction.Down
    });
}).ToList()));

Console.WriteLine(FindArea(data.Select(step => {
    var split = step.Split(" ");
    return (long.Parse(split[2][2..^2], NumberStyles.HexNumber), split[2][^2] switch {
        '0' => Direction.Right,
        '2' => Direction.Left,
        '3' => Direction.Up,
        '1' => Direction.Down
    });
}).ToList()));

long FindArea(List<(long, Direction)> steps) {
    var positions = new List<((long y, long x) loc, long dist, Direction dir, Direction cornerTo)>();
    var curr = (0L, 0L);
    for (var ii = 0; ii < steps.Count; ++ii) {
        var (len, dir) = steps[ii];
        var delta = DirToDelta(dir);
        positions.Add((curr = Add(curr, (delta.y * len, delta.x * len)), len, dir,
            steps[(ii + 1) % steps.Count].Item2));
    }
    var cornerRows = positions.Select(p => p.loc.y).Distinct().Order().ToList();
    return steps.Sum(s => s.Item1) +  //Border
           cornerRows.Sum(InternalWidthAt) + //Internal width on rows with corners
           Enumerable.Range(1, cornerRows.Count - 1) //Internal boxes over rows without corners
               .Sum(ii => InternalWidthAt(cornerRows[ii] - 1) * (cornerRows[ii] - 1 - cornerRows[ii - 1]));
    
    long InternalWidthAt(long y) {
        long internalWidth = 0;
        var crossedVerticals = 0;
        long lastCrossed = -1;
        Direction? lastCrossedCorner = null;
        foreach (var line in positions.Where(p =>
                         p.loc.y == y ||
                         p.loc.y > y && p.dir is Direction.Down && p.loc.y - p.dist < y ||
                         p.loc.y < y && p.dir is Direction.Up && p.loc.y + p.dist > y)
                     .OrderBy(p => p.loc.x)) {
            void CommitCrossing() {
                if (++crossedVerticals % 2 == 0)
                    internalWidth += line.loc.x - 1 - lastCrossed;
            }
            if (line.loc.y == y) { //Corner
                var cornerDir = IsVert(line.dir) ? line.dir : line.cornerTo;
                if (lastCrossedCorner is { } d) {
                    //Paired corners count as one crossing if the vert directions are the same
                    // eg Up->Right and then Right->Up (1 crossing); Up->Right and then Right->Down (2 crossings)
                    //Either way the second corner doesn't contribute to internal width
                    if (d != cornerDir)
                        ++crossedVerticals;
                    lastCrossedCorner = null;
                } else {
                    CommitCrossing();
                    lastCrossedCorner = cornerDir;
                }
            } else
                CommitCrossing();
            lastCrossed = line.loc.x;
        }
        return internalWidth;
    }
}

static bool IsVert(Direction d) => d is Direction.Down or Direction.Up;
static (long y, long x) Add((long y, long x) p1, (long y, long x) p2) => (p1.y + p2.y, p1.x + p2.x);

static (long y, long x) DirToDelta(Direction d) => d switch {
    Direction.Up => (-1, 0),
    Direction.Right => (0, 1),
    Direction.Down => (1, 0),
    Direction.Left => (0, -1),
};

enum Direction {
    Up,
    Right,
    Down,
    Left
}