import sys, joblib, os, lstm#, multisvm

from sklearn.svm import SVR
from sklearn.pipeline import Pipeline
from sklearn.feature_extraction.text import TfidfVectorizer

# Arguments:
# First argument specifies the classifier
# -svm, Support Vector Machine
# -rf, Random Forest
# -nb, Naïve Bayes
#
# Every argument afterwards are phrases to be predicted. Eg:
# python classifier_predictor.py -svm "First string." "Second string."

dir_path = os.path.dirname(os.path.realpath(__file__))
model_path = dir_path + "/Model"

def main():
    if len(sys.argv) < 3:
        print("Please supply correct number of arguments")
        exit(1)

    # Read every phrase from arguments
    predict_phrases = []
    for i in range(2, len(sys.argv)):
        predict_phrases.append(sys.argv[i].strip())

    clf = getClassifier(sys.argv[1])
    predict_results = clf.predict(predict_phrases)

    #print("Phrase: Result")
    for i in range(0, len(predict_phrases)):
        phrase = predict_phrases[i].encode('unicode-escape').decode('utf8')
        print(phrase)
        print(predict_results[i])


# Get classifier from supplied argument
def getClassifier(clf):
    switcher = {
        "-svm": joblib.load(model_path + "/svm_pipeline.joblib"),
        # "-nb": joblib.load(model_path + "/nb_pipeline.joblib"),
        "-rf": joblib.load(model_path + "/rf_pipeline.joblib"),
        "-lstm": lstm
    }

    # Check if wrong input
    if clf not in switcher:
        print("Please use one of the classifier arguments:")
        print("-svm")
        print("-nb")
        print("-rf")
        print("-lstm")
        exit(1)

    return switcher.get(clf)

if __name__ == "__main__":
    main()