using System.Text.RegularExpressions;

string[] colors = ["red", "green", "blue"];
var r = new Regex($"(\\d+) ({string.Join('|', colors)})");

var games = File.ReadLines("data.txt").Select(l => {
    var split = l.Split(':', ';');
    var id = int.Parse(split[0].Split(' ')[^1]);
    var turns = split.Skip(1).Select(turn => r.Matches(turn)
            .Select(m => (m.Groups[2].Value, int.Parse(m.Groups[1].ValueSpan)))
            .ToDictionary());
    return (id, turns);
}).ToList();

var legalGames = games.Where(game =>
    game.turns.All(d => !(MaybeGet(d, "red") > 12 || MaybeGet(d, "green") > 13 || MaybeGet(d, "blue") > 14)));
Console.WriteLine(legalGames.Sum(g => g.id));

var gamePowers = games.Select(game =>
    colors.Select(color => game.turns.Max(t => MaybeGet(t, color) ?? 0))
        .Aggregate(1, (x, y) => x * y));
Console.WriteLine(gamePowers.Sum());

V? MaybeGet<K, V>(Dictionary<K, V> dict, K key) where K : notnull where V : struct =>
    dict.TryGetValue(key, out var ret) ? ret : null;