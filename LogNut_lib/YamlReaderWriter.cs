using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Hurst.LogNut.Util;


namespace Hurst.LogNut
{
    internal class YamlReaderWriter
    {
        #region WritePropertiesThatHaveDefaultValueAttributeTo
        /// <summary>
        /// Newer alternative to WriteToFile. No XML - only simple YAML but without the library.
        /// </summary>
        /// <param name="pathname">the filesystem-pathname of the file to write it to</param>
        /// <param name="ofWhat">The object whose properties to write to the file</param>
        /// <param name="isToWriteAllValues">set this false if you want to write out only those values which are not at their default value</param>
        /// <returns>the number of properties that were written</returns>
        public static int WritePropertiesThatHaveDefaultValueAttributeTo( string pathname, object ofWhat, bool isToWriteAllValues )
        {
            if (ofWhat == null)
            {
                throw new ArgumentNullException( "ofWhat" );
            }
            if (pathname == null)
            {
                throw new ArgumentNullException( "pathname" );
            }

            // Remove the original file if it exists.
            if (File.Exists( pathname ))
            {
                File.Delete( pathname );
            }

            using (var stream = new StreamWriter( pathname ))
            {
                // Doing this via inflection, to avoid having to code the writing of every single property.
                // This depends upon having an attribute assigned to each of the properties that we want to save, which would also include their default values.

                // Only write those properties that are in some state other than their default..
                _className = ofWhat.GetType().Name;
                _hasWrittenClassName = false;
                _numberWritten = 0;

                var properties = ofWhat.GetType().GetProperties().Where( prop => prop.IsDefined( typeof( DefaultValueAttribute ), false ) ).OrderBy( p => p.Name );
                foreach (PropertyInfo propertyInfo in properties)
                {
                    // Here we are dealing only with properties that have the DefaultValue attribute.
                    DefaultValueAttribute attribute0 = GetAttribute( propertyInfo );
                    bool isToWriteThis = false;
                    string propertyName = propertyInfo.Name;
                    object propertyValue = propertyInfo.GetValue( obj: ofWhat, index: null );
                    var propertyType = propertyInfo.PropertyType;
                    //if (isToWriteAllValues)
                    //{
                    //    // We don't need the value of the attribute here, since we're writing all properties (that are marked with the attribute).
                    //    isToWriteThis = true;
                    //}
                    //else // write only those properties that have a non-default value.
                    //{
                    //    //Console.WriteLine( "attribute1.Value is " + StringLib.AsString( attribute1.Value ) );
                    //    if (attribute0.Value is Boolean)
                    //    {
                    //        bool isTrueByDefault = (bool)attribute0.Value;
                    //        bool booleanValue = (bool)propertyValue;

                    //        if (isTrueByDefault)
                    //        {
                    //            isToWriteThis = !booleanValue;
                    //        }
                    //        else
                    //        {
                    //            isToWriteThis = booleanValue;
                    //        }
                    //    }
                    //    else if (attribute0.Value is Byte)
                    //    {
                    //        byte defaultValue = (byte)attribute0.Value;
                    //        byte actualValue = (byte)propertyValue;
                    //        isToWriteThis = actualValue != defaultValue;
                    //    }
                    //    else if (attribute0.Value is Int32)
                    //    {
                    //        int defaultValue = (Int32)attribute0.Value;
                    //        int actualValue = (Int32)propertyValue;
                    //        isToWriteThis = actualValue != defaultValue;
                    //    }
                    //    else
                    //    {
                    //        if (propertyType.IsClass)
                    //        {
                    //            Console.WriteLine( "found IsClass?" );
                    //        }
                    //        else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof( Nullable<> ))
                    //        {
                    //            var t = propertyType.GetGenericArguments()[0];
                    //            Console.WriteLine( propertyName + ", IsGenericType, t = " + StringLib.AsString( t ) );
                    //            if (t == typeof( Int32 ))
                    //            {
                    //                isToWriteThis = true;
                    //            }
                    //        }
                    //        else if (propertyInfo.PropertyType.IsValueType)
                    //        {
                    //            // Even a nullable Int32 will be included here.
                    //            Console.WriteLine( propertyName + ", IsValueType, type = " + StringLib.AsString( propertyType ) );
                    //        }
                    //    }
                    //}


                    // Is it a nullable type?
                    if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof( Nullable<> ))
                    {
                        var t = propertyType.GetGenericArguments()[0];

                        if (t == typeof( Boolean ))
                        {
                            bool? nullableValue = (bool?)propertyValue;
                            if (!isToWriteAllValues)
                            {
                                if (propertyValue == null)
                                {
                                    if (attribute0.Value == null)
                                    {
                                        // Both the value of the property, and also the default-value, are null - so we do not need to write this.
                                        isToWriteThis = false;
                                    }
                                    else
                                    {
                                        isToWriteThis = true;
                                    }
                                }
                                else // propertyValue is not null
                                {
                                    if (attribute0.Value == null)
                                    {
                                        isToWriteThis = true;
                                    }
                                    else // neither propertyValue nor the default value are null.
                                    {
                                        // We need to write this value only if it is different than the default-value.
                                        bool? defaultValue = (bool?)attribute0.Value;
                                        isToWriteThis = nullableValue.Value != defaultValue.Value;
                                    }
                                }
                            }
                            else
                            {
                                isToWriteThis = true;
                            }
                            if (isToWriteThis)
                            {
                                if (nullableValue.HasValue)
                                {
                                    Write( stream, propertyName + ": " + nullableValue.Value.ToString().ToLower() );
                                }
                                else // it is null
                                {
                                    Write( stream, propertyName + ": null" );
                                }
                            }
                        }

                        else if (t == typeof( byte ))
                        {
                            byte? nullableValue = (byte?)propertyValue;
                            if (!isToWriteAllValues)
                            {
                                if (propertyValue == null)
                                {
                                    if (attribute0.Value == null)
                                    {
                                        // Both the value of the property, and also the default-value, are null - so we do not need to write this.
                                        isToWriteThis = false;
                                    }
                                    else
                                    {
                                        isToWriteThis = true;
                                    }
                                }
                                else // propertyValue is not null
                                {
                                    if (attribute0.Value == null)
                                    {
                                        isToWriteThis = true;
                                    }
                                    else // neither propertyValue nor the default value are null.
                                    {
                                        // We need to write this value only if it is different than the default-value.
                                        int defaultValue = (Int32)attribute0.Value;
                                        isToWriteThis = nullableValue.Value != defaultValue;
                                    }
                                }
                            }
                            else
                            {
                                isToWriteThis = true;
                            }
                            if (isToWriteThis)
                            {
                                if (nullableValue.HasValue)
                                {
                                    Write( stream, propertyName + ": " + nullableValue.Value );
                                }
                                else // it is null
                                {
                                    Write( stream, propertyName + ": null" );
                                }
                            }
                        }

                        else if (t == typeof( Decimal ))
                        {
                            decimal? nullableValue = (Decimal?)propertyValue;
                            if (!isToWriteAllValues)
                            {
                                if (propertyValue == null)
                                {
                                    if (attribute0.Value == null)
                                    {
                                        // Both the value of the property, and also the default-value, are null - so we do not need to write this.
                                        isToWriteThis = false;
                                    }
                                    else
                                    {
                                        isToWriteThis = true;
                                    }
                                }
                                else // propertyValue is not null
                                {
                                    if (attribute0.Value == null)
                                    {
                                        isToWriteThis = true;
                                    }
                                    else // neither propertyValue nor the default value are null.
                                    {
                                        // We need to write this value only if it is different than the default-value.
                                        // I did not see the default-value ever appear as Decimal, but I'm leaving that possiblity anyway.
                                        Type typeOfDefaultValue = attribute0.Value.GetType();
                                        if (typeOfDefaultValue == typeof( Double ))
                                        {
                                            double defaultValue = (Double)attribute0.Value;
                                            decimal decimalDefaultValue = (decimal)defaultValue;
                                            isToWriteThis = nullableValue.Value != decimalDefaultValue;
                                        }
                                        else if (typeOfDefaultValue == typeof( Int32 ))
                                        {
                                            int defaultValue = (Int32)attribute0.Value;
                                            decimal decimalDefaultValue = (decimal)defaultValue;
                                            isToWriteThis = nullableValue.Value != decimalDefaultValue;
                                        }
                                        else // the default-value is Decimal
                                        {
                                            decimal defaultValue = (Decimal)attribute0.Value;
                                            isToWriteThis = nullableValue.Value != defaultValue;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                isToWriteThis = true;
                            }
                            if (isToWriteThis)
                            {
                                if (nullableValue.HasValue)
                                {
                                    Write( stream, propertyName + ": " + nullableValue.Value.ToString( "G" ) );
                                }
                                else // it is null
                                {
                                    Write( stream, propertyName + ": null" );
                                }
                            }
                        }

                        else if (t == typeof( Double ))
                        {
                            double? nullableValue = (double?)propertyValue;
                            if (!isToWriteAllValues)
                            {
                                if (propertyValue == null)
                                {
                                    if (attribute0.Value == null)
                                    {
                                        // Both the value of the property, and also the default-value, are null - so we do not need to write this.
                                        isToWriteThis = false;
                                    }
                                    else
                                    {
                                        isToWriteThis = true;
                                    }
                                }
                                else // propertyValue is not null
                                {
                                    if (attribute0.Value == null)
                                    {
                                        isToWriteThis = true;
                                    }
                                    else // neither propertyValue nor the default value are null.
                                    {
                                        // We need to write this value only if it is different than the default-value.

                                        Type typeOfDefaultValue = attribute0.Value.GetType();
                                        if (typeOfDefaultValue == typeof( Double ))
                                        {
                                            double? defaultValue = (Double?)attribute0.Value;
                                            isToWriteThis = !MathLib.IsEssentiallyEqual( nullableValue.Value, defaultValue.Value );
                                        }
                                        else if (typeOfDefaultValue == typeof( Int32 ))
                                        {
                                            int defaultValue = (Int32)attribute0.Value;
                                            double doubleDefaultValue = (double)defaultValue;
                                            isToWriteThis = !MathLib.IsEssentiallyEqual( nullableValue.Value, doubleDefaultValue );
                                        }
                                    }
                                }
                            }
                            else
                            {
                                isToWriteThis = true;
                            }
                            if (isToWriteThis)
                            {
                                if (nullableValue.HasValue)
                                {
                                    Write( stream, propertyName + ": " + nullableValue.Value.ToString( "G" ) );
                                }
                                else // it is null
                                {
                                    Write( stream, propertyName + ": null" );
                                }
                            }
                        }

                        else if (t == typeof( float ))
                        {
                            float? nullableValue = (float?)propertyValue;
                            if (!isToWriteAllValues)
                            {
                                if (propertyValue == null)
                                {
                                    if (attribute0.Value == null)
                                    {
                                        // Both the value of the property, and also the default-value, are null - so we do not need to write this.
                                        isToWriteThis = false;
                                    }
                                    else
                                    {
                                        isToWriteThis = true;
                                    }
                                }
                                else // propertyValue is not null
                                {
                                    if (attribute0.Value == null)
                                    {
                                        isToWriteThis = true;
                                    }
                                    else // neither propertyValue nor the default value are null.
                                    {
                                        // We need to write this value only if it is different than the default-value.

                                        Type typeOfDefaultValue = attribute0.Value.GetType();
                                        if (typeOfDefaultValue == typeof( Double ))
                                        {
                                            double defaultValue = (Double)attribute0.Value;
                                            float floatDefaultValue = (float)defaultValue;
                                            isToWriteThis = !MathLib.IsEssentiallyEqual( nullableValue.Value, floatDefaultValue );
                                        }
                                        else if (typeOfDefaultValue == typeof( float ))
                                        {
                                            float floatDefaultValue = (float)attribute0.Value;
                                            isToWriteThis = !MathLib.IsEssentiallyEqual( nullableValue.Value, floatDefaultValue );
                                        }
                                        else if (typeOfDefaultValue == typeof( Int32 ))
                                        {
                                            int defaultValue = (Int32)attribute0.Value;
                                            float doubleDefaultValue = (float)defaultValue;
                                            isToWriteThis = !MathLib.IsEssentiallyEqual( nullableValue.Value, doubleDefaultValue );
                                        }
                                    }
                                }
                            }
                            else
                            {
                                isToWriteThis = true;
                            }
                            if (isToWriteThis)
                            {
                                if (nullableValue.HasValue)
                                {
                                    Write( stream, propertyName + ": " + nullableValue.Value.ToString( "G" ) );
                                }
                                else // it is null
                                {
                                    Write( stream, propertyName + ": null" );
                                }
                            }
                        }

                        else if (t == typeof( Int32 ))
                        {
                            int? nullableValue = (int?)propertyValue;
                            if (!isToWriteAllValues)
                            {
                                if (propertyValue == null)
                                {
                                    if (attribute0.Value == null)
                                    {
                                        // Both the value of the property, and also the default-value, are null - so we do not need to write this.
                                        isToWriteThis = false;
                                    }
                                    else
                                    {
                                        isToWriteThis = true;
                                    }
                                }
                                else // propertyValue is not null
                                {
                                    if (attribute0.Value == null)
                                    {
                                        isToWriteThis = true;
                                    }
                                    else // neither propertyValue nor the default value are null.
                                    {
                                        // We need to write this value only if it is different than the default-value.
                                        int defaultValue = (Int32)attribute0.Value;
                                        isToWriteThis = nullableValue.Value != defaultValue;
                                    }
                                }
                            }
                            else
                            {
                                isToWriteThis = true;
                            }
                            if (isToWriteThis)
                            {
                                if (nullableValue.HasValue)
                                {
                                    Write( stream, propertyName + ": " + nullableValue.Value );
                                }
                                else // it is null
                                {
                                    Write( stream, propertyName + ": null" );
                                }
                            }
                        }

                        else if (t == typeof( Int64 ))
                        {
                            Int64? nullableValue = (Int64?)propertyValue;
                            if (!isToWriteAllValues)
                            {
                                if (propertyValue == null)
                                {
                                    if (attribute0.Value == null)
                                    {
                                        // Both the value of the property, and also the default-value, are null - so we do not need to write this.
                                        isToWriteThis = false;
                                    }
                                    else
                                    {
                                        isToWriteThis = true;
                                    }
                                }
                                else // propertyValue is not null
                                {
                                    if (attribute0.Value == null)
                                    {
                                        isToWriteThis = true;
                                    }
                                    else // neither propertyValue nor the default value are null.
                                    {
                                        // We need to write this value only if it is different than the default-value.
                                        Type typeOfDefaultValue = attribute0.Value.GetType();
                                        if (typeOfDefaultValue == typeof( Int32 ))
                                        {
                                            Int32 defaultValue = (Int32)attribute0.Value;
                                            isToWriteThis = nullableValue.Value != defaultValue;
                                        }
                                        else
                                        {
                                            Int64 defaultValue = (Int64)attribute0.Value;
                                            isToWriteThis = nullableValue.Value != defaultValue;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                isToWriteThis = true;
                            }
                            if (isToWriteThis)
                            {
                                if (nullableValue.HasValue)
                                {
                                    Write( stream, propertyName + ": " + nullableValue.Value );
                                }
                                else // it is null
                                {
                                    Write( stream, propertyName + ": null" );
                                }
                            }
                        }

                        else if (t == typeof( sbyte ))
                        {
                            sbyte? nullableValue = (sbyte?)propertyValue;
                            if (!isToWriteAllValues)
                            {
                                if (propertyValue == null)
                                {
                                    if (attribute0.Value == null)
                                    {
                                        // Both the value of the property, and also the default-value, are null - so we do not need to write this.
                                        isToWriteThis = false;
                                    }
                                    else
                                    {
                                        isToWriteThis = true;
                                    }
                                }
                                else // propertyValue is not null
                                {
                                    if (attribute0.Value == null)
                                    {
                                        isToWriteThis = true;
                                    }
                                    else // neither propertyValue nor the default value are null.
                                    {
                                        // We need to write this value only if it is different than the default-value.
                                        int intDefaultValue = (int)attribute0.Value;
                                        sbyte defaultValue = (sbyte)intDefaultValue;
                                        isToWriteThis = nullableValue.Value != defaultValue;
                                    }
                                }
                            }
                            else
                            {
                                isToWriteThis = true;
                            }
                            if (isToWriteThis)
                            {
                                if (nullableValue.HasValue)
                                {
                                    Write( stream, propertyName + ": " + nullableValue.Value );
                                }
                                else // it is null
                                {
                                    Write( stream, propertyName + ": null" );
                                }
                            }
                        }

                        else if (t == typeof( uint ))
                        {
                            uint? nullableValue = (uint?)propertyValue;
                            if (!isToWriteAllValues)
                            {
                                if (propertyValue == null)
                                {
                                    if (attribute0.Value == null)
                                    {
                                        // Both the value of the property, and also the default-value, are null - so we do not need to write this.
                                        isToWriteThis = false;
                                    }
                                    else
                                    {
                                        isToWriteThis = true;
                                    }
                                }
                                else // propertyValue is not null
                                {
                                    if (attribute0.Value == null)
                                    {
                                        isToWriteThis = true;
                                    }
                                    else // neither propertyValue nor the default value are null.
                                    {
                                        // We need to write this value only if it is different than the default-value.
                                        Type typeOfDefaultValue = attribute0.Value.GetType();
                                        if (typeOfDefaultValue == typeof( Int64 ))
                                        {
                                            Int64 intDefaultValue = (Int64)attribute0.Value;
                                            uint defaultValue = (uint)intDefaultValue;
                                            isToWriteThis = nullableValue.Value != defaultValue;
                                        }
                                        else
                                        {
                                            int intDefaultValue = (int)attribute0.Value;
                                            uint defaultValue = (uint)intDefaultValue;
                                            isToWriteThis = nullableValue.Value != defaultValue;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                isToWriteThis = true;
                            }
                            if (isToWriteThis)
                            {
                                if (nullableValue.HasValue)
                                {
                                    Write( stream, propertyName + ": " + nullableValue.Value );
                                }
                                else // it is null
                                {
                                    Write( stream, propertyName + ": null" );
                                }
                            }
                        }

                        else if (t == typeof( UInt64 ))
                        {
                            UInt64? nullableValue = (UInt64?)propertyValue;
                            if (!isToWriteAllValues)
                            {
                                if (propertyValue == null)
                                {
                                    if (attribute0.Value == null)
                                    {
                                        // Both the value of the property, and also the default-value, are null - so we do not need to write this.
                                        isToWriteThis = false;
                                    }
                                    else
                                    {
                                        isToWriteThis = true;
                                    }
                                }
                                else // propertyValue is not null
                                {
                                    if (attribute0.Value == null)
                                    {
                                        isToWriteThis = true;
                                    }
                                    else // neither propertyValue nor the default value are null.
                                    {
                                        // We need to write this value only if it is different than the default-value.
                                        Type typeOfDefaultValue = attribute0.Value.GetType();
                                        if (typeOfDefaultValue == typeof( Int64 ))
                                        {
                                            Int64 intDefaultValue = (Int64)attribute0.Value;
                                            UInt64 defaultValue = (UInt64)intDefaultValue;

                                            string correctValueText = ( UInt64.MaxValue - 1 ).ToString();

                                            string valueText = nullableValue.Value.ToString();

                                            string defaultValueText = defaultValue.ToString();

                                            isToWriteThis = nullableValue.Value != defaultValue;
                                        }
                                        else if (typeOfDefaultValue == typeof( Single ))
                                        {
                                            //CBL When I set the default value to Int64.MaxValue, this comes in as a Single
                                            //    which is odd. I see no way to get the true value from it.
                                            //UInt64 uint64Value = (UInt64) attribute0.Value;
                                            throw new ArgumentException( message: "The literal default-value set for property " + propertyName + " evaluates to a Single. Fix that!" );
                                            //CBL
                                            //Single intDefaultValue = (Single)attribute0.Value;
                                            //string singleText = intDefaultValue.ToString( "G" );
                                            //UInt64 defaultValue = (UInt64)intDefaultValue;
                                            //isToWriteThis = nullableValue.Value != defaultValue;
                                        }
                                        else
                                        {
                                            int intDefaultValue = (int)attribute0.Value;
                                            UInt64 defaultValue = (UInt64)intDefaultValue;
                                            isToWriteThis = nullableValue.Value != defaultValue;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                isToWriteThis = true;
                            }
                            if (isToWriteThis)
                            {
                                if (nullableValue.HasValue)
                                {
                                    Write( stream, propertyName + ": " + nullableValue.Value );
                                }
                                else // it is null
                                {
                                    Write( stream, propertyName + ": null" );
                                }
                            }
                        }

                        else
                        {
                            Console.WriteLine( "Unrecognized nullable type: " + propertyName );
                        }
                    }
                    // end of nullable types.

                    else if (propertyType == typeof( Boolean ))
                    {
                        bool booleanValue = (bool)propertyValue;
                        if (!isToWriteAllValues)
                        {
                            bool isTrueByDefault = (bool)attribute0.Value;
                            if (isTrueByDefault)
                            {
                                isToWriteThis = !booleanValue;
                            }
                            else
                            {
                                isToWriteThis = booleanValue;
                            }
                        }
                        else
                        {
                            isToWriteThis = true;
                        }
                        if (isToWriteThis)
                        {
                            Write( stream, propertyName + ": " + booleanValue.ToString().ToLower() );
                        }
                    }
                    else if (propertyType == typeof( byte ))
                    {
                        byte byteValue = (Byte)propertyValue;
                        if (!isToWriteAllValues)
                        {
                            int intDefaultValue = (Int32)attribute0.Value;
                            byte defaultValue = (byte)intDefaultValue;
                            if (byteValue == defaultValue)
                            {
                                isToWriteThis = false;
                            }
                            else
                            {
                                isToWriteThis = true;
                            }
                        }
                        else
                        {
                            isToWriteThis = true;
                        }
                        if (isToWriteThis)
                        {
                            Write( stream, propertyName + ": " + byteValue );
                        }
                    }
                    else if (propertyType == typeof( sbyte ))
                    {
                        sbyte byteValue = (sbyte)propertyValue;
                        if (!isToWriteAllValues)
                        {
                            int intDefaultValue = (Int32)attribute0.Value;
                            sbyte defaultValue = (sbyte)intDefaultValue;
                            if (byteValue == defaultValue)
                            {
                                isToWriteThis = false;
                            }
                            else
                            {
                                isToWriteThis = true;
                            }
                        }
                        else
                        {
                            isToWriteThis = true;
                        }
                        if (isToWriteThis)
                        {
                            Write( stream, propertyName + ": " + byteValue );
                        }
                    }
                    else if (propertyType == typeof( Char ))
                    {
                        char value = (Char)propertyValue;
                        if (!isToWriteAllValues)
                        {
                            char defaultValue = (Char)attribute0.Value;
                            if (value == defaultValue)
                            {
                                isToWriteThis = false;
                            }
                            else
                            {
                                isToWriteThis = true;
                            }
                        }
                        else
                        {
                            isToWriteThis = true;
                        }
                        if (isToWriteThis)
                        {
                            Write( stream, propertyName + ": " + value );
                        }
                    }
                    else if (propertyType == typeof( Decimal ))
                    {
                        decimal decimalValue = (Decimal)propertyValue;
                        if (!isToWriteAllValues)
                        {
                            string stringDefaultValue = attribute0.Value.ToString();
                            decimal decimalDefaultValue;
                            if (Decimal.TryParse( stringDefaultValue, out decimalDefaultValue ))
                            {
                                if (Decimal.Equals( decimalValue, decimalDefaultValue ))
                                {
                                    isToWriteThis = false;
                                }
                                else
                                {
                                    isToWriteThis = true;
                                }
                            }
                            else
                            {
                                isToWriteThis = false;
                                Console.WriteLine( "invalid decimal value!" ); //CBL
                            }
                        }
                        else
                        {
                            isToWriteThis = true;
                        }
                        if (isToWriteThis)
                        {
                            Write( stream, propertyName + ": " + decimalValue );
                        }
                    }
                    else if (propertyType == typeof( Double ))
                    {
                        double value = (Double)propertyValue;
                        if (!isToWriteAllValues)
                        {
                            string stringDefaultValue = attribute0.Value.ToString();
                            double defaultValue;
                            if (Double.TryParse( stringDefaultValue, out defaultValue ))
                            {
                                if (Double.Equals( value, defaultValue ))
                                {
                                    isToWriteThis = false;
                                }
                                else
                                {
                                    isToWriteThis = true;
                                }
                            }
                            else
                            {
                                isToWriteThis = false;
                                Console.WriteLine( "invalid Double value!" ); //CBL
                            }
                        }
                        else
                        {
                            isToWriteThis = true;
                        }
                        if (isToWriteThis)
                        {
                            Write( stream, propertyName + ": " + value );
                        }
                    }
                    else if (propertyType.IsEnum)
                    {
                        if (!isToWriteAllValues)
                        {
                            // We don't actually require the specific enum-value, but rather just whether they are equal.
                            // Therefore we'll convert both into strings and compare them.
                            string stringDefaultValue = attribute0.Value.ToString();
                            string stringValue = propertyValue.ToString();
                            if (stringValue.Equals( stringDefaultValue, StringComparison.OrdinalIgnoreCase ))
                            {
                                isToWriteThis = false;
                            }
                            else
                            {
                                isToWriteThis = true;
                            }
                        }
                        else
                        {
                            isToWriteThis = true;
                        }
                        if (isToWriteThis)
                        {
                            Write( stream, propertyName + ": " + (Enum)propertyValue );
                        }
                    }
                    else if (propertyType == typeof( float ))
                    {
                        float value = (float)propertyValue;
                        if (!isToWriteAllValues)
                        {
                            string stringDefaultValue = attribute0.Value.ToString();
                            float defaultValue;
                            if (float.TryParse( stringDefaultValue, out defaultValue ))
                            {
                                if (float.Equals( value, defaultValue ))
                                {
                                    isToWriteThis = false;
                                }
                                else
                                {
                                    isToWriteThis = true;
                                }
                            }
                            else
                            {
                                isToWriteThis = false;
                                Console.WriteLine( "invalid float value!" ); //CBL
                            }
                        }
                        else
                        {
                            isToWriteThis = true;
                        }
                        if (isToWriteThis)
                        {
                            string stringValue = value.ToString( "G" );
                            Write( stream, propertyName + ": " + stringValue );
                        }
                    }
                    else if (propertyType == typeof( Int32 ))
                    {
                        int value = (Int32)propertyValue;
                        if (!isToWriteAllValues)
                        {
                            int defaultValue = (Int32)attribute0.Value;

                            if (value == defaultValue)
                            {
                                isToWriteThis = false;
                            }
                            else
                            {
                                isToWriteThis = true;
                            }
                        }
                        else
                        {
                            isToWriteThis = true;
                        }
                        if (isToWriteThis)
                        {
                            Write( stream, propertyName + ": " + value );
                        }
                    }
                    else if (propertyType == typeof( Int16 ))
                    {
                        Int16 value = (Int16)propertyValue;
                        if (!isToWriteAllValues)
                        {
                            // That attribute seems to just want to be an Int32 when it sees a whole number.
                            int defaultValue = (Int32)attribute0.Value;

                            if (value == defaultValue)
                            {
                                isToWriteThis = false;
                            }
                            else
                            {
                                isToWriteThis = true;
                            }
                        }
                        else
                        {
                            isToWriteThis = true;
                        }
                        if (isToWriteThis)
                        {
                            Write( stream, propertyName + ": " + value );
                        }
                    }
                    else if (propertyType == typeof( Int64 ))
                    {
                        Int64 value = (Int64)propertyValue;
                        string stringValue = value.ToString();
                        if (!isToWriteAllValues)
                        {
                            // That attribute seems to want to be an Int32 if that value is not too large,
                            // or an Int64 if it's too large to fit within an Int32.
                            // So, I just convert them both into strings and compare them that way.
                            string stringDefaultValue = attribute0.Value.ToString();
                            if (stringValue.Equals( stringDefaultValue ))
                            {
                                isToWriteThis = false;
                            }
                            else
                            {
                                isToWriteThis = true;
                            }
                        }
                        else
                        {
                            isToWriteThis = true;
                        }
                        if (isToWriteThis)
                        {
                            Write( stream, propertyName + ": " + stringValue );
                        }
                    }
                    else if (propertyType == typeof( uint ))
                    {
                        uint value = (uint)propertyValue;
                        string stringValue = value.ToString();
                        if (!isToWriteAllValues)
                        {
                            // That attribute seems to want to be an Int32 if that value is not too large,
                            // or an Int64 if it's too large to fit within an Int32.
                            // So, I just convert them both into strings and compare them that way.
                            Type typeOfDefaultValue = attribute0.Value.GetType();
                            if (typeOfDefaultValue == typeof( Int64 ))
                            {
                                Int64 intDefaultValue = (Int64)attribute0.Value;
                                uint defaultValue = (uint)intDefaultValue;
                                isToWriteThis = value != defaultValue;
                            }
                            else
                            {
                                int intDefaultValue = (int)attribute0.Value;
                                uint defaultValue = (uint)intDefaultValue;
                                isToWriteThis = value != defaultValue;
                            }


                            //string stringDefaultValue = attribute0.Value.ToString();
                            //if (stringValue.Equals( stringDefaultValue ))
                            //{
                            //    isToWriteThis = false;
                            //}
                            //else
                            //{
                            //    isToWriteThis = true;
                            //}
                        }
                        else
                        {
                            isToWriteThis = true;
                        }
                        if (isToWriteThis)
                        {
                            Write( stream, propertyName + ": " + stringValue );
                        }
                    }
                    else if (propertyType == typeof( UInt64 ))
                    {
                        UInt64 value = (UInt64)propertyValue;
                        string stringValue = value.ToString();
                        if (!isToWriteAllValues)
                        {
                            // That attribute seems to want to be an Int32 if that value is not too large,
                            // or an Int64 if it's too large to fit within an Int32.
                            // So, I just convert them both into strings and compare them that way.
                            string stringDefaultValue = attribute0.Value.ToString();
                            if (stringValue.Equals( stringDefaultValue ))
                            {
                                isToWriteThis = false;
                            }
                            else
                            {
                                isToWriteThis = true;
                            }
                        }
                        else
                        {
                            isToWriteThis = true;
                        }
                        if (isToWriteThis)
                        {
                            Write( stream, propertyName + ": " + stringValue );
                        }
                    }
                    else if (propertyType == typeof( string ))
                    {
                        string value = (String)propertyValue;
                        if (!isToWriteAllValues)
                        {
                            string defaultValue = null;
                            if (attribute0.Value != null)
                            {
                                defaultValue = (String)attribute0.Value;
                                if (propertyValue == null)
                                {
                                    isToWriteThis = true;
                                }
                                else // both propertyValue and defaultValue are non-null.
                                {
                                    isToWriteThis = !value.Equals( defaultValue );
                                }
                            }
                            else // default value is null.
                            {
                                if (propertyValue == null)
                                {
                                    isToWriteThis = false;
                                }
                                else
                                {
                                    isToWriteThis = true;
                                }
                            }
                        }
                        else
                        {
                            isToWriteThis = true;
                        }
                        if (isToWriteThis)
                        {
                            if (propertyValue == null)
                            {
                                Write( stream, propertyName + ": null" );
                            }
                            else
                            {
                                // Handle empty strings.
                                if (value.Length > 0)
                                {
                                    Write( stream, propertyName + ": " + (string)propertyValue );
                                }
                                else
                                {
                                    Write( stream, propertyName + ": empty" );
                                }
                            }
                        }
                    }
                    else if (propertyType == typeof( uint ))
                    {
                        uint value = (uint)propertyValue;
                        if (!isToWriteAllValues)
                        {
                            int intDefaultValue = (Int32)attribute0.Value;
                            uint defaultValue = (uint)intDefaultValue;
                            if (value == defaultValue)
                            {
                                isToWriteThis = false;
                            }
                            else
                            {
                                isToWriteThis = true;
                            }
                        }
                        else
                        {
                            isToWriteThis = true;
                        }
                        if (isToWriteThis)
                        {
                            Write( stream, propertyName + ": " + value );
                        }
                    }
                    else if (propertyType == typeof( ushort ))
                    {
                        ushort value = (ushort)propertyValue;
                        if (!isToWriteAllValues)
                        {
                            int intDefaultValue = (Int32)attribute0.Value;
                            ushort defaultValue = (ushort)intDefaultValue;
                            if (value == defaultValue)
                            {
                                isToWriteThis = false;
                            }
                            else
                            {
                                isToWriteThis = true;
                            }
                        }
                        else
                        {
                            isToWriteThis = true;
                        }
                        if (isToWriteThis)
                        {
                            Write( stream, propertyName + ": " + value );
                        }
                    }
                    else if (propertyType.IsValueType)
                    {
                        stream.WriteLine( "  " + propertyName + ": " + propertyValue );
                    }
                    else
                    {
                        Console.WriteLine( "Unrecognized type: " + propertyName + ", propertyType is " + propertyType );
                    }
                }
            }
            return _numberWritten;
        }

        private static void Write( StreamWriter stream, string text )
        {
            if (!_hasWrittenClassName)
            {
                stream.WriteLine( _className + ":" );
                _hasWrittenClassName = true;
            }
            stream.WriteLine( "  " + text );
            _numberWritten++;
        }

        private static DefaultValueAttribute GetAttribute( PropertyInfo prop )
        {
            var attributes = (DefaultValueAttribute[])prop.GetCustomAttributes( typeof( DefaultValueAttribute ), false );
            DefaultValueAttribute attribute0;
            if (attributes.Length > 0)
            {
                attribute0 = attributes[0];
            }
            else
            {
                attribute0 = null;
            }
            return attribute0;
        }

        #endregion WritePropertiesThatHaveDefaultValueAttributeTo

        #region ReadPropertiesFromFile
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pathname"></param>
        /// <param name="whatToRead"></param>
        /// <returns>false if there is an unrecognized value</returns>
        public static bool ReadPropertiesFromFile<T>( string pathname, T whatToRead ) where T : new()
        {
            // Check the argument value..
            if (pathname == null)
            {
                throw new ArgumentNullException( "pathname" );
            }
            // Remove the original file if it exists.
            if (!File.Exists( pathname ))
            {
                throw new FileNotFoundException( "File " + pathname + " was not found." );
            }

            bool r = true;
            string nameOfT = whatToRead.GetType().Name;

            using (var streamReader = new StreamReader( pathname ))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    //Console.WriteLine( line );

                    if (line.Equals( nameOfT + ":" ))
                    {
                        //Console.WriteLine( "  found class-name." );
                    }
                    else
                    {
                        // Loop through every property to find the one that this line of text identifies..
                        string nameOfProperty = line.PartBefore( ':' ).TrimStart();
                        string propertyText = line.PartAfter( ':' ).Trim();
                        //Console.WriteLine( @"  property """ + nameOfProperty + @""" with value """ + propertyText + @"""" );

                        bool hasFoundMatch = false;
                        //var properties = whatToRead.GetType().GetProperties().Where( prop => prop.IsDefined( typeof( DefaultValueAttribute ), false ) ).OrderBy( p => p.Name );
                        //var properties = typeof( T ).GetProperties().Where( prop => prop.IsDefined( typeof( DefaultValueAttribute ), false ) ).OrderBy( p => p.Name );
                        var properties = typeof( T ).GetProperties();
                        foreach (PropertyInfo propertyInfo in properties)
                        {
                            string propertyName = propertyInfo.Name;
                            if (propertyName == nameOfProperty)
                            {
                                //Console.WriteLine( "    found match." );
                                hasFoundMatch = true;
                                var propertyType = propertyInfo.PropertyType;
                                // Is it a nullable ?
                                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof( Nullable<> ))
                                {
                                    if (propertyText == "null")
                                    {
                                        propertyInfo.SetValue( obj: whatToRead, value: null, index: null );
                                    }
                                    else
                                    {
                                        var t = propertyType.GetGenericArguments()[0];
                                        if (t == typeof( Boolean ))
                                        {
                                            bool value = Boolean.Parse( propertyText );
                                            propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                        }
                                        else if (t == typeof( byte ))
                                        {
                                            byte value = Byte.Parse( propertyText );
                                            propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                        }
                                        else if (t == typeof( Decimal ))
                                        {
                                            decimal value = Decimal.Parse( propertyText );
                                            propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                        }
                                        else if (t == typeof( Double ))
                                        {
                                            double value = Double.Parse( propertyText );
                                            propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                        }
                                        else if (t == typeof( float ))
                                        {
                                            float value = float.Parse( propertyText );
                                            propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                        }
                                        else if (t == typeof( Int16 ))
                                        {
                                            Int16 value = Int16.Parse( propertyText );
                                            propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                        }
                                        else if (t == typeof( Int32 ))
                                        {
                                            int value = Int32.Parse( propertyText );
                                            propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                        }
                                        else if (t == typeof( Int64 ))
                                        {
                                            Int64 value = Int64.Parse( propertyText );
                                            propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                        }
                                        else if (t == typeof( sbyte ))
                                        {
                                            sbyte value = sbyte.Parse( propertyText );
                                            propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                        }
                                        else if (t == typeof( String ))
                                        {
                                            propertyInfo.SetValue( obj: whatToRead, value: propertyText, index: null );
                                        }
                                        else if (t.IsEnum)
                                        {
                                            propertyInfo.SetValue( obj: whatToRead, value: Enum.Parse( propertyType, propertyText ), index: null );
                                        }
                                        else if (t == typeof( uint ))
                                        {
                                            uint value = uint.Parse( propertyText );
                                            propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                        }
                                        else if (t == typeof( UInt64 ))
                                        {
                                            UInt64 value = UInt64.Parse( propertyText );
                                            propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                        }
                                        else
                                        {
                                            Console.WriteLine( "Failed to find type for nullable property " + propertyName );
                                            r = false;
                                        }
                                    }
                                }
                                else // not a nullable type
                                {
                                    if (propertyType == typeof( Boolean ))
                                    {
                                        bool value = Boolean.Parse( propertyText );
                                        propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                    }
                                    else if (propertyType == typeof( byte ))
                                    {
                                        byte value = Byte.Parse( propertyText );
                                        propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                    }
                                    else if (propertyType == typeof( Char ))
                                    {
                                        Char value = Char.Parse( propertyText );
                                        propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                    }
                                    else if (propertyType == typeof( Decimal ))
                                    {
                                        decimal value = Decimal.Parse( propertyText );
                                        propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                    }
                                    else if (propertyType == typeof( Double ))
                                    {
                                        double value = Double.Parse( propertyText );
                                        propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                    }
                                    else if (propertyType == typeof( float ))
                                    {
                                        float value = float.Parse( propertyText );
                                        propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                    }
                                    else if (propertyType == typeof( Int16 ))
                                    {
                                        Int16 value = Int16.Parse( propertyText );
                                        propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                    }
                                    else if (propertyType == typeof( Int32 ))
                                    {
                                        int value = Int32.Parse( propertyText );
                                        propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                    }
                                    else if (propertyType == typeof( Int64 ))
                                    {
                                        Int64 value = Int64.Parse( propertyText );
                                        propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                    }
                                    else if (propertyType == typeof( sbyte ))
                                    {
                                        sbyte value = sbyte.Parse( propertyText );
                                        propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                    }
                                    else if (propertyType == typeof( String ))
                                    {
                                        if (propertyText == "empty")
                                        {
                                            propertyInfo.SetValue( obj: whatToRead, value: String.Empty, index: null );
                                        }
                                        else if (propertyText == "null")
                                        {
                                            propertyInfo.SetValue( obj: whatToRead, value: null, index: null );
                                        }
                                        else
                                        {
                                            propertyInfo.SetValue( obj: whatToRead, value: propertyText, index: null );
                                        }
                                    }
                                    else if (propertyType.IsEnum)
                                    {
                                        propertyInfo.SetValue( obj: whatToRead, value: Enum.Parse( propertyType, propertyText ), index: null );
                                    }
                                    else if (propertyType == typeof( uint ))
                                    {
                                        uint value = uint.Parse( propertyText );
                                        propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                    }
                                    else if (propertyType == typeof( UInt64 ))
                                    {
                                        UInt64 value = UInt64.Parse( propertyText );
                                        propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                    }
                                    else if (propertyType == typeof( ushort ))
                                    {
                                        ushort value = ushort.Parse( propertyText );
                                        propertyInfo.SetValue( obj: whatToRead, value: value, index: null );
                                    }
                                    else
                                    {
                                        Console.WriteLine( "Failed to find type for property " + propertyName );
                                        r = false;
                                    }
                                }
                            }
                        }
                        if (!hasFoundMatch)
                        {
                            Console.WriteLine( "  failed to find match." );
                            r = false;
                        }
                    }
                }
            }
            return r;
        }
        #endregion

        #region fields

        private static string _className;
        private static bool _hasWrittenClassName;
        private static int _numberWritten;

        #endregion fields
    }
}