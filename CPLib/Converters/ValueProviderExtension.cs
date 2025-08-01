using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace CPLib.Converters
{
    [Serializable]
    /// <summary>
    /// 用于xaml中直接获取类型的实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ValueProviderExtension<T> : MarkupExtension
    {
        [global::System.Reflection.Obfuscation(Exclude = true, Feature = "renaming")]
        public T Value { get; set; }


        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this.Value;
        }

    }
    public class GetStringExtension : ValueProviderExtension<String>
    {
        public GetStringExtension() { }
        public GetStringExtension(string value) { Value = value; }
    }
    public class GetInt16Extension : ValueProviderExtension<Int16>
    {
        public GetInt16Extension() { }
        public GetInt16Extension(Int16 value) { Value = value; }
    }
    public class GetInt32Extension : ValueProviderExtension<Int32>
    {
        public GetInt32Extension() { }
        public GetInt32Extension(Int32 value) { Value = value; }
    }
    public class GetInt64Extension : ValueProviderExtension<Int64>
    {
        public GetInt64Extension() { }
        public GetInt64Extension(Int64 value) { Value = value; }
    }
    public class GetUInt16Extension : ValueProviderExtension<UInt16>
    {
        public GetUInt16Extension() { }
        public GetUInt16Extension(UInt16 value) { Value = value; }
    }
    public class GetUInt32Extension : ValueProviderExtension<UInt32>
    {
        public GetUInt32Extension() { }
        public GetUInt32Extension(UInt32 value) { Value = value; }
    }
    public class GetUInt64Extension : ValueProviderExtension<UInt64>
    {
        public GetUInt64Extension() { }
        public GetUInt64Extension(UInt64 value) { Value = value; }
    }
    public class GetByteExtension : ValueProviderExtension<Byte>
    {
        public GetByteExtension() { }
        public GetByteExtension(Byte value) { Value = value; }
    }
    public class GetSByteExtension : ValueProviderExtension<SByte>
    {
        public GetSByteExtension() { }
        public GetSByteExtension(SByte value) { Value = value; }
    }
    public class GetCharExtension : ValueProviderExtension<Char>
    {
        public GetCharExtension() { }
        public GetCharExtension(Char value) { Value = value; }
    }
    public class GetSingleExtension : ValueProviderExtension<Single>
    {
        public GetSingleExtension() { }
        public GetSingleExtension(Single value) { Value = value; }
    }
    public class GetDoubleExtension : ValueProviderExtension<Double>
    {
        public GetDoubleExtension() { }
        public GetDoubleExtension(Double value) { Value = value; }
    }
    public class GetDecimalExtension : ValueProviderExtension<Decimal>
    {
        public GetDecimalExtension() { }
        public GetDecimalExtension(Decimal value) { Value = value; }
    }
    public class GetBooleanExtension : ValueProviderExtension<Boolean>
    {
        public GetBooleanExtension() { }
        public GetBooleanExtension(Boolean value) { Value = value; }
    }
    public class GetDateTimeExtension : ValueProviderExtension<DateTime>
    {
        public GetDateTimeExtension() { }
        public GetDateTimeExtension(DateTime value) { Value = value; }
    }
    public class GetVisibilityExtension : ValueProviderExtension<Visibility>
    {
        public GetVisibilityExtension() { }
        public GetVisibilityExtension(Visibility value) { Value = value; }
    }
    public class GetOrientationExtension : ValueProviderExtension<Orientation>
    {
        public GetOrientationExtension() { }
        public GetOrientationExtension(Orientation value) { Value = value; }

    }
    public class GetVerticalAlignmentExtension : ValueProviderExtension<VerticalAlignment>
    {
        public GetVerticalAlignmentExtension() { }
        public GetVerticalAlignmentExtension(VerticalAlignment value) { Value = value; }
    }
    public class GetHorizontalAlignmentExtension : ValueProviderExtension<HorizontalAlignment>
    {
        public GetHorizontalAlignmentExtension() { }
        public GetHorizontalAlignmentExtension(HorizontalAlignment value) { Value = value; }
    }

    public class GetDayOfWeekExtension : ValueProviderExtension<DayOfWeek>
    {
        public GetDayOfWeekExtension() { }

        public GetDayOfWeekExtension(DayOfWeek value) { Value = value; }
    }


    /// <summary>
    /// 此转换器使用了反射，代码混淆会出现异常
    /// </summary>
    public class EnumValueProvider : MarkupExtension
    {
        public Type Type { get; set; }
        public string Name { get; set; }
        public EnumValueProvider()
        {


        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return System.Convert.ChangeType(Enum.Parse(this.Type, this.Name), this.Type);
        }
    }
}
