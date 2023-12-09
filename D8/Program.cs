using System.Text.RegularExpressions;

var directions = "LLRRRLRLLRLRRLRLRLRRRLLRRLRRRLRRRLRRRLRRRLRRRLRRLRLLRRRLRRLLRLRLLLRRLRRLRLRLRLRRRLRLRRRLRRLLLRRRLLRRLLRRLLRRRLLLLRLRLRRRLRLRRRLRLLLRLRRLRRRLRRRLRRRLRRRLLRRLLLLRRLLRRLLRRLRLRRRLRRRLRRRLRRLRRRLRRLRRLRRLRLRRRLRRLRRRLRRRLRRLRLRRRLRRLLRLRRLRRRLRLRRLRRRLRRLRRLRRRLLRRRR";

var regex = new Regex(@"(\S+) = \((\S+), (\S+)\)");
Dictionary<string, (string left, string right)> map = 
    regex.Matches(File.ReadAllText("data.txt"))
        .Select(m => (m.Groups[1].Value, (m.Groups[2].Value, m.Groups[3].Value)))
        .ToDictionary();

int StepsRequired(string start, string end) {
    string position = start;
    Dictionary<string, List<int>> observations = new();
    for (int steps = 0;; ++steps) {
        if (position == end)
            return steps;
        if (!observations.TryGetValue(position, out var observedAtSteps))
            observedAtSteps = observations[position] = [];
        if (observedAtSteps.Any(s => (steps - s) % directions.Length == 0))
            return -1;
        observedAtSteps.Add(steps);
        var (l, r) = map[position];
        position = directions[steps % directions.Length] == 'L' ? l : r;
    }
}
Console.WriteLine(StepsRequired("AAA", "ZZZ"));

var starts = map.Keys.Where(p => p.EndsWith('A')).ToArray();
var ends = map.Keys.Where(p => p.EndsWith('Z')).ToArray();
var timesToZ = starts.Select(s => 
    ends.Select(e => StepsRequired(s, e)).Where(x => x >= 0).ToArray()).ToArray();

//It turns out that each source can only reach one destination. Thus, no Cartesian product logic is necessary.
//Otherwise, we'd need to do this LCM calculation
// for all possible tuples in the Cartesian product of source->target pairings.
Console.WriteLine(timesToZ.Select(times => (long)times[0]).Aggregate((x, y) => {
    long gcdA = Math.Max(x, y), gcdB = Math.Min(x, y);
    while (gcdB > 0)
        (gcdA, gcdB) = (gcdB, gcdA % gcdB);
    return x * y / gcdA;
}));
