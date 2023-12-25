using Range = (int start, int end);

var data = File.ReadAllText("data.txt").ReplaceLineEndings("\n");
var dataSplit = data.Split("\n\n");
var gears = dataSplit[1].Split("\n").Select(l =>
    l[1..^1].Split(",").Select(scoreText => {
        var split = scoreText.Split("=");
        return (split[0], int.Parse(split[1]));
    }).ToDictionary()).ToList();
var workflows = dataSplit[0].Split("\n").Select(l => {
    var split = l.Split("{");
    return (split[0], split[1][..^1].Split(",").Select(rule => {
        if (rule.Contains(':')) {
            split = rule.Split(":");
            var result = split[1];
            var cond = split[0].Split('>', '<');
            return ((cond[0], split[0].Contains('>') ? 1 : -1, int.Parse(cond[1])), result);
        }
        return (((string cond, int cmp, int cmpTo)?)null, rule);
    }).ToList());
}).ToDictionary();


Console.WriteLine(gears.Where(g => ApproveGear(g)).Sum(g => g.Values.Sum()));
Console.WriteLine(ApproveGearRange(new() {
    { "x", (1, 4000) },
    { "m", (1, 4000) },
    { "a", (1, 4000) },
    { "s", (1, 4000) }
}).Sum(satisfy => satisfy.Values.Aggregate(1L, (acc, rg) => acc * (rg.end - rg.start + 1))));

bool ApproveGear(Dictionary<string, int> gear, string workflow = "in") {
    if (workflow == "A")
        return true;
    if (workflow == "R")
        return false;
    foreach (var rule in workflows[workflow]) {
        if (rule.Item1 is null)
            return ApproveGear(gear, rule.rule);
        var (cond, cmp, cmpTo) = rule.Item1.Value;
        if (cmp == gear[cond].CompareTo(cmpTo))
            return ApproveGear(gear, rule.rule);
    }
    return false;
}

IEnumerable<Dictionary<string, Range>> ApproveGearRange(Dictionary<string, Range>? gear, string workflow = "in") {
    if (workflow == "R" || gear is null)
        yield break;
    if (workflow == "A") {
        yield return gear;
        yield break;
    }
    foreach (var (maybeCond, target) in workflows[workflow]) {
        if (gear is null)
            break;
        if (maybeCond is null) {
            foreach (var v in ApproveGearRange(gear.ToDictionary(), target))
                yield return v;
        } else {
            var (cond, cmp, cmpTo) = maybeCond.Value;
            var score = gear[cond];
            var satisfy = gear.ToDictionary();
            if (cmp > 0) {
                if (score.end <= cmpTo)
                    satisfy = null;
                else if (score.start > cmpTo)
                    gear = null;
                else {
                    satisfy[cond] = (cmpTo + 1, score.end);
                    gear[cond] = (score.start, cmpTo);
                }
            } else {
                if (score.start >= cmpTo)
                    satisfy = null;
                else if (score.end < cmpTo)
                    gear = null;
                else {
                    satisfy[cond] = (score.start, cmpTo - 1);
                    gear[cond] = (cmpTo, score.end);
                }
            }
            foreach (var v in ApproveGearRange(satisfy, target))
                yield return v;
        }
    }
}