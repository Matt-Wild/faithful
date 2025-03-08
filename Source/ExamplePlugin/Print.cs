namespace Faithful
{
    internal static class Print
    {
        public static void Info(IPrintable _source, string _message)
        {
            // Log formatting message
            Log.Info(FormatMessage(_source, _message));
        }

        public static void Debug(IPrintable _source, string _message)
        {
            // Check if in debug mode
            if (Utils.debugMode)
            {
                // Log formatting message
                Log.Debug(FormatMessage(_source, _message));
            }
        }

        public static void Warning(IPrintable _source, string _message)
        {
            // Log formatting message
            Log.Warning(FormatMessage(_source, _message));
        }

        public static void Error(IPrintable _source, string _message)
        {
            // Log formatting message
            Log.Error(FormatMessage(_source, _message, _isError: true));
        }

        private static string FormatMessage(IPrintable _source, string _message, bool _isError = false)
        {
            // Construct the prefix using the class name and print identifier
            string formattedMessage = $"[{_source.GetType().Name} | {_source.printIdentifier}] | {_message}";

            // Ensure proper punctuation at the end
            char lastChar = formattedMessage[^1];
            if (!char.IsPunctuation(lastChar))
            {
                formattedMessage += _isError ? "!" : ".";
            }

            // Return formatted message
            return formattedMessage;
        }
    }

    internal interface IPrintable
    {
        public string printIdentifier { get; }
    }
}
