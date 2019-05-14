using Microsoft.VisualStudio.TestTools.UnitTesting;
using SAM;
using System.Collections.Generic;
using System.Text;

namespace TokenizerTest
{
    [TestClass]
    public class TokenizerTest
    {


        Tokenizer tokenizer;

        [TestInitialize]
        public void Initialize()
        {
            var setting = new Setting();
            tokenizer = new Tokenizer(setting);
        }


        [TestMethod]
        [Description("simple format test of questionmarks")]
        public void simpleFormattingTest_questionmark() {
            string questionmark = "??one? two???? three ?? four??";
            string questionmark_result1 = "|?|?|[END]|one|?|[END]|two|?|?|?|?|[END]|three|?|?|[END]|four|?|?|[END]";
            Assert.AreEqual(questionmark_result1, tokenizer.TokenizeComment(questionmark).ToTestString());
        }
        
        [TestMethod]
        [Description("simple format test of exclamationmark")]
        public void simpleFormattingTest_exlamationmark(){
            string exclamationmark = "!!one! two!!!! three !! four!!";
            string exclamationmark_result1 = "|!|!|[END]|one|!|[END]|two|!|!|!|!|[END]|three|!|!|[END]|four|!|!|[END]";
            Assert.AreEqual(exclamationmark_result1, tokenizer.TokenizeComment(exclamationmark).ToTestString());
        }
        
        [TestMethod]
        [Description("simple format test of punctation")]
        public void simpleFormattingTest_punctation(){
            string punctation = "..one. two.... three .. four..";
            string punctation_result1 = "|.|.|[END]|one|.|[END]|two|.|.|.|.|[END]|three|.|.|[END]|four|.|.|[END]";
            Assert.AreEqual(punctation_result1, tokenizer.TokenizeComment(punctation).ToTestString());
        }
        
        [TestMethod]
        [Description("simple format test of randomMarks")]
        public void simpleFormattingTest_randomMarks(){
            string randomMarks = "?!{one...?{ two!?% &three ? four?!";
            string randomMarks_result1 = "|?|!|[END]|{|one|.|.|.|?|[END]|{|two|!|?|[END]|%|&|three|?|[END]|four|?|!|[END]";
            Assert.AreEqual(randomMarks_result1, tokenizer.TokenizeComment(randomMarks).ToTestString());
        }

        [TestMethod]
        [Description("simple format test of no marks")]
        public void simpleFormattingTest_noMarks()
        {
            string no_marks = "hej med dig";
            string noMarks_result1 = "|hej|med|dig|[END]";
            Assert.AreEqual(noMarks_result1, tokenizer.TokenizeComment(no_marks).ToTestString());
        }


        [TestMethod]
        [Description("detection of a single abbrevation at end of string")]
        public void simpleFormattingTest_one_abbreviations_at_end()
        {
            string abbreviation_at_end = "Jeg hader dig osv.";
            string only_one_abbreviation_result1 = "|jeg|hader|dig|og|s�|videre|[END]";
            Assert.AreEqual(only_one_abbreviation_result1, tokenizer.TokenizeComment(abbreviation_at_end).ToTestString());
        }

        [TestMethod]
        [Description("simple abbreviation test")]
        public void simpleFormattingTest_one_abbreviations()
        {
            string abbreviation_at_end = "Jeg hader dig osv.";
            string only_one_abbreviation_result1 = "|jeg|hader|dig|og|s�|videre|[END]";
            Assert.AreEqual(only_one_abbreviation_result1, tokenizer.TokenizeComment(abbreviation_at_end).ToTestString());
        }

        [TestMethod]
        [Description("simple format test of only one abbreviation")]
        public void detection_abbrevation_followed_by_no_space()
        {
            string abbreviation_at_end = "Jeg hader dig osv."; 
            string only_one_abbreviation_result1 = "|jeg|hader|dig|og|s�|videre|[END]";
            Assert.AreEqual(only_one_abbreviation_result1, tokenizer.TokenizeComment(abbreviation_at_end).ToTestString());
            Assert.AreEqual(only_one_abbreviation_result1 + "|men|[END]", tokenizer.TokenizeComment(abbreviation_at_end + "men").ToTestString());
        }

        [TestMethod]
        [Description("tests the edge case 'f.eks.' ")]
        public void edge_case_detection_feks()
        {
            string abbreviation_edge_case_feks = " jeg er smuk, men du er f.eks. meget grim";
            string abbreviation_edge_case_feks_result = "|jeg|er|smuk|,|men|du|er|for|eksempel|meget|grim|[END]";
            Assert.AreEqual(abbreviation_edge_case_feks_result, tokenizer.TokenizeComment(abbreviation_edge_case_feks).ToTestString());
        }
        [TestMethod]
        [Description("test of no mark seperation")]
        public void noSeperateMark_seperation()
        {
            string test = " syd- og s�nderjylland";
            string result = "|syd|-|og|s�nderjylland|[END]";
            Assert.AreEqual(result, tokenizer.TokenizeComment(test).ToTestString());
        }

        [TestMethod]
        [Description("test of no mark seperation")]
        public void noSeperateMark_non_seperation()
        {
            string test = " jeg er mellem-stor";
            string result = "|jeg|er|mellem-stor|[END]";
            Assert.AreEqual(result, tokenizer.TokenizeComment(test).ToTestString());
        }

        [TestMethod]
        [Description("test of seperating numbers")]
        public void number_seperation()
        {
            string test = " s�tning1 og 2";
            string result = "|s�tning|1|og|2|[END]";
            Assert.AreEqual(result, tokenizer.TokenizeComment(test).ToTestString());
        }

        [TestMethod]
        [Description("test of not seperating large number")]
        public void number_no_seperation_large_number()
        {
            //should fail atm.
            string test = " 1.5 millioner";
            string result = "|1.5|millioner|[END]";
            Assert.AreEqual(result, tokenizer.TokenizeComment(test).ToTestString());
        }

        [TestMethod]
        [Description("simple test of non-replacement of abbreviation")]
        public void no_abbreviation_expanding_test()
        {
            var setting = new Setting(abbreviation_expanding_Enabled: false);
            tokenizer = new Tokenizer(setting);
            string abbreviation_in_middle = "Jeg hader osv. dig ";
            string abbreviation_in_middle_result = "|jeg|hader|osv|.|[END]|dig|[END]";

            Assert.AreEqual(abbreviation_in_middle_result, tokenizer.TokenizeComment(abbreviation_in_middle).ToTestString());
        }
        [TestMethod]
        [Description("simple test of replacing an abbreviation in the middle of a sentence")]
        public void detection_abbrevation_in_middle()
        {
            string abbreviation_in_middle = "Jeg hader osv. dig ";
            string abbreviation_in_middle_result = "|jeg|hader|og|s�|videre|dig|[END]";

            Assert.AreEqual(abbreviation_in_middle_result, tokenizer.TokenizeComment(abbreviation_in_middle).ToTestString());
        }

        [TestMethod]
        [Description("simple test of comment split with newline")]
        public void comment_split_newline()
        {
            string test = "s�tning \n s�tning ";
            string result = "|s�tning|[END]|\n|s�tning|[END]";

            Assert.AreEqual(result, tokenizer.TokenizeComment(test).ToTestString());
        }

        [TestMethod]
        [Description("simple test of comment split with newline without space")]
        public void comment_split_newline_no_space()
        {
            string test = "s�tning\n s�tning ";
            string result = "|s�tning|[END]|\n|s�tning|[END]";

            Assert.AreEqual(result, tokenizer.TokenizeComment(test).ToTestString());
        }
    }
}
