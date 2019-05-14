import argparse, joblib, csv, sys, os
import numpy as np
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches

from sklearn.svm import SVC
from sklearn.svm import SVR
from sklearn.pipeline import Pipeline
from sklearn.decomposition import PCA
from sklearn.decomposition import LatentDirichletAllocation
from sklearn.manifold import TSNE
from sklearn.metrics import confusion_matrix
from sklearn.metrics import accuracy_score
from sklearn.model_selection import train_test_split
from sklearn.feature_extraction.text import TfidfVectorizer

'''
SVM regression classifier
'''

# GLOBALS
dir_path = os.path.dirname(os.path.realpath(__file__))
data_path = dir_path + '/TrainingData/training_data.csv'
model_path = dir_path + '/Model/svm_pipeline.joblib'
stop_words_path = dir_path + '/TrainingData/stop_words_da.txt'

# Train our SVM model
def train_model(X_train, y_train):

    # Create data processing and classifier pipeline
    svm_pipeline = Pipeline([
        ('tfidf', TfidfVectorizer(sublinear_tf=False,
                                  # stop_words=load_stop_words(),
                                  use_idf=True,
                                  smooth_idf=False,
                                  lowercase=True,
                                  ngram_range=(1,2),
        )),
        ('svm', SVR(kernel='linear',
                    gamma='scale',
                    C=1.0,
                    shrinking=True,
        )),
    ])
    svm_pipeline = svm_pipeline.fit(X_train, y_train)

    joblib.dump(svm_pipeline, model_path)
    return svm_pipeline

# Loading data
def load_training_data(encoding='utf8'):
    csv_reader = csv.reader(open(data_path, encoding=encoding))
    corpus = []
    labels = []

    # Saving comments and likes in seperate lists
    for row in csv_reader:
        corpus.append(row[1])
        # labels.append(int(row[0]))

        if int(row[0]) < 0:
            labels.append(-1)
        elif int(row[0]) > 0:
            labels.append(1)
        else:
            labels.append(0)           

    labels = np.asarray(labels)

    return corpus, labels

# Get list of stop words
def load_stop_words():
    stop_words = []

    stop_words_list = open(stop_words_path, 'r')
    for word in stop_words_list.readlines():
        stop_words.append(word)

    return stop_words

def visualise_data(pipeline, X, y):
    tfidf = pipeline.named_steps['tfidf']
    X_transformed = tfidf.transform(X).todense()
    
    # TSNE
    # data2D = TSNE(n_components=3).fit_transform(X_transformed)
    
    # LDA
    # data2D = LatentDirichletAllocation(n_components=5).fit_transform(X_transformed)

    # PCA
    data2D = PCA(n_components=2).fit_transform(X_transformed)
    
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

    plt.show()

# <------------------------>
# <- EVALUATION FUNCTIONS ->
# <------------------------>
def get_svr_score(pipeline, X_test, y_test):
    # Getting the R squared (R^2) score with the same data used for training. This needs to change
    tfidf = pipeline.named_steps['tfidf']
    svm = pipeline.named_steps['svm']
    
    X_test_transformed = tfidf.transform(X_test)
    return svm.score(X_test_transformed, y_test)
    
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
X, y = load_training_data()
X_train, X_test, y_train, y_test = train_test_split(X, y) 

pipeline = train_model(X_train, y_train)
X_test = pipeline.named_steps['tfidf'].transform(X_test)
svr_score = pipeline.named_steps['svm'].score(X_test, y_test)

print('R^2 score: {}'.format(svr_score))

# visualise_data(pipeline, X, y)

