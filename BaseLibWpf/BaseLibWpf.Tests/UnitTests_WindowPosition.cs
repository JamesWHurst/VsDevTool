using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using Hurst.BaseLib;
using Hurst.BaseLibWpf;
using NUnit.Framework;


namespace BaseLibWpf.Tests
{
    [TestFixture]
    public class UnitTests_WindowPosition
    {
        [Test]
        public void SavePosition_NullWindow_ThrowsArgumentNullException()
        {
            var windowPosition = new WindowPosition();
            Window window = null;
            Assert.Throws<ArgumentNullException>( () => windowPosition.SavePosition( window ) );
        }

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_SavingLocationAndSizeAndWindowHasNotChanged_HasChangedReturnsFalse()
        {
            var windowPosition = new WindowPosition();
            windowPosition.IsSavingLocation = true;
            windowPosition.IsSavingSize = true;
            Window window = new Window();
            windowPosition.SavePosition( window );
            windowPosition.HasChanged = false;
            windowPosition.SavePosition( window );
            Assert.IsFalse( windowPosition.HasChanged );
        }

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_WindowIsMaximized_HasChangedReturnsTrue()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.SavePosition( window );
            windowPosition.HasChanged = false;
            window.WindowState = WindowState.Maximized;
            windowPosition.SavePosition( window );
            Assert.IsTrue( windowPosition.HasChanged );
        }

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_WindowIsMinimized_HasChangedReturnsTrue()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.SavePosition( window );
            windowPosition.HasChanged = false;
            window.WindowState = WindowState.Minimized;
            windowPosition.SavePosition( window );
            Assert.IsTrue( windowPosition.HasChanged );
        }

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_SavingLocationAndWindowLeftIsDifferent_HasChangedReturnsTrue()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.IsSavingLocation = true;
            window.Left = 20;
            window.Top = 10;
            window.Width = 100;
            window.Height = 60;
            windowPosition.SavePosition( window );
            windowPosition.HasChanged = false;
            window.Left = 1;
            windowPosition.SavePosition( window );
            Assert.IsTrue( windowPosition.HasChanged );
        }

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_SavingLocationAndWindowTopIsDifferent_HasChangedReturnsTrue()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.IsSavingLocation = true;
            window.Left = 20;
            window.Top = 10;
            window.Width = 100;
            window.Height = 60;
            windowPosition.SavePosition( window );
            windowPosition.HasChanged = false;
            window.Top = 3;
            windowPosition.SavePosition( window );
            Assert.IsTrue( windowPosition.HasChanged );
        }

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_SavingLocationAndWindowWidthIsDifferent_HasChangedReturnsFalse()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.IsSavingLocation = true;
            windowPosition.SavePosition( window );
            windowPosition.HasChanged = false;
            window.Width = 99;
            windowPosition.SavePosition( window );
            Assert.IsFalse( windowPosition.HasChanged );
        }

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_SavingLocationAndWindowHeightIsDifferent_HasChangedReturnsFalse()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.IsSavingLocation = true;
            windowPosition.SavePosition( window );
            windowPosition.HasChanged = false;
            window.Height = 919;
            windowPosition.SavePosition( window );
            Assert.IsFalse( windowPosition.HasChanged );
        }

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_SavingSizeAndWindowWidthIsDifferent_HasChangedReturnsTrue()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.IsSavingSize = true;
            window.Left = 20;
            window.Top = 10;
            window.Width = 100;
            window.Height = 60;
            windowPosition.SavePosition( window );
            windowPosition.HasChanged = false;
            window.Width = 999;
            windowPosition.SavePosition( window );
            Assert.IsTrue( windowPosition.HasChanged );
        }

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_SavingSizeAndWindowHeightIsDifferent_HasChangedReturnsTrue()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.IsSavingSize = true;
            window.Left = 20;
            window.Top = 10;
            window.Width = 100;
            window.Height = 60;
            windowPosition.SavePosition( window );
            windowPosition.HasChanged = false;
            window.Height = 999;
            windowPosition.SavePosition( window );
            Assert.IsTrue( windowPosition.HasChanged );
        }

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_SavingSizeAndWindowLeftIsDifferent_HasChangedReturnsFalse()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.IsSavingSize = true;
            window.Left = 10;
            window.Top = 20;
            windowPosition.SavePosition( window );
            windowPosition.HasChanged = false;
            window.Left = 11;
            windowPosition.SavePosition( window );
            Assert.IsFalse( windowPosition.HasChanged );
        }

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_SavingSizeAndWindowTopIsDifferent_HasChangedReturnsFalse()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.IsSavingSize = true;
            window.Left = 10;
            window.Top = 20;
            windowPosition.SavePosition( window );
            windowPosition.HasChanged = false;
            window.Top = 21;
            windowPosition.SavePosition( window );
            Assert.IsFalse( windowPosition.HasChanged );
        }

        // SavingSize

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_SavingSize_LoadPositionRestoresWidth()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.IsSavingSize = true;
            window.Width = 100;
            window.Height = 60;
            windowPosition.SavePosition( window );
            window.Width = 101;
            window.Height = 61;
            windowPosition.LoadPosition( window );
            Assert.AreEqual( 100, window.Width );
        }

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_SavingSize_LoadPositionRestoresHeight()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.IsSavingSize = true;
            window.Width = 100;
            window.Height = 60;
            windowPosition.SavePosition( window );
            window.Width = 101;
            window.Height = 61;
            windowPosition.LoadPosition( window );
            Assert.AreEqual( 60, window.Height );
        }

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_SavingSize_LoadPositionDoesNotRestoreLeft()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.IsSavingSize = true;
            window.Left = 20;
            window.Top = 10;
            window.Width = 100;
            window.Height = 60;
            windowPosition.SavePosition( window );
            window.Left = 21;
            window.Top = 11;
            window.Width = 101;
            window.Height = 61;
            windowPosition.LoadPosition( window );
            Assert.AreEqual( 21, window.Left );
        }

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_SavingSize_LoadPositionDoesNotRestoreTop()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.IsSavingSize = true;
            window.Left = 20;
            window.Top = 10;
            window.Width = 100;
            window.Height = 60;
            windowPosition.SavePosition( window );
            window.Left = 21;
            window.Top = 11;
            window.Width = 101;
            window.Height = 61;
            windowPosition.LoadPosition( window );
            Assert.AreEqual( 11, window.Top );
        }

        // SavingLocation

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_SavingLocation_LoadPositionRestoresLeft()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.IsSavingLocation = true;
            window.Left = 20;
            window.Top = 10;
            window.Width = 100;
            window.Height = 60;
            windowPosition.SavePosition( window );
            window.Left = 21;
            window.Top = 11;
            window.Width = 101;
            window.Height = 61;
            windowPosition.LoadPosition( window );
            Assert.AreEqual( 20, window.Left );
        }

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_SavingLocation_LoadPositionRestoresTop()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.IsSavingLocation = true;
            window.Left = 20;
            window.Top = 10;
            window.Width = 100;
            window.Height = 60;
            windowPosition.SavePosition( window );
            window.Left = 21;
            window.Top = 11;
            window.Width = 101;
            window.Height = 61;
            windowPosition.LoadPosition( window );
            Assert.AreEqual( 10, window.Top );
        }

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_SavingLocation_LoadPositionDoesNotRestoreWidth()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.IsSavingLocation = true;
            window.Left = 20;
            window.Top = 10;
            window.Width = 100;
            window.Height = 60;
            windowPosition.SavePosition( window );
            window.Left = 21;
            window.Top = 11;
            window.Width = 101;
            window.Height = 61;
            windowPosition.LoadPosition( window );
            Assert.AreEqual( 101, window.Width );
        }

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_SavingLocation_LoadPositionDoesNotRestoreHeight()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.IsSavingLocation = true;
            window.Left = 20;
            window.Top = 10;
            window.Width = 100;
            window.Height = 60;
            windowPosition.SavePosition( window );
            window.Left = 21;
            window.Top = 11;
            window.Width = 101;
            window.Height = 61;
            windowPosition.LoadPosition( window );
            Assert.AreEqual( 61, window.Height );
        }

        // SavingSize and SavingLocation

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_SavingLocationAndSize_LoadPositionRestoresAll()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.IsSavingLocation = true;
            windowPosition.IsSavingSize = true;
            window.Left = 20;
            window.Top = 10;
            window.Width = 100;
            window.Height = 60;
            windowPosition.SavePosition( window );
            window.Left = 21;
            window.Top = 11;
            window.Width = 101;
            window.Height = 61;
            windowPosition.LoadPosition( window );
            Assert.AreEqual( 20, window.Left );
            Assert.AreEqual( 10, window.Top );
            Assert.AreEqual( 100, window.Width );
            Assert.AreEqual( 60, window.Height );
        }

        // protect against uninitialized values

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_SavingLocationAndSizeButNoValues_NothingChanged()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.IsSavingLocation = true;
            windowPosition.IsSavingSize = true;
            window.Left = 21;
            window.Top = 11;
            window.Width = 101;
            window.Height = 61;
            windowPosition.LoadPosition( window );
            Assert.AreEqual( 21, window.Left );
            Assert.AreEqual( 11, window.Top );
            Assert.AreEqual( 101, window.Width );
            Assert.AreEqual( 61, window.Height );
        }

        // WindowState

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_SavingLocationAndSizeWindowWasNormal_WindowStateIsRestored()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.IsSavingLocation = true;
            windowPosition.IsSavingSize = true;
            window.Left = 20;
            window.Top = 10;
            window.Width = 100;
            window.Height = 60;
            windowPosition.SavePosition( window );
            window.WindowState = WindowState.Maximized;
            windowPosition.LoadPosition( window );
            Assert.AreEqual( WindowState.Normal, window.WindowState );
        }

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_SavingLocationAndSizeWindowWasMaximized_WindowStateIsRestored()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.IsSavingLocation = true;
            windowPosition.IsSavingSize = true;
            window.Left = 20;
            window.Top = 10;
            window.Width = 100;
            window.Height = 60;
            window.WindowState = WindowState.Maximized;
            windowPosition.SavePosition( window );
            window.WindowState = WindowState.Normal;
            windowPosition.LoadPosition( window );
            Assert.AreEqual( WindowState.Maximized, window.WindowState );
        }

        [Test, Apartment( ApartmentState.STA )]
        public void SavePosition_SavingLocationAndSizeWindowWasMinimized_WindowStateIsRestored()
        {
            var windowPosition = new WindowPosition();
            Window window = new Window();
            windowPosition.IsSavingLocation = true;
            windowPosition.IsSavingSize = true;
            window.Left = 20;
            window.Top = 10;
            window.Width = 100;
            window.Height = 60;
            window.WindowState = WindowState.Minimized;
            windowPosition.SavePosition( window );
            window.WindowState = WindowState.Normal;
            windowPosition.LoadPosition( window );
            Assert.AreEqual( WindowState.Minimized, window.WindowState );
        }

    }
}
