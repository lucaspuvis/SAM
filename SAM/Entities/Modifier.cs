using SAM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SAM.PlaceHolders
{
    public abstract class Modifier : IModifier
    {
        public readonly float ModValue;
        public readonly int LookupValue;
        public readonly List<string> stopCharacters = new List<string>() {
            "!",
            ".",
            "?",
            ":",
            "(",
            ")"
        };

        public Modifier(float modValue, int lookupValue) {
            ModValue = modValue;
            LookupValue = lookupValue;
        }

        public abstract void ModifySentiment(ref Token[] tokens, int position);
    }

    public class Add : Modifier
    {
        public Add(float modValue, int lookupValue) : base(modValue, lookupValue)
        {
        }
        
        public override void ModifySentiment(ref Token[] tokens, int position)
        {
            for (int i = 1; i <= LookupValue; i++)
            {
                if (position + i > tokens.Length) break;
                var nextWord = tokens[i + position];
                if (stopCharacters.Contains(nextWord.Word)) break;
                if (nextWord.Sentiment != 0)
                {
                    tokens[i + position].Sentiment = nextWord.Sentiment + ModValue;
                    break;
                }
            }
        }
    }

    public class Sub : Modifier
    {
        public Sub(float modValue, int lookupValue) : base(modValue, lookupValue) {}
        
        public override void ModifySentiment(ref Token[] tokens, int position)
        {
            for (int i = 1; i <= LookupValue; i++)
            {
                if (position + i > tokens.Length) break;
                var nextWord = tokens[i + position];
                if (stopCharacters.Contains(nextWord.Word)) break;
                if (nextWord.Sentiment != 0)
                {
                    tokens[i + position].Sentiment = nextWord.Sentiment - ModValue;
                    break;
                }
            }
        }
    }

    public class Mult : Modifier
    {
        public Mult(float modValue, int lookupValue) : base(modValue, lookupValue) {}
        
        public override void ModifySentiment(ref Token[] tokens, int position) {

            for (int i = 1; i <= LookupValue; i++)
            {
                if (position + i >= tokens.Length) break;
                var nextWord = tokens[i + position];
                if (stopCharacters.Contains(nextWord.Word)) break;
                if (nextWord.Sentiment != 0)
                {
                    tokens[i + position].Sentiment = nextWord.Sentiment * ModValue;
                    break;
                }
            }
        }
    }

    public class Special : Modifier
    {
        private readonly List<string> characters = new List<string>() {
            "?",
            "!"
        };

        public Special(float modValue, int lookupValue) : base(modValue, lookupValue) {}

        public override void ModifySentiment(ref Token[] tokens, int position)
        {
            var sb = new StringBuilder();
            for (int i = position; i < tokens.Length; i++)
            {
                var word = tokens[i].Word;
                if (word.Length != 1 && !characters.Contains(word)) break;
                sb.Append(word);
            }
            var regex = new Regex(@"(\![\!\?]+)(?=\?)|(\?[\!\?]+)(?=\!)");
            if (regex.IsMatch(sb.ToString())) {
                tokens[position].Sentiment = -2.0f;
                for (int i = 1; i < sb.Length; i++)
                {
                    if (position + i >= tokens.Length) break;
                    tokens[position + i] = new Token("null", 0.0f);
                }
            }
        }
    }

    public class Repeating : Modifier
    {
        private readonly string character;
        private readonly int amount;
        public Repeating(float modValue, int lookupValue, string character, int amount) : base(modValue, lookupValue)
        {
            this.character = character;
            this.amount = amount;
        }

        public override void ModifySentiment(ref Token[] tokens, int position)
        {
            var characterCount = 0;
            for (int i = position; i < tokens.Length; i++) {
                var token = tokens[i];
                if (!(token.Word == character)) break;
                characterCount++;
            }
            if (characterCount >= amount) {
                tokens[position].Sentiment = ModValue;
                for (int i = 1; i < characterCount; i++)
                {
                    if (position + i >= tokens.Length) break;
                    tokens[position + i] = new Token("null", 0.0f);
                }
            }
        }
    }

    public class Vending : Modifier
    {
        private readonly Token[] Words;
        
        public Vending(float modValue, int lookupValue, IEnumerable<Token> words) : base(modValue, lookupValue)
        {
            Words = words.ToArray();
        }

        public override void ModifySentiment(ref Token[] tokens, int position)
        {
            if (LookupValue > 0) // if we're looking ahead
            {
                for (int i = 1; i <= LookupValue; i++)
                {
                    if (position + i >= tokens.Length) return; // no full match on vending before end of sentence
                    if (tokens[i + position].Word == Words[i - 1].Word) continue; // match!
                    else return; //no match, no vending, no fun
                }
                for (int i = 0; i <= LookupValue; i++) // only goes here if we have full match
                {
                    if (i != LookupValue) tokens[i + position].Sentiment = 0; // nullify whatever values are there
                    else tokens[i + position].Sentiment = ModValue; // add vending value
                }
                return;
            }
            else // we're looking behind
            {
                for (int i = 1; (-i <= LookupValue && !(Words.Length - i < 0)); i++) 
                {
                    if (position-i <= 0) return; // no full match before end of sentence
                    if (tokens[position-i].Word == Words[i-1].Word) continue; // match!
                    else return; // no match, no vending, no fun
                }
                for (int i = 0; -i <= LookupValue; i++)
                {
                    if (-i != LookupValue) tokens[position - i].Sentiment = 0; // nullify whatever values are there
                    else { tokens[position].Sentiment = ModValue; return; } //  add vending value and 
                }
                return;
            }
        }
    }
}