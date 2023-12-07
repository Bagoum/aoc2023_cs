int PossibleWins(params (int time, long distance)[] records) => records.Select(g =>
        Enumerable.Range(0, g.time)
            .Count(t => t * (long)(g.time - t) > g.distance))
    .Aggregate(1, (x, y) => x * y);

Console.WriteLine(PossibleWins((57, 291), (72, 1172), (69, 1176), (92, 2026)));
Console.WriteLine(PossibleWins((57726992, 291117211762026)));