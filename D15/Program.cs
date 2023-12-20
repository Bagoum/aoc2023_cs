var steps = File.ReadAllText("data.txt").Split(",").ToArray();

Console.WriteLine(steps.Sum(Hash));

var boxes = Enumerable.Range(0, 256).Select(_ => new List<(string label, int size)>()).ToArray();
foreach (var step in steps) {
    var split = step.Split('-', '=');
    var box = Hash(split[0]);
    var existingIndex = boxes[box].FindIndex(ele => ele.label == split[0]);
    if (step.Contains('-') && existingIndex >= 0)
        boxes[box].RemoveAt(existingIndex);
    else if (step.Contains('='))
        if (existingIndex >= 0)
            boxes[box][existingIndex] = (split[0], int.Parse(split[1]));
        else
            boxes[box].Add((split[0], int.Parse(split[1])));
}
Console.WriteLine(Enumerable.Range(0, boxes.Length).Sum(i =>
    (i + 1) * Enumerable.Range(0, boxes[i].Count).Sum(j =>
        (j + 1) * boxes[i][j].size)));


static int Hash(string content) => content.Aggregate(0, (acc, x) =>
    ((acc + x) * 17) % 256);