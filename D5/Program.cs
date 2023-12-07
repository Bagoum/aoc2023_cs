long[] seeds = {
    41218238, 421491713, 1255413673, 350530906, 944138913, 251104806, 481818804, 233571979, 2906248740, 266447632,
    3454130719, 50644329, 1920342932, 127779721, 2109326496, 538709762, 3579244700, 267233350, 4173137165, 60179884
};

var maps = File.ReadAllText("maps.txt").ReplaceLineEndings()
    .Split(Environment.NewLine + Environment.NewLine)
    .Select(mapData => mapData.Split(Environment.NewLine).Skip(1).Select(l => {
        var arr = l.Split(' ').Select(long.Parse).ToList();
        return (arr[0], arr[1], arr[2]);
    }).ToList()).ToArray();

var locations = seeds.Select(s => maps.Aggregate(s, DoMap));
Console.WriteLine(locations.Min());

var locationRanges = maps.Aggregate(seeds.Chunk(2).Select(p => (p[0], p[0] + p[1])), DoMonadicMap);
Console.WriteLine(locationRanges.Min(l => l.Item1));

long DoMap(long val, List<(long dst, long src, long len)> map) {
    foreach (var (dst, src, len) in map)
        if (val >= src && val < src + len)
            return dst + (val - src);
    return val;
}

IEnumerable<(long, long)> DoMonadicMap(IEnumerable<(long, long)> ranges, List<(long dst, long src, long len)> map) {
    var compressed = new List<(long, long)>();
    foreach (var (start, end) in ranges.OrderBy(r => r.Item1))
        if (compressed.Count == 0 || compressed[^1].Item2 < start)
            compressed.Add((start, end));
        else
            compressed[^1] = (compressed[^1].Item1, end);
    var unprocessed = new Queue<(long, long)>(compressed);
    while (unprocessed.Count > 0) {
        var (start, end) = unprocessed.Dequeue();
        foreach (var (dst, src, len) in map) {
            if (start >= src && start < src + len) {
                if (end <= src + len) {
                    yield return (dst + start - src, dst + end - src);
                } else {
                    yield return (dst + start - src, dst + len);
                    unprocessed.Enqueue((src + len, end));
                }
                goto found_split;
            }
            if (start < src && end > src) {
                if (end <= src + len) {
                    yield return (dst, dst + end - src);
                    unprocessed.Enqueue((start, src));
                } else {
                    yield return (dst, dst + len);
                    unprocessed.Enqueue((start, src));
                    unprocessed.Enqueue((src + len, end));
                }
                goto found_split;
            }
        }
        yield return (start, end);
        found_split: ;
    }
}