using System;
using System.IO;

namespace SoundTest
{
    public class WaveStuff
    {

    }

    public static class WaveNoteFreq
    {
        // 4th octave
        public static double A = 220f;
        public static double Asharp = 466.16f;
        public static double B = 246.94f;
        public static double C = 261.63f;
        public static double Csharp = 277.18f;
        public static double D = 293.66f;
        public static double Dsharp = 311.13f;
        public static double E = 329.63f;
        public static double F = 349.23f;
        public static double Fsharp = 369.99f;
        public static double G = 392f;
        public static double Gsharp = 415.30f;
    }

    public static class WaveNoteLength
    {
        public static double Whole = 1.00;
        public static double ThreeQuarters = 0.75;
        public static double Half = 0.50;
        public static double Quarter = 0.25;
        public static double Eighth = 0.125;
    }

    public class WaveHeader
    {
        public string sGroupID; // RIFF
        public uint dwFileLength; // total file length minus 8, which is taken up by RIFF
        public string sRiffType; // always WAVE

        /// <summary>
        /// Initializes a WaveHeader object with the default values.
        /// </summary>
        public WaveHeader()
        {
            dwFileLength = 0;
            sGroupID = "RIFF";
            sRiffType = "WAVE";
        }
    }

    public class WaveFormatChunk
    {
        public string sChunkID;         // Four bytes: "fmt "
        public uint dwChunkSize;        // Length of header in bytes
        public ushort wFormatTag;       // 1 (MS PCM)
        public ushort wChannels;        // Number of channels
        public uint dwSamplesPerSec;    // Frequency of the audio in Hz... 44100
        public uint dwAvgBytesPerSec;   // for estimating RAM allocation
        public ushort wBlockAlign;      // sample frame size, in bytes
        public ushort wBitsPerSample;    // bits per sample

        /// <summary>
        /// Initializes a format chunk with the following properties:
        /// Sample rate: 44100 Hz
        /// Channels: Stereo
        /// Bit depth: 16-bit
        /// </summary>

        public WaveFormatChunk()
        {
            sChunkID = "fmt ";
            dwChunkSize = 16;
            wFormatTag = 1;
            wChannels = 2;
            dwSamplesPerSec = 44100;
            wBitsPerSample = 16;
            wBlockAlign = (ushort)(wChannels * (wBitsPerSample / 8));
            dwAvgBytesPerSec = dwSamplesPerSec * wBlockAlign;
        }

        public WaveFormatChunk(bool stereo)
        {
            sChunkID = "fmt ";
            dwChunkSize = 16;
            wFormatTag = 1;
            wChannels = stereo ? (ushort)2 : (ushort)1;
            dwSamplesPerSec = 22050;
            wBitsPerSample = 16;
            wBlockAlign = (ushort)(wChannels * (wBitsPerSample / 8));
            dwAvgBytesPerSec = dwSamplesPerSec * wBlockAlign;
        }
    }

    public class WaveDataChunk
    {
        public string sChunkID;     // "data"
        public uint dwChunkSize;    // Length of header in bytes
        public short[] shortArray;  // 8-bit audio

        /// <summary>
        /// Initializes a new data chunk with default values.
        /// </summary>
        public WaveDataChunk()
        {
            shortArray = new short[0];
            dwChunkSize = 0;
            sChunkID = "data";
        }
    }

    public enum WaveExampleType
    {
        ExampleSineWave = 0,
        TetrisWave = 1,
        MessingWave = 2,
        MonoWave = 3
    }

    public class WaveGenerator
    {
        // Header, Format, Data chunks
        WaveHeader header;
        WaveFormatChunk format;
        WaveDataChunk data;

