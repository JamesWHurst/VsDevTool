using System;


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// This class exists to provide a number of misc enum-type related utility methods.
    /// </summary>
    public static class EnumLib
    {
        /// <summary>
        /// Return the next enum-value of the given type that comes after the given value,
        /// or the first enum-value if the given value is already the last one.
        /// </summary>
        /// <typeparam name="T">the specific enum-type</typeparam>
        /// <param name="src">the given value of type T</param>
        /// <returns>the next value</returns>
        public static T Next<T>( this T src ) where T : struct
        {
            if (!typeof( T ).IsEnum) throw new ArgumentException( String.Format( "Argumnent {0} is not an Enum", typeof( T ).FullName ) );

            T[] Arr = (T[])Enum.GetValues( src.GetType() );
            int j = Array.IndexOf<T>( Arr, src ) + 1;
            return (j == Arr.Length) ? Arr[0] : Arr[j];
        }

        /// <summary>
        /// Return the preceeding enum-value of the given type that comes before the given value,
        /// or the last enum-value if the given value is already the last one.
        /// </summary>
        /// <typeparam name="T">the specific enum-type</typeparam>
        /// <param name="src">the given value of type T</param>
        /// <returns>the preceeding value</returns>
        public static T Previous<T>( this T src ) where T : struct
        {
            if (!typeof( T ).IsEnum) throw new ArgumentException( String.Format( "Argumnent {0} is not an Enum", typeof( T ).FullName ) );

            T[] Arr = (T[])Enum.GetValues( src.GetType() );
            int j = Array.IndexOf<T>( Arr, src ) - 1;
            T newValue = (j == 0) ? Arr[Arr.Length - 1] : Arr[j];
            return newValue;
        }

    }
}
