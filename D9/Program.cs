using System.Text.RegularExpressions;

var regex = new Regex(@"-?\d+");
var lines = File.ReadLines("data.txt").Select(l => regex.Matches(l).Select(m => long.Parse(m.Value)).ToList()).ToList();

Console.WriteLine(lines.Select(Extrapolate).Sum());
Console.WriteLine(lines.Select(l => Extrapolate((l as IEnumerable<long>).Reverse().ToList())).Sum());

long Extrapolate(List<long> values) {
    List<List<long>> pyramid = [values.ToList()];
    while (pyramid[^1] is var lastLayer && lastLayer.Any(x => x != 0))
        pyramid.Add(Enumerable.Range(0, lastLayer.Count - 1).Select(i => lastLayer[i + 1] - lastLayer[i]).ToList());
    pyramid[^1].Add(0);
    for (int ii = pyramid.Count - 2; ii >= 0; --ii)
        pyramid[ii].Add(pyramid[ii][^1] + pyramid[ii + 1][^1]);
    return pyramid[0][^1];
}