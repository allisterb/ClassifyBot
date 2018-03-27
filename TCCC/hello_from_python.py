import sys
import pip
import vaderSentiment

if __name__ == '__main__':
    # Download the data set from URL
    print("Hello from " + sys.executable)
    analyzer = vaderSentiment.SentimentIntensityAnalyzer()