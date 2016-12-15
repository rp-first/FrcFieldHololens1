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



using System;
using UnityEngine;
using System.Collections;

namespace FirstUtilities
{
	public class ValuePublisher : MonoBehaviour
	{
		public ValuePublisher ()
		{}

		public void Subscribe(int index, Action<ValueItem> onValueUpdate) {
			
			// create subscribers for index
			if (!_subscribers.ContainsKey (index)) {
				_subscribers [index] = new ArrayList ();
			}

			// get the subscriber list
			ArrayList subscribersForItem = _subscribers [index] as ArrayList;

			// add the new function
			subscribersForItem.Add (onValueUpdate);

			// update the list
			_subscribers [index] = subscribersForItem;

			// if there is already a value, 
			// send an initial value
			if (_values.ContainsKey (index)) {
				onValueUpdate (_values [index] as ValueItem);
			}
		}

		public void Unsubscribe(int index, Action<ValueItem> onValueUpdate) {

			// no key, no subscribers
			if (!_subscribers.ContainsKey (index)) {
				return;
			}

			// get the subscriber list
			ArrayList subscribersForItem = _subscribers [index] as ArrayList;

			// remove the function
			subscribersForItem.Remove (onValueUpdate);

			// update the list
			_subscribers [index] = subscribersForItem;
		}

		public void Publish(int index, ValueItem valueToPush) {
			if (!_values.ContainsKey(index)) {
				_values.Add (index, valueToPush);
				// update value
				StartCoroutine(pushToSubscribers(index));
			}
			else {
				ValueItem vi = _values [index] as ValueItem;
				if (vi.value != valueToPush.value) {
					_values[index] = valueToPush;
					// update value
					StartCoroutine(pushToSubscribers(index));
				}
			}
		}

		private Hashtable _values = new Hashtable();
		private Hashtable _subscribers = new Hashtable();

		private IEnumerator pushToSubscribers(int index) {
			
			// get the subscriber list
			ArrayList subscribersForItem = _subscribers [index] as ArrayList;

			if (subscribersForItem != null) {

				foreach (Action<ValueItem> onValueUpdate in subscribersForItem) {
					try {
						onValueUpdate (_values [index] as ValueItem);
					} catch (Exception e) {
						Debug.LogException (e);
					}
				}
			}

			yield break;
		}
	}
}
