using Hand = (HandType type, string full);

var handTypesByScore = Enum.GetValues<HandType>().OrderDescending().ToList();
string cardsByScore(bool wildcard) => wildcard ? "AKQT98765432J" : "AKQJT98765432";

Console.WriteLine(GetWinnings(false));
Console.WriteLine(GetWinnings(true));

long GetWinnings(bool wildcard) => 
    File.ReadLines("data.txt").Select(l => {
        var split = l.Split(' ');
        return (hand: Analyze(split[0], wildcard), gamble: int.Parse(split[1]));
    }).OrderBy(x => x.hand, Comparer<Hand>.Create((a, b) => {
        if (a.type != b.type)
            return a.type - b.type;
        for (int ii = 0; ii < a.full.Length; ++ii)
            if (a.full[ii] != b.full[ii])
                return cardsByScore(wildcard).IndexOf(b.full[ii]) - cardsByScore(wildcard).IndexOf(a.full[ii]);
        return 0;
    })).Select((x, i) => x.gamble * (long)(i + 1)).Sum();

Hand Analyze(string hand, bool wildcard) => 
    (from hType in handTypesByScore 
        from cType in cardsByScore(wildcard) 
        where Matches(hType, cType, hand, wildcard) 
        select (hType, hand)).First();

bool Matches(HandType type, char card, string hand, bool wildcard) {
    return type switch {
        HandType.High => hand.Count(Pred) == 1,
        HandType.Pair => hand.Count(Pred) == 2,
        HandType.TwoPair => hand.Count(Pred) == 2 && 
                            //Note that this is strictly speaking not accurate as it "overfilters" wildcards.
                            //For example, 2JJ34 is a two-pair for leader 4, but the check below will return false,
                            //since it filters out JJ4 in the nested call.
                            //However, this doesn't matter, as in any such cases, there is a higher priority
                            // hand type (three kind > two pair, four kind > full house) fitting the overfiltering.
                            cardsByScore(wildcard).Any(card2 => card2 != card && Matches(HandType.Pair, card2, 
                                new string(hand.Where(c => !Pred(c)).ToArray()), wildcard)),
        HandType.ThreeKind => hand.Count(Pred) == 3,
        HandType.FullHouse => hand.Count(Pred) == 3 && 
                              cardsByScore(wildcard).Any(card2 => card2 != card && Matches(HandType.Pair, card2, 
                                  new string(hand.Where(c => !Pred(c)).ToArray()), wildcard)),
        HandType.FourKind => hand.Count(Pred) == 4,
        HandType.FiveKind => hand.Count(Pred) == 5,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
    bool Pred(char c) => c == card || (wildcard && c == 'J');
}

enum HandType {
    High = 0,
    Pair = 1,
    TwoPair = 2,
    ThreeKind = 3,
    FullHouse = 4,
    FourKind = 5,
    FiveKind = 6
}
