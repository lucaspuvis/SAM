using System;
using System.Collections.Generic;
using System.Text;

namespace SAM.PlaceHolders
{
    /// <summary>
    /// A token is a word or a mark e.g '!'.
    /// A token also holds its sentiment value. This is mainly used in lexical-classifiers
    /// </summary>
    public class Token
    {
        public string Word { get; }
        public float Sentiment { get; set; }

        public Token(string word, float sentiment) {
            Word = word;
            Sentiment = sentiment;
        }

        public override string ToString()
        {
            return Word;
        }
    }
}
