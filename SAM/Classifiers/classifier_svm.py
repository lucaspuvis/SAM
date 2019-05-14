import argparse, joblib, csv, sys, os
import numpy as np
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
import pandas as pd

from mpl_toolkits.mplot3d import Axes3D

from yellowbrick.text import TSNEVisualizer

from sklearn.cluster import KMeans
from sklearn.svm import SVC, LinearSVC
from sklearn.pipeline import Pipeline
from sklearn.decomposition import PCA
from sklearn.metrics import confusion_matrix, f1_score
from sklearn.model_selection import train_test_split, StratifiedKFold, GridSearchCV, learning_curve
from sklearn.feature_extraction.text import TfidfVectorizer

'''
SVM classifier
'''

# GLOBALS
# Paths
dir_path = os.path.dirname(os.path.realpath(__file__))
data_path = dir_path + '/TrainingData/training_data_all.csv'
test_data_path = dir_path + '/TrainingData/HypothesisData.csv'
model_path = dir_path + '/Model/svm_pipeline.joblib'
stop_words_path = dir_path + '/TrainingData/stop_words_da.txt'

# Rest

# Train our SVM model
def train_model(X, y, auto_split=False):
    # Create data processing and classifier pipeline
    svm_pipeline = Pipeline([
        ('tfidf', TfidfVectorizer(ngram_range=(1,10),
                                  analyzer='char_wb',
                                  stop_words=load_stop_words(),
                                  use_idf=False,
                                  smooth_idf=True,
                                  sublinear_tf=False
        )),
        ('svm', LinearSVC(C=3))
    ])

    # Parameters for Grid Search. This is used for finding the best values for processing and classifying
    parameters = {#'tfidf__stop_words':(load_stop_words(), None),
                #   'tfidf__smooth_idf':(True, False),
                #   'tfidf__sublinear_tf':(True, False),
    }

    out = open('svm_f1score.txt', 'w+')
    
    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.1)
    skf = StratifiedKFold(4, True)
    if auto_split is True:
        X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.1)
        clf = GridSearchCV(svm_pipeline, parameters, cv=skf.split(X_train, y_train), verbose=2, return_train_score=True, n_jobs=-1)
        clf.fit(X_train, y_train)
        clf = clf.best_estimator_

        cm = confusion_matrix(y_test, clf.predict(X_test))
        plt.figure()
        plot_confusion_matrix(cm)

        y_pred = clf.predict(X_test)
        f_score = f1_score(y_true=y_test, y_pred=y_pred, average='weighted')
        score = clf.score(X_test, y_test)
        out.write('{}, {}\n'.format(score, f_score))
    else:
        clf = GridSearchCV(svm_pipeline, parameters, cv=skf.split(X, y), verbose=2, return_train_score=True, n_jobs=-1)
        X_test, y_test = load_test_dataset(squish_classes=True)
        clf.fit(X, y)
        clf = clf.best_estimator_

        cm = confusion_matrix(y_test, clf.predict(X_test))
        plot_confusion_matrix(cm)
        svm_score = clf.score(X_test, y_test)

        y_pred = clf.predict(X_test)
        f_score = f1_score(y_true=y_test, y_pred=y_pred, average='weighted')
        out.write('{}, {}\n'.format(svm_score, f_score))

    print(cm)
    print('SVM Accuracy: {}'.format(round(svm_score*100, 4)))
    print('SVM F1 Score: {}'.format(round(f_score*100, 4)))

    joblib.dump(clf, model_path)
    return clf

def load_dataset(encoding='utf8', squish_classes=True):
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

def load_test_dataset(encoding='utf-8-sig', squish_classes=True):
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
    data2D = PCA(n_components=3).fit_transform(X_transformed.todense())
    
    # Plot the datapoints with different colors depending on label
    for i in range(0, len(data2D)):
        if int(y[i]) < 0:
            plt.plot(data2D[i, 0], data2D[i, 1], "yo")
        elif int(y[i]) == 0:
            plt.plot(data2D[i, 0], data2D[i, 1], "bo")
        else:
            plt.plot(data2D[i, 0], data2D[i, 1], "co")
            
    # Labels for the plot        
    negative_plt = mpatches.Patch(color='yellow', label='Negative')
    neutral_plt = mpatches.Patch(color='blue', label='Neutral')
    positive_plt = mpatches.Patch(color='cyan', label='Positive')
    plt.legend(handles=[positive_plt, neutral_plt, negative_plt])
    plt.show()

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
    plt.show()

