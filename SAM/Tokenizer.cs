using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SAM.PlaceHolders;
using System.Threading;
using System.Text;

namespace SAM
{
    /// <summary>
    /// This class tokenizes strings(Creates lists of tokens)
    /// </summary>
    public class Tokenizer
    {
        private Setting settings;

        public Tokenizer(Setting settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Takes a list of raw comments and converts them into a list of TokenizedComments
        /// </summary>
        /// <param name="comments">An IEnumerable of comments containing several sentences</param>
        /// <returns>IEnumerable of TokenizedComment</returns>

        public IEnumerable<TComment> TokenizeComments(IEnumerable<(int,string)> comments)
        {
            var tokenizedComments = new List<TComment>();

            foreach ((int,string) identifier in comments)
            {
                tokenizedComments.Add(TokenizeComment(identifier.Item2));
            }

            return tokenizedComments;
        }

        /// <summary>
        /// Takes a raw comment and splits it into sentences, which are then tokenized.
        /// </summary>
        /// <param name="comment">A comment containing several sentences</param>
        /// <returns>A TComment -> List of TSentences -> a list of tokens(words)</returns>

        public TComment TokenizeComment(string comment)
        {
            string original = comment;
            comment = comment.ToLower();

            sanitize(ref comment);

            var tokenComment = new List<TSentence>();
            IEnumerable<string> sentences;
            sentences = SplitComment(comment);
            var counter = 0;

            foreach (string sentence in sentences)
            {
                tokenComment.Add(TokenizeSentence(sentence, counter++, false));
            }

            return new TComment(tokenComment, new CUID(Thread.CurrentThread.ManagedThreadId + DateTime.Today.Millisecond, original));
        }

        /// <summary>
        /// Splits a sentence into tokens and filters out empty.
        /// Call this method with standAlone = true, if you don't need the sentence to be split up (i.e handled like a complete comment)
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="id">A unique id for the sentence</param>
        /// <param name="standAlone">defaults to true, set to false if sanitizing has happened elsewhere</param>
        /// <returns>returns a tokenized sentence -> list of tokens</returns>
        /// 
        public TSentence TokenizeSentence(string sentence, int id, bool standAlone = true)
        {
            var original = sentence;

            if (standAlone)
            {
                sentence = sentence.ToLower();
                sanitize(ref sentence);
            }
            addWhiteSpaces(ref sentence);

            var tokens = sentence.Split(" ").Where(x => !string.IsNullOrEmpty(x)).ToList();
            var convertedTokens = new List<Token>();
            foreach (string token in tokens)
            {
                var internedToken = Program.InternConcurrentSafe(token);
                convertedTokens.Add(new Token(internedToken, 0));
            }
            return new TSentence(convertedTokens, new SUID(id,original));
        }

        /// <summary>
        /// This method is responsible for calling all the methods that preproccess the tokens.
        /// </summary>
        /// <param name="input">string to sanitize</param>

        private void sanitize(ref string input)
        {
            if (settings.removeLinks)
                RemoveLinks(ref input);

            if (settings.abbreviation_expansion)
                ExpandAbbrevations(ref input);

            

            // should split numbers here aswell
        } 

        /// <summary>
        /// This method replaces links, such that they will not interfere with sentence-splitting. It is not getting all links atm.
        /// </summary>
        /// <param name="sentence"></param>

        private void RemoveLinks(ref string sentence)
        {
            var reg = new Regex(settings.findLinks);
            var sanitized = reg.Replace(sentence, "LINK");
        }

        /// <summary>
        /// This method checks a string for abbreviations and replaces them with what they stand for. 
        /// </summary>
        /// <param name="comment"></param>

        private void ExpandAbbrevations(ref string comment)
        {
            var reg = new Regex(settings.abbreviationFinder);

            var abb = reg.Match(comment);

            while (abb.Success)
            {
                abb = reg.Match(comment);
                var match = abb.ToString().Trim();
                var sanitized = match.Replace(".", "");
                if (settings.abbreviations.ContainsKey(sanitized))
                {
                    var replacement = settings.abbreviations[sanitized];
                    comment = comment.Replace(match, replacement);
                }
                else //fuck
                {
                    var replacement = "ERROR";
                    comment = comment.Replace(match, replacement); // if not removed or changed = infinite loop

                    FileWriter.WriteErrorLog("[404: AbbreviationFinderRegexMismatch] Regex found: '" + match + "' -> lookup in dictionary with -> '" + sanitized + "', but no match found. This is either a bad abbreviationRegex and/or a missing dictionary entry." + Environment.NewLine + "Comment: " + comment);
                }
                abb = reg.Match(comment);
            }
        }

        /// <summary>
        /// This method takes a comment and splits it into sentences. 
        /// </summary>
        /// <param name="c"></param>
        /// <returns>An Enumerable of sentences</returns>
        /// 
        public IEnumerable<string> SplitComment(string c)
        {

            c = (new Regex("(\\r)?\n").Replace(c, " \n ")); // only fucking way to handle annoying edge case
            
            var newLineFinder = new Regex("$", RegexOptions.Multiline); // Splits a sentence if it contains an "ENTER" / newline.

            var seperated = newLineFinder.Split(c);
            var sentences = new List<string>();
            var reg = new Regex(settings.sentenceEndFinder);

            foreach (string comment in seperated)
            {
                var match = reg.Match(comment);
                var remaining = comment;
                var lastMatch = 0;

                while (match.Success)
                {
                    //if (reg1.IsMatch(match.Value)) continue;   idea for passing no_seperation_large_number_test

                    lastMatch = match.Index;
                    if (lastMatch != 0)
                    {
                        var substring = remaining.Substring(0, lastMatch + 1);
                        remaining = remaining.Substring(lastMatch + 1);
                        sentences.Add(substring);
                    }

                    match = reg.Match(remaining);
                }//no more matches, should still add remaining unless it's empty anyways
                if (!match.Success && remaining != "")
                    sentences.Add(remaining);
            }
            /* edge-case hunting. 
             * this fixes:
             * Expected:<|jeg|hader|dig|og|så|videre|[END]>. 
             * Actual:<|jeg|hader|dig|osv|.|[END]>.
             * (We need to know when a sentence is ending, and we can only do that by waiting to expand endings till now)
             */ 
            
            if (settings.abbreviation_expansion)
            {
                var abbreg = new Regex(settings.abbreviationFinder);

                for (int i = 0; i < sentences.Count; i++)
                {
                    if (abbreg.IsMatch(sentences[i] + " "))
                    {
                        var token = sentences[i] + " ";
                        ExpandAbbrevations(ref token); 
                        sentences[i] = token.Trim();
                    }
                }
            }

            return sentences;
        }

        /// <summary>
        /// Takes a sentence and adds spaces around marks, such that it is ready for tokenization (Splitting on whitespace)
        /// The reason we remove emojiis before and add them again after are related to encoding issues. An emojii consists of several chars, and when we iterate over a string pr. char
        /// we actually end up breaking all the emojiis. 
        /// This method calls AddWhiteSpacesAroundMarks where the actual whitespace-adding occurs.
        /// </summary>
        /// <param name="sentence"></param>

        private void addWhiteSpaces(ref string sentence)
        {
            var tmp = sentence; // create copy of sentence locally

            var s = new Regex(settings.emojiPattern); // should be a emojii regex in settings
            var placeholder = new Regex("EMOJI");
            var emojis = new List<string>();
            var matches = s.Matches(sentence);

            foreach(Match m in matches) // get each matched Emoji
            {
                emojis.Add(m.ToString());
            }

            var addedPlaceHolder = s.Replace(tmp, " " + placeholder.ToString() + " "); //capital letters are unique at this point. Remove smileys, so they won't break in next method call

            AddWhiteSpaceAroundMarks(ref addedPlaceHolder); // very specific way to handle adding spaces to the string

            
            foreach(var emoji in emojis) // run through each captured emoji and add them back in one by one
            {
                addedPlaceHolder = placeholder.Replace(addedPlaceHolder, emoji,1);
            }

            sentence = addedPlaceHolder;
        }

        /// <summary>
        /// This method adds spaces around marks except the ones in markNotToSeperat
        /// </summary>
        /// <param name="sentence"></param>
        
        private void AddWhiteSpaceAroundMarks(ref string sentence)
        {
            var reg = new Regex(settings.markNotToSeperate);
            // below will split on the marksplitter regex

            var length = sentence.Length;
            for (int i = 0; i < length; ++i)
            {
                if (char.IsWhiteSpace(sentence[i])) continue;
                if (!char.IsLetter(sentence[i]))
                {
                    if (!reg.IsMatch(sentence[i].ToString())) {
                        // is not a nonMarkToSeperate
                        if (i != 0 && (char.IsLetterOrDigit(sentence[i - 1])))
                        {
                            //is not a letter and is a seperateable mark (add space before   'hej[!]'   -> 'hej [!]')
                            sentence = sentence.Insert(i, " ");
                            ++length;
                            ++i;
                        }
                        //is not a letter and is seperatable, add space after 'hej [!]!' -> 'hej [!] !'
                        sentence = sentence.Insert(i + 1, " ");
                        ++i;
                        ++length;

                    }
                    
                    else
                    { // is a nonMarkToSeperate
                        if (length - 1 > i && char.IsLetter(sentence[i + 1]) && 0 < i && char.IsLetter(sentence[i - 1])) continue; // hej-hej -> hej[-]hej

                        if (length-1 > i && !char.IsWhiteSpace(sentence[i + 1]))
                        {
                            //is nonMarkToSeperate, but next is not letter e.g "klap [-]!" -> "klap [-] !"
                            sentence = sentence.Insert(i + 1, " ");
                            ++i;
                            ++length;
                        }
                        if (0 < i && !char.IsWhiteSpace(sentence[i - 1]))
                        {
                            //is nonMarkToSeperate, but next is not letter e.g "syd- og" -> "syd [-] og"
                            sentence = sentence.Insert(i, " ");
                            ++i;
                            ++length;
                        }                         
                    }
                }
            }
        }
    }
}
