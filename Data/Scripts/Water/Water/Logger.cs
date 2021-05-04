using VRage.Utils;

public static class Logger
{
    public static string LogDefaultIdentifier = "ITGCore3-WaterThrusters: ";
    public static bool enableLogging = false;

    public static void AddMsg(string message, bool debugOnly = false, string identifier = "")
    {
        if (enableLogging == false) return;

        string thisIdentifier = "";

        if (identifier == "")
        {

            thisIdentifier = LogDefaultIdentifier;

        }
        
        MyLog.Default.WriteLineAndConsole(thisIdentifier + message);
    }
}