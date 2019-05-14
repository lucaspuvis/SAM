using CsvHelper;
using System;
using SAM.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SAM.PlaceHolders;

namespace SAM
{
    public static class CSVReader
    {
        /*
         * ReadComments - takes a .csv file with a header and parses
         * it into a list of strings containing all the comments.
         */

        public static IEnumerable<string> ReadCommentsYield(string filename)
        {
            var comments = new List<string>();
            var reader = new StreamReader(filename, Encoding.UTF8);
            var csv = new CsvReader(reader);

            csv.Configuration.Delimiter = ",";

            while (csv.Read())
            {
                //add spaces to comments to make parsing not-shit
                //counter = DateTime.Now.Millisecond;
                yield return (" " + csv.GetField(1).ToLower().Trim());
            }
            reader.Close();
            reader.Dispose();
        }

        public static IEnumerable<(string,string)> ReadComparrisonsYield(string filename)
        {// original, label rating
            var comments = new List<string>();
            var reader = new StreamReader(filename, Encoding.UTF8);
            var csv = new CsvReader(reader);

            csv.Configuration.Delimiter = ",";
            while (csv.Read())
            {
                yield return (csv.GetField(0),csv.GetField(1).ToLower().Trim());
            }
            reader.Close();
            reader.Dispose();
        }

        public static IDictionary<string,int> ReadLexicon(string filename)
        {
            var lex = new Dictionary<string, int>();

            using (var reader = new StreamReader(filename))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.HasHeaderRecord = false;
                csv.Configuration.Delimiter = ",";
                while (csv.Read())
                {
                    var word = Program.InternConcurrentSafe(csv.GetField(0).ToLower());
                    var score = Int32.Parse(csv.GetField(1));
                    if (!lex.TryAdd(word, score))
                        FileWriter.WriteErrorLog("[Lexicon] Couldn't add '" + word + "' with score " + score + " -> is it unique in the lex?");
                }
                reader.Close();
            }
            return lex;
        }

        public static IDictionary<string, List<IModifier>> ReadTriggerLex(string filename)
        {
            var lex = new Dictionary<string, List<IModifier>>();

            using (var reader = new StreamReader(filename))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.HasHeaderRecord = true;
                //Triggerword	ModifierType	Lookahead	ModValue	Extra
                csv.Read();
                csv.Configuration.Delimiter = ",";
                while (csv.Read())
                {
                    var mods = new List<IModifier>();
                    var tword = Program.InternConcurrentSafe(csv.GetField(0).ToLower());
                    var lookahead = int.Parse(csv.GetField(2));
                    var modValue = float.Parse(csv.GetField(3).Replace(".",","));
                    
                    IModifier modifier = null; // this could be waaaay prettier... Just wanted to try a switch case
                    switch (csv.GetField(1))
                    {
                        case "mult":
                            modifier = new Mult(modValue, lookahead);
                            break;
                        case "v":
                            var tokens = new List<Token>();
                            foreach (string s in csv.GetField(4).Split("|")) // couldn't a vending just take strings instead?... no reason to have a token
                                tokens.Add(new Token(s,0));
                            modifier = new Vending(modValue, lookahead, tokens);
                            break;
                        case "neg":
                            modifier = new Mult(-1.0f, lookahead);
                            break;
                        case "repeating":
                            modifier = new Repeating(modValue, lookahead, tword, int.Parse(csv.GetField(4)));
                            break;
                        case "special":
                            modifier = new Special(modValue, lookahead);
                            break;
                        default :
                            FileWriter.WriteErrorLog("[TriggerLexicon] Couldn't match triggerword '" + tword + " -> is it unique in the lex?");
                            break;

                    }

                    if (!lex.ContainsKey(tword))
                    {
                        mods.Add(modifier);
                        if(!lex.TryAdd(tword,mods))
                            FileWriter.WriteErrorLog("[Lexicon] Couldn't add '" + tword + "' with mod " + modifier + " -> is it unique in the lex?");
                    }
                    else
                    {
                        lex.TryGetValue(tword, out mods);
                        mods.Add(modifier);
                        lex.Remove(tword);
                        lex.Add(tword, mods);
                    }
                        
                }
                reader.Close();
            }
            return lex;
        }

    }
}