        public WaveGenerator(WaveExampleType type)
        {
            // Init chunks
            header = new WaveHeader();
            format = type == WaveExampleType.MonoWave ? new WaveFormatChunk(false) : new WaveFormatChunk();
            data = new WaveDataChunk();

            // Fill the data array with sample data
            switch (type)
            {
                case WaveExampleType.ExampleSineWave:
                    {
                        // Number of samples = sample rate * channels * bytes per sample
                        uint numSamples = format.dwSamplesPerSec * format.wChannels;

                        // Initialize the 16-bit array
                        data.shortArray = new short[numSamples];

                        int amplitude = 32760; // Max amplitude for 16-bit audio
                        double freq = 440.0f; // Concert A: 440Hz

                        // The "angle" used in the function, adjusted for the number of channels and sample rate.
                        // This value is like the period of the wave.
                        double t = (Math.PI * 2 * freq) / (format.dwSamplesPerSec * format.wChannels);

                        for (uint i = 0; i < numSamples - 1; i++)
                        {
                            // Fill with a simple sine wave at max amplitude
                            for (int channel = 0; channel < format.wChannels; channel++)
                            {
                                data.shortArray[i + channel] = Convert.ToInt16(amplitude * Math.Sin(t * i));
                            }
                        }

                        // Calculate data chunk size in bytes
                        data.dwChunkSize = (uint)(data.shortArray.Length * (format.wBitsPerSample / 8));

                        break;
                    }
                case WaveExampleType.TetrisWave:
                    {
                        double[] tetrisNoteSequence =
                        {
                        WaveNoteFreq.E, WaveNoteFreq.B, WaveNoteFreq.C, WaveNoteFreq.D,
                        WaveNoteFreq.C, WaveNoteFreq.B, WaveNoteFreq.A, WaveNoteFreq.A,
                        WaveNoteFreq.C, WaveNoteFreq.E, WaveNoteFreq.D, WaveNoteFreq.C, WaveNoteFreq.B,
                        WaveNoteFreq.C, WaveNoteFreq.D, WaveNoteFreq.E, WaveNoteFreq.C, WaveNoteFreq.A
                        };

                        double[] tetrisNoteLength =
                        {
                        WaveNoteLength.Quarter, WaveNoteLength.Eighth, WaveNoteLength.Eighth, WaveNoteLength.Quarter,
                        WaveNoteLength.Eighth, WaveNoteLength.Eighth, WaveNoteLength.Quarter, WaveNoteLength.Eighth,
                        WaveNoteLength.Eighth, WaveNoteLength.Quarter, WaveNoteLength.Eighth, WaveNoteLength.Eighth, WaveNoteLength.ThreeQuarters,
                        WaveNoteLength.Eighth, WaveNoteLength.Quarter, WaveNoteLength.Quarter, WaveNoteLength.Quarter, WaveNoteLength.Half
                        };

                        double secondsLong = 0;
                        foreach (double noteLen in tetrisNoteLength)
                        {
                            secondsLong += noteLen;
                        }

                        // Number of samples = sample rate * channels * bytes per sample
                        uint numSamples = (uint)(format.dwSamplesPerSec * format.wChannels * secondsLong);

                        // Initialize the 16-bit array
                        data.shortArray = new short[numSamples];

                        // Max amplitude for 16-bit audio
                        int amplitude = 32760;

                        // Keep track of samples recorded
                        uint samplesRecorded = 0;

                        // Loop through each note
                        for (int i = 0; i < tetrisNoteSequence.Length; i++)
                        {
                            // The "angle" used in the function, adjusted for the number of channels and sample rate.
                            // This value is like the period of the wave.
                            double t = (Math.PI * 2 * tetrisNoteSequence[i]) / (format.dwSamplesPerSec * format.wChannels);

                            // The length in samples of the note
                            uint noteLengthSamples = (uint)(format.dwSamplesPerSec * format.wChannels * tetrisNoteLength[i]);

                            uint endTheNoteHere = samplesRecorded + noteLengthSamples;
                            for (uint j = samplesRecorded; j < endTheNoteHere - 1; j++)
                            {
                                // Convert to int16
                                short conv = Convert.ToInt16(amplitude * Math.Sin(t * j));

                                // Fill with a simple sine wave at max amplitude
                                for (int channel = 0; channel < format.wChannels; channel++)
                                {
                                    data.shortArray[j + channel] = conv;
                                }
                            }

                            samplesRecorded += noteLengthSamples;
                        }


                        // Calculate data chunk size in bytes
                        data.dwChunkSize = (uint)(data.shortArray.Length * (format.wBitsPerSample / 8));

                        break;
                    }
                case WaveExampleType.MessingWave:
                    {
                        // Number of samples = sample rate * channels * bytes per sample * seconds
                        uint numSamples = format.dwSamplesPerSec * format.wChannels * 40;

                        // Initialize the 16-bit array
                        data.shortArray = new short[numSamples];

                        int amplitude = 32760; // Max amplitude for 16-bit audio
                        double freq = 20.0f; // Concert A: 440Hz
                        double hz = 0.0009;

                        // Stereo volume interleave
                        double stereoInterleaveFreq = 0.5f;

                        // Channel amplitudes
                        double c = (Math.PI * 2 * stereoInterleaveFreq) / (format.dwSamplesPerSec * format.wChannels);

                        for (uint i = 0; i < numSamples - 1; i++)
                        {
                            // The "angle" used in the function, adjusted for the number of channels and sample rate.
                            // This value is like the period of the wave.
                            double t = (Math.PI * 2 * freq) / (format.dwSamplesPerSec * format.wChannels);
                            double s = Math.Sin(t * i);
                            double d = Math.Sin(c * i);
                            double channelAmp = amplitude * d;

                            // Fill with a simple sine wave at max amplitude
                            var sound = Convert.ToInt16(channelAmp * s);
                            data.shortArray[i] = sound; // Channel 1
                            data.shortArray[i + 1] = sound; // Channel 2

                            freq += hz;
                        }

                        // Calculate data chunk size in bytes
                        data.dwChunkSize = (uint)(data.shortArray.Length * (format.wBitsPerSample / 8));

                        break;
                    }
                case WaveExampleType.MonoWave:
                    {
                        // Number of samples = sample rate * channels * bytes per sample
                        uint numSamples = format.dwSamplesPerSec * format.wChannels;

                        // Initialize the 16-bit array
                        data.shortArray = new short[numSamples];

                        int amplitude = 32760; // Max amplitude for 16-bit audio
                        double freq = 40.0f; // Concert A: 440Hz

                        // The "angle" used in the function, adjusted for the number of channels and sample rate.
                        // This value is like the period of the wave.
                        double t = (Math.PI * 2 * freq) / (format.dwSamplesPerSec * format.wChannels);

                        for (uint i = 0; i < numSamples - 1; i++)
                        {
                            // Fill with a simple sine wave at max amplitude
                            for (int channel = 0; channel < format.wChannels; channel++)
                            {
                                data.shortArray[i + channel] = Convert.ToInt16(amplitude * Math.Sin(t * i));
                            }
                        }

                        // Calculate data chunk size in bytes
                        data.dwChunkSize = (uint)(data.shortArray.Length * (format.wBitsPerSample / 8));

                        break;
                    }
            }
        }

