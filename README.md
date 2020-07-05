# Sentiment Analysis Multi-tool
A multi-tool for danish sentiment analysis. It is the bachelor project of Steffan Eybye Christensen, Lucas Puvis de Chavannes, Peter Due Jensen & Mads Kongsbak, under supervision of assistant professor Leon Derczynski at ITU

## HOWTO

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


## REQUIREMENTS
To run this program, you need Windows and the following software installed on your computer:
* .NET Core 2.1
* Python 3.6
* Pip3

## PYTHON CLASSIFIER
To run the classifier, navigate to the first Sentimentinator folder in a shell, and execute the following command to install the required packages:
```
pip3 install -r requirements.txt
```
There are two classifiers that can be trained
* classifier_svm.py
* classifier_svm_experimental.py

To train the models, run the python scripts and exchange the filename with the desired:
```
python3 Sentimentinator/Classifiers/classifier_svm.py
```

## FURTHER READING
Further information about the dataset we gathered can be read [here](https://github.com/steffan267/Sentiment-Analysis-on-Danish-Social-Media).

If you wish to read more about both the dataset and this software, you can read our [thesis](https://github.com/lucaspuvis/SAM/blob/master/Thesis.pdf).

## LICENSE
This code is licenced CC-BY: you may use it for commercial and non-commercial purposes, and you MUST acknowledge the source clearly if you so. Acknowledge this reference for SAM:

Mads Guldborg Kjeldgaard Kongsbak, Steffan Eybye Christensen, Lucas Høyberg Puvis de Chavannes, Peter Due Jensen. "Sentiment Analysis Multitool, SAM". 2019. Bachelor dissertation, IT University of Copenhagen.

Bibtex:
```
@techreport{SAMsentiment,
  author={Mads Guldborg Kjeldgaard Kongsbak and Steffan Eybye Christensen and Lucas Høyberg Puvis~de~Chavannes and Peter Due Jensen},
  title={Sentiment Analysis Multitool, SAM},
  year={2019},
  institution={IT University of Copenhagen},
}
```




For details, see [LICENSE.md](LICENSE.md).
