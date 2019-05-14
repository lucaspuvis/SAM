using System;
using System.Collections.Generic;
using System.Text;
using SAM.PlaceHolders;

namespace SAM
{
    /// <summary>
    /// Tokenized Sentence.
    /// A tokenized sentence holds a list of tokens and a unique identifier.
    /// </summary>
    public class TSentence
    {
        public readonly SUID identifier;
        public readonly IEnumerable<Token> tokens;

        public TSentence(IEnumerable<Token> tokenizedSentence, SUID identifier){
            this.identifier = identifier;
            tokens = tokenizedSentence;
        }

        override
        public string ToString()
        {
            return identifier.getOriginal();
        }

        public IEnumerator<Token> GetEnumerator()
        {
            return tokens.GetEnumerator();
        }
    }
}
