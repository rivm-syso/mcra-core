using OxyPlot;

namespace MCRA.Utils.Charting.OxyPlot {
    /// <summary>
    /// Custom colors from www.creativecolorschemes.com
    /// </summary>
    public  static class CustomPalettes  {

        private static readonly byte[] _distinctTone = new byte[] { 
            100,149,237,255,215,0,255,69,0,
            154,205,50,0,139,139,123,104,238,
            210,105,30,30,144,255,0,255,127,
            255,140,0,50,205,50,0,206,209,
            238,130,238,188,143,143,119,136,153,
            135,206,235,255,99,71,127,255,0,
            72,209,204,0,191,255,221,160,221,
            255,192,203,244,164,96,176,224,230,
            65,105,225,250,128,114,218,165,32,
            152,251,152,135,206,250,147,112,219,
            128,0,0,128,128,0,30,144,255,
            186,85,211,240,255,240,230,230,250,
            218,112,214,95,158,160,139,0,0,
            255,160,122,124,252,0148,0,211,
        };

        private static readonly byte[] _earthTone = new byte[] { 
            73, 56, 41, 129, 108, 91, 169, 161, 140,
            97, 51, 24, 133, 87, 35, 185, 156, 107,
            143, 59, 27, 213, 117, 0, 219, 202, 105, 
            64, 79, 36, 102, 141, 60, 189, 208, 156,
            78, 97, 114, 131, 146, 159, 163, 173, 184,
        };

        private static readonly byte[] _artDecoTone = new byte[] { 
            239, 62, 91, 242, 98, 121, 246, 143, 160,
            75, 37, 109, 111, 84, 149, 160, 158, 214,
            63, 100, 126, 104, 143, 173, 159, 193, 211,
            0, 176, 178, 82, 204, 206, 149, 212, 122,
            103, 124, 138, 178, 162, 150, 201, 201, 201,
        };

        private static readonly byte[] _lightBlueTone = new byte[] {
            47, 86, 233, 45, 100, 245, 47, 141, 255,
            51, 171, 249, 52, 204, 255, 82, 219, 255,
            23, 236, 236, 110, 255, 255, 168, 255, 255,
            169, 255, 47, 255, 173, 47, 255, 47, 154,
            48, 120, 238, 110, 193, 248, 174, 234, 255,
        };

        private static readonly byte[] _gorgeousTone = new byte[] { 
            0, 104, 132, 0, 144, 158, 137, 219, 236,
            237, 0, 38, 250, 157, 0, 255, 208, 141,
            176, 0, 81, 246, 131, 112, 254, 171, 185,
            110, 0, 108, 145, 39, 143, 207, 151, 215,
            0, 0, 0, 91, 91, 91, 212, 212, 212,
        };

        private static readonly byte[] _coolTone = new byte[] { 
            0, 65, 89, 101, 168, 196, 179, 206, 226,
            140, 101, 211, 154, 147, 236, 202, 185, 241,
            0, 82, 165, 65, 179, 247, 129, 203, 248,
            0, 173, 206, 89, 219, 241, 158, 231, 250,
            0, 197, 144, 115, 235, 174, 181, 249, 211,
        };

        private static readonly byte[] _beachTone = new byte[] { 
            195, 54, 44, 255, 134, 66, 244, 220, 181,
            129, 108, 91, 195, 183, 172, 231, 227, 215,
            102, 141, 60, 177, 221, 161, 229, 243, 207, 
            0, 151, 172, 60, 214, 230, 151, 234, 244,
            0, 121, 150, 6, 194, 244, 95, 216, 250,
        };

        private static readonly byte[] _warmTone = new byte[] { 
            121, 63, 13, 172, 112, 61, 195, 142, 99,
            228, 153, 105, 229, 174, 134, 238, 197, 169, 
            110, 118, 73, 157, 151, 84, 199, 195, 151,
            180, 168, 81, 223, 210, 124, 231, 227, 181,
            132, 109, 116, 183, 166, 173, 211, 201, 206,
         };

        private static readonly byte[] _purpleTone = new byte[] { 
            84, 22, 180, 112, 39, 195, 185, 76, 225,
            150, 0, 205, 164, 66, 220, 181, 100, 227,
            228, 0, 224, 236, 71, 233, 244, 147, 242,
            0, 181, 236, 204, 255, 0, 255, 106, 0, 
            67, 59, 103, 96, 88, 133, 148, 141, 179,
        };

