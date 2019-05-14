# Sentiment Analysis Multi-tool
A multi-tool for danish sentiment analysis. It is the bachelor project of Steffan Eybye Christensen, Lucas Puvis de Chavannes, Peter Due Jensen & Mads Kongsbak, under supervision of assistant professor Leon Derczynski at ITU

# HOWTO

The executeable has several different classifiers and functionality that you can use.

To run a classifier use:

```
SAM svm -r MyData.csv
```

The available classifiers are:
* svm - Support vector machine
* rb - Rule based(lexical) classification
* lstm - Long term short memory 
* random - Random classification

If you want to create training/test data for the classifiers, you can instead do:
```
SAM data -r MyData.csv
```
This will random order your data and take 10% as test data.


# REQUIREMENTS
To run this program, you need Windows and the following software installed on your computer:
* .NET Core 2.1
* Python 3.6
* Pip3

# PYTHON CLASSIFIER
To run the classifier, navigate to the first Sentimentinator folder in a shell, and execute the following command to install the required packages:
```
pip3 install -r requirements.txt
```
There are multiple classifiers that can be trained
* classifier_svm.py
* classifier_svr.py
* classifier_rf.py

To train the models, run the python scripts and exchange the filename with the desired:
```
python3 Sentimentinator/Classifiers/classifier_svm.py
```
# LINKS
[Google drive](https://drive.google.com/drive/folders/1LtszLRUf3tT_FSs842AJzRE1S_DxCSbx?usp=sharing)

