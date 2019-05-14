using SAM.PlaceHolders;

namespace SAM.Interfaces
{
    /*
     *  This is a modifying object. This object will be loaded in for the ruleBases classifier, and will contain a way to manipulate a token array
     *  when you're evaluating it.
     * 
     */

    public interface IModifier
    {
        /// <summary>
        /// Modifies the sentiment of a token-array from a given position
        /// </summary>
        /// <param name="tokens">a tokenized array</param>
        /// <param name="position">position to modify from</param>
        void ModifySentiment(ref Token[] tokens, int position);
    }
}
