using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;

namespace SamplePlugin;

// Change class from internal to public
public class WrathStateChecker
{
    private bool IsWrathEnabledInternal = false;

    public void ChatMessageHandler(XivChatType type, int a2, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        // Log the type and message for debugging
        System.Console.WriteLine($"[Chat] Type: {type}, Message: {message.TextValue}");

        // Check for Wrath's Auto-Rotation messages
        if (message.TextValue.Contains("Auto-Rotation set to ON"))
        {
            IsWrathEnabledInternal = true;
            System.Console.WriteLine("Wrath Auto-Rotation: Enabled");
        }
        else if (message.TextValue.Contains("Auto-Rotation set to OFF"))
        {
            IsWrathEnabledInternal = false;
            System.Console.WriteLine("Wrath Auto-Rotation: Disabled");
        }
    }

    public bool IsWrathEnabled()
    {
        return IsWrathEnabledInternal;
    }
}
