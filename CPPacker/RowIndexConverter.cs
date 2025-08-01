using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace CPPacker
{
    public class RowIndexConverter : System.Windows.Markup.MarkupExtension, IMultiValueConverter
    {
        public int StartIndex { get; set; } = 0;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                DataGridRow row = values[0] as DataGridRow;
                if (row.DataContext?.GetType().FullName == "MS.Internal.NamedObject") return null;
                return (StartIndex + row.GetIndex()).ToString();

            }
            catch (Exception)
            {

                return Binding.DoNothing;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }


        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }



}
