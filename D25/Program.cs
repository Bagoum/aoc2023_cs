var vertices = new Dictionary<string, Vertex>();
Vertex GetOrAddVertex(string name) => vertices.TryGetValue(name, out var v) ? v : vertices[name] = new(name);
foreach (var line in File.ReadLines("data.txt")) {
    var split = line.Split(' ');
    var v1 = GetOrAddVertex(split[0][..^1]);
    foreach (var v2 in split.Skip(1).Select(GetOrAddVertex)) {
        v1.Outgoing[v2] = new Edge(v1, v2);
        v2.Outgoing[v1] = new Edge(v2, v1);
    }
}

var source = vertices.Values.First();
foreach (var sink in vertices.Values) {
    if (sink == source) continue;
    var (f, ef) = MaximumFlow(source, sink);
    if (f != 3) continue;
    var reachable = ReachableNodes(source, ef.Keys.Where(e => ef[e] > 0).ToHashSet());
    Console.WriteLine(reachable * (vertices.Count - reachable));
    return;
}

(int, Dictionary<Edge, int>) MaximumFlow(Vertex source, Vertex sink) {
    var totalFlow = 0;
    var edgeFlow = new Dictionary<Edge, int>();
    int CurrentFlow(Edge e) => edgeFlow.GetValueOrDefault(e, 0);
    while (true) {
        var augmentingPathTo = new Dictionary<Vertex, Edge>();
        var toVisit = new Queue<Vertex>();
        toVisit.Enqueue(source);
        augmentingPathTo[source] = null!;
        while (toVisit.TryDequeue(out var nxt) && !augmentingPathTo.ContainsKey(sink))
            foreach (var e in nxt.Outgoing.Values.Where(e => CurrentFlow(e) <= 0 && augmentingPathTo.TryAdd(e.To, e)))
                toVisit.Enqueue(e.To);
        if (augmentingPathTo.TryGetValue(sink, out var edge)) {
            for (var e = edge; e != null!; e = augmentingPathTo[e.From]) {
                edgeFlow[e] = CurrentFlow(e) + 1;
                edgeFlow[e.To.Outgoing[e.From]] = CurrentFlow(e.To.Outgoing[e.From]) - 1;
            }
            totalFlow += 1;
        } else return (totalFlow, edgeFlow);
    }
}

int ReachableNodes(Vertex source, HashSet<Edge> dontUse) {
    var visited = new HashSet<Vertex>();
    var toVisit = new Queue<Vertex>();
    toVisit.Enqueue(source);
    while (toVisit.TryDequeue(out var nxt)) {
        if (visited.Add(nxt))
            foreach (var e in nxt.Outgoing.Values.Except(dontUse))
                toVisit.Enqueue(e.To);
    }
    return visited.Count;
}


record Vertex(string Name) {
    public Dictionary<Vertex, Edge> Outgoing { get; } = new();
}

record Edge(Vertex From, Vertex To);