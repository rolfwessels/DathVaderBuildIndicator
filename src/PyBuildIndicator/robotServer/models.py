from RPi import GPIO
import os
import re
import urllib
import urllib2
from robotServer.helperClasses import MediaPlayer

TEXT_SPEECH = "Text2Speech"

PLAYSOUND = "playsound"
GP_IO = 'GpIO'

RESOURCES_TEXT_SPEACH_ = 'Resources/text2speach/'
RESOURCES_SOUNDS_ = 'Resources/sounds/'


class Sequences(object):
    def __init__(self, typeName):
        self.__type = typeName
        self.Done = False
        self.BeginTime = 0


    def get_Type(self):
        return self.__type

    Type = property(fget=get_Type)

    def Execute(self, timedDifference):
        if timedDifference > self.BeginTime and not self.Done:
            self.ExecuteFirstInstance()
            self.Done = True
        pass

    def ExecuteFirstInstance(self):
        pass


class SequencesPlaySound(Sequences):
    def __init__(self, fileName=""):
        super(SequencesPlaySound, self).__init__(PLAYSOUND)
        self.File = fileName

    def ExecuteFirstInstance(self):
        print "play the file ", self.File
        MediaPlayer().Play(RESOURCES_SOUNDS_ + self.File)
        super(SequencesPlaySound, self).ExecuteFirstInstance()


class SequencesGpIo(Sequences):
    def __init__(self, pin, isOn):
        super(SequencesGpIo, self).__init__(GP_IO)
        self.Pin = pin
        self.IsOn = isOn

    def ExecuteFirstInstance(self):
        print "set pin", self.Pin, " to ", self.IsOn
        GPIO.output(self.Pin, self.IsOn)
        super(SequencesGpIo, self).ExecuteFirstInstance()


class SequencesText2Speech(Sequences):
    def __init__(self, text=""):
        super(SequencesText2Speech, self).__init__(TEXT_SPEECH)
        self.Text = text

    def ConvertToFileName(self, text):
        return re.sub('[^A-z0-9]', '_', text);

    def Download(self, url, toFile=None):
        request = urllib2.Request(url)
        request.add_header('User-Agent',
                           'Mozilla/5.0 (Windows; U; Windows NT 5.1; it; rv:1.8.1.11) Gecko/20071127 Firefox/2.0.0.11')
        opener = urllib2.build_opener()
        f = open(toFile, 'w')
        f.write(opener.open(request).read())
        f.flush()
        f.close()

    def ExecuteFirstInstance(self):
        self.SaveTo = RESOURCES_TEXT_SPEACH_ + self.ConvertToFileName(self.Text) + '.mp3'
        if not os.path.exists(self.SaveTo):
            url = 'http://translate.google.com/translate_tts?tl=en&q=' + urllib.quote_plus(self.Text)
            print "downloading file " + self.SaveTo
            self.Download(url, self.SaveTo)
        MediaPlayer().Play(self.SaveTo)
        super(SequencesText2Speech, self).ExecuteFirstInstance()


class Passive(object):
    def __init__(self, params=None):
        self.Initialize()
        if params is not None:
            self.__dict__.update(params)
            self.Compositions = []
            for v in params["Compositions"]:
                self.Compositions.append(Choreography(v))

    def Initialize(self):
        self.Interval = 5000
        self.StartTime = "06:00"
        self.SleepTime = "19:00"
        self.LastRun = None
        self.Compositions = []


class Choreography(object):
    def __init__(self, params=None):
        self.__sequences = []
        if params is not None:
            self.__dict__.update(params)
            self.__sequences = []
            for v in params["Sequences"]:
                sequences = Sequences(v['Type'])
                if v['Type'] == PLAYSOUND:
                    sequences = SequencesPlaySound(v['File'])
                elif v['Type'] == GP_IO:
                    sequences = SequencesGpIo(v['Pin'], v['IsOn'])
                elif v['Type'] == TEXT_SPEECH:
                    sequences = SequencesText2Speech(v['Text'])
                sequences.BeginTime = v['BeginTime']
                self.__sequences.append(sequences)

    def get_Sequences(self):
        return self.__sequences

    def set_Sequences(self, value):
        self.__sequences = value

    Sequences = property(fget=get_Sequences, fset=set_Sequences)

    @staticmethod
    def SimpleChoreographyPlaySound(fileName):
        choreography = Choreography()
        choreography.Sequences.append(SequencesPlaySound(fileName))
        return choreography

    @staticmethod
    def SimpleSequencesText2Speech(text):
        choreography = Choreography()
        choreography.Sequences.append(SequencesText2Speech(text))
        return choreography

    @staticmethod
    def SimpleSequencesGpIo(pin, state):
        choreography = Choreography()
        choreography.Sequences.append(SequencesGpIo(pin,state))
        return choreography
