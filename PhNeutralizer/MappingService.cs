using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PhNeutralizer
{
    public static class MappingService
    {
        public static string MapDoubleToTextBox1Dc(double v)
        {
            return v.ToString("0.0");

        }
        public static string MapDoubleToTextBox3Dc(double v)
        {
            return v.ToString("0.000");

        }
        public static double MapTextBoxToDouble(string v)
        {
            var temp = 0.0;
            if (double.TryParse(v, out temp))
            {
                return temp;
            }

            throw new FormatException($"Invalid double format in value: '{v}'");
        }
        public static string MapIntToTextBox(int v)
        {
            return v.ToString();

        }
        public static int MapTextBoxToInt(string v)
        {
            var temp = 0;
            if (int.TryParse(v, out temp))
            {
                return temp;
            }

            throw new FormatException($"Invalid integer format in value: '{v}'");
        }
        public static int MapDoubleToProgressBar(double v)
        {
            return (int)(v * 10);
        }
        public static double MapProgressBarToDouble(int v)
        {
            return v * 0.1;
        }

        public static Color MapPhToColor(double ph)
        {
            var neutralPh = 7.5;
            var maxChannelValue = 255;

            var BValue = 0.0;
            var RValue = 0.0;
            if (ph > neutralPh)
            {
                BValue = maxChannelValue;
                RValue = ((neutralPh * 2) - ph) * maxChannelValue / neutralPh;
            }
            else if (ph < neutralPh)
            {
                RValue = maxChannelValue;
                BValue = ph * maxChannelValue / neutralPh;
            }
            else
            {
                BValue = maxChannelValue;
                RValue = maxChannelValue;
            }

            return Color.FromArgb((int)RValue, 0, (int)BValue);
        }

        public static Color MapPhToColor2(double ph)
        {
            var phColors = GetPhColors();
            if (ph % 1 == 0)
            {
                return phColors[(int)ph];
            }

            var r = 0;
            var g = 0;
            var b = 0;

            var lowerPh = phColors[(int)Math.Floor(ph)];
            var higherPh = phColors[(int)Math.Ceiling(ph)];
            var delta = ph - Math.Floor(ph);
            
            var changeMap = GetPhColorChangeMap(ph);

            if (changeMap.Contains("R"))
            {
                r = higherPh.R > lowerPh.R ?
                    (int)((higherPh.R - lowerPh.R) * delta) + lowerPh.R :
                    (int)((lowerPh.R - higherPh.R) * delta) + higherPh.R;

                g = lowerPh.G;
                b = lowerPh.B;
            }
            if (changeMap.Contains("G"))
            {
                g = higherPh.G > lowerPh.G ?
                    (int)((higherPh.G - lowerPh.G) * delta) + lowerPh.G :
                    (int)((lowerPh.G - higherPh.G) * delta) + higherPh.G;

                r = lowerPh.R;
                b = lowerPh.B;
            }
            if (changeMap.Contains("B"))
            {
                b = higherPh.B > lowerPh.B ?
                    lowerPh.B + (int)((higherPh.B - lowerPh.B) * delta) :
                    higherPh.B + (int)((lowerPh.B - higherPh.B) * delta);

                r = lowerPh.R;
                g = lowerPh.G;               
            }

            return Color.FromArgb(r, g, b);
        }

        public static List<Color> GetPhColors()
        {
            var colors = new List<Color>();
            //colors.Add(Color.FromArgb(0, 0, 0));    //-1     +256    ---     ---     256             256             0             R hits zero - other sums = 0; other deltas = 0
            //colors.Add(Color.FromArgb(255, 0, 0));    //0      ---     +128    ---     128             128             256             R hits max  - other sums = 0; other deltas = 0
            //colors.Add(Color.FromArgb(255, 128, 0));    //1      ---     +64     ---     64              64              384
            //colors.Add(Color.FromArgb(255, 192, 0));    //2      ---     +64     ---     64              64              448
            //colors.Add(Color.FromArgb(255, 224, 0));    //3      -64     -32     ---     96              -96             512             G hits max - other sums = max; other deltas = 0

            //colors.Add(Color.FromArgb(255, 255, 0));    //4      -32     ---     ---     32              -32             416   
            //colors.Add(Color.FromArgb(192, 255, 0));    //5      -32     ---     ---     32              -32             384
            //colors.Add(Color.FromArgb(128, 224, 0));    //6      -128    -32     ---     160             -160            352
            //colors.Add(Color.FromArgb(0, 160, 0));    //7      ---     ---     +128    128             128             192         R hits zero


            //colors.Add(Color.FromArgb(0, 160, 128));    //8      ---     ---     +96     96              96              320
            //colors.Add(Color.FromArgb(0, 160, 224));    //9      ---     -32     +32     64              0               416


            //colors.Add(Color.FromArgb(0, 160, 255));    //10     ---     -32     ---     32              -32             384         B hits max


            //colors.Add(Color.FromArgb(0, 128, 255));    //11     +64     -64     ---     128             0               384

            //colors.Add(Color.FromArgb(64, 64, 255));    //12     +64     -64     ---     128             0               384
            //colors.Add(Color.FromArgb(128, 32, 255));    //13     +32     -32     ---     64              9               384
            //colors.Add(Color.FromArgb(160, 0, 255));    //14                             ---             ---             384         G hits zero

            //return colors;


            // 256
            // 128
            // 64
            // 32
            //
            //                                                            //      1       2       4       
            //                                                            //      Changes for valsues     Changes abs     Changes sum     Values sum      Notes
            //                                                            //      ---     ---     ---     0               0               0
            //                                                            //      ---     ---     ---     0               0               0
            //                                                            //      ---     ---     ---     0               0               0
            //colors.Add(Color.FromArgb(0,        0,          0   ));    //-1     +256    ---     ---     256             256             0             R hits zero - other sums = 0; other deltas = 0
            colors.Add(Color.FromArgb(255, 0, 0));    //0      ---     +128    ---     128             128             256             R hits max  - other sums = 0; other deltas = 0
            colors.Add(Color.FromArgb(255, 128, 0));    //1      ---     +64     ---     64              64              384
            colors.Add(Color.FromArgb(255, 192, 0));    //2      ---     +64     ---     64              64              448
            colors.Add(Color.FromArgb(255, 255, 0));    //3      -64     -32     ---     96              -96             512             G hits max - other sums = max; other deltas = 0

            colors.Add(Color.FromArgb(192, 224, 0));    //4      -32     ---     ---     32              -32             416   
            colors.Add(Color.FromArgb(160, 224, 0));    //5      -32     ---     ---     32              -32             384
            colors.Add(Color.FromArgb(128, 224, 0));    //6      -128    -32     ---     160             -160            352
            colors.Add(Color.FromArgb(0, 192, 0));    //7      ---     ---     +128    128             128             192         R hits zero


            colors.Add(Color.FromArgb(0, 192, 128));    //8      ---     ---     +96     96              96              320
            colors.Add(Color.FromArgb(0, 192, 224));    //9      ---     -32     +32     64              0               416


            colors.Add(Color.FromArgb(0, 160, 255));    //10     ---     -32     ---     32              -32             384         B hits max


            colors.Add(Color.FromArgb(0, 128, 255));    //11     +64     -64     ---     128             0               384

            colors.Add(Color.FromArgb(64, 64, 255));    //12     +64     -64     ---     128             0               384
            colors.Add(Color.FromArgb(128, 32, 255));    //13     +32     -32     ---     64              9               384
            colors.Add(Color.FromArgb(160, 0, 255));    //14                             ---             ---             384         G hits zero

            return colors;
        }

        private static List<Color> GetPhColors2()
        {
            var colors = new List<Color>();

            var x = 32;
                                                                                                                                               
            colors.Add(Color.FromArgb(255,      0,          0));    //  0       256     0       0        8x  0x  0x      8       8   0   0     
            colors.Add(Color.FromArgb(255,      128,        0));    //  1       0       128     0        8x  4x  0x      12      0   4   0     
            colors.Add(Color.FromArgb(255,      192,        0));    //  2       0       64      0        8x  6x  0x      14      0   2   0     
            colors.Add(Color.FromArgb(255,      255,        0));    //  3       0       64      0        8x  8x  0x      16      0   2   0     
            colors.Add(Color.FromArgb(192,      224,        0));    //  4       -64     -32     0        6x  7x  0x      13      2   1   0     
            colors.Add(Color.FromArgb(160,      224,        0));    //  5       -32     0       0        5x  7x  0x      12      1   0   0     
            colors.Add(Color.FromArgb(128,      224,        0));    //  6       -32     0       0        4x  7x  0x      11      1   0   0     
            colors.Add(Color.FromArgb(0,        192,        0));    //  7       -128    -32     0        0x  6x  0x      6       4   1   0     
            colors.Add(Color.FromArgb(0,        192,        128));  //  8       0       0       128      0x  6x  4x      10      0   0   4     
            colors.Add(Color.FromArgb(0,        192,        224));  //  9       0       0       96       0x  6x  7x      13      0   0   3     
            colors.Add(Color.FromArgb(0,        160,        255));  //  10      0       -32     32       0x  5x  8x      13      0   1   1     
            colors.Add(Color.FromArgb(0,        128,        255));  //  11      0       -32     0        0x  4x  8x      12      0   1   0     
            colors.Add(Color.FromArgb(64,       64,         255));  //  12      64      -64     0        2x  2x  8x      12      2   2   0     
            colors.Add(Color.FromArgb(128,      32,         255));  //  13      64      -32     0        4x  1x  8x      13      2   1   0     
            colors.Add(Color.FromArgb(160,      0,          255));  //  14      32      -32     0        5x  0x  8x      13      1   1   0     


            // 8000 2114 0000 221
            // 0422 1001 0011 211
            // 0000 0000 4310 000

            //800
            //040
            //020v
            //m
            //020
            //210
            //100
            //100
            //410
            //004
            //003
            //011
            //010
            //220
            //210
            //110

            return colors;
        }


        private static Color GetColorForPh(int ph)
        {
            if (ph > 14)
            {
                ph = 14;
            }


            var r = 0;
            var g = 1;
            var b = 2;

            var minValue = 0;
            var maxValue = 256;
            

            var rgb = new int[3] { 0, 0, 0 };
            var delta = new int[3] { 0, 0, 0 };
            var change = new int[3] { 1, 0, 0 };

            var leftToRight = true;
            


            for (var x = 0; x <= ph; x++)
            {
                if (change[r] == 1)
                {

                }

                if (rgb.Sum() == 0)
                {
                    rgb[r] = 256;
                    delta[r] = 256;
                }
                else
                {

                }
            }


            rgb[r] = rgb[r] == 256 ? 255 : rgb[r];
            rgb[g] = rgb[r] == 256 ? 255 : rgb[g];
            rgb[b] = rgb[r] == 256 ? 255 : rgb[b];

            return Color.FromArgb(rgb[r], rgb[g], rgb[b]);
        }




        /*
        private static Color GetColorForPh(int ph)
        {
            var rgbMax = 256; // -1
            var smallChange = 32;
            var maxSingleChange = rgbMax / 2;

            var rgbValues = new int[] { 0, 0, 0 };            
            var rgbDeltas = new int[] { 0, 0, 0 };
            var nextChange = new int[] { 0, 0, 0 };
            var totalDeltas = new int[] { 0, 0, 0 };
            var multipliers = new int[] { 1, 1, 1 };


            for (var x = 0; x < 15; x++)
            {
                var rgbsSum = 0;
                var rgbsZeroCount = 0;
                var prevDeltasSum = 0;
                var totalDeltasSum = 0;
                var totalDeltasSignedSum = 0;

                for (var y = 0; y <= rgbValues.Length; y++)
                {
                    rgbsSum += rgbValues[y];
                    rgbsZeroCount += rgbValues[y] == 0 ? 1 : 0;
                    prevDeltasSum += rgbDeltas[y];
                    totalDeltasSum += totalDeltas[y];
                    totalDeltasSignedSum += totalDeltas[y] * multipliers[y];
                }

                if (rgbsSum == 0) // || totalDeltasSum < rgbMax || x == 0 || totalDeltasSum == 0)
                {
                    rgbDeltas[0] = rgbMax;
                    rgbDeltas[1] = 0;
                    rgbDeltas[2] = 0;

                    rgbValues[0] += rgbDeltas[0] * multipliers[0];
                    rgbValues[1] += rgbDeltas[1] * multipliers[1];
                    rgbValues[2] += rgbDeltas[2] * multipliers[2];

                    totalDeltas[0] += rgbDeltas[0];
                    totalDeltas[1] += rgbDeltas[1];
                    totalDeltas[2] += rgbDeltas[2];
                }
                else
                {
                    multipliers[0] = totalDeltas[0] == rgbMax ? -1 : totalDeltas[0] == 0 ? 1 : multipliers[0];
                    multipliers[1] = totalDeltas[1] == rgbMax ? -1 : totalDeltas[1] == 0 ? 1 : multipliers[1];
                    multipliers[2] = totalDeltas[2] == rgbMax ? -1 : totalDeltas[2] == 0 ? 1 : multipliers[2];

                    if (totalDeltas[0] == rgbMax)
                    {
                        
                    }
                    else
                    {

                    }

                    if (totalDeltas[1] == rgbMax)
                    {

                    }
                    else
                    {

                    }

                    if (totalDeltas[2] == rgbMax)
                    {

                    }
                    else
                    {

                    }
                }

            }




            if (ph > 14)
            {
                throw new ArgumentOutOfRangeException(nameof(ph));
            }

            



            var lastRChange = 0;
            var lastGChange = 0;
            var lastBChange = 0;

            for (var x = 0; x < 15; x++)
            {
                if (r == rgbMax)
                {
                    if (g == rgbMax)
                    {
                        r -= 2 * smallChange;
                        g -= smallChange;
                    }
                    if (g == 0)
                    {
                        g += r / 2;
                    }
                    else if (g < rgbMax)
                    {
                        g += smallChange;
                    }
                }

                if (g == rgbMax)
                {

                }
                else
                {

                }

                if (b == rgbMax)
                {

                }
            }

            return Color.Black;
        }
        */
        private static string GetPhColorChangeMap()
        {
            return "G+|G+|G+|R-G-|R-|R-|R-G-|B+|B+|G-B+|G0|R+G-|R+G-|R+G-";
        }

        private static string GetPhColorChangeMap(double ph)
        {
            var parts = GetPhColorChangeMap().Split(new char[] { '|' });
            return parts.Length > ph ? parts[(int)ph] : "";
        }
    }
}
