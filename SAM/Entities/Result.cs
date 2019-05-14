using System;
using System.Collections.Generic;
using System.Text;
using SAM.Interfaces;

namespace SAM
{
    /// <summary>
    /// When a string has been evaluated, the Result object keeps track of the label, rating and Identifier. This is very usefull in a multithreaded scenario
    /// </summary>
    class Result : IComparable
    {
        public readonly IIdentifier identifier;
        public readonly string label;
        public readonly float rating;

        public Result(IIdentifier identifier, string label, float rating)
        {
            this.identifier = identifier;
            this.label = label;
            this.rating = rating;
        }

        public int CompareTo(Object obj)
        {
            Result res = (Result)obj;
            string s = res.label;

            if (label == s)
                return 1;
            else
                return 0;
        }

        public override string ToString()
        {
            return "("+label+","+rating+") - "+identifier;
        }
    }

    /// <summary>
    /// Compare testdata objects with program results e.g CResult is the result of comparing 2 results. (test data vs actual program evalutation)
    /// </summary>
    class CResult : Result
    {
        public readonly bool hit;
        public readonly string actual;

        public CResult(IIdentifier identifier, string l, float r, bool hit, string actual = "") : base(identifier, l, r)
        {
            this.hit = hit;
            this.actual = actual;
        }
        public CResult(Result res, bool hit, string actual = "") : base(res.identifier, res.label, res.rating)
        {
            this.hit = hit;
            this.actual = actual;
        }

        public override string ToString()
        {
            if (hit)
            {
                return hit + " (" + label + "," + rating + ") - " + identifier;
            }
            else
            {
                return hit + " was : " + actual + " - Expected: (" + label + ") - " + identifier;
            }
        }
    }
}
