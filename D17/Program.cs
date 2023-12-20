var board = File.ReadLines("data.txt")
    .Select(s => s.Select(c => c - '0').ToArray()).ToArray();
var h = board.Length;
var w = board[0].Length;

Console.WriteLine(MinimumTravel(0, 3));
Console.WriteLine(MinimumTravel(4, 10));

int MinimumTravel(int minStraight, int maxStraight) {
    var visited = new HashSet<((int, int), Direction, int)>();
    var toVisit = new PriorityQueue<((int, int), Direction, int), int>();
    toVisit.Enqueue(((0, 0), Direction.Right, 0), 0);
    toVisit.Enqueue(((0, 0), Direction.Down, 0), 0);
    while (toVisit.TryDequeue(out var pos, out var heat)) {
        var ((y, x), dir, straight) = pos;
        if (!visited.Add(pos))
            continue;
        if (y == h - 1 && x == h - 1 && straight >= minStraight)
            return heat;
        void MoveInDir(Direction d, int s) {
            var next = Add((y, x), DirToDelta(d));
            if (next.y < 0 || next.y >= h || next.x < 0 || next.x >= w) return;
            toVisit.Enqueue((next, d, s), heat+board[next.y][next.x]);
        }
        if (straight >= minStraight) 
            if (dir is Direction.Down or Direction.Up) {
                MoveInDir(Direction.Left, 1);
                MoveInDir(Direction.Right, 1);
            } else {
                MoveInDir(Direction.Up, 1);
                MoveInDir(Direction.Down, 1);
            }
        if (straight < maxStraight)
            MoveInDir(dir, straight + 1);
    }
    throw new Exception();
}
static (int y, int x) Add((int y, int x) p1, (int y, int x) p2) => (p1.y + p2.y, p1.x + p2.x);

static (int y, int x) DirToDelta(Direction d) => d switch {
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