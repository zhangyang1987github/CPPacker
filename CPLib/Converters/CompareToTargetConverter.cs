using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace CPLib.Converters
{
    public class CompareToTargetConverter : System.Windows.Markup.MarkupExtension, IValueConverter
    {
        public CompareToTargetConverter()
        {
        }
        public CompareToTargetConverter(Comparation comparation, object compareTarget, object trueResult, object falseResult)
        {
            this.Comparation = Comparation;
            this.CompareTarget = compareTarget;
            this.TrueResult = trueResult;
            this.FalseResult = falseResult;
        }

        [global::System.Reflection.Obfuscation(Exclude = true, Feature = "renaming")]
        public object TrueResult { get; set; } = true;

        [global::System.Reflection.Obfuscation(Exclude = true, Feature = "renaming")]
        public object FalseResult { get; set; } = false;

        [global::System.Reflection.Obfuscation(Exclude = true, Feature = "renaming")]
        public object CompareTarget { get; set; } = true;

        [global::System.Reflection.Obfuscation(Exclude = true, Feature = "renaming")]
        public Comparation Comparation { get; set; } = Comparation.Equals;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (Comparation == Comparation.Equals)
            {
                if (object.Equals(value, this.CompareTarget))
                {
                    return TrueResult;
                }
                else
                {
                    return FalseResult;
                }
            }
            else if (Comparation == Comparation.NotEquals)
            {
                if (object.Equals(value, this.CompareTarget))
                {
                    return FalseResult;
                }
                else
                {
                    return TrueResult;
                }
            }
            else if (Comparation == Comparation.Bigger)
            {
                if (value is IComparable && (value as IComparable).CompareTo(this.CompareTarget) > 0)
                {
                    return TrueResult;
                }
                else
                {
                    return FalseResult;
                }
            }
            else if (Comparation == Comparation.Smaller)
            {
                if (value is IComparable && (value as IComparable).CompareTo(this.CompareTarget) < 0)
                {
                    return TrueResult;
                }
                else
                {
                    return FalseResult;
                }
            }
            else if (Comparation == Comparation.BiggerOrEquals)
            {
                if (value is IComparable && (value as IComparable).CompareTo(this.CompareTarget) >= 0)
                {
                    return TrueResult;
                }
                else
                {
                    return FalseResult;
                }
            }
            else if (Comparation == Comparation.SmallerOrEquals)
            {
                if (value is IComparable && (value as IComparable).CompareTo(this.CompareTarget) <= 0)
                {
                    return TrueResult;
                }
                else
                {
                    return FalseResult;
                }
            }
            else
            {
                return Binding.DoNothing;
            }


        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
    [Serializable]
    public enum Comparation
    {
        Equals,
        NotEquals,
        Bigger,
        Smaller,
        BiggerOrEquals,
        SmallerOrEquals,
    }

    [Serializable]
    public class CalcConverter : System.Windows.Markup.MarkupExtension, IValueConverter
    {

        public Operator Operator { get; set; } = Operator.Add;

        public decimal Number2 { get; set; } = 0;

        /// <summary>
        /// 是否将第一操作数和第二操作数的位置调换
        /// </summary>
        public bool Reverse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal number1 = System.Convert.ToDecimal(value);


            decimal num1 = number1;
            decimal num2 = this.Number2;

            if (this.Reverse)
            {
                decimal tempNum = num1;
                num1 = num2;
                num2 = tempNum;
            }


            if (this.Operator == Operator.Add)
            {
                return (decimal)(num1 + num2);
            }
            else if (this.Operator == Operator.Subtract)
            {
                return (decimal)(num1 - num2);
            }
            else if (this.Operator == Operator.Multiply)
            {
                return (decimal)(num1 * num2);
            }
            else if (this.Operator == Operator.Divide)
            {
                return (decimal)(num1 / num2);
            }
            else if (this.Operator == Operator.Mod)
            {
                return (decimal)(num1 % num2);
            }
            else if (this.Operator == Operator.Sqrt)
            {
                return (decimal)Math.Sqrt((double)number1);
            }
            else if (this.Operator == Operator.Pow)
            {
                return (decimal)Math.Pow((double)num1, (double)num2);
            }
            else
            {
                return Binding.DoNothing;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    [Serializable]
    public enum Operator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Mod,
        Sqrt,
        Pow,
    }
}
