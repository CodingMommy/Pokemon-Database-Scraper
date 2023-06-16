// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Serialization;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.ComponentModel;
using Microsoft.UI.Windowing;

using Microsoft.UI.Composition.SystemBackdrops;
using System.Runtime.InteropServices; // For DllImport
using WinRT; // required to support Window.As<ICompositionSupportsSystemBackdrop>()
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PokemonDatabaseScraper
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        WindowsSystemDispatcherQueueHelper m_wsdqHelper; // See below for implementation.
        MicaController m_backdropController;
        SystemBackdropConfiguration m_configurationSource;

        public MainWindow()
        {
            this.InitializeComponent();
            TrySetSystemBackdrop();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            LoadCardLibrary();
        }

        bool TrySetSystemBackdrop()
        {
            if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
            {
                m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
                m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

                // Create the policy object.
                m_configurationSource = new SystemBackdropConfiguration();
                this.Activated += Window_Activated;
                this.Closed += Window_Closed;
                ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;

                // Initial configuration state.
                m_configurationSource.IsInputActive = true;
                SetConfigurationSourceTheme();

                m_backdropController = new Microsoft.UI.Composition.SystemBackdrops.MicaController() { Kind = MicaKind.BaseAlt };
    
            // Enable the system backdrop.
            // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
            m_backdropController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                m_backdropController.SetSystemBackdropConfiguration(m_configurationSource);
                return true; // succeeded
            }
            return false; // Mica is not supported on this system
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            /// m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
            
            if (args.WindowActivationState == WindowActivationState.Deactivated) {
                m_backdropController.Kind = MicaKind.Base;
            } else
            {
                m_backdropController.Kind = MicaKind.BaseAlt;
            }
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            // Make sure any Mica/Acrylic controller is disposed
            // so it doesn't try to use this closed window.
            if (m_backdropController != null)
            {
                m_backdropController.Dispose();
                m_backdropController = null;
            }
            this.Activated -= Window_Activated;
            m_configurationSource = null;
        }

        private void Window_ThemeChanged(FrameworkElement sender, object args)
        {
            if (m_configurationSource != null)
            {
                SetConfigurationSourceTheme();
            }
        }

        private void SetConfigurationSourceTheme()
        {
            switch (((FrameworkElement)this.Content).ActualTheme)
            {
                case ElementTheme.Dark: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Dark; break;
                case ElementTheme.Light: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Light; break;
                case ElementTheme.Default: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Default; break;
            }
        }

        private void SaveCardLibrary()
        {
            XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<CardSet>));
            using (StreamWriter wr = new StreamWriter("Assets\\Cards\\Cards.xml"))
            {
                xs.Serialize(wr, Globals.CardSets);
            }
            xs = null;
        }

        private void LoadCardLibrary()
        {
            if (File.Exists("Assets\\Cards\\Cards.xml"))
            {
                XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<CardSet>));
                using StreamReader rd = new StreamReader("Assets\\Cards\\Cards.xml");
                Globals.CardSets = xs.Deserialize(rd) as ObservableCollection<CardSet>;
                xs = null;
            } else {
                CardLibraryTeachingTip.IsOpen = true;
                NavBar.IsPaneOpen = true;
            }
        }

        private void NavBar_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {

            }
            else
            {
                // find NavigationViewItem with Content that equals InvokedItem
                var item = sender.MenuItems.OfType<NavigationViewItem>().First(x => (string)x.Content == (string)args.InvokedItem);
                NavBar_Navigate(item);
            }
        }

        private void NavBar_Navigate(NavigationViewItem item)
        {
            NavBar.IsPaneOpen = false;
            ContentFrame.Navigate(Assembly.GetExecutingAssembly().GetType($"PokemonDatabaseScraper.Views.{item.Tag}"), null, new EntranceNavigationTransitionInfo());
            switch (item.Tag)
            {
                case "CardLibraryView":
                    break;

                case "ScrapePokemonDBView":
                    CardLibraryTeachingTip.IsOpen = false;
                    break;

                case "ExportLibraryView":
                    SaveCardLibrary();
                    break;
            }
        }

        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {

        }

        private void NavBar_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(Assembly.GetExecutingAssembly().GetType($"PokemonDatabaseScraper.Views.CardLibraryView"), null, new EntranceNavigationTransitionInfo());
            NavBar.SelectedItem = NavBar.MenuItems[0];
        }

        private void NavBar_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {

        }
    }

    class WindowsSystemDispatcherQueueHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        struct DispatcherQueueOptions
        {
            internal int dwSize;
            internal int threadType;
            internal int apartmentType;
        }

        [DllImport("CoreMessaging.dll")]
        private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object dispatcherQueueController);

        object m_dispatcherQueueController = null;
        public void EnsureWindowsSystemDispatcherQueueController()
        {
            if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
            {
                // one already exists, so we'll just use it.
                return;
            }

            if (m_dispatcherQueueController == null)
            {
                DispatcherQueueOptions options;
                options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
                options.threadType = 2;    // DQTYPE_THREAD_CURRENT
                options.apartmentType = 2; // DQTAT_COM_STA

                CreateDispatcherQueueController(options, ref m_dispatcherQueueController);
            }
        }
    }

    public static class Globals
    {
        public static ObservableCollection<CardSet> CardSets = new ObservableCollection<CardSet>();
    }

    public class CardSet
    {
        public CardSet()
        {
            Cards = new ObservableCollection<Card>();
        }

        public string Name { get; set; }
        public ObservableCollection<Card> Cards { get; private set; }
    }

    public class Card
    {
        public string Name { get; set; }
        public int CardNumber { get; set; }

        [XmlIgnore]
        public Uri ImageUri { get; set; }

        [XmlAttribute("Uri")]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string UriString
        {
            get { return ImageUri == null ? null : ImageUri.OriginalString; }
            set { ImageUri = value == null ? null : new Uri(value); }
        }
    }

}
