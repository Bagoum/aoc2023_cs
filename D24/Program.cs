using System.Numerics;
using System.Text.RegularExpressions;

var regex = new Regex(@"-?\d+");

var data = File.ReadLines("data.txt")
    .Select(line => regex.Matches(line).Select(m => long.Parse(m.Value)).ToList()).ToList();

var xyHail = data.Select(line => new Path(new(line[0], line[1]), new(line[3], line[4]))).ToList();

long collisions = 0;
for (int ii = 0; ii < xyHail.Count; ++ii) {
    var a = xyHail[ii];
    var da = a.End - a.Start;
    for (int jj = ii + 1; jj < xyHail.Count; ++jj) {
        var b = xyHail[jj];
        var db1 = b.Start - a.Start;
        var db2 = b.End - a.Start;
        var proj1 = Vector2.Dot(da, db1) / da.LengthSquared() * da;
        var oproj1 = db1 - proj1;
        var proj2 = Vector2.Dot(da, db2) / da.LengthSquared() * da;
        var oproj2 = db2 - proj2;
        //Verify that b crosses the *ray* a.Start->a.End
        if (Signs(oproj1) == Signs(oproj2))
            continue;
        //Verify that b crosses the *line segment* a.Start-a.End
        var t = oproj1.Length() / (oproj1.Length() + oproj2.Length());
        var intersect = Vector2.Lerp(db1, db2, t);
        if (Vector2.Dot(intersect, da) <= 0 || intersect.LengthSquared() > da.LengthSquared())
            continue;
        //Verify that the actual intersection is within the zone
        var realIntersect = intersect + a.Start;
        if (realIntersect.X < Path.minLoc || realIntersect.X > Path.maxLoc || 
            realIntersect.Y < Path.minLoc || realIntersect.Y > Path.maxLoc)
            continue;
        ++collisions;
    }
}
Console.WriteLine(collisions);


//Find axis-specific velocities that recur across multiple hailstones.
// For Hi = <Six,Siy,Siz> + <Vix,Viy,Viz>t, colliding with the rock at time Ti,
// if Vix is shared among some set of hailstones,
// then the rock's x-velocity (Vrx) must be one of Vix +- (Sjx-Six)/(Tj-Ti).
//Since Vrx,Vix,Sjx,Six are integerial, (Tj-Ti) is constrained to be one of the divisors of (Sjx-Six).
//(Actually, on revision, this isn't generally true, but it just so happens to work with the input.)
HashSet<long> ValidVelocities(int axis) {
    HashSet<long>? velocities = null;
    bool UpdateVelocities(IEnumerable<long> possible) {
        if (velocities is null) 
            velocities = possible.ToHashSet();
        else 
            velocities.IntersectWith(possible);
        return velocities.Count == 1;
    }
    foreach (var velGroup in data.Select(l => (pos: l[axis], vel: l[axis + 3])).GroupBy(x => x.vel)) {
        var stones = velGroup.ToArray();
        for (int ii = 0; ii < stones.Length; ++ii) {
            for (int jj = ii + 1; jj < stones.Length; ++jj) {
                var locDelta = Math.Abs(stones[jj].pos - stones[ii].pos);
                List<long> possibleVels = new();
                for (long factor = 1; factor * factor <= locDelta; ++factor) {
                    if (locDelta % factor == 0) {
                        possibleVels.Add(velGroup.Key + locDelta / factor);
                        possibleVels.Add(velGroup.Key - locDelta / factor);
                        possibleVels.Add(velGroup.Key + factor);
                        possibleVels.Add(velGroup.Key - factor);
                    }
                }
                if (UpdateVelocities(possibleVels))
                    return velocities!;  //early exit for optimization
            }
        }
    }
    return velocities!;
}
var xvs = ValidVelocities(0);
var yvs = ValidVelocities(1);
var zvs = ValidVelocities(2);
foreach (var vrx in xvs)
foreach (var vry in yvs)
foreach (var vrz in zvs) {
    var a = data[0];
    var b = data[1];
    //Create a system of equations using Sr + (Vr-Vi)Ti = Si for two stones
    //The variables are Srx, Sry, Srz, Ta, Tb
    if (SolveMatrix(new decimal[,] {
        { 1, 0, 0, vrx - a[3], 0, a[0] }, //1*Srx + 0*Sry + 0*Srz + (vrx-va)*Ta + 0*Tb = Sax
        { 0, 1, 0, vry - a[4], 0, a[1] },
        { 0, 0, 1, vrz - a[5], 0, a[2] },
        { 1, 0, 0, 0, vrx - b[3], b[0] },
        { 0, 1, 0, 0, vry - b[4], b[1] },
        { 0, 0, 1, 0, vrz - b[5], b[2] },
    }) is [var srx, var sry, var srz, ..]) {
        //Check the determined value of Sr and Vr against all stones, using Ti = (Sr-Si)/(Vi-Vr) = ds/dv
        foreach (var stone in data) {
            var dsx = srx - stone[0];
            var dvx = stone[3] - vrx;
            var dsy = sry - stone[1];
            var dvy = stone[4] - vry;
            var dsz = srz - stone[2];
            var dvz = stone[5] - vrz;
            if (Math.Abs(dsx*dvy-dsy*dvx) > 0.000001m || 
                Math.Abs(dsy*dvz-dsz*dvy) > 0.000001m || 
                Math.Abs(dsx*dvz-dsz*dvx) > 0.000001m) {
                goto next_vel_test;
            }
        }
        Console.WriteLine(Math.Round(srx+sry+srz));
        return;
    }
    next_vel_test: ;
}

