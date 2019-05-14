import joblib
import Classifiers.classifier_svm as svm

modelpath = svm.dir_path + '/Model/'

def load_model(filepath):
    return joblib.load(filepath)

if __name__ != '__main__':
    is_pos = load_model(modelpath + 'posSvm.joblib')
    is_neg = load_model(modelpath + 'negSvm.joblib')
    is_neu = load_model(modelpath + 'neuSvm.joblib')
    polarizer = load_model(modelpath + 'polarity.joblib')

def train_svm(X, y, X_test, y_test, modelpath):
        return svm.train_model(X, y, X_test, y_test, modelpath, False)

def main():
    all_train_x, all_train_y = svm.load_dataset()
    test_x, test_y = svm.load_test_dataset()

    positives, negatives, neutrals = [], [], []
    non_pos, non_neg, non_neu = [], [], []
    test_pos, test_neu, test_neg = [], [], []

    for i in range(0, len(all_train_x)):
        x, y = all_train_x[i], all_train_y[i]
        
        if y > 0:
            positives.append(x)
        elif y < 0: 
            negatives.append(x)
        else:
            neutrals.append(x)

    non_pos = negatives + neutrals
    non_neg = positives + neutrals
    non_neu = positives + negatives

    for i in range(0, len(test_x)):
        x, y = test_x[i], test_y[i]
        
        if y > 0:
            test_pos.append(x)
        elif y < 0: 
            test_neg.append(x)
        else:
            test_neg.append(x)

    # Positive vs non-positive
    X1 = positives + non_pos
    y1 = [True for _ in positives] + [False for _ in non_pos]
    testX1 = test_pos + test_neg + test_neu
    testy1 = [True for _ in test_pos] + [False for _ in test_neg + test_neu]

    # Negative vs non-negative
    X2 = negatives + non_neg
    y2 = [True for _ in negatives] + [False for _ in non_neg]
    testX2 = test_neg + test_pos + test_neu
    testy2 = [True for _ in test_neg] + [False for _ in test_pos + test_neu]

    # Neutral vs non-neutral
    X3 = neutrals + non_neu
    y3 = [True for _ in neutrals] + [False for _ in non_neu]
    testX3 = test_neu + test_neg + test_pos
    testy3 = [True for _ in test_neu] + [False for _ in test_neg + test_pos]

    X4 = positives + negatives
    y4 = [1 for x in positives] + [-1 for x in negatives]
    testX4 = test_pos + test_neg
    testy4 = [1 for _ in test_pos] + [-1 for _ in test_neg]

    print("Training positive svm")
    train_svm(X1, y1, testX1, testy1, modelpath + 'posSvm.joblib')
    print("Training negative svm")
    train_svm(X2, y2, testX2, testy2, modelpath + 'negSvm.joblib')
    print("Training neutral svm")
    train_svm(X3, y3, testX3, testy3, modelpath + 'neuSvm.joblib')
    print("Training polarity svm")
    train_svm(X4, y4, testX4, testy4, modelpath + 'polarity.joblib')

# Need to find the right order to predict in
def predict(sentences):
    results = []

    for i in range(0, len(sentences)):
        s = [sentences[i]]

        neu = is_neu.predict(s)
        pos = is_pos.predict(s)
        neg = is_neg.predict(s)
        polarity = polarizer.predict(s)[0]

        if not(pos) and not(neg) and neu:
            results.append(0)
            continue
        if pos and neg and not(neu):
            print("{0} was both positive and negative".format(sentences[i]))
            results.append(polarity)
            continue
        if pos:
            results.append(1)
            continue
        if neg:
            results.append(-1)
            continue
        else:
            results.append(0);

        # if pos:
        #     if polarity > 0:
        #         results.append(1)
        #         continue
        #     if not(neg or neu):
        #         results.append(1)
        #         continue
        #     if neu or neg:
        #         results.append(0)
        #         continue
        # if neg:
        #     if polarity < 0:
        #         results.append(-1)
        #         continue
        #     if not(neu):
        #         results.append(-1)
        #         continue
        #     if neu:
        #         results.append(0)
        #         continue
        # else:
        #     results.append(0)

    return results

if (__name__ == '__main__'):
   main()
