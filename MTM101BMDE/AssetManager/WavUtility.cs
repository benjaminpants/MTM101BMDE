using System;
using System.IO;
using System.Text;
using UnityEngine;

// The following is a modified version of https://github.com/Unity3dAzure/UnityWebSocketDemo/blob/master/Assets/BingSpeech/WavDataUtility.cs
// Some checks have been removed, and all code relating to exporting audio clips has been completely destroyed.
// I'll probably switch this out for something else later, maybe I'll ask for Fasguy's FaDe library source.

namespace MTM101BaldAPI.AssetManager
{
    public static class WavDataUtility
    {

        #region Wav bytes to Unity AudioClip

        public static AudioClip ToAudioClip(byte[] fileBytes, string name = "wav")
        {
            int headerOffset = 0;
            int sampleRate = 16000;
            UInt16 channels = 1;
            int subchunk2 = fileBytes.Length;

            // check for RIF header
            Boolean includeWavFileHeader = true;
            byte[] fileHeaderChars = new byte[4];
            Array.Copy(fileBytes, 0, fileHeaderChars, 0, 4);
            string fileHeader = Encoding.ASCII.GetString(fileHeaderChars);
            if (!fileHeader.Equals("RIFF"))
            {
                includeWavFileHeader = false;
            }

            if (includeWavFileHeader)
            {
                int subchunk1 = BitConverter.ToInt32(fileBytes, 16);

                // NB: Only uncompressed PCM wav files are supported.
                UInt16 audioFormat = BitConverter.ToUInt16(fileBytes, 20);
                Debug.AssertFormat(audioFormat == 1 || audioFormat == 65534, "Detected format code: '{0}', but only PCM and WaveFormatExtensable uncompressed formats are supported.", audioFormat);

                channels = BitConverter.ToUInt16(fileBytes, 22);
                sampleRate = BitConverter.ToInt32(fileBytes, 24);
                UInt16 bitDepth = BitConverter.ToUInt16(fileBytes, 34);

                Debug.AssertFormat(bitDepth == 16, "Detected bit depth: '{0}', but only 16 bit format is supported.", bitDepth);

                headerOffset = 16 + 4 + subchunk1 + 4;
                subchunk2 = BitConverter.ToInt32(fileBytes, headerOffset);
            }

            float[] data;
            data = Convert16BitByteArrayToAudioClipData(fileBytes, headerOffset, subchunk2);

            AudioClip audioClip = AudioClip.Create(name, data.Length, (int)channels, sampleRate, false);
            audioClip.SetData(data, 0);
            return audioClip;
        }

        private static float[] Convert16BitByteArrayToAudioClipData(byte[] source, int headerOffset, int dataSize)
        {
            int wavSize = dataSize;

            // only required if there is a header
            if (headerOffset != 0)
            {
                wavSize = BitConverter.ToInt32(source, headerOffset);
                headerOffset += sizeof(int);
                Debug.AssertFormat(wavSize > 0 && wavSize == dataSize, "Failed to get valid 16-bit wav size: {0} from data bytes: {1} at offset: {2}", wavSize, dataSize, headerOffset);
            }

            int x = sizeof(Int16); // block size = 2
            int convertedSize = wavSize / x;

            //Debug.Log ("Convert header offset: " + headerOffset + " dataSize: " + dataSize + " size: " + convertedSize);

            float[] data = new float[convertedSize];

            Int16 maxValue = Int16.MaxValue;

            int offset = 0;
            int i = 0;
            while (i < convertedSize)
            {
                offset = i * x + headerOffset;
                data[i] = (float)BitConverter.ToInt16(source, offset) / maxValue;
                ++i;
            }

            Debug.AssertFormat(data.Length == convertedSize, "AudioClip .wav data is wrong size: {0} == {1}", data.Length, convertedSize);

            return data;
        }

        #endregion

    }
}