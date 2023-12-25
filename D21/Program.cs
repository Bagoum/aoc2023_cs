var data = File.ReadAllLines("data.txt");
var h = data.Length;
var w = data[0].Length;
var start = (y: 0, x: 0);
for (var row = 0; row < h; ++row)
for (var col = 0; col < w; ++col)
    if (data[row][col] == 'S')
        start = (row, col);

Console.WriteLine(ReachableInSteps(64, new[]{start}).Count());

var itrs = 26501365;
long n = itrs / h;
var offset = itrs % h;
var a = ReachableInSteps(2 * h + offset, new[]{start}).GroupBy(Bucket).ToDictionary(kv => kv.Key, kv=>(long)kv.Count());
/*
 Let the total number of steps be nh+offset. In this case, h=w=131 and n is even.
 After 2h+offset steps, we have the following blocks (top left is block index (-2,-2)):
 
  - A U B -
  A W O X B
  L O E O R
  C Y O Z D
  - C S D -
  
 E and O are fully accessible, so they are identical in all quadrants. The other categories are not
  fully accessible within the given number of steps and thus are different in each quadrant.

 Adding 2h more steps adds 8k E, 8k+4 O, 2 ABCD, 2 WXYZ, where the current number of steps is 2kh+offset.
 The total number of each type of block is:
 ULRS: 1
 ABCD: n
 WXYZ: n-1
 E: 1+(0+8+16+...8(n/2-1)) = n^2-2n+1
 O: 4+12+20+...(8(n/2-1)+4) = n^2
 */

Console.WriteLine(
    a[(-2,0)] + a[(2,0)] + a[(0,2)] + a[(0,-2)] + //ULRS
    n * (a[(-2,-1)] + a[(-2,1)] + a[(2,-1)] + a[(2,1)]) + //ABCD
    (n-1) * (a[(-1,-1)] + a[(-1,1)] + a[(1,-1)] + a[(1,1)]) + //WXYZ
    (n*n-2*n+1) * a[(0, 0)] + //E
    (n*n) * a[(1,0)] //O
);

(int, int) Bucket((int y, int x) loc) => ((int)Math.Floor(1.0 * loc.y / h), (int)Math.Floor(1.0 * loc.x / w));

int mod(int x, int around) => (x %= around) < 0 ? x + around : x;

bool IsSteppable((int y, int x) loc) => data[mod(loc.y, h)][mod(loc.x, w)] != '#';

IEnumerable<(int, int)> ReachableFrom((int y, int x) loc) => new[] {
        (loc.y - 1, loc.x),
        (loc.y + 1, loc.x),
        (loc.y, loc.x - 1),
        (loc.y, loc.x + 1)
    }.Where(IsSteppable);

IEnumerable<(int, int)> ReachableInSteps(int steps, IEnumerable<(int, int)> starts) {
    while (steps-- > 0)
        starts = starts.SelectMany(ReachableFrom).Distinct();
    return starts;
}