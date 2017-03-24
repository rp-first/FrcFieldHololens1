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




public class ValueItem
{
	public ValueItem(string name, object value, object defaultValue)
	{
		this.name = name;
		this.value = value;
		this.defaultValue = defaultValue;
	}
	public readonly object value;
	public readonly object defaultValue;
	public readonly string name;
}

public enum PROTOCOL
{
	NONE,
	UDP,
	TCP
}

public enum MSG_TYPE: byte
{
    UNKNOWN,
    PARAMS,
    VIDEO,
    ACK=0xFF,
    NACK=0xFE
}
