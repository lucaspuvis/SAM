import argparse, joblib, csv, sys, os
import numpy as np
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches

from mpl_toolkits.mplot3d import Axes3D

from sklearn.naive_bayes import *
from sklearn.pipeline import Pipeline
from sklearn.decomposition import PCA
from sklearn.metrics import confusion_matrix
from sklearn.model_selection import train_test_split, StratifiedKFold, GridSearchCV
from sklearn.feature_extraction.text import TfidfVectorizer

'''
SVM classifier
'''

# GLOBALS
# Paths
dir_path = os.path.dirname(os.path.realpath(__file__))
data_path = dir_path + '/TrainingData/training_data.csv'
test_data_path = dir_path + '/TrainingData/test_data.csv'
model_path = dir_path + '/Model/nb_pipeline.joblib'
stop_words_path = dir_path + '/TrainingData/stop_words_da.txt'

# Rest
test_size = 0.1

# Train our SVM model
def train_model(X, y, auto_split=True):
    # Create data processing and classifier pipeline
    svm_pipeline = Pipeline([
        ('tfidf', TfidfVectorizer(ngram_range=(1,3),
                                  analyzer='word',
                                #   stop_words=load_stop_words()
                                #   use_idf=True,
                                #   smooth_idf=True,
                                #   sublinear_tf=True
        )),
        ('nb', MultinomialNB())
    ])

    # Parameters for Grid Search. This is used for finding the best values for processing and classifying
    parameters = {#'tfidf__stop_words':(load_stop_words(), None),
                  'tfidf__use_idf':(True, False),
                  'tfidf__smooth_idf':(True, False),
                  'tfidf__sublinear_tf':(True, False),
    }
    skf = StratifiedKFold(10, True)
    clf = GridSearchCV(svm_pipeline, parameters, cv=skf.split(X, y), verbose=3)
    if auto_split is True:
        X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.1)
        clf.fit(X_train, y_train)
        clf = clf.best_estimator_

        cm = confusion_matrix(y_test, clf.predict(X_test))
        plt.figure()
        plot_confusion_matrix(cm)
        svm_score = clf.score(X_test, y_test)

        print(cm)
        print('SVM Score: {}'.format(round(svm_score*100, 4)))
        plt.show()
    else:
        X_test, y_test = load_test_dataset()
        clf.fit(X, y)
        clf = clf.best_estimator_

        cm = confusion_matrix(y_test, clf.predict(X_test))
        plt.figure()
        plot_confusion_matrix(cm)
        svm_score = clf.score(X_test, y_test)

        print(cm)
        print('SVM Score: {}'.format(round(svm_score*100, 4)))
        plt.show()

    # plot_data_2d(svm_pipeline.named_steps['tfidf'].transform(X_train), y_train)
    joblib.dump(clf, model_path)
    return clf

def load_dataset(encoding='utf8'):
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
        #X.append(row[1])
        if int(row[0]) < 0:
            X.append(row[1])
            y.append(-1)
        elif int(row[0]) > 0:
            X.append(row[1])
            y.append(1)
        else:
            pass
            #y.append(-0)

    y = np.asarray(y)
    X = np.asarray(X)

    return X, y

def load_test_dataset(encoding='utf8'):
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
    csv_reader = csv.reader(open(test_data_path, encoding=encoding))
    X, y = [], []

    # Saving comments and likes in seperate lists
    for row in csv_reader:
        #X.append(row[1])
        if int(row[0]) < 0:
            X.append(row[1])
            y.append(-1)
        elif int(row[0]) > 0:
            X.append(row[1])
            y.append(1)
        else:
            pass
            #y.append(0)           

    y = np.asarray(y)
    X = np.asarray(X)

    return X, y


# Get list of stop words
def load_stop_words():
    stop_words = []

    stop_words_list = open(stop_words_path, 'r')
    for word in stop_words_list.readlines():
        stop_words.append(word.replace('\n', ''))

    return stop_words

# <---------------------->
# <- PLOTTING FUNCTIONS ->
# <---------------------->

def plot_data_2d(X_transformed, y):
    # PCA
    data2D = PCA(n_components=2).fit_transform(X_transformed.todense())
    
    # Plot the datapoints with different colors depending on label
    for i in range(0, len(data2D)):
        if y[i] < 0:
            plt.plot(data2D[i, 0], data2D[i, 1], "yo")
        elif y[i] == 0:
            plt.plot(data2D[i, 0], data2D[i, 1], "bo")
        else:
            plt.plot(data2D[i, 0], data2D[i, 1], "co")
            
    # Labels for the plot        
    negative_plt = mpatches.Patch(color='yellow', label='Negative')
    neutral_plt = mpatches.Patch(color='blue', label='Neutral')
    positive_plt = mpatches.Patch(color='cyan', label='Positive')
    plt.legend(handles=[positive_plt, neutral_plt, negative_plt])
    plt.show

def plot_data_3d(X_transformed, y):
    '''
    Loads training data and splits it into test and train sets
    Parameters
    -----------
    X_transformed: The corpus transformed to a feature space,
    y: The labels
    '''

    fig = plt.figure()
    ax = fig.add_subplot(111, projection='3d')

    data3d = PCA(n_components=3).fit_transform(X_transformed.todense())
    # data3d = TSNE(n_components=3).fit_transform(X_transformed.todense())

    # 
    neg_xs, neg_ys, neg_zs = [], [], []
    neu_xs, neu_ys, neu_zs = [], [], []
    pos_xs, pos_ys, pos_zs = [], [], []

    for i in range(0, len(y)):
        if y[i] < 0:
            neg_xs.append(data3d[i, 0])
            neg_ys.append(data3d[i, 1])
            neg_zs.append(data3d[i, 2])
        if y[i] == 0:
            neu_xs.append(data3d[i, 0])
            neu_ys.append(data3d[i, 1])
            neu_zs.append(data3d[i, 2])
        else:
            pos_xs.append(data3d[i, 0])
            pos_ys.append(data3d[i, 1])
            pos_zs.append(data3d[i, 2])

    ax.scatter(neg_xs, neg_ys, neg_zs, c='b')
    ax.scatter(neu_xs, neu_ys, neu_zs, c='r')
    ax.scatter(pos_xs, pos_ys, pos_zs, c='g')
    ax.set_xlabel('X')
    ax.set_ylabel('Y')
    ax.set_zlabel('Z')

def plot_confusion_matrix(cm, title='SVM Confusion matrix', cmap=plt.get_cmap('Blues')):
    plt.imshow(cm, interpolation='nearest', cmap=cmap)
    plt.title(title)
    plt.colorbar()
    tick_marks = np.arange(3)
    plt.xticks(tick_marks, [-1, 0, 1], rotation=45)
    plt.yticks(tick_marks, [-1, 0, 1])
    plt.tight_layout()
    plt.ylabel('True label')
    plt.xlabel('Predicted label')

# <---------------------->
# <- SCRIPT STARTS HERE ->
# <---------------------->

# Train model first time
X, y = load_dataset() 

pipeline = train_model(X, y, False)
print(pipeline.named_steps['tfidf'])
# X_transformed = pipeline.named_steps['tfidf'].transform(X)
# plot_data_2d(X_transformed, y)
