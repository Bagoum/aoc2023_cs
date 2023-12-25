var data = File.ReadAllLines("data.txt");
var moduleList = data.Select(line => {
    var key = line.Split(" -> ")[0];
    var module = key[0] switch {
        '%' => new Module.Flipper(key[1..]) as Module,
        '&' => new Module.Conjunct(key[1..]),
        _ => new Module.Named(key)
    };
    return (module.Name, module);
}).ToList();
var modules = moduleList.ToDictionary();
for (int ii = 0; ii < data.Length; ++ii) {
    foreach (var target in data[ii].Split(" -> ")[1].Split(", ")) {
        (modules.TryGetValue(target, out var m) ?
                m :
                (modules[target] = new Module.Named(target)))
            .AddReceivesFrom(moduleList[ii].Item2);
    }
}
modules["broadcaster"].AddReceivesFrom(modules["button"] = new Module.Named("button"));

var rawTotal = Enumerable.Range(0, 1000)
    .Select(_ => CountPulses(modules["button"], 0, out var _))
    .Aggregate(Add);
Console.WriteLine(rawTotal.Item1 * rawTotal.Item2);
Console.WriteLine(PulsesToSend(modules["rx"], 0));

long PulsesToSend(Module m, int sendsPulse) {
    if (m is Module.Named)
        return m.ReceivesFrom.Min(r => PulsesToSend(r, sendsPulse));
    //Assume that nested conjuncts are periodic, so for conjuncts A,B,C->X,
    //PulsesToSend(X, 0) = LCM{m in [A,B,C]}(PulsesToSend(m, 1))
    if (m is Module.Conjunct && m.ReceivesFrom.All(x => x is Module.Conjunct)) {
        return sendsPulse == 0 ?
            m.ReceivesFrom.Aggregate(1L, (acc, x) => {
                var pulses = PulsesToSend(x, 1);
                long gcdA = Math.Max(acc, pulses), gcdB = Math.Min(acc, pulses);
                while (gcdB > 0)
                    (gcdA, gcdB) = (gcdB, gcdA % gcdB);
                return acc * pulses / gcdA;
            }):
            m.ReceivesFrom.Min(x => PulsesToSend(x, 0));
    }
    foreach (var mod in modules.Values)
        mod.ResetState();
    for (var ii = 1;; ++ii) {
        CountPulses(m, sendsPulse, out var triggered);
        if (triggered)
            return ii;
    }
}

(int low, int high) CountPulses(Module watchSender, int watchPulse, out bool triggered) {
    var low = 0;
    var high = 0;
    triggered = false;
    var pulses = new Queue<(Module, Module, int)>();
    void Send(Module src, int type, ref bool triggered) {
        if (src == watchSender && type == watchPulse)
            triggered = true;
        foreach (var target in src.SendsTo)
            pulses.Enqueue((src, target, type));
    }
    Send(modules["button"], 0, ref triggered);
    while (pulses.Count > 0) {
        var (src, target, type) = pulses.Dequeue();
        if (type == 0)
            ++low;
        else
            ++high;
        if (target is Module.Flipper ff) {
            if (type == 0)
                Send(ff, ff.State = 1 - ff.State, ref triggered);
        } else if (target is Module.Conjunct cj) {
            cj.LastReceptions[src] = type;
            Send(cj, cj.LastReceptions.All(kv => kv.Value == 1) ? 0 : 1, ref triggered);
        } else {
            Send(target, type, ref triggered);
        }
    }
    return (low, high);
}

(int, int) Add((int a, int b) x, (int a, int b) y) => (x.a + y.a, x.b + y.b);

abstract class Module(string Name) {
    public string Name { get; } = Name;
    public List<Module> ReceivesFrom { get; } = new();
    public List<Module> SendsTo { get; } = new();

    public virtual void AddReceivesFrom(Module m) {
        ReceivesFrom.Add(m);
        m.SendsTo.Add(this);
    }

    public virtual void ResetState() { }

    public class Named(string Name) : Module(Name) { }

    public class Flipper(string Name) : Module(Name) {
        public int State { get; set; } = 0;
        public override void ResetState() => State = 0;
    }

    public class Conjunct(string Name) : Module(Name) {
        public Dictionary<Module, int> LastReceptions { get; } = new();

        public override void ResetState() {
            foreach (var k in LastReceptions.Keys)
                LastReceptions[k] = 0;
        }

        public override void AddReceivesFrom(Module m) {
            base.AddReceivesFrom(m);
            LastReceptions[m] = 0;
        }
    }
}