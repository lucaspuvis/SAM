import pickle, os, sys

stderr = sys.stderr
sys.stderr = open(os.devnull, 'w')
import keras as K
sys.stderr = stderr

#import keras as K

dir_path = os.path.dirname(os.path.realpath(__file__))
model_path      = dir_path + '/Model/LSTM_model.h5'
tokenizer_path  = dir_path + '/Model/tokenizer.pickle'

max_text_length = 20

def load_model(filepath):
    return K.models.load_model(filepath)
    
def load_tokenizer(filepath):
    with open(filepath, 'rb') as handle:
        return pickle.load(handle)

model = load_model(model_path)
tokenizer = load_tokenizer(tokenizer_path)

def predict_sentence(sentence):
    X = tokenizer.texts_to_sequences(sentence)

    X = K.preprocessing.sequence.pad_sequences(X, maxlen=max_text_length, padding='pre', truncating='pre')

    return model.predict_classes(X)[0][0]

def predict_sentences(sentences):
    X = tokenizer.texts_to_sequences(sentences)

    X = K.preprocessing.sequence.pad_sequences(X, maxlen=max_text_length, padding='pre', truncating='pre')

    return model.predict_classes(X)

def predict(X):
    results = []

    if type(X) == str:
        prediction = predict_sentence(X)
        results.append(prediction)
    else:
        predictions = predict_sentences(X)
        for pred in predictions:
            results.append(pred[0])
    
    return results

# Skal fikses så den siger enten -1 eller 1 afhængig af pred

# sentences = "jeg kan godt lige memes"

# sentences2 = ["jakob er min lillebror", "babyer hører ikke til i folketingssalen!"]

# print("prediction single string: {}".format(predict(sentences)))
# print("prediction string list : {}".format(predict(sentences2)))

