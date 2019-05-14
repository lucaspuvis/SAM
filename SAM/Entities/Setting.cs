using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SAM
{
    /// <summary>
    /// The settings object is loaded in from a .json file at runtime. All values not specified will be defaulted. 
    /// The settings object contains most settings that is not suited for arguments.
    /// </summary>
    public class Setting
    {
        public string abbreviationFinder { get; set; }
        public string markNotToSeperate { get; set; }
        public string sentenceEndFinder { get; set; }
        public string nonAlphaNumeric { get; set; }
        public string findLinks { get; set; }
        public string emojiPattern { get; set; }
        public bool abbreviation_expansion { get; set; }
        public bool writeNonMatchTokens { get; set; }
        public bool threaded { get; set; }
        public bool printResult { get; set; }
        public bool removeLinks { get; set; }
        public bool compare { get; set; }

        public Setting( //constructor paramters - setting default params
                        string abbreviationFinder = default_AbbreviationFinder,
                        string markNotToSeperate = default_MarkNotToSeperate,
                        string sentenceEndFinder = default_CommentSentenceSplit,
                        string nonAlphaNumeric = default_nonAlphaNumeric,
                        string findLinks = default_findLinks,
                        string emoji = default_emojiPattern,
                        bool abbreviation_expanding_Enabled = true,
                        bool writeNonMatchTokens = false,
                        bool threaded = true,
                        bool printResult = false,
                        bool removeLinks = true,
                        bool compare = false)
        {
            //Set field constants at creating time of object 
            this.findLinks = findLinks;
            this.abbreviationFinder = abbreviationFinder;
            this.markNotToSeperate = markNotToSeperate;
            this.sentenceEndFinder = sentenceEndFinder;
            this.nonAlphaNumeric = nonAlphaNumeric;
            this.abbreviation_expansion = abbreviation_expanding_Enabled;
            this.writeNonMatchTokens = writeNonMatchTokens;
            this.threaded = threaded;
            this.printResult = printResult;
            this.removeLinks = removeLinks;
            this.compare = compare;
            this.emojiPattern = emoji;

            //current way to load abbreviation dictionary into a CONCURRENT dictionary. Should probably have an optional param named abbreviationsInit
            foreach (KeyValuePair<string, string> pair in abbreviationsInit)
            {
                abbreviations.TryAdd(pair.Key, pair.Value);
            }
            abbreviationsInit = null;
        }

        // The regex is carefully constructed as is, so if change needed: be very wary of side-effects.
        // important node when building the regex builder is that it should always be sort from longest abbrevation to smallest
        private const string default_AbbreviationFinder = @"(f\.eks\.?)|(?<=\ )(adm|adr|afd|alm|ang|bh|bl\.a|dvs|e\.g|eksl|inkl|evt|f\.eks|feks|fhv|frk|fx|h\.c|hhv|hr|i\.e|ifb|ifl|jf|jr|jvf|kbh|mio|ml|mr|mrs|o\.fl|ofl|osv|opg|org|p\.t|pga|ph\.d|s\.u|sek|vedr|vejl|vedr|vha|vh|lign|mfl|m\.fl|mia|mm|m\.m|mvh|mv|dsv|a\.i|d\.|m\.)[\.\!\?\ ]?(\ |$)(?=[\.\!\?]?)|\.t[hv]\ ";
        // save this for problems with emojiis
        private const string default_emojiPattern = ("👎|👿|💤|🖕🏼|😁|😎|😐|😒|😡|😢|😤|😨|😬|😰|😱|🤦‍|😖|🤢|🤬|🤭|🤮");
        private const string default_MarkNotToSeperate  = @"[\-]";
        private const string default_CommentSentenceSplit = (@"[\.|\!|\?](?!\.|\?|\!)");
        private const string default_nonAlphaNumeric = @"[^a-zæøå\s\-é:]";
        private const string default_findLinks = @"((http(s)?:\/\/)|(www\.))([\w-]+.)+[\w-]+(\/[\w- .\/?%&=]+)?";

        //Default data
        public ConcurrentDictionary<string, string> abbreviations { get; set; } = new ConcurrentDictionary<string, string>();
        // dictionaries should be available to create on runtime from a file
        private Dictionary<string, string> abbreviationsInit { get; set; } = new Dictionary<string, string>(){
            {"osv","og så videre"},
            {"adm","administration"},
            {"adr","adresse"},
            {"afd","afdeling"},
            {"alm","almindelig"},
            {"ang","angående"},
            {"bh","brystholder"},
            {"bla","blandt andet"},
            {"d","den"},
            {"dvs","det vil sige"},
            {"eg","for eksempel"},
            {"eksl","ekslusiv"},
            {"evt","eventuelt"},
            {"feks","for eksempel"},
            {"fhv","forhenværende"},
            {"frk","frøken"},
            {"fx","for eksempel"},
            {"hc","hans christian"},
            {"hhv","henholdsvis"},
            {"hr","herre"},
            {"ie","det vil sige"},
            {"ifb","i forbindelse"},
            {"ifl","ifølge"},
            {"inkl","inklusiv"},
            {"jf","jævnfør"},
            {"jr","junior"},
            {"jvf","jævnfør"},
            {"kbh","københavn"},
            {"m","med"},
            {"mio","millioner"},
            {"ml","milliliter"},
            {"mr","mister"},
            {"mrs","misses"},
            {"ofl","og flere"},
            {"opg","opgang"},
            {"org","organisation"},
            {"pt","for tiden"},
            {"pga","på grund af"},
            {"phd","philosophiae doctor"},
            {"su","SU"},
            {"sek","sekund"},
            {"th","til højre"},
            {"vejl","vejledende"},
            {"vedr","vedrørende"},
            {"vh","venlig hilsen"},
            {"vha","ved hjælp af"},
            {"lign","lignende"},
            {"mfl","med flere"},
            {"mia","milliard"},
            {"mm","med mere"},
            {"mv","med videre"},
            {"mvh","med venlig hilsen"},
            {"dsv","desværre"},
            {"ai","indtil videre"}};
    }
}

