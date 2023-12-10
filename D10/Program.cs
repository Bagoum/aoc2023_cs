using Pos = (int y, int x);

var map = File.ReadLines("data.txt").Select(x => x.ToCharArray()).ToArray();

Pos start = (from row in Enumerable.Range(0, map.Length)
    from col in Enumerable.Range(0, map[row].Length)
    select (row, col)).First(yx => map[yx.row][yx.col] == 'S');
var directions = Enum.GetValues<Dir>().Select(dir => {
        var src = Add(start, ToDelta(dir));
        if (IsOob(src)) return ((Dir dir, HashSet<(Pos p, Dir d)> visited)?) null;
        var pipe = map[src.y][src.x];
        if (pipe != '.' && EntersTo(pipe).Contains(dir) && TryToReach(src, start, dir, out var visited))
                return (dir, visited);
        return null;
    }).Where(x => x != null).Select(x => x.Value).ToArray();

Console.WriteLine(directions[0].visited.Count / 2);

map[start.y][start.x] = "|-LJ7F".First(d => EntersTo(d).Intersect(directions.Select(x => Invert(x.dir))).Count() == 2);
var border = directions[0].visited.Select(pd => pd.p).ToHashSet();

Console.WriteLine(Enumerable.Range(0, map.Length).Sum(row => 
    Enumerable.Range(0, map[row].Length).Sum(col => {
        if (border.Contains((row, col))) return 0;
        int crossings = 0;
        for (Pos p = (row, col - 1); !IsOob(p); p = (p.y, p.x - 1))
            if (border.Contains(p) && "F7|".Contains(map[p.y][p.x]))
                ++crossings;
        return crossings % 2;
    })));

bool IsOob(Pos p) => p.y < 0 || p.y >= map.Length || p.x < 0 || p.x >= map[p.y].Length;

bool TryToReach(Pos source, Pos target, Dir towards, out HashSet<(Pos, Dir)> visited) {
    visited = [];
    for (var curr = source; curr != target;) {
        if (IsOob(curr) || !visited.Add((curr, towards)))
            return false;
        var c = map[curr.y][curr.x];
        if (c == '.')
            return false;
        var pipeSources = EntersTo(c);
        curr = Add(curr, ToDelta(towards = Invert(pipeSources[1 - Array.IndexOf(pipeSources, towards)])));
    }
    visited.Add((target, towards));
    return true;
}

Dir[] EntersTo(char pipe) => pipe switch {
    '|' => [Dir.Up, Dir.Down],
    '-' => [Dir.Left, Dir.Right],
    'L' => [Dir.Down, Dir.Left],
    'J' => [Dir.Down, Dir.Right],
    'F' => [Dir.Up, Dir.Left],
    '7' => [Dir.Up, Dir.Right]
};
Pos Add(Pos a, Pos b) => (a.y + b.y, a.x + b.x);
Pos ToDelta(Dir d) => d switch {
    Dir.Up => (-1, 0),
    Dir.Down => (1, 0),
    Dir.Left => (0, -1),
    Dir.Right => (0, 1),
};
Dir Invert(Dir d) => d switch {
    Dir.Up => Dir.Down,
    Dir.Down => Dir.Up,
    Dir.Left => Dir.Right,
    Dir.Right => Dir.Left,
};
enum Dir {
    Up,
    Down,
    Left,
    Right,
}