//Two-pass gaussian elimination for an augmented matrix
decimal[]? SolveMatrix(decimal[,] matrix) {
    void AddRow(int src, int target, decimal multiplier) {
        if (multiplier == 0) return;
        for (int i = 0; i < matrix.GetLength(1); ++i)
            matrix[target,i] += multiplier * matrix[src,i];
    }
    void MulRow(int row, decimal multiplier) {
        for (int i = 0; i < matrix.GetLength(1); ++i)
            matrix[row,i] *= multiplier;
    }
    for (int ii = 0; ii < matrix.GetLength(1) - 1; ++ii) {
        for (int jj = ii + 1; jj < matrix.GetLength(1); ++jj) {
            AddRow(ii, jj, -matrix[jj,ii] / matrix[ii,ii]);
        }
    }
    int ir = matrix.GetLength(0) - 1;
    //Overdetermined rows
    for (; ir >= matrix.GetLength(1) - 1; --ir) {
        for (int col = 0; col < matrix.GetLength(1); ++col)
            if (Math.Abs(matrix[ir,col]) > 0.0000000001m)
                return null;
    }
    for (; ir >= 0; --ir) {
        MulRow(ir, 1.0m/matrix[ir,ir]);
        for (int jj = ir - 1; jj >= 0; --jj) {
            AddRow(ir, jj, -matrix[jj,ir]);
        }
    }
    return Enumerable.Range(0, matrix.GetLength(0)).Select(i => matrix[i, matrix.GetLength(1) - 1]).ToArray();
}

Vector2 Signs(Vector2 a) => new(Math.Sign(a.X), Math.Sign(a.Y));

record Path {
    public const long minLoc = 200000000000000, maxLoc = 400000000000000;
    public Vector2 Start { get; }

    public Vector2 End { get; }
    public Vector2 Vel { get; }
    public float Time { get; }
    public Path(Vector2 start, Vector2 vel) {
        this.Start = start;
        this.Vel = vel;
        Time = Math.Min(TimeOnAxis(Start.X, vel.X), TimeOnAxis(Start.Y, vel.Y));
        End = Start + Time * Vel;
    }


    private static float TimeOnAxis(float start, float vel) =>
    vel switch {
        > 0 => maxLoc >= start ? (maxLoc - start) / vel : float.PositiveInfinity,
        < 0 => minLoc <= start ? (minLoc - start) / vel : float.PositiveInfinity,
        _ => float.PositiveInfinity
    };
}
    