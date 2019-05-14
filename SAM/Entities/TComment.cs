using System.Collections.Generic;
using System.Text;
using SAM.PlaceHolders;

namespace SAM
{
    /// <summary>
    /// A Tokenized comment.
    /// Contains a list of TSentences and a unique identifier
    /// </summary>
    public class TComment {

        public readonly IEnumerable<TSentence> tokenizedSentences;
        public readonly CUID identifier;

        public TComment(IEnumerable<TSentence> tokenizedSentences, CUID identifier)
        {
            this.tokenizedSentences = tokenizedSentences;
            this.identifier = identifier;

        }

        override
        public string ToString()
        {
            return identifier.getOriginal();
        }

        public string ToTestString()
        { // for testing
            var sb = new StringBuilder();
            foreach(TSentence sentence in tokenizedSentences){
                foreach(Token token in sentence.tokens) {
                    sb.Append("|" + token);
                }
                sb.Append("|[END]");
            }
            return sb.ToString();
        }

        public IEnumerator<TSentence> GetEnumerator()
        {
            return tokenizedSentences.GetEnumerator();
        }

    }
}
