//    Copyright 2016 United States Government as represented by the
//    Administrator of the National Aeronautics and Space Administration.
//    All Rights Reserved.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//      http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.



//using UnityEngine.WSA.Speech;
using UnityEngine.Windows.Speech;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using System;

public class SpeechRecognizer : MonoBehaviour {
    [Serializable]
    public class SpeechKeywordEvent : UnityEvent<string> { }
 
    [Serializable]
    public class SpeechEvent
    {
        public string phrase;
        public SpeechKeywordEvent speechAction;
    }
    //Then let's add a few fields to your class to store the recognizer and keyword->action dictionary:

    KeywordRecognizer keywordRecognizer;
//Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();
    //Now add a keyword to the dictionary(e.g.inside of a Start() method). We're adding the "activate" keyword in this example:


        public List<SpeechEvent> eventTriggers;
//An example handler is:
private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
{
        // System.Action keywordAction;
        // if the keyword recognized is in our dictionary, call that Action.
        SpeechEvent handler = eventTriggers.Find((x) => x.phrase.Equals( args.text));
        Debug.Log("got the phrase: " + args.text);
        if (handler != null)
        {
            handler.speechAction.Invoke(args.text);
        }
}


//
	// Use this for initialization
	void Start () {

        //Create the keyword recognizer and tell it what we want to recognize:
        List<string> keywords = new List<string>();
        foreach (SpeechEvent eventHandler in eventTriggers)
            keywords.Add(eventHandler.phrase);
        keywordRecognizer = new KeywordRecognizer(keywords.ToArray());
        //Now register for the OnPhraseRecognized event
        
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;

        try
        {
            keywordRecognizer.Start();
        }
        catch (Exception e) { }
    }
    void OnDestroy()
    {
        if(keywordRecognizer != null)
            keywordRecognizer.Stop();
    }

}
