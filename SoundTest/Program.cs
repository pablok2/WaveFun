using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace SoundTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = @"C:\Users\Pavel\Desktop\stereo.wav";
            WaveGenerator wave = new WaveGenerator(WaveExampleType.ExampleSineWave);
            wave.Save(filePath);
        }
    }
}
