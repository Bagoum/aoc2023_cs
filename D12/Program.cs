using System.Text.RegularExpressions;

var regex = new Regex(@"\d+");
var configs = File.ReadLines("data.txt").Select(s => {
    var split = s.Split(' ');
    return (data: split[0], nums: regex.Matches(split[1]).Select(x => int.Parse(x.Value)).ToArray());
}).ToArray();

Console.WriteLine(configs.Select(c => Matches(c.data, c.nums)).Sum());
Console.WriteLine(configs.Select(c => 
    Matches(string.Join('?', Enumerable.Repeat(c.data, 5)),
        Enumerable.Repeat(c.nums, 5).SelectMany(i => i).ToArray())).Sum());

long Matches(string data, int[] reqs) {
    Dictionary<(int, int, int), long> memo = new();
    long MatchesAt(int dataIndex, int reqIndex, int contiguous) {
        if (dataIndex >= data.Length) {
            if (contiguous == 0 && reqIndex == reqs.Length
                || contiguous == reqs[^1] && reqIndex + 1 == reqs.Length)
                return 1;
            return 0;
        }
        if (memo.TryGetValue((dataIndex, reqIndex, contiguous), out var res))
            return res;
        long Memoize(long result) => memo[(dataIndex, reqIndex, contiguous)] = result;
        var req = reqIndex >= reqs.Length ? -1 : reqs[reqIndex];
        long ResultsForEmpty() {
            if (contiguous == 0)
                return MatchesAt(dataIndex + 1, reqIndex, 0);
            if (contiguous == req)
                return MatchesAt(dataIndex + 1, reqIndex + 1, 0);
            return 0;
        }
        long ResultsForHash() =>
            contiguous >= req ?
                0 : MatchesAt(dataIndex + 1, reqIndex, contiguous + 1);
        return Memoize(data[dataIndex] switch {
            '.' => ResultsForEmpty(),
            '#' => ResultsForHash(),
            '?' => ResultsForEmpty() + ResultsForHash()
        });
    }
    return MatchesAt(0, 0, 0);
}