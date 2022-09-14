using System.Text.RegularExpressions;



namespace ArtistRecordCount.Helper
{
    public static class InputValidation
    {
        public static string IsInputValid(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return "Please enter the name of the Artist";
            }
            if (!Regex.Match(inputString, "^[A-Z][a-zA-Z]*$").Success)
                return CustomMessage();
            return inputString;

            static string CustomMessage()
            {
                return "The name of the artist is not valid";
            }
        }
    }
}