        private static readonly byte[] _elegantTone = new byte[] { 
            80, 40, 18, 96, 54, 24, 133, 87, 35,
            137, 32, 52, 122, 26, 87, 111, 37, 108,
            0, 52, 77, 0, 48, 102, 87, 82, 126,
            0, 66, 54, 64, 70, 22, 159, 155, 116,
            33, 41, 48, 134, 143, 152, 195, 200, 205,
        };

        private static readonly byte[] _greenTone = new byte[] { 
            18, 173, 42, 99, 209, 62, 144, 245, 0,
            18, 230, 3, 99, 255, 32, 185, 255, 77,
            18, 247, 41, 15, 245, 145, 140, 250, 202,
            66, 66, 238, 255, 62, 51, 255, 217, 51,
            53, 98, 68, 75, 140, 97, 170, 209, 183, 
        };

        /// <summary>
        /// Distinct colors
        /// </summary>
        /// <returns></returns>
        public static OxyPalette DistinctTone(int number) {
            var completePalette = getPalette(_distinctTone);
            if (number > completePalette.Count) {
                return OxyPalettes.Hue(number);
            }
            return new OxyPalette(getPalette(_distinctTone).Take(number));
        }

        /// <summary>
        /// Description: 15 colors 
        /// The Bluetone is mainly made of deep navy blue, light royal blue, greenish teal blue, bright sky blue as well as muddy grayish blue. The contrasting green, orange, and blue increase their dramatic and dynamic effects.
        /// Meanings: 
        /// Fresh, refreshing, lively, enthusiastic, dynamic 
        /// Implications: 
        /// Smart, confidence, credible, dependable
        /// Associations: 
        /// Sky, ocean, man, technology
        /// </summary>
        /// <returns></returns>
        public static OxyPalette BlueTone(int number) {
            return Monochrome(number, 0.5883, .2, .5, 1, 1, true);
        }

        /// <summary>
        /// Description: 15 colors 
        /// Earthtone colors come from natural things around us: brown soil, green leaf, cloudy sky, as well as the red sun. These palettes can create a warm, nature-friendly atmosphere.
        /// Meanings: 
        /// Warm, safe, protective, sturdy, durable, rough 
        /// Implications:
        /// Earthy, environmental, welcoming, bold
        /// Associations: 
        /// Soil, forest, wood, countryside
        /// </summary>
        /// <returns></returns>
        public static OxyPalette EarthTone(int number) {
            return new OxyPalette(getPalette(_earthTone).Take(number));
        }

        /// <summary>
        /// Reverse EarthTone
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static OxyPalette EarthToneReverse(int number) {
            return new OxyPalette(getPalette(_earthTone).Reverse().Take(number));
        }

        /// <summary>
        /// Description: 15 colors 
        /// This scheme is influenced by the Art Deco style in Progressive art design, that moves away from nature to a more industrial characteristic.
        /// Meanings: 
        /// Stylish, alluring, fashionable, 
        /// Implications: 
        /// Creative, urban, industrial
        /// Associations: 
        /// Art Deco design style
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static OxyPalette ArtDecoTone(int number) {
            return new OxyPalette(getPalette(_artDecoTone).Take(number));
        }

        /// <summary>
        /// Reverse ArtDecoTone
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static OxyPalette ArtDecoToneReverse(int number) {
            return new OxyPalette(getPalette(_artDecoTone).Reverse().Take(number));
        }
        /// <summary>
        /// Description: 15 colors 
        /// The Gorgeous scheme is extremely beautiful due to the mixing of sweet and fascinating colors.
        /// Meanings: 
        /// Beautiful, attractive, admirable, stunning
        /// Implications: 
        /// Elegant, generosity, lovely, superb 
        /// Associations: 
        /// High-class products 
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static OxyPalette GorgeousTone(int number) {
            return new OxyPalette(getPalette(_gorgeousTone).Take(number));
        }

        /// <summary>
        /// Reverse GorgeousTone
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static OxyPalette GorgeousToneReverse(int number) {
            return new OxyPalette(getPalette(_gorgeousTone).Reverse().Take(number));
        }
        /// <summary>
        /// Description: 15 colors
        /// The Cool scheme combines various cool shades of colors in the spectrum. The fresh blue, purple, and green make this scheme so fresh and cold.
        /// Meanings: 
        /// Cold, fresh, crisp, wet, clean, clear 
        /// Implications: 
        /// Refreshing, soothing, open, truthful
        /// Associations: 
        /// Rain, water, ice, snow, mint, air, pool, crystal
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static OxyPalette CoolTone(int number) {
            return new OxyPalette(getPalette(_coolTone).Take(number));
        }

