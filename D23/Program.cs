using Pos = (int y, int x);

var data = File.ReadAllLines("data.txt");
var h = data.Length;
var w = data[0].Length;
(int dy, int dx)[] AllDeltas = [(0, 1), (0, -1), (-1, 0), (1, 0)];

Console.WriteLine(FindLongestPathInGraph(MakeGraph(true).outgoing));

var (nsout, nsin) = MakeGraph(false);
//If a vertex V has exactly two outgoing/incoming edges (in the structure VA <-> V <-> VB),
// then we delete the edges VA<->V,V<->VB and replace them with edges VA<->VB (using combined weight).
foreach (var v in nsout.Keys.ToList()) {
    if (nsout[v].Count == 2 && nsin[v].Count == 2) {
        var ea = nsout[v][0];
        var eb = nsout[v][1];
        nsout[ea.to].Remove(ea.Reverse);
        nsout[eb.to].Remove(eb.Reverse);
        nsin[ea.to].Remove(ea);
        nsin[eb.to].Remove(eb);
        nsout.Remove(v);
        nsin.Remove(v);
        var aToB = new Edge(ea.to, eb.to, ea.weight + eb.weight);
        nsout[ea.to].Add(aToB);
        nsin[ea.to].Add(aToB.Reverse);
        nsout[eb.to].Add(aToB.Reverse);
        nsin[eb.to].Add(aToB);
    }
}
Console.WriteLine(FindLongestPathInGraph(nsout));

(Dictionary<Pos, List<Edge>> outgoing, Dictionary<Pos, List<Edge>> incoming) MakeGraph(bool restrictSlopes) {
    Dictionary<Pos, List<Edge>> outgoing = new();
    Dictionary<Pos, List<Edge>> incoming = new();
    void AddEdge(Pos from, Pos to) {
        if (!outgoing.TryGetValue(from, out var ses))
            outgoing[from] = ses = new();
        if (!incoming.TryGetValue(to, out var tos))
            incoming[to] = tos = new();
        var e = new Edge(from, to, 1);
        ses.Add(e);
        tos.Add(e);
    }
    for (var y = 0; y < h; ++y)
    for (var x = 0; x < w; ++x) {
        foreach (var delta in data[y][x] switch {
                     '>' when restrictSlopes => new[] { AllDeltas[0] },
                     '<' when restrictSlopes => [AllDeltas[1]],
                     '^' when restrictSlopes => [AllDeltas[2]],
                     'v' when restrictSlopes => [AllDeltas[3]],
                     '#' => [],
                     _ => AllDeltas
                 }) {
            int ny  = y + delta.dy, nx = x + delta.dx;
            if (ny < 0 || ny >= h || nx < 0 || nx >= w || data[ny][nx] == '#')
                continue;
            AddEdge((y, x), (ny, nx));
        }
    }
    return (outgoing, incoming);
}
int FindLongestPathInGraph(Dictionary<Pos, List<Edge>> outgoing) {
    //This is a DFS algorithm where we store the recursive stack in a data structure (to avoid stack overflow)
    var fringe = new Stack<int>(); //int continueFrom: index of the next child that should be examined
    fringe.Push(0);
    var path = new List<Edge>() { new((-1,-1), (0,1), 0) };
    var set = new HashSet<Pos>() {path[0].to};
    int maxPathLength = 0;
    while (fringe.TryPop(out var continueFrom)) {
        var last = path[^1].to;
        if (last == (h - 1, w - 2)) {
            maxPathLength = Math.Max(maxPathLength, path.Sum(e => e.weight));
            goto ascend_level;
        }
        var edges = outgoing[last];
        for (int ii = continueFrom; ii < edges.Count; ++ii) {
            if (!set.Add(edges[ii].to))
                continue;
            path.Add(edges[ii]);
            fringe.Push(ii + 1); //Store current level
            fringe.Push(0); //Descend a level (with edge appended to path)
            goto examine_next;
        }
        ascend_level : ;
        set.Remove(last);
        path.RemoveAt(path.Count - 1);
        examine_next: ;
    }
    return maxPathLength;
}

record Edge(Pos from, Pos to, int weight) {
    public Edge Reverse => this with { from = to, to = from };
}