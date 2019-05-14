import argparse, joblib, csv, sys, os
import numpy as np
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches

from mpl_toolkits.mplot3d import Axes3D

from sklearn.svm import SVC, LinearSVC
from sklearn.pipeline import Pipeline
from sklearn.decomposition import PCA
from sklearn.metrics import confusion_matrix, f1_score
from sklearn.model_selection import train_test_split, StratifiedKFold, GridSearchCV
from sklearn.feature_extraction.text import TfidfVectorizer

'''
Experimental SVM Classifier that manually trains two SVM's
'''

dir_path = os.path.dirname(os.path.realpath(__file__))
data_path = dir_path + '/TrainingData/training_data_all.csv'
test_data_path = dir_path + '/TrainingData/test_data.csv'
pos_model_path = dir_path + '/Model/svm_pos_pipeline.joblib'
neg_model_path = dir_path + '/Model/svm_neg_pipeline.joblib'
stop_words_path = dir_path + '/TrainingData/stop_words_da.txt'

class ExperimentalSVM:
    def __init__(self):
        X, y = self.load_dataset()
        X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.1)
        X_train, y_pos, y_neg = self.load_training_data(X_train, y_train)
        self.clf_pos, self.clf_neg = self.train_classifiers(X_train, y_pos, y_neg, force_train=True)

        fscore = self.f1score(X_test, y_test)
        acc = self.evaluate(X_test, y_test)

        print('f_score: {}'.format(fscore))
        print('Accuracy: {}'.format(acc))
    
    def load_dataset(self, encoding='utf8', squish_classes=True):
        '''
        Loads training data and splits it into test and train sets
        Parameters
        -----------
        encoding: The encoding of the file loaded. Default is UTF-8
        
        Returns
        -------
        X: The sentences,
        y: The labels
        '''
        csv_reader = csv.reader(open(data_path, encoding=encoding))
        X, y = [], []

        # Saving comments and likes in seperate lists
        for row in csv_reader:
            X.append(row[1])
            if squish_classes:
                if int(row[0]) < 0:
                    y.append(-1)
                elif int(row[0]) > 0:
                    y.append(1)
                else:
                    y.append(0)           
            else:
                y.append(row[0])        

        y = np.asarray(y)
        X = np.asarray(X)

        return X, y

    def load_training_data(self, X, y, encoding='utf8'):
        '''
        Loads training data and splits it into test and train sets
        Parameters
        ----------
        encoding: The encoding of the file loaded. Default is UTF-8
        
        Returns
        -------
        X: The phrases as a numpy array
        y_pos: The labels for the positive classifier as a numpy array
        y_neg: The labels for the negative classifier as a numpy array
        '''
        X_pos, X_neu, X_neg = [], [], []
        y_pos, y_neu, y_neg = [], [], []        
        # with open(data_path, encoding=encoding) as data:
        #     csv_reader = csv.reader(data)
        #     for row in csv_reader:
        #         if int(row[0]) < 0:
        #             X_neg.append(row[1])
        #             y_neg.append(-1)
        #         elif int(row[0]) > 0:
        #             X_pos.append(row[1])
        #             y_pos.append(1)
        #         else:
        #             X_neu.append(row[1])
        #             y_neu.append(0)
        for phrase, label in zip(X, y):
            if int(label) < 0:
                X_neg.append(phrase)
                y_neg.append(-1)
            elif int(label) > 0:
                X_pos.append(phrase)
                y_pos.append(1)
            else:
                X_neu.append(phrase)
                y_neu.append(0)

        X = X_neg + X_neu + X_pos

        # Create the labels for the negative/rest classifier
        y_neg_rest = [0 for x in range(0, len(y_neu) + len(y_pos))]
        y_neg = y_neg + y_neg_rest

        # Create the labels for the positive/rest classifier
        y_pos_rest = [0 for x in range(0, len(X) - len(y_pos))]
        print('X: {}'.format(len(X)))
        print('y_pos: {}'.format(len(y_pos)))   
        print('y_pos_rest: {}'.format(len(y_pos_rest)))
        y_pos = y_pos_rest + y_pos

        return np.asarray(X), np.asarray(y_pos), np.asarray(y_neg)


    def train_classifiers(self, X, y_pos, y_neg, force_train=False):
        if os.path.isfile(pos_model_path) and os.path.isfile(neg_model_path) and not force_train:
            return joblib.load(pos_model_path), joblib.load(neg_model_path)

        svm_pipeline = Pipeline([
        ('tfidf', TfidfVectorizer(ngram_range=(1,10),
                                  analyzer='char_wb',
                                  stop_words=self.load_stop_words(),
                                  use_idf=False,
                                  smooth_idf=True,
                                  sublinear_tf=False)),
        ('svm', LinearSVC(C=3))
        ])

        # Parameters for Grid Search. This is used for finding the best values for processing and classifying
        parameters = {#'tfidf__max_df':(0.25, 0.50, 0.75, 1.0),
                    #'tfidf__min_df':(1, 2, 3),
                    #'tfidf__use_idf':(True, False),
                    #'tfidf__smooth_idf':(True, False),
                    # 'tfidf__sublinear_tf':(True, False),
        }

        clf_neg = GridSearchCV(svm_pipeline, parameters, cv=2, verbose=3, n_jobs=-1)
        clf_pos = GridSearchCV(svm_pipeline, parameters, cv=2, verbose=3, n_jobs=-1)

        clf_neg.fit(X, y_neg)        
        clf_pos.fit(X, y_pos)        

        joblib.dump(clf_neg, neg_model_path)
        joblib.dump(clf_pos, pos_model_path)
        return clf_pos, clf_neg


    def load_stop_words(self):
        stop_words = []

        stop_words_list = open(stop_words_path, 'r')
        for word in stop_words_list.readlines():
            stop_words.append(word)

        return stop_words
    

    def predict(self, sentence):
        sentence = [sentence]
        res_pos = self.clf_pos.predict(sentence)[0]
        res_neg = self.clf_neg.predict(sentence)[0]

        res = res_neg + res_pos
        if res == 1 or res == -1:
            return res
        elif res_pos == 0 and res_neg == 0:
            return res
        else:
            return 0
            # raise Exception('Result was invalid. res_pos: {}, res_neg: {}, res: {}'.format(res_pos, res_neg, res))

    def evaluate(self, X_test, y_test, encoding='utf8'):
        # X_test, y_test = [], []
        # with open(test_data_path, encoding=encoding) as data:
            # csv_reader = csv.reader(data)
            # for row in csv_reader:
                # X_test.append(row[1])
                # y_test.append(row[0])
        # 
        # X_test = np.asarray(X_test)
        # y_test = np.asarray(y_test)

        res_pos = self.clf_pos.predict(X_test)
        res_neg = self.clf_neg.predict(X_test)

        correct = 0
        false = 0
        for y, pos, neg in zip(y_test, res_pos, res_neg):
            res = pos + neg
            if res == int(y):
                correct += 1
            else:
                false += 1

        return correct/901*100

    def f1score(self, X_test, y_test, encoding='utf8'):
        y_pred = []
        for X in X_test:
            y_pred.append(self.predict(X))

        return f1_score(y_true=y_test, y_pred=y_pred, average='weighted')

if __name__ == '__main__':
    clf = ExperimentalSVM()
