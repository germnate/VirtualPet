public sealed class MosslightHollowService
{
    private const string DenClearing = "den-clearing";
    private const string LanternNook = "lantern-nook";
    private const string WhisperingBrook = "whispering-brook";
    private const string BrambleGate = "bramble-gate";
    private const string SunlitGlade = "sunlit-glade";

    private readonly object _syncRoot = new();
    private readonly Dictionary<string, Scene> _scenes;

    private StoryState _state = new();

    public MosslightHollowService()
    {
        _scenes = new Dictionary<string, Scene>(StringComparer.OrdinalIgnoreCase)
        {
            [DenClearing] = new(
                DenClearing,
                "Den Clearing",
                "A snug den rests beneath a bent cedar tree. Fireflies drift above three paths that lead north, east, and west.",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["north"] = LanternNook,
                    ["east"] = WhisperingBrook,
                    ["west"] = BrambleGate,
                }),
            [LanternNook] = new(
                LanternNook,
                "Lantern Nook",
                "A low branch cradles an old brass lantern. Moss here glows faintly, as if waiting for someone brave enough to carry a little light.",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["south"] = DenClearing,
                }),
            [WhisperingBrook] = new(
                WhisperingBrook,
                "Whispering Brook",
                "A narrow brook chatters over smooth stones. A reed sign points toward a tiny stone bridge puzzle beside the water.",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["west"] = DenClearing,
                }),
            [BrambleGate] = new(
                BrambleGate,
                "Bramble Gate",
                "A round iron gate is woven through with gentle brambles. A brass keyhole glints at its center, and beyond it you can glimpse a bright glade.",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["east"] = DenClearing,
                    ["north"] = SunlitGlade,
                }),
            [SunlitGlade] = new(
                SunlitGlade,
                "Sunlit Glade",
                "Golden grass sways in a ring of warm light. You have reached the end of this first Mosslight Hollow preview.",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["south"] = BrambleGate,
                }),
        };
    }

    public StoryResponse GetOpening()
    {
        lock (_syncRoot)
        {
            var sceneResponse = BuildCurrentSceneResponse();

            return new StoryResponse(
                "Welcome to Mosslight Hollow. Small choices matter here, so look closely and trust your curiosity.\n\n"
                + sceneResponse.Reply
                + "\n\nType 'help' if you want a quick list of useful commands.",
                sceneResponse.SceneName,
                sceneResponse.SceneId);
        }
    }

    public StoryResponse ProcessCommand(string input)
    {
        lock (_syncRoot)
        {
            var normalized = Normalize(input);

            if (string.IsNullOrWhiteSpace(normalized))
            {
                return new StoryResponse("Tell the story what you want to do first.");
            }

            if (normalized is "help" or "commands" or "?")
            {
                return new StoryResponse(BuildHelpText());
            }

            if (normalized is "look" or "look around")
            {
                return BuildCurrentSceneResponse();
            }

            if (normalized is "inventory" or "bag" or "items")
            {
                return new StoryResponse(DescribeInventory());
            }

            if (TryGetDirection(normalized, out var direction))
            {
                return Move(direction);
            }

            if (normalized.StartsWith("take ", StringComparison.Ordinal))
            {
                return new StoryResponse(TakeItem(normalized[5..]));
            }

            if (normalized.StartsWith("examine ", StringComparison.Ordinal)
                || normalized.StartsWith("inspect ", StringComparison.Ordinal)
                || normalized.StartsWith("read ", StringComparison.Ordinal))
            {
                var target = normalized.Split(' ', 2, StringSplitOptions.TrimEntries)[1];
                return new StoryResponse(Examine(target));
            }

            if (normalized.StartsWith("use ", StringComparison.Ordinal))
            {
                return new StoryResponse(UseItem(normalized[4..]));
            }

            if (normalized.StartsWith("answer ", StringComparison.Ordinal)
                || normalized.StartsWith("solve ", StringComparison.Ordinal))
            {
                var answer = normalized.Split(' ', 2, StringSplitOptions.TrimEntries)[1];
                return new StoryResponse(AnswerPuzzle(answer));
            }

            if (normalized is "open gate" or "unlock gate")
            {
                return new StoryResponse(UnlockGate());
            }

            return new StoryResponse(
                "The hollow does not understand that yet. Try commands like 'look', 'go east', 'take lantern', 'answer 7', or 'inventory'.");
        }
    }

    private StoryResponse BuildCurrentSceneResponse()
    {
        var scene = _scenes[_state.CurrentSceneId];
        var details = scene.Description;
        var sceneHint = GetSceneHint(scene.Id);
        var visibleItems = GetVisibleItems(scene.Id).ToList();
        var exits = GetVisibleExits(scene.Id).ToList();

        var reply = details;

        if (!string.IsNullOrWhiteSpace(sceneHint))
        {
            reply += $"\n\n{sceneHint}";
        }

        if (visibleItems.Count > 0)
        {
            reply += $"\n\nYou notice: {string.Join(", ", visibleItems)}.";
        }

        reply += $"\n\nExits: {string.Join(", ", exits)}.";
        return new StoryResponse(reply, scene.Name, scene.Id);
    }

    private string GetSceneHint(string sceneId)
    {
        return sceneId switch
        {
            LanternNook when !_state.Inventory.Contains("lantern") => "The lantern looks light enough to carry.",
            WhisperingBrook when !_state.BrookPuzzleSolved => "Three stones show the numbers 3, 4, and ?. The reed sign asks, 'What is the sum of the first two stones?'",
            WhisperingBrook when _state.BrookPuzzleSolved && !_state.Inventory.Contains("brass key") => "The safe path glows. A small brass key gleams in a shell beside the water.",
            BrambleGate when !_state.GateUnlocked => "The gate is locked. If you find the right key, try 'use brass key on gate'.",
            BrambleGate when _state.GateUnlocked => "The gate stands open, and the path north is clear.",
            SunlitGlade => "You can head south if you want to wander the preview again.",
            _ => ""
        };
    }

    private IEnumerable<string> GetVisibleItems(string sceneId)
    {
        if (sceneId == LanternNook && !_state.Inventory.Contains("lantern"))
        {
            yield return "lantern";
        }

        if (sceneId == WhisperingBrook && _state.BrookPuzzleSolved && !_state.Inventory.Contains("brass key"))
        {
            yield return "brass key";
        }
    }

    private IEnumerable<string> GetVisibleExits(string sceneId)
    {
        if (sceneId == BrambleGate && !_state.GateUnlocked)
        {
            return new[] { "east" };
        }

        return _scenes[sceneId].Exits.Keys.OrderBy(direction => direction);
    }

    private string DescribeInventory()
    {
        if (_state.Inventory.Count == 0)
        {
            return "Your paws are empty. A lantern might help, and locked things rarely open themselves.";
        }

        return "You are carrying:\n- " + string.Join("\n- ", _state.Inventory.OrderBy(item => item));
    }

    private StoryResponse Move(string direction)
    {
        var scene = _scenes[_state.CurrentSceneId];

        if (!scene.Exits.TryGetValue(direction, out var destination))
        {
            return new StoryResponse($"There is no path {direction} from here.");
        }

        if (_state.CurrentSceneId == BrambleGate && direction == "north" && !_state.GateUnlocked)
        {
            return new StoryResponse("The gate does not budge. You need a key before you can go north.");
        }

        _state.CurrentSceneId = destination;
        var sceneResponse = BuildCurrentSceneResponse();
        return new StoryResponse($"You head {direction}.\n\n{sceneResponse.Reply}", sceneResponse.SceneName, sceneResponse.SceneId);
    }

    private string TakeItem(string itemName)
    {
        if (MatchesItem(itemName, "lantern"))
        {
            if (_state.CurrentSceneId != LanternNook || _state.Inventory.Contains("lantern"))
            {
                return "There is no lantern here to take.";
            }

            _state.Inventory.Add("lantern");
            return "You lift the lantern from the branch. Its warm light makes the shadows seem less bossy.";
        }

        if (MatchesItem(itemName, "brass key") || MatchesItem(itemName, "key"))
        {
            if (_state.CurrentSceneId != WhisperingBrook || !_state.BrookPuzzleSolved || _state.Inventory.Contains("brass key"))
            {
                return "There is no key here to take.";
            }

            _state.Inventory.Add("brass key");
            return "You pick up the brass key and tuck it safely away.";
        }

        return $"You cannot take '{itemName}' right now.";
    }

    private string Examine(string target)
    {
        if (target.Contains("lantern", StringComparison.Ordinal))
        {
            return _state.Inventory.Contains("lantern")
                ? "The lantern glows with steady amber light. It is more comforting than magical, but sometimes comfort is its own kind of magic."
                : "The lantern is brass, polished by many patient paws. It looks safe to carry.";
        }

        if (target.Contains("stone", StringComparison.Ordinal) || target.Contains("sign", StringComparison.Ordinal) || target.Contains("brook", StringComparison.Ordinal))
        {
            if (_state.CurrentSceneId != WhisperingBrook)
            {
                return "You do not see that here.";
            }

            return _state.BrookPuzzleSolved
                ? "The stepping stones shimmer in a calm line across the brook. The riddle has already done its work."
                : "The sign reads: 'Add the first two stones to wake the third.' The numbered stones show 3 and 4.";
        }

        if (target.Contains("gate", StringComparison.Ordinal) || target.Contains("keyhole", StringComparison.Ordinal) || target.Contains("bramble", StringComparison.Ordinal))
        {
            if (_state.CurrentSceneId != BrambleGate)
            {
                return "You do not see a gate here.";
            }

            return _state.GateUnlocked
                ? "The iron gate stands open now, its brambles curled back like tiny green ribbons."
                : "The keyhole is brass and just the right size for a small key. The gate will not open by pushing alone.";
        }

        if (target.Contains("glade", StringComparison.Ordinal) && _state.CurrentSceneId == SunlitGlade)
        {
            return "The glade is peaceful and bright. For now, it marks the end of this short adventure slice.";
        }

        return "You study it carefully, but nothing new stands out yet.";
    }

    private string UseItem(string instruction)
    {
        if (instruction.Contains("lantern", StringComparison.Ordinal))
        {
            if (!_state.Inventory.Contains("lantern"))
            {
                return "You reach for a lantern, but you are not carrying one.";
            }

            if (_state.CurrentSceneId == WhisperingBrook && !_state.BrookPuzzleSolved)
            {
                return "By lantern light, the numbers on the stones are easy to read: 3 and 4. Their sum is 7.";
            }

            return "You raise the lantern. The path ahead looks friendlier in its amber glow.";
        }

        if (instruction.Contains("key", StringComparison.Ordinal) && instruction.Contains("gate", StringComparison.Ordinal))
        {
            return UnlockGate();
        }

        return "That does not seem to help here.";
    }

    private string UnlockGate()
    {
        if (_state.CurrentSceneId != BrambleGate)
        {
            return "There is no gate here to unlock.";
        }

        if (_state.GateUnlocked)
        {
            return "The gate is already open.";
        }

        if (!_state.Inventory.Contains("brass key"))
        {
            return "You need a key before the gate will open.";
        }

        _state.GateUnlocked = true;
        return "The brass key turns with a cheerful click. The bramble gate swings open, and the path north is finally clear.";
    }

    private string AnswerPuzzle(string answer)
    {
        if (_state.CurrentSceneId != WhisperingBrook)
        {
            return "There is no puzzle here asking for an answer.";
        }

        if (_state.BrookPuzzleSolved)
        {
            return "The brook puzzle is already solved.";
        }

        if (answer == "7")
        {
            _state.BrookPuzzleSolved = true;
            return "The third stone lights up with a soft green glow. A shell beside the brook opens and reveals a brass key.";
        }

        return "The brook splashes in disagreement. Try adding the first two numbered stones again.";
    }

    private static bool TryGetDirection(string command, out string direction)
    {
        direction = command switch
        {
            "north" or "n" => "north",
            "south" or "s" => "south",
            "east" or "e" => "east",
            "west" or "w" => "west",
            _ => ""
        };

        if (!string.IsNullOrEmpty(direction))
        {
            return true;
        }

        if (!command.StartsWith("go ", StringComparison.Ordinal))
        {
            return false;
        }

        var parsedDirection = command[3..].Trim();
        if (parsedDirection is "north" or "south" or "east" or "west")
        {
            direction = parsedDirection;
            return true;
        }

        return false;
    }

    private static bool MatchesItem(string rawValue, string itemName)
    {
        return rawValue.Equals(itemName, StringComparison.Ordinal)
            || rawValue.Equals($"the {itemName}", StringComparison.Ordinal)
            || rawValue.Equals($"a {itemName}", StringComparison.Ordinal);
    }

    private static string Normalize(string input)
    {
        return string.Join(
            ' ',
            input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .ToLowerInvariant();
    }

    private static string BuildHelpText()
    {
        return "Useful commands:\n- look\n- go north, south, east, or west\n- take lantern\n- examine stones\n- answer 7\n- use brass key on gate\n- inventory";
    }

    private sealed record Scene(
        string Id,
        string Name,
        string Description,
        Dictionary<string, string> Exits);

    private sealed class StoryState
    {
        public string CurrentSceneId { get; set; } = DenClearing;

        public HashSet<string> Inventory { get; } = new(StringComparer.OrdinalIgnoreCase);

        public bool BrookPuzzleSolved { get; set; }

        public bool GateUnlocked { get; set; }
    }
}