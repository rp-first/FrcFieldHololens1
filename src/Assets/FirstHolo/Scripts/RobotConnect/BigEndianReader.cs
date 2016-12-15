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



using System.IO;
using System;

class NetworkEndianReader : BinaryReader
{
    private byte[] a16 = new byte[2];
    private byte[] a32 = new byte[4];
    private byte[] a64 = new byte[8];

    public NetworkEndianReader(System.IO.Stream stream) : base(stream) { }

    public override int ReadInt32()
    {
        a32 = base.ReadBytes(4);
        if(BitConverter.IsLittleEndian) Array.Reverse(a32);
        return BitConverter.ToInt32(a32, 0);
    }
    public override short ReadInt16()
    {
        a16 = base.ReadBytes(2);
        if (BitConverter.IsLittleEndian) Array.Reverse(a16);
        return BitConverter.ToInt16(a16, 0);
    }
    public override long ReadInt64()
    {
        a64 = base.ReadBytes(8);
        if (BitConverter.IsLittleEndian) Array.Reverse(a64);
        return BitConverter.ToInt64(a64, 0);
    }
    public override float ReadSingle()
    {
        a32 = base.ReadBytes(4);
        if (BitConverter.IsLittleEndian) Array.Reverse(a32);
        return BitConverter.ToSingle(a32, 0);
    }
    public override double ReadDouble()
    {
        a64 = base.ReadBytes(8);
        if (BitConverter.IsLittleEndian) Array.Reverse(a64);
        return BitConverter.ToSingle(a64, 0);
    }

}
