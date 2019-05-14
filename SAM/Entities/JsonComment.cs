using System;
using System.Collections.Generic;

namespace SAM
{
    public class JsonComment
    {
        public string Comment;
        public List<string> Sentences;

        public JsonComment()
        {
            this.Comment = "";
            this.Sentences = new List<string>();
        }

        public JsonComment(string FullComment)
        {
            this.Comment = FullComment;
            this.Sentences = new List<string>();
        }

        public JsonComment(string FullComment, List<string> Sentences)
        {
            this.Comment = FullComment;
            this.Sentences = Sentences;
        }

        public void AddSentence(string Sentence)
        {
            this.Sentences.Add(Sentence);
        }

        override
        public string ToString()
        {
            var s = "[";
            foreach (var ss in Sentences)
            {
                s += ss + ", ";
            }
            s += "]";
            return "Comment: " + Comment + Environment.NewLine + "Sentences: " + s + Environment.NewLine;
        }
    }
}
