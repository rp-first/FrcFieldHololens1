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
using UnityEngine.UI;

namespace FirstUtilities
{
	public class SimpleSubscibeLabel : MonoBehaviour
	{
		public Text nameText;
		public Text dataText;
		public int subscibeIndex = 0;

		public void Start() {
			UWPServer.Publisher.Subscribe (subscibeIndex, onUpdatedValue);
		}

		void onUpdatedValue(ValueItem vi) {
			if (nameText != null) {
				nameText.text = vi.name;
			}

			if (dataText != null) {
				dataText.text = vi.value.ToString();
			}
		}
	}
}

