import sys, joblib, os, socket
import time
from _thread import *
import threading 
from sklearn.svm import SVC
from sklearn.pipeline import Pipeline
from sklearn.feature_extraction.text import TfidfVectorizer
sys.path.insert(0, './Model/')

import warnings
warnings.filterwarnings("ignore")

#https://www.geeksforgeeks.org/socket-programming-multi-threading-python/
# address and port is arbitrary

def server(host='127.0.0.1', port=9999):
  # create socket
  with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
    sock.bind((host, port))

    sock.listen(1000) # number of connections in buffer?

    ready_signaler()

    while True:
        # establish connection with client 
        c, addr = sock.accept()
        print("1")
        # Start a new thread and return its identifier 
        start_new_thread(threaded, (c,)) 

# thread fuction 
def threaded(c): 
    request = c.recv(4096)
    if request:
         evalstring = repr(request.decode('utf-8'))
         res = clf.predict([evalstring])
         c.sendall(str(res[0]).encode('utf-8'))
         time.sleep(200)
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
        "svm": joblib.load(model_path + "/svm_pipeline.joblib"),
        # "nb": joblib.load(model_path + "/nb_pipeline.joblib"),
        "rf": joblib.load(model_path + "/rf_pipeline.joblib")
    }
    
    # Check if wrong input
    if clf not in switcher:
        print("Please use one of the classifier arguments:")
        print("svm")
        print("nb")
        print("rf")
        exit(1)

    return switcher.get(clf)



if __name__ == "__main__":
  clf = get_classifier(sys.argv[1])
  server()