import sys, joblib, os, socket, Classifiers.lstm, Classifiers.multisvm

from sklearn.svm import SVC
from sklearn.pipeline import Pipeline
from sklearn.feature_extraction.text import TfidfVectorizer
sys.path.insert(0, './Model/')

import time
import warnings
warnings.filterwarnings("ignore")

#https://www.geeksforgeeks.org/socket-programming-multi-threading-python/
# address and port is arbitrary

def server(host='127.0.0.1', port=9999):
  # create socket
  with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
    sock.bind((host, port))

    sock.listen(100) # number of connections in buffer?

    ready_signaler()

    # permit to access
    conn, addr = sock.accept()
    try:
      with conn as c:
        while True:
          request = c.recv(4096)
          if not request:
            print("Got here")

          evalstring = str(request,'utf-8')
          #run svm-script here
          # main should take argument such that program can pick what script to run here

          res = clf.predict([evalstring])
          c.sendall(str(res[0]).encode('utf-8'))
    except:
      print("Shutting down server...")

def ready_signaler():
    signaler = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_address = ('localhost', 9998)
    signaler.connect(server_address)
    signal = "server ready"
    signaler.sendall(signal.encode('utf-8'))

# GLOBALS
dir_path = os.path.dirname(os.path.realpath(__file__))
model_path = dir_path + "/Classifiers/Model"


# Get classifier from supplied argument
def get_classifier(clf):
    switcher = {
        "svm": Classifiers.multisvm, #joblib.load(model_path + "/svm_pipeline.joblib"),
        # "nb": joblib.load(model_path + "/nb_pipeline.joblib"),
        "rf": joblib.load(model_path + "/rf_pipeline.joblib"),
        "lstm": Classifiers.lstm
    }
    
    # Check if wrong input
    if clf not in switcher:
        print("Please use one of the classifier arguments:")
        print("svm")
        print("nb")
        print("rf")
        print("lstm")
        exit(1)

    return switcher.get(clf)



if __name__ == "__main__":
  clf = get_classifier(sys.argv[1])
  server()