        public void Save(string filePath)
        {
            // Create a file (it always overwrites)
            FileStream fileStream = new FileStream(filePath, FileMode.Create);

            // Use BinaryWriter to write the bytes to the file
            BinaryWriter writer = new BinaryWriter(fileStream);

            // Write the header
            writer.Write(header.sGroupID.ToCharArray());
            writer.Write(header.dwFileLength);
            writer.Write(header.sRiffType.ToCharArray());

            // Write the format chunk
            writer.Write(format.sChunkID.ToCharArray());
            writer.Write(format.dwChunkSize);
            writer.Write(format.wFormatTag);
            writer.Write(format.wChannels);
            writer.Write(format.dwSamplesPerSec);
            writer.Write(format.dwAvgBytesPerSec);
            writer.Write(format.wBlockAlign);
            writer.Write(format.wBitsPerSample);

            // Write the data chunk
            writer.Write(data.sChunkID.ToCharArray());
            writer.Write(data.dwChunkSize);
            foreach (short dataPoint in data.shortArray)
            {
                writer.Write(dataPoint);
            }

            writer.Seek(4, SeekOrigin.Begin);
            uint filesize = (uint)writer.BaseStream.Length;
            writer.Write(filesize - 8);

            // Clean up
            writer.Close();
            fileStream.Close();
        }
    }
}