using ArtistRecordCount.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArtistRecordCount.Interfaces
{
    public interface ICalculateWord
    {
        int CalculateWord(string String);
        double CalculateAverage(List<int> words);
    }



    public class CalculateWord : ICalculateWord
    {
        public double CalculateAverage(List<int> wordCount)
        {
           if(wordCount.Count == 0) { return 0; }
           return wordCount.Average();
        }

        int ICalculateWord.CalculateWord(string input)
        {
            if (string.IsNullOrEmpty(input))
                return 0;
            var output = new ResultWordCountModel();
            string modifiedInput = input.Replace(Environment.NewLine, " ");
            output.Words = GetSeparateWords(modifiedInput);

            //foreach(string word in output.Words)
            //{
            //    Console.WriteLine(word);
            //}


            return output.WordCount = output.Words.Count;

        }

        private List<string> GetSeparateWords(string modifiedInput)
        {
            List<string> output;

            Regex alphaCheck = new Regex("[^a-zA-Z ]");

            string cleanedInput = alphaCheck.Replace(modifiedInput, " ");

            output = cleanedInput.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.ToLower()).ToList();

            return output;
        }

    }

}
