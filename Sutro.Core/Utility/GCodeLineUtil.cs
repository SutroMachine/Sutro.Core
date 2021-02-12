using Sutro.Core.Models.GCode;

namespace gs.utility
{
    public static class GCodeLineUtil
    {
        public static bool ExtractFillType(GCodeLine line, ref string featureType)
        {
            if (line.Comment != null)
            {
                int indexOfFillType = line.Comment.IndexOf("feature");
                if (indexOfFillType >= 0)
                {
                    featureType = line.Comment.Substring(indexOfFillType + 8).Trim();
                    return true;
                }
            }
            return false;
        }
    }
}