using System;

namespace gs
{
    public class StringUtil
    {
        public static string FormatSettingOverride(string s)
        {
            // Split into path and value
            string[] pathValue = s.Split(':');
            if (pathValue.Length != 2)
                throw new Exception("Need setting in \"KeyName:Value\" format; got " + s);

            // Split the path to deal with nested values
            string[] keys = pathValue[0].Split('.');

            // Surround value with quotes if first character is letter
            // This is required to make enumerations work
            string result = pathValue[1];
            if (Char.IsLetter(result[0]) &&
                !(result == "true" || result == "false"))
            {
                result = "\"" + result + "\"";
            }

            // Construct the nested string
            for (int i = keys.Length - 1; i >= 0; i--)
                result = "{\"" + keys[i] + "\":" + result + "}";

            return result;
        }
    }
}