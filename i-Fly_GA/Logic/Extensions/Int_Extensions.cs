using System;
using System.Globalization;

namespace I_Fly.Logic
{
    public static class Int_Extensions
    {
        #region General

        public static int Random(this int p_input, int min, int max)
        {
            p_input = new Random().Next(min, max);

            return p_input;
        }

        public static bool Is_Integer(this object p_input)
        {
            return int.TryParse(p_input.ToString(), out _);
        }

        public static bool Is_Decimal(this object p_input)
        {
            return Decimal.TryParse(p_input.ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.CreateSpecificCulture("en-US"), out _); //CreateSpecificCulture could be dynamic
        }

        #endregion

        #region Randomness

        public static double Next_Double(this Random p_input, double p_min, double p_max)
        {
            return Math.Round((p_input.NextDouble() * (p_max - p_min) + p_min), 2);
        }

        public static int Next_Int(this Random p_input, int p_min, int p_max)
        {
            return p_input.Next(p_min, p_max);
        }

        public static bool Next_Bool(this Random p_input, int p_probability = 50)
        {
            return p_input.NextDouble() < p_probability / 100.0;
        }

        #endregion
    }
}