        /// <summary>
        /// Reverse CoolTone
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static OxyPalette CoolToneRevers(int number) {
            return new OxyPalette(getPalette(_coolTone).Reverse().Take(number));
        }

        /// <summary>
        /// Description: 15 colors 
        /// The Beach scheme comes from what we usually see on the beach such as sunshine, sand, tree, the sky, and the sea.
        /// Meanings: 
        /// Cold, clear, bracing, refreshing, relaxing 
        /// Implications: 
        /// Windy, seaside, sandy, aquatic, informal
        /// Associations: 
        /// Beach, sky, ocean, sea, sand, sunshine
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static OxyPalette BeachTone(int number) {
            return new OxyPalette(getPalette(_beachTone).Take(number));
        }

        /// <summary>
        /// Reverse BeachTone
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static OxyPalette BeachToneReverse(int number) {
            return new OxyPalette(getPalette(_beachTone).Reverse().Take(number));
        }

        /// <summary>
        /// Description: 15 colors 
        /// The Purpletone has a variety of purple tones including royal purple, indigo, violet, and lavender.
        /// Meanings: 
        /// Elegant, sensual, regal, glorious
        /// Implications: 
        /// Spiritual, nostalgic, mysterious, magical, astonishing
        /// Associations: 
        /// Lavender, grape, magic
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static OxyPalette PurpleTone(int number) {
            return new OxyPalette(getPalette(_purpleTone).Take(number));
        }

        /// <summary>
        /// Reverse PurpleTone
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static OxyPalette PurpleToneReverse(int number) {
            return new OxyPalette(getPalette(_purpleTone).Reverse().Take(number));
        }

        /// <summary>
        /// Description: 15 colors
        /// The Warm scheme encompasses yellow, brown, and gray from golden sunshine, creating a friendly, relaxing atmosphere.
        /// Meanings: 
        /// Warm, mild, tender, cozy
        /// Implications: 
        /// Inviting, nice, kind, friendly
        /// Associations: 
        /// Heat, sunshine, dry, fall, candlelight
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static OxyPalette WarmTone(int number) {
            return new OxyPalette(getPalette(_warmTone).Take(number));
        }

        /// <summary>
        /// Reverse WarmTone
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static OxyPalette WarmToneReverse(int number) {
            return new OxyPalette(getPalette(_warmTone).Reverse().Take(number));
        }
        /// <summary>
        /// Description: 15 colors 
        /// The Elegant scheme originally comes from the colors of jewel, ruby, emerald, sapphire as well as gold, silver, bronze, and copper.
        /// Meanings: 
        /// Graceful, lofty, luxurious, polished, famed
        /// Implications: 
        /// Intelligent, talented, sophisticated, respectful
        /// Associations: 
        /// Gold, silver, bronze, copper, emerald, sapphire
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static OxyPalette ElegantTone(int number) {
            return new OxyPalette(getPalette(_elegantTone).Take(number));
        }

        /// <summary>
        /// Reverse ElegantTone
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static OxyPalette ElegantToneReverse(int number) {
            return new OxyPalette(getPalette(_elegantTone).Reverse().Take(number));
        }

        /// <summary>
        /// Description:  15 colors
        /// he Greentone contains bluish green, pure green, yellowish green, and grayish green in contrast with bright magenta, purple and yellow. This scheme is so fresh and soothing.
        /// eanings: 
        /// appy, fresh, healing, soothing, encouraging 
        /// mplications: 
        /// atural, healthy, hygiene, safe, positive
        /// ssociations: 
        /// Plants, vegetable, grass, leaves
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static OxyPalette GreenTone(int number) {
            return new OxyPalette(getPalette(_greenTone).Take(number));
        }

        /// <summary>
        /// Reverse GreenTone
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static OxyPalette GreenToneReverse(int number) {
            return new OxyPalette(getPalette(_greenTone).Reverse().Take(number));
        }

        /// <summary>
        /// Aggregate
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static OxyPalette DietaryNonDietaryColors(int number) {
            var colors = new List<OxyColor> {
                OxyColors.Orange,
                OxyColors.CornflowerBlue
            };
            colors.AddRange(getPalette(_lightBlueTone).Reverse());
            return new OxyPalette(colors.Take(number));
        }

