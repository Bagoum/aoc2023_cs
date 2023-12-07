string[] numNames = { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
var lines = File.ReadLines("data.txt").ToList();

var result1 = lines.Sum(l => {
        var nums = l.Where(char.IsDigit).ToList();
        return 10 * (nums[0] - '0') + (nums[^1] - '0');
    });
Console.WriteLine(result1);

var result2 = lines.Sum(l => {
        var ns = Enumerable.Range(0, l.Length).Select(i => {
                var substr = l.AsSpan()[i..];
                for (int j = 0; j < numNames.Length; ++j)
                    if (substr.StartsWith(numNames[j]))
                        return j + 1;
                return char.IsDigit(substr[0]) ? substr[0] - '0' : (int?)null;
            }).Where(x => x != null).ToList();
        return 10 * ns[0]!.Value + ns[^1]!.Value;
    });
Console.WriteLine(result2);
