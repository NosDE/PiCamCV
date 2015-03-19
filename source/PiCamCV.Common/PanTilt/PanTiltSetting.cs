﻿namespace PiCamCV.ConsoleApp.Runners.PanTilt
{
    public class PanTiltSetting
    {
        public decimal? PanPercent { get; set; }
        public decimal? TiltPercent{get;set;}


        public PanTiltSetting(decimal pan, decimal tilt)
        {
            PanPercent = pan;
            TiltPercent = tilt;
        }

        public PanTiltSetting()
        {
            
        }

        public PanTiltSetting Clone()
        {
            var clone = new PanTiltSetting();
            clone.TiltPercent = TiltPercent;
            clone.PanPercent = PanPercent;
            return clone;
        }

        public override string ToString()
        {
            return string.Format("Pan={0}%, Tilt={1}%", PanPercent, TiltPercent);
        }
    }
}