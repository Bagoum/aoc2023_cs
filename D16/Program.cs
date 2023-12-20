var board = File.ReadAllLines("data.txt");
var h = board.Length;
var w = board[0].Length;
Console.WriteLine(EnergizedBy(((0, 0), Direction.Right)));
Console.WriteLine(
    Enumerable.Range(0, board.Length).SelectMany(y => new[] { ((y, 0), Direction.Right), ((y, w - 1), Direction.Left) })
        .Concat(
            Enumerable.Range(0, w).SelectMany(x => new[] { ((0, x), Direction.Down), ((h - 1, x), Direction.Up) }))
        .Max(EnergizedBy)
);

int EnergizedBy(((int, int), Direction) start) {
    var visited = new Dictionary<(int, int), Direction>();
    var toVisit = new Queue<((int y, int x) loc, Direction dir)>();
    toVisit.Enqueue(start);
    while (toVisit.TryDequeue(out var nxt)) {
        var ((y, x), dir) = nxt;
        if (y < 0 || y >= h || x < 0 || x >= w)
            continue;
        if (visited.TryGetValue((y, x), out var ds)) {
            if ((ds & dir) > 0) continue;
            visited[(y, x)] = ds | dir;
        } else {
            visited[(y, x)] = dir;
        }
        void EnqueueTowards(Direction nextDir) =>
            toVisit.Enqueue((Add((y, x), DirToDelta(nextDir)), nextDir));
        var obstacle = board[y][x];
        if (obstacle == '.' ||
            (obstacle == '-' && dir is Direction.Left or Direction.Right) ||
            (obstacle == '|' && dir is Direction.Up or Direction.Down)) {
            EnqueueTowards(dir);
        } else if (obstacle == '-') {
            EnqueueTowards(Direction.Left);
            EnqueueTowards(Direction.Right);
        } else if (obstacle == '|') {
            EnqueueTowards(Direction.Up);
            EnqueueTowards(Direction.Down);
        } else if (obstacle == '\\') {
            EnqueueTowards(dir switch {
                Direction.Up => Direction.Left,
                Direction.Right => Direction.Down,
                Direction.Down => Direction.Right,
                Direction.Left => Direction.Up
            });
        } else if (obstacle == '/') {
            EnqueueTowards(dir switch {
                Direction.Up => Direction.Right,
                Direction.Right => Direction.Up,
                Direction.Down => Direction.Left,
                Direction.Left => Direction.Down
            });
        }
    }
    return visited.Count;
}


static (int y, int x) Add((int y, int x) p1, (int y, int x) p2) => (p1.y + p2.y, p1.x + p2.x);

static (int y, int x) DirToDelta(Direction d) => d switch {
    Direction.Up => (-1, 0),
    Direction.Right => (0, 1),
    Direction.Down => (1, 0),
    Direction.Left => (0, -1),
};

[Flags]
enum Direction {
    Up = 1 << 0,
    Right = 1 << 1,
    Down = 1 << 2,
    Left = 1 << 3
}