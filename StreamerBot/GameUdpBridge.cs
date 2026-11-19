// Paste this into Streamer.bot. It is not a Unity script.
//
// Setup:
// 1. Platforms: connect the Twitch broadcaster and the YouTube broadcaster.
//    On YouTube, turn on auto-monitor for the live broadcast.
// 2. Commands: create !accel, !turn, !power, !vote, and !quiz.
//    On each command, open Sources and check Twitch Message and YouTube Message.
// 3. Actions -> Add Action, name it "Game UDP Bridge".
// 4. Add a Command trigger for each of those five commands.
// 5. Add sub-action: Core -> C# -> Execute C# Code.
// 6. Replace the template with this file, then Compile, then Save and Compile.
//
// Unity's StreamerBotListener is already bound to UDP port 11000 and expects:
//   !accel 0.5          joystick forward/back, clamped to -1..1
//   !turn 0.5           wheel, 0 = full left, 0.5 = center, 1 = full right
//   !power on|off
//   !vote <poll title>   spaces are removed, then matched to a buff or debuff:
//                        Full Sail, Nimble, Ghost, Magnet,
//                        Anchor, Drunk Wheel, Engine Cut, Crosswind, Barrel Roll
//   !quiz Question?|option|option|answerIndex
//
// Twitch and YouTube poll close: send !vote %poll.winningChoice.title%
// Use those choice titles. "Full Sail" arrives as fullsail. Kill is not a poll choice.
//
// Set permissions and cooldowns on each command trigger in Streamer.bot.
// Returning false stops any sub-actions placed after this one.

using System;
using System.Globalization;

public class CPHInline
{
    const int GamePort = 11000;

    public bool Execute()
    {
        CPH.TryGetArg("command", out string command);
        CPH.TryGetArg("rawInput", out string rawInput);
        CPH.TryGetArg("user", out string user);
        CPH.TryGetArg("commandSource", out string source);
        if (string.IsNullOrWhiteSpace(source))
            CPH.TryGetArg("userType", out source);

        command = (command ?? "").Trim().ToLowerInvariant();
        rawInput = (rawInput ?? "").Trim();
        user = string.IsNullOrWhiteSpace(user) ? "chat" : user;
        source = (source ?? "").Trim().ToLowerInvariant();

        if (!TryBuildPayload(command, rawInput, out string payload, out string error))
        {
            Reply(source, $"@{user} {error}");
            CPH.LogWarn($"{source}: {error}");
            return false;
        }

        CPH.BroadcastUdp(GamePort, payload);
        CPH.LogInfo($"Game UDP {GamePort} from {source}: {payload}");
        return true;
    }

    void Reply(string source, string message)
    {
        if (source == "youtube")
            CPH.SendYouTubeMessage(message);
        else if (source == "twitch")
            CPH.SendMessage(message);
    }

    static bool TryBuildPayload(string command, string rawInput, out string payload, out string error)
    {
        payload = null;
        error = null;

        string cmd = command.StartsWith("!") ? command : "!" + command;

        switch (cmd)
        {
            case "!accel":
                if (!TryParseFloat(rawInput, out float accel))
                {
                    error = "Use a number, like !accel 0.5. Range is -1 (back) to 1 (forward).";
                    return false;
                }

                accel = Clamp(accel, -1f, 1f);
                payload = "!accel " + Format(accel);
                return true;

            case "!turn":
                if (!TryParseFloat(rawInput, out float turn))
                {
                    error = "Use a number, like !turn 0.5. Range is 0 (left) to 1 (right), 0.5 is center.";
                    return false;
                }

                turn = Clamp(turn, 0f, 1f);
                payload = "!turn " + Format(turn);
                return true;

            case "!power":
                string power = rawInput.ToLowerInvariant();
                if (power != "on" && power != "off")
                {
                    error = "Use !power on or !power off.";
                    return false;
                }

                payload = "!power " + power;
                return true;

            case "!vote":
                string vote = rawInput.ToLowerInvariant().Replace(" ", "");
                if (!IsVote(vote))
                {
                    error = "Poll choices: Full Sail, Nimble, Ghost, Magnet, Anchor, Drunk Wheel, Engine Cut, Crosswind, Barrel Roll.";
                    return false;
                }

                payload = "!vote " + vote;
                return true;

            case "!quiz":
                if (rawInput.Split('|').Length < 4)
                {
                    error = "Use !quiz Question?|answer|wrong|wrong|0";
                    return false;
                }

                payload = "!quiz " + rawInput;
                return true;

            default:
                error = "That command is not wired to the game.";
                return false;
        }
    }

    static bool IsVote(string vote)
    {
        switch (vote)
        {
            case "fullsail":
            case "nimble":
            case "ghost":
            case "magnet":
            case "anchor":
            case "drunkwheel":
            case "enginecut":
            case "crosswind":
            case "barrelroll":
                return true;
            default:
                return false;
        }
    }

    static bool TryParseFloat(string text, out float value)
    {
        return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            || float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
    }

    static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    static string Format(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }
}