        /// <summary>
        /// Converts to argA
        /// </summary>
        /// <param name="argA"></param>
        /// <param name="palette"></param>
        /// <returns></returns>
        public static OxyPalette ArgAPalette(byte argA, OxyPalette palette) {
            argA = argA > 255 ? (byte)255 : argA;
            return new OxyPalette() {
                Colors = palette.Colors.Select(c => OxyColor.FromAColor(argA, c)).ToList()
            };
        }

        public static OxyPalette Monochrome(int number, double hue, double saturationMin, double valueMin, double saturationMax, double valueMax, bool reverse = false) {
            var tints = createSaturationValueTints(number, saturationMin, valueMin, saturationMax, valueMax);
            var colors = new List<OxyColor>();
            for (int i = 0; i < number; i++) {
                var color = OxyColor.FromHsv(hue, tints[i, 0], tints[i, 1]);
                colors.Add(color);
            }
            if (reverse) {
                colors.Reverse();
            }
            return new OxyPalette(colors);
        }

        public static OxyPalette Analogous(int number, double hue, double saturationMin, double valueMin, double saturationMax, double valueMax) {
            var hues = new double[] {
                getFractional(hue - 1D / 12),
                hue,
                getFractional(hue + 1D / 12),
            };
            return MultiColor(number, hues, saturationMin, valueMin, saturationMax, valueMax);
        }

        public static OxyPalette Triadic(int number, double hue, double saturationMin, double valueMin, double saturationMax, double valueMax) {
            var hues = new double[3] {
                getFractional(hue - 1D / 3),
                hue,
                getFractional(hue + 1D / 3)
            };
            return MultiColor(number, hues, saturationMin, valueMin, saturationMax, valueMax);
        }

        public static OxyPalette SplitComplementary(int number, double hue, double saturationMin, double valueMin, double saturationMax, double valueMax) {
            var hues = new double[3] {
                getFractional(hue + 5D / 12),
                hue,
                getFractional(hue - 5D / 12)
            };
            return MultiColor(number, hues, saturationMin, valueMin, saturationMax, valueMax);
        }

        public static OxyPalette MultiColor(int number, double[] hues, double saturationMin, double valueMin, double saturationMax, double valueMax) {
            var numHues = hues.Length;
            var numTints = (int)Math.Ceiling((double)number / numHues);
            var tints = createSaturationValueTints(numTints, saturationMin, valueMin, saturationMax, valueMax);
            var colors = new List<OxyColor>();
            var ixTint = 0;
            while (colors.Count < number) {
                for (int ixHue = 0; ixHue < numHues && colors.Count < number; ixHue++) {
                    var color = OxyColor.FromHsv(hues[ixHue], tints[ixTint, 0], tints[ixTint, 1]);
                    colors.Add(color);
                }
                ixTint++;
            }
            return new OxyPalette(colors);
        }

        public static double[,] createSaturationValueTints(int number, double saturationMin, double valueMin, double saturationMax, double valueMax) {
            var result = new double[number, 2];
            var counter = (int)Math.Floor(number / 2D);
            var ix = 0;
            var s = saturationMin;
            var v = valueMax;
            var stepS = (saturationMax - saturationMin) / counter;
            while (ix < counter) {
                result[ix, 0] = s;
                result[ix, 1] = v;
                s += stepS;
                ix++;
            }
            if (number % 2 == 1) {
                s = saturationMax;
                v = valueMax;
                result[ix, 0] = saturationMax;
                result[ix, 1] = valueMax;
                ix++;
            }
            var stepV = (valueMax - valueMin) / counter;
            while (ix < number) {
                v -= stepV;
                result[ix, 0] = s;
                result[ix, 1] = v;
                ix++;
            }
            return result;
        }

        private static double getFractional(double x) {
            while (x > 1) {
                x -= 1;
            }
            while (x < 0) {
                x += 1;
            }
            return x;
        }

        private static IList<OxyColor> getPalette(byte[] tone) {
            var colors = new List<OxyColor>();
            var counter = tone.Length / 3;
            var ix = 0;
            for (int i = 0; i < counter; i++) {
                colors.Add(OxyColor.FromRgb(tone[ix * 3], tone[ix * 3 + 1], tone[ix * 3 + 2]));
                ix++;
            }
            return colors;
        }
    }
}