# From https://scikit-learn.org/stable/auto_examples/model_selection/plot_learning_curve.html#sphx-glr-auto-examples-model-selection-plot-learning-curve-py
def plot_learning_curve(estimator, title, X, y, ylim=None, cv=None,
                        n_jobs=None, train_sizes=np.linspace(.1, 1.0, 10)):
    """
    Generate a simple plot of the test and training learning curve.

    Parameters
    ----------
    estimator : object type that implements the "fit" and "predict" methods
        An object of that type which is cloned for each validation.

    title : string
        Title for the chart.

    X : array-like, shape (n_samples, n_features)
        Training vector, where n_samples is the number of samples and
        n_features is the number of features.

    y : array-like, shape (n_samples) or (n_samples, n_features), optional
        Target relative to X for classification or regression;
        None for unsupervised learning.

    ylim : tuple, shape (ymin, ymax), optional
        Defines minimum and maximum yvalues plotted.

    cv : int, cross-validation generator or an iterable, optional
        Determines the cross-validation splitting strategy.
        Possible inputs for cv are:
          - None, to use the default 3-fold cross-validation,
          - integer, to specify the number of folds.
          - :term:`CV splitter`,
          - An iterable yielding (train, test) splits as arrays of indices.

        For integer/None inputs, if ``y`` is binary or multiclass,
        :class:`StratifiedKFold` used. If the estimator is not a classifier
        or if ``y`` is neither binary nor multiclass, :class:`KFold` is used.

        Refer :ref:`User Guide <cross_validation>` for the various
        cross-validators that can be used here.

    n_jobs : int or None, optional (default=None)
        Number of jobs to run in parallel.
        ``None`` means 1 unless in a :obj:`joblib.parallel_backend` context.
        ``-1`` means using all processors. See :term:`Glossary <n_jobs>`
        for more details.

    train_sizes : array-like, shape (n_ticks,), dtype float or int
        Relative or absolute numbers of training examples that will be used to
        generate the learning curve. If the dtype is float, it is regarded as a
        fraction of the maximum size of the training set (that is determined
        by the selected validation method), i.e. it has to be within (0, 1].
        Otherwise it is interpreted as absolute sizes of the training sets.
        Note that for classification the number of samples usually have to
        be big enough to contain at least one sample from each class.
        (default: np.linspace(0.1, 1.0, 5))
    """
    plt.figure()
    plt.title(title)
    if ylim is not None:
        plt.ylim(*ylim)
    plt.xlabel("Training examples")
    plt.ylabel("Score")
    train_sizes, train_scores, test_scores = learning_curve(
        estimator, X, y, cv=8, n_jobs=n_jobs, train_sizes=train_sizes)
    train_scores_mean = np.mean(train_scores, axis=1)
    train_scores_std = np.std(train_scores, axis=1)
    test_scores_mean = np.mean(test_scores, axis=1)
    test_scores_std = np.std(test_scores, axis=1)
    plt.grid()

    plt.fill_between(train_sizes, train_scores_mean - train_scores_std,
                     train_scores_mean + train_scores_std, alpha=0.1,
                     color="r")
    plt.fill_between(train_sizes, test_scores_mean - test_scores_std,
                     test_scores_mean + test_scores_std, alpha=0.1, color="g")
    plt.plot(train_sizes, train_scores_mean, 'o-', color="r",
             label="Training score")
    plt.plot(train_sizes, test_scores_mean, 'o-', color="g",
             label="Cross-validation score")

    plt.legend(loc="best")
    return plt

# <---------------------->
# <- SCRIPT STARTS HERE ->
# <---------------------->

# Train model first time
X, y = load_dataset(squish_classes=True)

pipeline = train_model(X, y, auto_split=False)
X_transformed = pipeline.named_steps['tfidf'].transform(X)

# tsne = TSNEVisualizer()
# tsne.fit(X_transformed, y)
# tsne.poof()
