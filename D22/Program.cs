using System.Text.RegularExpressions;
using Pos = (int x, int y, int z);
var r = new Regex(@"\d+");

List<Range> settled = new();
List<List<int>> leanedOnBy = new();
List<List<int>> leansOn = new();
foreach (var b in File.ReadLines("data.txt").Select(line => {
             var m = r.Matches(line);
             return new Range(
                 (int.Parse(m[0].Value), int.Parse(m[1].Value), int.Parse(m[2].Value)),
                 (int.Parse(m[3].Value), int.Parse(m[4].Value), int.Parse(m[5].Value))
             );
         }).OrderBy(b => b.start.z)) {
    var zWall = 0;
    var thisLeansOn = new List<int>();
    for (int ii = settled.Count - 1; ii >= 0; --ii) {
        if (b.XYIntersect(settled[ii])) {
            var intersectZ = settled[ii].end.z;
            if (intersectZ < zWall) 
                continue;
            if (intersectZ > zWall)
                thisLeansOn.Clear();
            zWall = intersectZ;
            thisLeansOn.Add(ii);
        }
    }
    foreach (var idx in thisLeansOn)
        leanedOnBy[idx].Add(settled.Count);
    settled.Add(b.Fall(zWall));
    leanedOnBy.Add([]);
    leansOn.Add(thisLeansOn);
}

Console.WriteLine(Enumerable.Range(0, settled.Count).Count(bi => leanedOnBy[bi].All(bj => leansOn[bj].Count > 1)));
Console.WriteLine(Enumerable.Range(0, settled.Count).Sum(bi => {
    var dependents = new List<int>() { bi };
    for (int ii = 0; ii < dependents.Count; ++ii) {
        dependents.AddRange(
            leanedOnBy[dependents[ii]].Where(jj => !dependents.Contains(jj) 
                                                   && leansOn[jj].Except(dependents).Count() == 0));
    }
    return dependents.Count - 1;
}));

record Range(Pos start, Pos end) {
    public bool XYIntersect(Range other) =>
        ((start.x >= other.start.x && start.x <= other.end.x) ||
         (end.x >= other.start.x && end.x <= other.end.x) ||
         (start.x <= other.start.x && end.x >= other.end.x)) &&
        ((start.y >= other.start.y && start.y <= other.end.y) ||
         (end.y >= other.start.y && end.y <= other.end.y) ||
         (start.y <= other.start.y && end.y >= other.end.y));

    public Range Fall(int zWall) {
        var dist = start.z - zWall - 1;
        return new((start.x, start.y, start.z - dist), (end.x, end.y, end.z - dist));
    }
}