// Created by Ron 'Maxwolf' McDowell (ron.mcdowell@gmail.com) 
// Timestamp 12/31/2015@4:49 AM

using System.Collections.Generic;

namespace ClassifyBot.Annotator.Wunderkind
{
    public class SpinningPixel
    {
        #region Constructors
        public SpinningPixel(string frame1, string frame2, string frame3, string frame4)
        {
            animation = new List<string> { frame1, frame2, frame3, frame4 };
            counter = 0;
        }

        public SpinningPixel() : this("/", "-", @"\", "|")
        {

        }
        #endregion

        #region Methods
        public string Step()
        {
            var barText = animation[counter];
            counter++;
            if (counter == animation.Count)
                counter = 0;

            return barText;
        }
        #endregion

        #region Fields
        private List<string> animation;

        private int counter;
        #endregion
    }
}