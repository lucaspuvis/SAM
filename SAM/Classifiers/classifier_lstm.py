import numpy as np
import keras as K
import tensorflow as tf
import os, csv, pickle

from keras.preprocessing.text import Tokenizer
from keras.callbacks import EarlyStopping
from keras.callbacks import ModelCheckpoint
from keras.callbacks import CSVLogger

dir_path = os.path.dirname(os.path.realpath(__file__))
train_path = dir_path + '/TrainingData/training_data.csv'
test_path = dir_path + '/TrainingData/test_data.csv'

# Parameters for the neural network
max_words       = 10000
max_text_length = 20
embed_vec_len   = 64
bat_size        = 64
max_epochs      = 50

model_path      = dir_path + '/Model/LSTM_model.h5'
tokenizer_path  = dir_path + '/Model/tokenizer.pickle'

# Loads labeled data from csv file
def load_labeled_data(filepath):
    sentences, labels = [], []
    csv_reader = csv.reader(open(filepath, encoding='utf8'))

    for row in csv_reader:
        label = int(row[0])
        sentences.append(row[1])
        
        if label > 0:
            labels.append(1)
        elif label < 0:
            labels.append(-1)
        else:
            labels.append(0)
            
    return sentences, labels

def train_model(tokenizer, x_train, y_train, x_test, y_test, modelpath):
    X_train = tokenizer.texts_to_sequences(x_train)
    X_train = K.preprocessing.sequence.pad_sequences(X_train, maxlen=max_text_length, padding='pre', truncating='pre')

    X_test = tokenizer.texts_to_sequences(x_test)
    X_test = K.preprocessing.sequence.pad_sequences(X_test, maxlen=max_text_length, padding='pre', truncating='pre')

    print("Creating LSTM model")

    e_init      = K.initializers.RandomUniform(-0.01, 0.01)
    init        = K.initializers.glorot_uniform()
    simple_adam = K.optimizers.Adam()

    log_path = dir_path + '/Model/training.log'

    # Callbacks for the LSTM
    stop_early  = EarlyStopping(monitor='val_acc', patience=5, mode='auto') # Stop if there is no improvement after 3 epochs
    save_best   = ModelCheckpoint(model_path, monitor='val_acc', verbose=1, save_best_only=True, save_weights_only=False, mode='auto', period=1)
    csv_logger  = CSVLogger(log_path)

    callbacks = []
    callbacks.append(stop_early)
    callbacks.append(save_best)
    callbacks.append(csv_logger)

    model = K.models.Sequential()
    model.add(K.layers.Embedding(input_dim=max_words, output_dim=embed_vec_len, embeddings_initializer=e_init, mask_zero=True))
    model.add(K.layers.LSTM(units=100, kernel_initializer=init, dropout=0.5, recurrent_dropout=0.5))
    model.add(K.layers.Dense(units=1, kernel_initializer=init, activation='sigmoid'))

    model.compile(loss='binary_crossentropy', optimizer=simple_adam, metrics=['acc'])
    print(model.summary)

    print('Started training model')
    model.fit(X_train, y_train, epochs=max_epochs, batch_size=bat_size, shuffle=True, verbose=1, validation_data=(X_test, y_test), callbacks=callbacks)
    print('Training complete')

    model.save(modelpath)

    return model

def main():
    train_x, train_y = load_labeled_data(train_path)
    test_x, test_y = load_labeled_data(test_path)

    all_sentences = train_x + test_x

    tokenizer = Tokenizer(num_words=max_words, split=' ', lower=True)
    tokenizer.fit_on_texts(all_sentences)

    train_model(tokenizer, train_x, train_y, test_x, test_y, model_path)

    save_tokenizer(tokenizer_path, tokenizer)

def load_model(filepath):
    return K.models.load_model(filepath)

def save_tokenizer(filepath, tokenizer):
    with open(filepath, 'wb') as handle:
        pickle.dump(tokenizer, handle, protocol=pickle.HIGHEST_PROTOCOL)
    
def load_tokenizer(filepath):
    with open(filepath, 'rb') as handle:
        return pickle.load(handle)
    


if __name__ == '__main__':
    main()

model = load_model(model_path)
tokenizer = load_tokenizer(tokenizer_path)

def predict_sentence(sentence):
    X = tokenizer.texts_to_sequences(sentence)

    X = K.preprocessing.sequence.pad_sequences(X, maxlen=max_text_length, padding='pre', truncating='pre')

    y_prob = model.predict(X)
    return y_prob.argmax(axis=-1)


def predict_sentences(sentences):
    X = tokenizer.texts_to_sequences(sentences)

    X = K.preprocessing.sequence.pad_sequences(X, maxlen=max_text_length, padding='pre', truncating='pre')

    y_prob = model.predict(X)
    return y_prob.argmax(axis=-1)


sentence = "Du er den sødeste i verden, og jeg elsker dig! "
res = predict_sentence(sentence)

print("'{}' predicted as {}".format(sentence, res[0]))
